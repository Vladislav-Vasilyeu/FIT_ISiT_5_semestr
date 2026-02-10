#include "HT.h"
#include <iostream>
#include <string>
#include <cstring>
#include <ctime>

namespace HT {
    // Расширение HTHANDLE
    struct HTHANDLEImpl : public HTHANDLE {
        CRITICAL_SECTION cs;
        volatile LONG running;
        HANDLE snapshot_thread;
        LPVOID table_addr;
        int slot_size;

        HTHANDLEImpl() : HTHANDLE(), running(0), snapshot_thread(NULL), table_addr(NULL), slot_size(0) {
            InitializeCriticalSection(&cs);
        }

        HTHANDLEImpl(int Capacity, int SecSnapshotInterval, int MaxKeyLength, int MaxPayloadLength, const char fname[512])
            : HTHANDLE(Capacity, SecSnapshotInterval, MaxKeyLength, MaxPayloadLength, fname),
            running(0), snapshot_thread(NULL), table_addr(NULL), slot_size(0) {
            InitializeCriticalSection(&cs);
        }

        ~HTHANDLEImpl() {
            DeleteCriticalSection(&cs);
        }
    };

    // Реализация конструкторов HTHANDLE (с переименованным параметром)
    HTHANDLE::HTHANDLE() : Capacity(0), SecSnapshotInterval(0), MaxKeyLength(0), MaxPayloadLength(0),
        File(INVALID_HANDLE_VALUE), FileMapping(NULL), Addr(NULL), lastsnaptime(0) {
        FileName[0] = '\0';
        LastErrorMessage[0] = '\0';
    }

    HTHANDLE::HTHANDLE(int Capacity, int SecSnapshotInterval, int MaxKeyLength, int MaxPayloadLength, const char fname[512])
        : Capacity(Capacity), SecSnapshotInterval(SecSnapshotInterval), MaxKeyLength(MaxKeyLength),
        MaxPayloadLength(MaxPayloadLength), File(INVALID_HANDLE_VALUE), FileMapping(NULL), Addr(NULL), lastsnaptime(0) {
        strcpy_s(FileName, 512, fname);
        LastErrorMessage[0] = '\0';
    }

    // Конструкторы Element
    Element::Element() : key(NULL), keylength(0), payload(NULL), payloadlength(0) {}
    Element::Element(const void* k, int kl) : key(k), keylength(kl), payload(NULL), payloadlength(0) {}
    Element::Element(const void* k, int kl, const void* p, int pl) : key(k), keylength(kl), payload(p), payloadlength(pl) {}
    Element::Element(Element* oe, const void* np, int npl) : key(oe->key), keylength(oe->keylength), payload(np), payloadlength(npl) {}

    // Хэш-функция
    unsigned long hash_key(const char* str, int len, int capacity) {
        unsigned long hash = 5381; // автор алгоритма (Дэниел Бернштейн) выбрал это число, оно достаточно большое и простое, что помогает равномерно распределить хеши по таблице
        for (int i = 0; i < len; ++i) {
            hash = ((hash << 5) + hash) + static_cast<unsigned char>(str[i]);  // unsigned для безопасности
        } // сдвиг вправо на 5 позиций
        return hash % capacity; // хеш очень большой, и он просто не влезет в нашу таблицу
    }

    // SnapshotProc
    DWORD WINAPI SnapshotProc(LPVOID param) { // Long Pointer Void, тип данных, указатель на void
        HTHANDLEImpl* ht = static_cast<HTHANDLEImpl*>(param);
        while (ht->running) {
            Sleep(ht->SecSnapshotInterval * 1000);
            if (ht->running) {
                EnterCriticalSection(&ht->cs);
                FlushViewOfFile(ht->Addr, 0);
                time(&ht->lastsnaptime);
                LeaveCriticalSection(&ht->cs);
            }
        }
        return 0;
    }

    // Create
    HTHANDLE* Create(int Capacity, int SecSnapshotInterval, int MaxKeyLength, int MaxPayloadLength, const char FileName[512]) {
        HTHANDLEImpl* ht = new HTHANDLEImpl(Capacity, SecSnapshotInterval, MaxKeyLength, MaxPayloadLength, FileName);
        ht->File = CreateFileA(ht->FileName, GENERIC_READ | GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);//
        if (ht->File == INVALID_HANDLE_VALUE) {
            strcpy_s(ht->LastErrorMessage, 512, "CreateFile failed");
            delete ht;
            return NULL;
        }

        // Заголовок
        DWORD written;
        WriteFile(ht->File, &ht->Capacity, sizeof(int), &written, NULL);
        WriteFile(ht->File, &ht->SecSnapshotInterval, sizeof(int), &written, NULL);
        WriteFile(ht->File, &ht->MaxKeyLength, sizeof(int), &written, NULL);
        WriteFile(ht->File, &ht->MaxPayloadLength, sizeof(int), &written, NULL);

        int header_size = 4 * sizeof(int);
        ht->slot_size = sizeof(bool) + sizeof(int) + (ht->MaxKeyLength + 1) + sizeof(int) + (ht->MaxPayloadLength + 1);
        LARGE_INTEGER total_size;
        total_size.QuadPart = (LONGLONG)header_size + (LONGLONG)ht->Capacity * ht->slot_size;
        LARGE_INTEGER pos = { 0 };
        pos.QuadPart = total_size.QuadPart;
        SetFilePointerEx(ht->File, pos, NULL, FILE_BEGIN);
        SetEndOfFile(ht->File);

        ht->FileMapping = CreateFileMappingA(ht->File, NULL, PAGE_READWRITE, total_size.HighPart, total_size.LowPart, NULL); // флаг защиты страниц памяти, указывает как защищать страницы, page-префикс, readwrite-разрешает запись и чтение 
        if (ht->FileMapping == NULL) {
            strcpy_s(ht->LastErrorMessage, 512, "CreateFileMapping failed");
            CloseHandle(ht->File);
            delete ht;
            return NULL;
        }

        ht->Addr = MapViewOfFile(ht->FileMapping, FILE_MAP_ALL_ACCESS, 0, 0, 0);
        if (ht->Addr == NULL) {
            strcpy_s(ht->LastErrorMessage, 512, "MapViewOfFile failed");
            CloseHandle(ht->FileMapping);
            CloseHandle(ht->File);
            delete ht;
            return NULL;
        }

        ht->table_addr = (LPVOID)((char*)ht->Addr + header_size);
        memset(ht->table_addr, 0, static_cast<size_t>(ht->Capacity * ht->slot_size)); // заполняет блок памяти нулями

        ht->running = 1;
        ht->snapshot_thread = CreateThread(NULL, 0, SnapshotProc, ht, 0, NULL);
        if (ht->snapshot_thread == NULL) {
            strcpy_s(ht->LastErrorMessage, 512, "CreateThread for snapshot failed");
            UnmapViewOfFile(ht->Addr);
            CloseHandle(ht->FileMapping);
            CloseHandle(ht->File);
            delete ht;
            return NULL;
        }

        return ht;
    }

    // Open
    HTHANDLE* Open(const char FileName[512]) {
        HTHANDLEImpl* ht = new HTHANDLEImpl();
        strcpy_s(ht->FileName, 512, FileName);

        ht->File = CreateFileA(ht->FileName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
        if (ht->File == INVALID_HANDLE_VALUE) {
            strcpy_s(ht->LastErrorMessage, 512, "OpenFile failed");
            delete ht;
            return NULL;
        }

        DWORD readed;
        int cap, sec, mk, mp;
        ReadFile(ht->File, &cap, sizeof(int), &readed, NULL);
        ReadFile(ht->File, &sec, sizeof(int), &readed, NULL);
        ReadFile(ht->File, &mk, sizeof(int), &readed, NULL);
        ReadFile(ht->File, &mp, sizeof(int), &readed, NULL);

        LARGE_INTEGER fsize;
        GetFileSizeEx(ht->File, &fsize);
        int header_size = 4 * sizeof(int);
        int slot_s = sizeof(bool) + sizeof(int) + (mk + 1) + sizeof(int) + (mp + 1);
        LONGLONG expected = (LONGLONG)header_size + (LONGLONG)cap * slot_s;
        if (fsize.QuadPart != expected) {
            strcpy_s(ht->LastErrorMessage, 512, "Invalid file size");
            CloseHandle(ht->File);
            delete ht;
            return NULL;
        }

        ht->FileMapping = CreateFileMappingA(ht->File, NULL, PAGE_READWRITE, 0, 0, NULL);
        if (ht->FileMapping == NULL) {
            strcpy_s(ht->LastErrorMessage, 512, "CreateFileMapping in Open failed");
            CloseHandle(ht->File);
            delete ht;
            return NULL;
        }

        ht->Addr = MapViewOfFile(ht->FileMapping, FILE_MAP_ALL_ACCESS, 0, 0, 0);
        if (ht->Addr == NULL) {
            strcpy_s(ht->LastErrorMessage, 512, "MapViewOfFile in Open failed");
            CloseHandle(ht->FileMapping);
            CloseHandle(ht->File);
            delete ht;
            return NULL;
        }

        ht->Capacity = cap;
        ht->SecSnapshotInterval = sec;
        ht->MaxKeyLength = mk;
        ht->MaxPayloadLength = mp;
        ht->slot_size = slot_s;
        ht->table_addr = (LPVOID)((char*)ht->Addr + header_size);

        ht->running = 1;
        ht->snapshot_thread = CreateThread(NULL, 0, SnapshotProc, ht, 0, NULL);
        if (ht->snapshot_thread == NULL) {
            strcpy_s(ht->LastErrorMessage, 512, "CreateThread in Open failed");// 3 способа синхронизации и отличия
            UnmapViewOfFile(ht->Addr);
            CloseHandle(ht->FileMapping);
            CloseHandle(ht->File);
            delete ht;
            return NULL;
        }

        return ht;
    }

    // Snap
    BOOL Snap(const HTHANDLE* hthandle) {
        if (hthandle == NULL || hthandle->Addr == NULL) return FALSE;
        HTHANDLEImpl* ht = const_cast<HTHANDLEImpl*>(static_cast<const HTHANDLEImpl*>(hthandle)); // вот эту строчку
        EnterCriticalSection(&ht->cs);
        BOOL res = FlushViewOfFile(ht->Addr, 0);
        time(&ht->lastsnaptime);
        LeaveCriticalSection(&ht->cs);
        return res;
    }

    // Close
    BOOL Close(const HTHANDLE* hthandle) {
        if (hthandle == NULL) return FALSE;
        HTHANDLEImpl* ht = const_cast<HTHANDLEImpl*>(static_cast<const HTHANDLEImpl*>(hthandle));

        InterlockedExchange(&ht->running, 0);
        if (ht->snapshot_thread != NULL) {
            WaitForSingleObject(ht->snapshot_thread, INFINITE);
            CloseHandle(ht->snapshot_thread);
            ht->snapshot_thread = NULL;
        }

        EnterCriticalSection(&ht->cs);
        if (ht->Addr != NULL) {
            Snap(hthandle);
            UnmapViewOfFile(ht->Addr);
            ht->Addr = NULL;
            ht->table_addr = NULL;
        }
        LeaveCriticalSection(&ht->cs);

        if (ht->FileMapping != NULL) {
            CloseHandle(ht->FileMapping);
            ht->FileMapping = NULL;
        }
        if (ht->File != INVALID_HANDLE_VALUE) {
            CloseHandle(ht->File);
            ht->File = INVALID_HANDLE_VALUE;
        }

        ht->LastErrorMessage[0] = '\0';
        delete ht;
        return TRUE;
    }

    // Insert
    BOOL Insert(const HTHANDLE* hthandle, const Element* element) {
        if (hthandle == NULL || element == NULL || element->key == NULL || element->keylength <= 0 || element->keylength > hthandle->MaxKeyLength ||
            element->payload == NULL || element->payloadlength <= 0 || element->payloadlength > hthandle->MaxPayloadLength) {
            if (hthandle) strcpy_s(const_cast<HTHANDLE*>(hthandle)->LastErrorMessage, 512, "Invalid params for Insert");
            return FALSE;
        }

        HTHANDLEImpl* ht = const_cast<HTHANDLEImpl*>(static_cast<const HTHANDLEImpl*>(hthandle)); //а налаогично
        const char* key_str = static_cast<const char*>(element->key);
        EnterCriticalSection(&ht->cs);

        unsigned long h = hash_key(key_str, element->keylength, ht->Capacity);
        for (int i = 0; i < ht->Capacity; ++i) {
            int idx = (h + i) % ht->Capacity;
            char* slot = static_cast<char*>(ht->table_addr) + idx * ht->slot_size;

            bool* occ = reinterpret_cast<bool*>(slot);
            int* kl = reinterpret_cast<int*>(slot + sizeof(bool));
            char* k_start = slot + sizeof(bool) + sizeof(int);

            if (!(*occ)) {
                *occ = true;
                *kl = element->keylength;
                memcpy(k_start, key_str, element->keylength);
                k_start[element->keylength] = '\0';  // null-terminate key

                char* pl_ptr = k_start + ht->MaxKeyLength + 1;//колизия и один ключ
                int* pl = reinterpret_cast<int*>(pl_ptr);
                char* p_start = pl_ptr + sizeof(int);
                *pl = element->payloadlength;
                const char* payload_str = static_cast<const char*>(element->payload);
                memcpy(p_start, payload_str, element->payloadlength);// куда, откуда, сколько
                p_start[element->payloadlength] = '\0';  // null-terminate payload

                LeaveCriticalSection(&ht->cs);
                return TRUE;
            }

            if (*kl == element->keylength && memcmp(k_start, key_str, element->keylength) == 0) { // сравнивает, что, с чем, скотлько
                strcpy_s(ht->LastErrorMessage, 512, "Key already exists");
                LeaveCriticalSection(&ht->cs);
                return FALSE;
            }
        }

        strcpy_s(ht->LastErrorMessage, 512, "Table full");
        LeaveCriticalSection(&ht->cs);
        return FALSE;
    }

    // Get
    Element* Get(const HTHANDLE* hthandle, const Element* element) {
        if (hthandle == NULL || element == NULL || element->key == NULL || element->keylength <= 0) {
            if (hthandle) strcpy_s(const_cast<HTHANDLE*>(hthandle)->LastErrorMessage, 512, "Invalid params for Get");
            return NULL;
        }

        HTHANDLEImpl* ht = const_cast<HTHANDLEImpl*>(static_cast<const HTHANDLEImpl*>(hthandle));
        const char* key_str = static_cast<const char*>(element->key);
        EnterCriticalSection(&ht->cs);

        unsigned long h = hash_key(key_str, element->keylength, ht->Capacity);
        for (int i = 0; i < ht->Capacity; ++i) {
            int idx = (h + i) % ht->Capacity;
            char* slot = static_cast<char*>(ht->table_addr) + idx * ht->slot_size;

            bool* occ = reinterpret_cast<bool*>(slot);
            if (!(*occ)) continue;

            int* kl = reinterpret_cast<int*>(slot + sizeof(bool));
            char* k_start = slot + sizeof(bool) + sizeof(int);
            if (*kl == element->keylength && memcmp(k_start, key_str, element->keylength) == 0) {
                char* pl_ptr = k_start + ht->MaxKeyLength + 1;
                int* pl = reinterpret_cast<int*>(pl_ptr);
                char* p_start = pl_ptr + sizeof(int);
                Element* res = new Element(k_start, *kl, p_start, *pl);
                LeaveCriticalSection(&ht->cs);
                return res;
            }
        }

        strcpy_s(ht->LastErrorMessage, 512, "Key not found");
        LeaveCriticalSection(&ht->cs);
        return NULL;
    }

    // Update
    BOOL Update(const HTHANDLE* hthandle, const Element* oldelement, const void* newpayload, int newpayloadlength) {
        if (hthandle == NULL || oldelement == NULL || oldelement->key == NULL || oldelement->keylength <= 0 ||
            newpayload == NULL || newpayloadlength <= 0 || newpayloadlength > hthandle->MaxPayloadLength) {
            if (hthandle) strcpy_s(const_cast<HTHANDLE*>(hthandle)->LastErrorMessage, 512, "Invalid params for Update");
            return FALSE;
        }

        HTHANDLEImpl* ht = const_cast<HTHANDLEImpl*>(static_cast<const HTHANDLEImpl*>(hthandle));
        const char* key_str = static_cast<const char*>(oldelement->key);
        EnterCriticalSection(&ht->cs);

        unsigned long h = hash_key(key_str, oldelement->keylength, ht->Capacity);
        for (int i = 0; i < ht->Capacity; ++i) {
            int idx = (h + i) % ht->Capacity;
            char* slot = static_cast<char*>(ht->table_addr) + idx * ht->slot_size;

            bool* occ = reinterpret_cast<bool*>(slot);
            if (!(*occ)) continue;

            int* kl = reinterpret_cast<int*>(slot + sizeof(bool));
            char* k_start = slot + sizeof(bool) + sizeof(int);
            if (*kl == oldelement->keylength && memcmp(k_start, key_str, oldelement->keylength) == 0) {
                char* pl_ptr = k_start + ht->MaxKeyLength + 1;
                int* pl = reinterpret_cast<int*>(pl_ptr);
                char* p_start = pl_ptr + sizeof(int);
                *pl = newpayloadlength;
                const char* np_str = static_cast<const char*>(newpayload);
                memcpy(p_start, np_str, newpayloadlength);
                p_start[newpayloadlength] = '\0';

                LeaveCriticalSection(&ht->cs);
                return TRUE;
            }
        }

        strcpy_s(ht->LastErrorMessage, 512, "Key not found for Update");
        LeaveCriticalSection(&ht->cs);
        return FALSE;
    }

    // Delete
    BOOL Delete(const HTHANDLE* hthandle, const Element* element) {
        if (hthandle == NULL || element == NULL || element->key == NULL || element->keylength <= 0) {
            if (hthandle) strcpy_s(const_cast<HTHANDLE*>(hthandle)->LastErrorMessage, 512, "Invalid params for Delete");
            return FALSE;
        }

        HTHANDLEImpl* ht = const_cast<HTHANDLEImpl*>(static_cast<const HTHANDLEImpl*>(hthandle));
        const char* key_str = static_cast<const char*>(element->key);
        EnterCriticalSection(&ht->cs);

        unsigned long h = hash_key(key_str, element->keylength, ht->Capacity);
        for (int i = 0; i < ht->Capacity; ++i) {
            int idx = (h + i) % ht->Capacity;
            char* slot = static_cast<char*>(ht->table_addr) + idx * ht->slot_size;

            bool* occ = reinterpret_cast<bool*>(slot);
            if (!(*occ)) continue;

            int* kl = reinterpret_cast<int*>(slot + sizeof(bool));
            char* k_start = slot + sizeof(bool) + sizeof(int);
            if (*kl == element->keylength && memcmp(k_start, key_str, element->keylength) == 0) {
                *occ = false;
                LeaveCriticalSection(&ht->cs);
                return TRUE;
            }
        }

        strcpy_s(ht->LastErrorMessage, 512, "Key not found for Delete");
        LeaveCriticalSection(&ht->cs);
        return FALSE;
    }

    // GetLastError
    char* GetLastError(HTHANDLE* ht) {
        if (ht == NULL) return NULL;
        HTHANDLEImpl* impl = static_cast<HTHANDLEImpl*>(ht);
        return (impl->LastErrorMessage[0] == '\0') ? NULL : impl->LastErrorMessage;
    }

    // print
    void print(const Element* element) {
        if (element == NULL) {
            std::cout << "NULL element" << std::endl;
            return;
        }
        const char* k = static_cast<const char*>(element->key);
        const char* p = static_cast<const char*>(element->payload);
        std::cout << "Key: [" << std::string(k, element->keylength) << "], Payload: [" << std::string(p, element->payloadlength) << "]" << std::endl;
    }
}