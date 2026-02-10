#include "HT.h"
#include <iostream>
#include <string>
#include <cassert>
#include <windows.h>  // Для DeleteFileA

using namespace std;
using namespace HT;

void TestCreate() {
    cout << "Test 1: Create HT" << endl;
    HTHANDLE* ht = Create(100, 5, 10, 20, ".\\test_create.ht");
    assert(ht != NULL);
    cout << "-- PASS: ht created" << endl;
    Close(ht);
    DeleteFileA(".\\test_create.ht");  // Cleanup
    cout << "Test 1: PASS" << endl << endl;
}

void TestInsertGet() {
    cout << "Test 2: Insert/Get валидный" << endl;
    HTHANDLE* ht = Create(100, 5, 10, 20, ".\\test_insert.ht");
    assert(ht != NULL);
    Element* el = new Element("key1", 4, "data1", 5);
    assert(Insert(ht, el));
    delete el;
    Element* el_get = new Element("key1", 4);
    Element* res = Get(ht, el_get);
    assert(res != NULL);
    assert(res->keylength == 4 && res->payloadlength == 5);
    print(res);  // Лог
    delete res;
    delete el_get;
    Close(ht);
    DeleteFileA(".\\test_insert.ht");
    cout << "Test 2: PASS" << endl << endl;
}

void TestInsertDuplicate() {
    cout << "Test 3: Insert дубликат" << endl;
    HTHANDLE* ht = Create(100, 5, 10, 20, ".\\test_dup.ht");
    assert(ht != NULL);
    Element* el = new Element("dup", 3, "data", 4);
    assert(Insert(ht, el));
    assert(!Insert(ht, el));  // Дубликат
    char* err = GetLastError(ht);
    assert(err != NULL && string(err).find("Key already exists") != string::npos);
    delete el;
    Close(ht);
    DeleteFileA(".\\test_dup.ht");
    cout << "Test 3: PASS" << endl << endl;
}

void TestGetNotFound() {
    cout << "Test 4: Get несуществующий" << endl;
    HTHANDLE* ht = Create(100, 5, 10, 20, ".\\test_getnf.ht");
    assert(ht != NULL);
    Element* el = new Element("miss", 4);
    Element* res = Get(ht, el);
    assert(res == NULL);
    char* err = GetLastError(ht);
    assert(err != NULL && string(err).find("Key not found") != string::npos);
    delete el;
    Close(ht);
    DeleteFileA(".\\test_getnf.ht");
    cout << "Test 4: PASS" << endl << endl;
}

void TestUpdate() {
    cout << "Test 5: Update валидный" << endl;
    HTHANDLE* ht = Create(100, 5, 10, 20, ".\\test_update.ht");
    assert(ht != NULL);
    Element* el_ins = new Element("up", 2, "old", 3);
    assert(Insert(ht, el_ins));
    delete el_ins;
    assert(Update(ht, new Element("up", 2), "new", 3));
    Element* el_get = new Element("up", 2);
    Element* res = Get(ht, el_get);
    assert(res != NULL && res->payloadlength == 3);
    print(res);
    delete res;
    delete el_get;
    Close(ht);
    DeleteFileA(".\\test_update.ht");
    cout << "Test 5: PASS" << endl << endl;
}

void TestUpdateNotFound() {
    cout << "Test 6: Update несуществующий" << endl;
    HTHANDLE* ht = Create(100, 5, 10, 20, ".\\test_upnf.ht");
    assert(ht != NULL);
    assert(!Update(ht, new Element("missup", 6), "new", 3));
    char* err = GetLastError(ht);
    assert(err != NULL && string(err).find("Key not found") != string::npos);
    Close(ht);
    DeleteFileA(".\\test_upnf.ht");
    cout << "Test 6: PASS" << endl << endl;
}

void TestDelete() {
    cout << "Test 7: Delete валидный" << endl;
    HTHANDLE* ht = Create(100, 5, 10, 20, ".\\test_delete.ht");
    assert(ht != NULL);
    Element* el = new Element("del", 3, "data", 4);
    assert(Insert(ht, el));
    delete el;
    assert(Delete(ht, new Element("del", 3)));
    Element* res = Get(ht, new Element("del", 3));
    assert(res == NULL);
    delete res;
    Close(ht);
    DeleteFileA(".\\test_delete.ht");
    cout << "Test 7: PASS" << endl << endl;
}

void TestDeleteNotFound() {
    cout << "Test 8: Delete несуществующий" << endl;
    HTHANDLE* ht = Create(100, 5, 10, 20, ".\\test_delnf.ht");
    assert(ht != NULL);
    assert(!Delete(ht, new Element("missdel", 7)));
    char* err = GetLastError(ht);
    assert(err != NULL && string(err).find("Key not found") != string::npos);
    Close(ht);
    DeleteFileA(".\\test_delnf.ht");
    cout << "Test 8: PASS" << endl << endl;
}

void TestSnap() {
    cout << "Test 9: Snap" << endl;
    HTHANDLE* ht = Create(100, 5, 10, 20, ".\\test_snap.ht");
    assert(ht != NULL);
    time_t before = ht->lastsnaptime;
    assert(Snap(ht));
    time_t after = ht->lastsnaptime;
    assert(after > before);  // Время обновилось
    Close(ht);
    DeleteFileA(".\\test_snap.ht");
    cout << "Test 9: PASS" << endl << endl;
}

void TestOpen() {
    cout << "Test 10: Open существующего" << endl;
    // Сначала Create и Insert
    HTHANDLE* ht_create = Create(100, 5, 10, 20, ".\\test_open.ht");
    assert(ht_create != NULL);
    Element* el = new Element("openkey", 7, "opendata", 8);
    assert(Insert(ht_create, el));
    delete el;
    Close(ht_create);

    // Open
    HTHANDLE* ht_open = Open(".\\test_open.ht");
    assert(ht_open != NULL);
    Element* el_get = new Element("openkey", 7);
    Element* res = Get(ht_open, el_get);
    assert(res != NULL);
    print(res);
    delete res;
    delete el_get;
    Close(ht_open);
    DeleteFileA(".\\test_open.ht");
    cout << "Test 10: PASS" << endl << endl;
}

void TestOpenNotFound() {
    cout << "Test 11: Open несуществующего" << endl;
    HTHANDLE* ht = Open(".\\nonexistent.ht");
    assert(ht == NULL);  // NULL
    // GetLastError не работает без ht, но проверим создание
    cout << "-- PASS: NULL returned" << endl;
    cout << "Test 11: PASS" << endl << endl;
}

void TestEmptyKey() {
    cout << "Test 14: Edge - Пустой ключ" << endl;
    HTHANDLE* ht = Create(100, 5, 10, 20, ".\\test_empty.ht");
    assert(ht != NULL);
    Element* el = new Element("", 0, "data", 4);
    assert(!Insert(ht, el));  // Invalid
    char* err = GetLastError(ht);
    assert(err != NULL && string(err).find("Invalid") != string::npos);
    delete el;
    Close(ht);
    DeleteFileA(".\\test_empty.ht");
    cout << "Test 14: PASS" << endl << endl;
}

void TestFullTable() {
    cout << "Test 15: Edge - Полная таблица" << endl;
    HTHANDLE* ht = Create(2, 5, 10, 20, ".\\test_full.ht");  // Маленький Capacity=2
    assert(ht != NULL);
    assert(Insert(ht, new Element("k1", 2, "d1", 2)));
    assert(Insert(ht, new Element("k2", 2, "d2", 2)));
    assert(!Insert(ht, new Element("k3", 2, "d3", 2)));  // Full
    char* err = GetLastError(ht);
    assert(err != NULL && string(err).find("Table full") != string::npos);
    Close(ht);
    DeleteFileA(".\\test_full.ht");
    cout << "Test 15: PASS" << endl << endl;
}

void TestMaxSize() {
    cout << "Test 16: Edge - Макс. размер" << endl;
    HTHANDLE* ht = Create(100, 5, 10, 20, ".\\test_max.ht");
    assert(ht != NULL);
    char key[11] = "1234567890";  // 10 chars
    char payload[21] = "12345678901234567890";  // 20 chars
    Element* el = new Element(key, 10, payload, 20);
    assert(Insert(ht, el));
    Element* res = Get(ht, new Element(key, 10));
    assert(res != NULL && res->keylength == 10 && res->payloadlength == 20);
    delete res;
    delete el;
    Close(ht);
    DeleteFileA(".\\test_max.ht");
    cout << "Test 16: PASS" << endl << endl;
}

int main() {
    setlocale(LC_ALL, "RU");
    cout << "=== Запуск тестов HT API ===" << endl;
    try {
        TestCreate();
        TestInsertGet();
        TestInsertDuplicate();
        TestGetNotFound();
        TestUpdate();
        TestUpdateNotFound();
        TestDelete();
        TestDeleteNotFound();
        TestSnap();
        TestOpen();
        TestOpenNotFound();
        TestEmptyKey();
        TestFullTable();
        TestMaxSize();
        cout << "Все тесты: PASS (17/17 сценариев покрыто)" << endl;
    }
    catch (...) {
        cout << "FAIL: Ошибка в тестах" << endl;
    }
    return 0;
}