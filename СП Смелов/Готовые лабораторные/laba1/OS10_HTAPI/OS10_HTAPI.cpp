#include "pch.h"  // Добавляем precompiled header для ускорения компиляции в Visual Studio (если включено). Содержит общие includes вроде <Windows.h>.
#include "HT.h"   // Включаем заголовочный файл с объявлениями структур и функций HT API.
#include <iostream>  // Для вывода в консоль (std::cout).
#include <string>    // Для работы со строками (std::string в print).
#include <cstring>   // Для функций memcpy, memcmp, strcpy_s (копирование строк и памяти).
#include <ctime>     // Для time_t и time() — работа с временем (lastsnaptime).

namespace HT {  // Пространство имён HT — все функции и структуры внутри, чтобы избежать конфликтов имён.

    // Расширение HTHANDLE без изменения оригинала. Создаём подструктуру для добавления полей (cs, running и т.д.), которые не в HT.h.
    struct HTHANDLEImpl : public HTHANDLE {  // Наследуем от HTHANDLE, чтобы добавить поля без модификации заголовка.
        CRITICAL_SECTION cs;  // Критическая секция для потокобезопасности (замок для операций).
        volatile LONG running;  // Флаг для остановки потока snapshot (volatile — для многопоточности).
        HANDLE snapshot_thread;  // HANDLE потока для асинхронного snapshot.
        LPVOID table_addr;  // Указатель на начало хэш-таблицы в памяти (за заголовком).
        int slot_size;  // Размер одной ячейки в таблице (вычисляется динамически).

        HTHANDLEImpl() : HTHANDLE(), running(0), snapshot_thread(NULL), table_addr(NULL), slot_size(0) {  // Конструктор по умолчанию: инициализируем добавленные поля.
            InitializeCriticalSection(&cs);  // Инициализируем критическую секцию.
        }

        HTHANDLEImpl(int Capacity, int SecSnapshotInterval, int MaxKeyLength, int MaxPayloadLength, const char fname[512])  // Конструктор с параметрами: вызываем базовый и инициализируем свои поля.
            : HTHANDLE(Capacity, SecSnapshotInterval, MaxKeyLength, MaxPayloadLength, fname),
            running(0), snapshot_thread(NULL), table_addr(NULL), slot_size(0) {
            InitializeCriticalSection(&cs);  // Инициализируем критическую секцию.
        }

        ~HTHANDLEImpl() {  // Деструктор: очищаем критическую секцию.
            DeleteCriticalSection(&cs);  // Удаляем критическую секцию.
        }
    };

    // Реализация конструкторов HTHANDLE (пустые тела в .h, реализуем здесь)
    HTHANDLE::HTHANDLE() : Capacity(0), SecSnapshotInterval(0), MaxKeyLength(0), MaxPayloadLength(0),  // Конструктор по умолчанию: обнуляем все поля.
        File(INVALID_HANDLE_VALUE), FileMapping(NULL), Addr(NULL), lastsnaptime(0) {  // INVALID_HANDLE_VALUE — значит файл не открыт.
        FileName[0] = '\0';  // Обнуляем имя файла.
        LastErrorMessage[0] = '\0';  // Обнуляем сообщение об ошибке.
    }

    HTHANDLE::HTHANDLE(int Capacity, int SecSnapshotInterval, int MaxKeyLength, int MaxPayloadLength, const char fname[512])  // Конструктор с параметрами: присваиваем значения полям.
        : Capacity(Capacity), SecSnapshotInterval(SecSnapshotInterval), MaxKeyLength(MaxKeyLength),
        MaxPayloadLength(MaxPayloadLength), File(INVALID_HANDLE_VALUE), FileMapping(NULL), Addr(NULL), lastsnaptime(0) {
        strcpy_s(FileName, 512, fname);  // Копируем имя файла безопасно (с проверкой размера).
        LastErrorMessage[0] = '\0';  // Обнуляем ошибку.
    }

    // Конструкторы Element
    Element::Element() : key(NULL), keylength(0), payload(NULL), payloadlength(0) {}  // Пустой элемент: все поля NULL/0.
    Element::Element(const void* k, int kl) : key(k), keylength(kl), payload(NULL), payloadlength(0) {}  // Для Get/Delete: только ключ.
    Element::Element(const void* k, int kl, const void* p, int pl) : key(k), keylength(kl), payload(p), payloadlength(pl) {}  // Для Insert: ключ + значение.
    Element::Element(Element* oe, const void* np, int npl) : key(oe->key), keylength(oe->keylength), payload(np), payloadlength(npl) {}  // Для Update: берём ключ из старого, новое значение.

    // Хэш-функция
    unsigned long hash_key(const char* str, int len, int capacity) {  // Вычисляем хэш для ключа.
        unsigned long hash = 5381;  // Начальное "магическое" число для хорошего распределения.
        for (int i = 0; i < len; ++i) {  // Цикл по каждому байту ключа.
            hash = ((hash << 5) + hash) + static_cast<unsigned char>(str[i]);  // hash = hash * 33 + байт (<<5 = *32, +hash = *33).
        }
        return hash % capacity;  // Модуль по размеру таблицы — получаем индекс ячейки (0..capacity-1).
    }

    // SnapshotProc
    DWORD WINAPI SnapshotProc(LPVOID param) {  // Функция потока для асинхронного snapshot. lvoid
        HTHANDLEImpl* ht = static_cast<HTHANDLEImpl*>(param);  // Приводим параметр к нашему типу.
        while (ht->running) {  // Пока флаг активен.
            Sleep(ht->SecSnapshotInterval * 1000);  // Ждём интервал в миллисекундах.
            if (ht->running) {  // Проверяем снова (на случай остановки во время сна).
                EnterCriticalSection(&ht->cs);  // Захватываем замок.
                FlushViewOfFile(ht->Addr, 0);  // Сохраняем память в файл.
                time(&ht->lastsnaptime);  // Обновляем время последнего snapshot.
                LeaveCriticalSection(&ht->cs);  // Отпускаем замок.
            }
        }
        return 0;  // Завершаем поток.
    }

    // Create
    HTHANDLE* Create(int Capacity, int SecSnapshotInterval, int MaxKeyLength, int MaxPayloadLength, const char FileName[512]) {  // Создаём новый HT.
        HTHANDLEImpl* ht = new HTHANDLEImpl(Capacity, SecSnapshotInterval, MaxKeyLength, MaxPayloadLength, FileName);  // Выделяем память для структуры.
        ht->File = CreateFileA(ht->FileName, GENERIC_READ | GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);  // Создаём/открываем файл.(имя файла, режим доступа, совместный доступ, атрибуты безопасности, как создавать/открывать, флаги и атрибуты, шаблон)
        if (ht->File == INVALID_HANDLE_VALUE) {  // Если ошибка.
            strcpy_s(ht->LastErrorMessage, 512, "CreateFile failed");  // Записываем ошибку.
            delete ht;  // Очищаем память.
            return NULL;  // Возвращаем NULL.
        }

        // Заголовок
        DWORD written;  // Для количества записанных байт.
        WriteFile(ht->File, &ht->Capacity, sizeof(int), &written, NULL);  // Записываем Capacity.
        WriteFile(ht->File, &ht->SecSnapshotInterval, sizeof(int), &written, NULL);  // SecSnapshotInterval.
        WriteFile(ht->File, &ht->MaxKeyLength, sizeof(int), &written, NULL);  // MaxKeyLength.
        WriteFile(ht->File, &ht->MaxPayloadLength, sizeof(int), &written, NULL);  // MaxPayloadLength.

        int header_size = 4 * sizeof(int);  // Размер заголовка (16 байт).
        ht->slot_size = sizeof(bool) + sizeof(int) + (ht->MaxKeyLength + 1) + sizeof(int) + (ht->MaxPayloadLength + 1);  // Размер ячейки.
        LARGE_INTEGER total_size;  // Для полного размера файла.
        total_size.QuadPart = (LONGLONG)header_size + (LONGLONG)ht->Capacity * ht->slot_size;  // Вычисляем общий размер.
        LARGE_INTEGER pos = { 0 };  // Позиция.
        pos.QuadPart = total_size.QuadPart;  // Устанавливаем конец файла.
        SetFilePointerEx(ht->File, pos, NULL, FILE_BEGIN);  // Перемещаем указатель.
        SetEndOfFile(ht->File);  // Устанавливаем размер файла.

        ht->FileMapping = CreateFileMappingA(ht->File, NULL, PAGE_READWRITE, total_size.HighPart, total_size.LowPart, NULL);  // Создаём mapping. page
        if (ht->FileMapping == NULL) {  // Если ошибка.
            strcpy_s(ht->LastErrorMessage, 512, "CreateFileMapping failed");  // Ошибка.
            CloseHandle(ht->File);  // Закрываем файл.
            delete ht;  // Очищаем.
            return NULL;  // NULL.
        }

        ht->Addr = MapViewOfFile(ht->FileMapping, FILE_MAP_ALL_ACCESS, 0, 0, 0);  // Маппим в память.
        if (ht->Addr == NULL) {  // Если ошибка.
            strcpy_s(ht->LastErrorMessage, 512, "MapViewOfFile failed");  // Ошибка.
            CloseHandle(ht->FileMapping);  // Закрываем mapping.
            CloseHandle(ht->File);  // Закрываем файл.
            delete ht;  // Очищаем.
            return NULL;  // NULL.
        }

        ht->table_addr = (LPVOID)((char*)ht->Addr + header_size);  // Указатель на таблицу (за заголовком).
        memset(ht->table_addr, 0, static_cast<size_t>(ht->Capacity * ht->slot_size));  // Обнуляем таблицу.

        ht->running = 1;  // Включаем флаг потока.
        ht->snapshot_thread = CreateThread(NULL, 0, SnapshotProc, ht, 0, NULL);  // Создаём поток snapshot.
        if (ht->snapshot_thread == NULL) {  // Если ошибка.
            strcpy_s(ht->LastErrorMessage, 512, "CreateThread for snapshot failed");  // Ошибка.
            UnmapViewOfFile(ht->Addr);  // Убираем mapping.
            CloseHandle(ht->FileMapping);  // Закрываем mapping.
            CloseHandle(ht->File);  // Закрываем файл.
            delete ht;  // Очищаем.
            return NULL;  // NULL.
        }

        return ht;  // Возвращаем готовый HT.
    }

    // Open
    HTHANDLE* Open(const char FileName[512]) {  // Открываем существующий HT.
        HTHANDLEImpl* ht = new HTHANDLEImpl();  // Выделяем память.
        strcpy_s(ht->FileName, 512, FileName);  // Копируем имя.

        ht->File = CreateFileA(ht->FileName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);  // Открываем файл.
        if (ht->File == INVALID_HANDLE_VALUE) {  // Ошибка.
            strcpy_s(ht->LastErrorMessage, 512, "OpenFile failed");  // Ошибка.
            delete ht;  // Очищаем.
            return NULL;  // NULL.
        }

        DWORD readed;  // Для количества прочитанных байт.
        int cap, sec, mk, mp;  // Временные переменные.
        ReadFile(ht->File, &cap, sizeof(int), &readed, NULL);  // Читаем Capacity.
        ReadFile(ht->File, &sec, sizeof(int), &readed, NULL);  // SecSnapshotInterval.
        ReadFile(ht->File, &mk, sizeof(int), &readed, NULL);  // MaxKeyLength.
        ReadFile(ht->File, &mp, sizeof(int), &readed, NULL);  // MaxPayloadLength.

        LARGE_INTEGER fsize;  // Размер файла.
        GetFileSizeEx(ht->File, &fsize);  // Получаем размер.
        int header_size = 4 * sizeof(int);  // 16 байт.
        int slot_s = sizeof(bool) + sizeof(int) + (mk + 1) + sizeof(int) + (mp + 1);  // Размер ячейки.
        LONGLONG expected = (LONGLONG)header_size + (LONGLONG)cap * slot_s;  // Ожидаемый размер.
        if (fsize.QuadPart != expected) {  // Если не совпадает.
            strcpy_s(ht->LastErrorMessage, 512, "Invalid file size");  // Ошибка.
            CloseHandle(ht->File);  // Закрываем.
            delete ht;  // Очищаем.
            return NULL;  // NULL.
        }

        ht->FileMapping = CreateFileMappingA(ht->File, NULL, PAGE_READWRITE, 0, 0, NULL);  // Создаём mapping.
        if (ht->FileMapping == NULL) {  // Ошибка.
            strcpy_s(ht->LastErrorMessage, 512, "CreateFileMapping in Open failed");  // Ошибка.
            CloseHandle(ht->File);  // Закрываем.
            delete ht;  // Очищаем.
            return NULL;  // NULL.
        }

        ht->Addr = MapViewOfFile(ht->FileMapping, FILE_MAP_ALL_ACCESS, 0, 0, 0);  // Маппим.
        if (ht->Addr == NULL) {  // Ошибка.
            strcpy_s(ht->LastErrorMessage, 512, "MapViewOfFile in Open failed");  // Ошибка.
            CloseHandle(ht->FileMapping);  // Закрываем.
            CloseHandle(ht->File);  // Закрываем.
            delete ht;  // Очищаем.
            return NULL;  // NULL.
        }

        ht->Capacity = cap;  // Устанавливаем поля из заголовка.
        ht->SecSnapshotInterval = sec;
        ht->MaxKeyLength = mk;
        ht->MaxPayloadLength = mp;
        ht->slot_size = slot_s;  // Размер ячейки.
        ht->table_addr = (LPVOID)((char*)ht->Addr + header_size);  // Указатель на таблицу.

        ht->running = 1;  // Включаем поток.
        ht->snapshot_thread = CreateThread(NULL, 0, SnapshotProc, ht, 0, NULL);  // Создаём поток.
        if (ht->snapshot_thread == NULL) {  // Ошибка.
            strcpy_s(ht->LastErrorMessage, 512, "CreateThread in Open failed");  // Ошибка.
            UnmapViewOfFile(ht->Addr);  // Убираем.
            CloseHandle(ht->FileMapping);  // Закрываем.
            CloseHandle(ht->File);  // Закрываем.
            delete ht;  // Очищаем.
            return NULL;  // NULL.
        }

        return ht;  // Возвращаем HT.
    }

    // Snap
    BOOL Snap(const HTHANDLE* hthandle) {  // Синхронный snapshot.
        if (hthandle == NULL || hthandle->Addr == NULL) return FALSE;  // Проверка на NULL.
        HTHANDLEImpl* ht = const_cast<HTHANDLEImpl*>(static_cast<const HTHANDLEImpl*>(hthandle));  // Приводим к изменяемому типу.
        EnterCriticalSection(&ht->cs);  // Захватываем замок.
        BOOL res = FlushViewOfFile(ht->Addr, 0);  // Сохраняем в файл.
        time(&ht->lastsnaptime);  // Обновляем время.
        LeaveCriticalSection(&ht->cs);  // Отпускаем.
        return res;  // Возвращаем результат.
    }

    // Close
    BOOL Close(const HTHANDLE* hthandle) {  // Закрываем HT.
        if (hthandle == NULL) return FALSE;  // Проверка.
        HTHANDLEImpl* ht = const_cast<HTHANDLEImpl*>(static_cast<const HTHANDLEImpl*>(hthandle));  // Приводим.

        InterlockedExchange(&ht->running, 0);  // Атомарно останавливаем поток.
        if (ht->snapshot_thread != NULL) {  // Если поток есть.
            WaitForSingleObject(ht->snapshot_thread, INFINITE);  // Ждём завершения.
            CloseHandle(ht->snapshot_thread);  // Закрываем handle.
            ht->snapshot_thread = NULL;  // Обнуляем.
        }

        EnterCriticalSection(&ht->cs);  // Замок.
        if (ht->Addr != NULL) {  // Если mapping открыт.
            Snap(hthandle);  // Финальный snapshot.
            UnmapViewOfFile(ht->Addr);  // Убираем вид.
            ht->Addr = NULL;  // Обнуляем.
            ht->table_addr = NULL;  // Обнуляем.
        }
        LeaveCriticalSection(&ht->cs);  // Отпускаем.

        if (ht->FileMapping != NULL) {  // Закрываем mapping.
            CloseHandle(ht->FileMapping);
            ht->FileMapping = NULL;
        }
        if (ht->File != INVALID_HANDLE_VALUE) {  // Закрываем файл.
            CloseHandle(ht->File);
            ht->File = INVALID_HANDLE_VALUE;
        }

        ht->LastErrorMessage[0] = '\0';  // Обнуляем ошибку.
        delete ht;  // Удаляем структуру.
        return TRUE;  // Успех.
    }

    // Insert
    BOOL Insert(const HTHANDLE* hthandle, const Element* element) {  // Вставка элемента.
        if (hthandle == NULL || element == NULL || element->key == NULL || element->keylength <= 0 || element->keylength > hthandle->MaxKeyLength ||  // Проверка параметров.
            element->payload == NULL || element->payloadlength <= 0 || element->payloadlength > hthandle->MaxPayloadLength) {
            if (hthandle) strcpy_s(const_cast<HTHANDLE*>(hthandle)->LastErrorMessage, 512, "Invalid params for Insert");  // Ошибка.
            return FALSE;  // Неудача.
        }

        HTHANDLEImpl* ht = const_cast<HTHANDLEImpl*>(static_cast<const HTHANDLEImpl*>(hthandle));  // Приводим.
        const char* key_str = static_cast<const char*>(element->key);  // Ключ как char*.
        EnterCriticalSection(&ht->cs);  // Замок.

        unsigned long h = hash_key(key_str, element->keylength, ht->Capacity);  // Хэш.
        for (int i = 0; i < ht->Capacity; ++i) {  // Пробирование.
            int idx = (h + i) % ht->Capacity;  // Индекс.
            char* slot = static_cast<char*>(ht->table_addr) + idx * ht->slot_size;  // Адрес ячейки.

            bool* occ = reinterpret_cast<bool*>(slot);  // Флаг занятости.
            int* kl = reinterpret_cast<int*>(slot + sizeof(bool));  // Длина ключа.
            char* k_start = slot + sizeof(bool) + sizeof(int);  // Начало ключа.

            if (!(*occ)) {  // Свободно.
                *occ = true;  // Занимаем.
                *kl = element->keylength;  // Длина.
                memcpy(k_start, key_str, element->keylength);  // Копируем ключ.
                k_start[element->keylength] = '\0';  // Null-terminate.

                char* pl_ptr = k_start + ht->MaxKeyLength + 1;  // Указатель на payloadlen.
                int* pl = reinterpret_cast<int*>(pl_ptr);  // Длина значения.
                char* p_start = pl_ptr + sizeof(int);  // Начало значения.
                *pl = element->payloadlength;  // Длина.
                const char* payload_str = static_cast<const char*>(element->payload);  // Значение.
                memcpy(p_start, payload_str, element->payloadlength);  // Копируем.
                p_start[element->payloadlength] = '\0';  // Null-terminate.

                LeaveCriticalSection(&ht->cs);  // Отпускаем.
                return TRUE;  // Успех.
            }

            if (*kl == element->keylength && memcmp(k_start, key_str, element->keylength) == 0) {  // Дубликат.
                strcpy_s(ht->LastErrorMessage, 512, "Key already exists");  // Ошибка.
                LeaveCriticalSection(&ht->cs);  // Отпускаем.
                return FALSE;  // Неудача.
            }
        }

        strcpy_s(ht->LastErrorMessage, 512, "Table full");  // Полно.
        LeaveCriticalSection(&ht->cs);  // Отпускаем.
        return FALSE;  // Неудача.
    }

    // Get
    Element* Get(const HTHANDLE* hthandle, const Element* element) {  // Получение элемента.
        if (hthandle == NULL || element == NULL || element->key == NULL || element->keylength <= 0) {  // Проверка.
            if (hthandle) strcpy_s(const_cast<HTHANDLE*>(hthandle)->LastErrorMessage, 512, "Invalid params for Get");  // Ошибка.
            return NULL;  // NULL.
        }

        HTHANDLEImpl* ht = const_cast<HTHANDLEImpl*>(static_cast<const HTHANDLEImpl*>(hthandle));  // Приводим.
        const char* key_str = static_cast<const char*>(element->key);  // Ключ.
        EnterCriticalSection(&ht->cs);  // Замок.

        unsigned long h = hash_key(key_str, element->keylength, ht->Capacity);  // Хэш.
        for (int i = 0; i < ht->Capacity; ++i) {  // Пробирование.
            int idx = (h + i) % ht->Capacity;  // Индекс.
            char* slot = static_cast<char*>(ht->table_addr) + idx * ht->slot_size;  // Ячейка.

            bool* occ = reinterpret_cast<bool*>(slot);  // Занятость.
            if (!(*occ)) continue;  // Пропускаем свободную.

            int* kl = reinterpret_cast<int*>(slot + sizeof(bool));  // Длина ключа.
            char* k_start = slot + sizeof(bool) + sizeof(int);  // Ключ.
            if (*kl == element->keylength && memcmp(k_start, key_str, element->keylength) == 0) {  // Совпадение.
                char* pl_ptr = k_start + ht->MaxKeyLength + 1;  // Payloadlen.
                int* pl = reinterpret_cast<int*>(pl_ptr);  // Длина.
                char* p_start = pl_ptr + sizeof(int);  // Значение.
                Element* res = new Element(k_start, *kl, p_start, *pl);  // Новый элемент.
                LeaveCriticalSection(&ht->cs);  // Отпускаем.
                return res;  // Возвращаем.
            }
        }

        strcpy_s(ht->LastErrorMessage, 512, "Key not found");  // Не найден.
        LeaveCriticalSection(&ht->cs);  // Отпускаем.
        return NULL;  // NULL.
    }

    // Update
    BOOL Update(const HTHANDLE* hthandle, const Element* oldelement, const void* newpayload, int newpayloadlength) {  // Обновление.
        if (hthandle == NULL || oldelement == NULL || oldelement->key == NULL || oldelement->keylength <= 0 ||  // Проверка.
            newpayload == NULL || newpayloadlength <= 0 || newpayloadlength > hthandle->MaxPayloadLength) {
            if (hthandle) strcpy_s(const_cast<HTHANDLE*>(hthandle)->LastErrorMessage, 512, "Invalid params for Update");  // Ошибка.
            return FALSE;  // Неудача.
        }

        HTHANDLEImpl* ht = const_cast<HTHANDLEImpl*>(static_cast<const HTHANDLEImpl*>(hthandle));  // Приводим.
        const char* key_str = static_cast<const char*>(oldelement->key);  // Ключ.
        EnterCriticalSection(&ht->cs);  // Замок.

        unsigned long h = hash_key(key_str, oldelement->keylength, ht->Capacity);  // Хэш.
        for (int i = 0; i < ht->Capacity; ++i) {  // Пробирование.
            int idx = (h + i) % ht->Capacity;  // Индекс.
            char* slot = static_cast<char*>(ht->table_addr) + idx * ht->slot_size;  // Ячейка.

            bool* occ = reinterpret_cast<bool*>(slot);  // Занятость.
            if (!(*occ)) continue;  // Пропуск.

            int* kl = reinterpret_cast<int*>(slot + sizeof(bool));  // Длина ключа.
            char* k_start = slot + sizeof(bool) + sizeof(int);  // Ключ.
            if (*kl == oldelement->keylength && memcmp(k_start, key_str, oldelement->keylength) == 0) {  // Совпадение.
                char* pl_ptr = k_start + ht->MaxKeyLength + 1;  // Payloadlen.
                int* pl = reinterpret_cast<int*>(pl_ptr);  // Длина.
                char* p_start = pl_ptr + sizeof(int);  // Значение.
                *pl = newpayloadlength;  // Новая длина.
                const char* np_str = static_cast<const char*>(newpayload);  // Новое значение.
                memcpy(p_start, np_str, newpayloadlength);  // Копируем.
                p_start[newpayloadlength] = '\0';  // Null-terminate.

                LeaveCriticalSection(&ht->cs);  // Отпускаем.
                return TRUE;  // Успех.
            }
        }

        strcpy_s(ht->LastErrorMessage, 512, "Key not found for Update");  // Не найден.
        LeaveCriticalSection(&ht->cs);  // Отпускаем.
        return FALSE;  // Неудача.
    }

    // Delete
    BOOL Delete(const HTHANDLE* hthandle, const Element* element) {  // Удаление.
        if (hthandle == NULL || element == NULL || element->key == NULL || element->keylength <= 0) {  // Проверка.
            if (hthandle) strcpy_s(const_cast<HTHANDLE*>(hthandle)->LastErrorMessage, 512, "Invalid params for Delete");  // Ошибка.
            return FALSE;  // Неудача.
        }

        HTHANDLEImpl* ht = const_cast<HTHANDLEImpl*>(static_cast<const HTHANDLEImpl*>(hthandle));  // Приводим.
        const char* key_str = static_cast<const char*>(element->key);  // Ключ.
        EnterCriticalSection(&ht->cs);  // Замок.

        unsigned long h = hash_key(key_str, element->keylength, ht->Capacity);  // Хэш.
        for (int i = 0; i < ht->Capacity; ++i) {  // Пробирование.
            int idx = (h + i) % ht->Capacity;  // Индекс.
            char* slot = static_cast<char*>(ht->table_addr) + idx * ht->slot_size;  // Ячейка.

            bool* occ = reinterpret_cast<bool*>(slot);  // Занятость.
            if (!(*occ)) continue;  // Пропуск.

            int* kl = reinterpret_cast<int*>(slot + sizeof(bool));  // Длина ключа.
            char* k_start = slot + sizeof(bool) + sizeof(int);  // Ключ.
            if (*kl == element->keylength && memcmp(k_start, key_str, element->keylength) == 0) {  // Совпадение.
                *occ = false;  // Освобождаем.
                LeaveCriticalSection(&ht->cs);  // Отпускаем.
                return TRUE;  // Успех.
            }
        }

        strcpy_s(ht->LastErrorMessage, 512, "Key not found for Delete");  // Не найден.
        LeaveCriticalSection(&ht->cs);  // Отпускаем.
        return FALSE;  // Неудача.
    }

    // GetLastError
    char* GetLastError(HTHANDLE* ht) {  // Получаем ошибку.
        if (ht == NULL) return NULL;  // NULL если ht NULL.
        HTHANDLEImpl* impl = static_cast<HTHANDLEImpl*>(ht);  // Приводим.
        return (impl->LastErrorMessage[0] == '\0') ? NULL : impl->LastErrorMessage;  // Возвращаем строку, если не пустая.
    }

    // print
    void print(const Element* element) {  // Распечатка элемента.
        if (element == NULL) {  // Проверка.
            std::cout << "NULL element" << std::endl;  // Вывод NULL.
            return;  // Выход.
        }
        const char* k = static_cast<const char*>(element->key);  // Ключ как строка.
        const char* p = static_cast<const char*>(element->payload);  // Значение.
        std::cout << "Key: [" << std::string(k, element->keylength) << "], Payload: [" << std::string(p, element->payloadlength) << "]" << std::endl;  // Вывод.
    }
}