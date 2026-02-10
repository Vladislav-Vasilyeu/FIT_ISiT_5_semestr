#include "HT.h"
#include <iostream>
#include <string>

using namespace std;
using namespace HT;

int main() {
    setlocale(LC_ALL, "RU");
    cout << "=== Демонстрация 2 экземпляров HT (через библиотеку OS10_HTAPI) ===" << endl;
    HTHANDLE* ht1 = NULL;
    HTHANDLE* ht2 = NULL;

    try {
        // Создание первого экземпляра
        ht1 = Create(1000, 10, 256, 512, "D:\\Универ\\СП Смелов\\Готовые лабораторные\\laba1\\HT1.ht");
        if (!ht1) throw string("-- Create ht1: error");
        cout << "-- Create ht1: success" << endl;

        // Создание второго экземпляра (одновременно)
        ht2 = Create(1000, 10, 256, 512, "D:\\Универ\\СП Смелов\\Готовые лабораторные\\laba1\\HT2.ht");
        if (!ht2) throw string("-- Create ht2: error");
        cout << "-- Create ht2: success" << endl;

        // Операции на ht1
        Element* el_insert1 = new Element("keyA", 4, "payloadA", 8);
        if (Insert(ht1, el_insert1)) cout << "-- Insert in ht1 (keyA): success" << endl;
        else throw string("-- Insert in ht1: error");
        delete el_insert1;

        Element* el_get1 = new Element("keyA", 4);
        Element* hte1 = Get(ht1, el_get1);
        if (hte1) {
            cout << "-- Get in ht1: success" << endl;
            print(hte1);
            delete hte1;
        }
        else throw string("-- Get in ht1: error");
        delete el_get1;

        Element* el_update1 = new Element("keyA", 4);
        if (Update(ht1, el_update1, "updatedA", 8)) cout << "-- Update in ht1: success" << endl;
        else throw string("-- Update in ht1: error");
        delete el_update1;

        el_get1 = new Element("keyA", 4);
        hte1 = Get(ht1, el_get1);
        if (hte1) {
            cout << "-- Get after update in ht1: success" << endl;
            print(hte1);
            delete hte1;
        }
        else throw string("-- Get after update in ht1: error");
        delete el_get1;

        // Операции на ht2 (чередование)
        Element* el_insert2 = new Element("keyB", 4, "payloadB", 8);
        if (Insert(ht2, el_insert2)) cout << "-- Insert in ht2 (keyB): success" << endl;
        else throw string("-- Insert in ht2: error");
        delete el_insert2;

        Element* el_get2 = new Element("keyB", 4);
        Element* hte2 = Get(ht2, el_get2);
        if (hte2) {
            cout << "-- Get in ht2: success" << endl;
            print(hte2);
            delete hte2;
        }
        else throw string("-- Get in ht2: error");
        delete el_get2;

        // Delete в ht1
        el_get1 = new Element("keyA", 4);
        if (Delete(ht1, el_get1)) cout << "-- Delete in ht1: success" << endl;
        else throw string("-- Delete in ht1: error");
        delete el_get1;

        el_get1 = new Element("keyA", 4);
        hte1 = Get(ht1, el_get1);
        if (!hte1) cout << "-- Get after delete in ht1: success (not found)" << endl;
        else {
            delete hte1;
            throw string("-- Delete in ht1 failed");
        }
        delete el_get1;

        // Snap на обоих (синхронно)
        if (Snap(ht1)) cout << "-- Snap ht1: success" << endl;
        else throw string("-- Snap ht1: error");
        if (Snap(ht2)) cout << "-- Snap ht2: success" << endl;
        else throw string("-- Snap ht2: error");

        // Close ht2
        if (Close(ht2)) cout << "-- Close ht2: success" << endl;
        else throw string("-- Close ht2: error");
        ht2 = NULL;

        if (Close(ht1)) cout << "-- Close ht1: success" << endl;
        else throw string("-- Close ht1: error");
        ht1 = NULL;

        // Open ht1 (демонстрация персистентности)
        ht1 = Open("D:\\Универ\\СП Смелов\\Готовые лабораторные\\laba1\\HT1.ht");  // keyA удалён, но файл существует
        if (!ht1) throw string("-- Open ht1: error");
        cout << "-- Open ht1: success" << endl;

        // Insert в открытый ht1
        Element* el_insert_open = new Element("keyC", 4, "payloadC_open", 13);
        if (Insert(ht1, el_insert_open)) cout << "-- Insert in opened ht1 (keyC): success" << endl;
        else throw string("-- Insert in opened ht1: error");
        delete el_insert_open;

        Element* el_get_open = new Element("keyC", 4);
        Element* hte_open = Get(ht1, el_get_open);
        if (hte_open) {
            cout << "-- Get in opened ht1: success" << endl;
            print(hte_open);
            delete hte_open;
        }
        else throw string("-- Get in opened ht1: error");
        delete el_get_open;

        if (Close(ht1)) cout << "-- Close opened ht1: success" << endl;
        else throw string("-- Close opened ht1: error");
        ht1 = NULL;

        cout << "\nВсе операции с 2 экземплярами завершены успешно!" << endl;
    }
    catch (const string& msg) {
        cout << msg << endl;
        if (ht1) {
            char* err1 = GetLastError(ht1);
            if (err1) cout << "ht1 error: " << err1 << endl;
            Close(ht1);
            ht1 = NULL;
        }
        if (ht2) {
            char* err2 = GetLastError(ht2);
            if (err2) cout << "ht2 error: " << err2 << endl;
            Close(ht2);
            ht2 = NULL;
        }
    }

    return 0;
}