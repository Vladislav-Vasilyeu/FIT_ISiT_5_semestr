#include "HT.h"
#include <iostream>
#include <string>

using namespace std;
using namespace HT;

int main() {
    setlocale(LC_ALL, "RU");
    cout << "=== OS10_01 ===" << endl;
    cout << "=== Демонстрация Create + операции ===" << endl;
    HTHANDLE* ht = NULL;
    try {
        ht = Create(1000, 10, 256, 512, "D:\\Универ\\СП Смелов\\Готовые лабораторные\\laba1\\HTspace.ht");
        if (!ht) throw string("-- Create: error");

        cout << "-- Create: success" << endl;

        Element* el_insert = new Element("key222", 6, "payload", 7);
        if (Insert(ht, el_insert)) cout << "-- Insert: success" << endl;
        else throw string("-- Insert: error");
        delete el_insert;

        Element* el_get = new Element("key222", 6);
        Element* hte = Get(ht, el_get);
        if (hte) {
            cout << "-- Get: success" << endl;
            print(hte);
            delete hte;
        }
        else throw string("-- Get: error");
        delete el_get;

        if (Snap(ht)) cout << "-- Snap: success" << endl;
        else throw string("-- Snap: error");

        Element* el_update = new Element("key222", 6);  // payload не нужен
        if (Update(ht, el_update, "newpayload", 9)) cout << "-- Update: success" << endl;
        else throw string("-- Update: error");
        delete el_update;

        el_get = new Element("key222", 6);
        hte = Get(ht, el_get);
        if (hte) {
            cout << "-- Get after update: success" << endl;
            print(hte);
            delete hte;
        }
        else throw string("-- Get after update: error");
        delete el_get;

        el_get = new Element("key222", 6);
        if (Delete(ht, el_get)) cout << "-- Delete: success" << endl;
        else throw string("-- Delete: error");
        delete el_get;

        el_get = new Element("key222", 6);
        hte = Get(ht, el_get);
        if (!hte) cout << "-- Get after delete: success (not found)" << endl;
        else {
            delete hte;
            throw string("-- Delete failed");
        }
        delete el_get;

        if (Close(ht)) cout << "-- Close: success" << endl;
        else throw string("-- Close: error");
        ht = NULL;
    }
    catch (const string& msg) {
        cout << msg << endl;
        if (ht) {
            char* err = GetLastError(ht);
            if (err) cout << err << endl;
            Close(ht);
            ht = NULL;
        }
    }

    cout << "\n=== Демонстрация Open + операции ===" << endl;
    try {
        ht = Open("D:\\Универ\\СП Смелов\\Готовые лабораторные\\laba1\\HTspace.ht");
        if (!ht) throw string("-- Open: error");

        cout << "-- Open: success" << endl;

        Element* el_insert_open = new Element("key333", 6, "payload_open", 11);
        if (Insert(ht, el_insert_open)) cout << "-- Insert: success" << endl;
        else throw string("-- Insert in open: error");
        delete el_insert_open;

        Element* el_get_open = new Element("key333", 6);
        Element* hte_open = Get(ht, el_get_open);
        if (hte_open) {
            cout << "-- Get in open: success" << endl;
            print(hte_open);
            delete hte_open;
        }
        else throw string("-- Get in open: error");
        delete el_get_open;

        if (Close(ht)) cout << "-- Close after open: success" << endl;
        else throw string("-- Close after open: error");
        ht = NULL;
    }
    catch (const string& msg) {
        cout << msg << endl;
        if (ht) {
            char* err = GetLastError(ht);
            if (err) cout << err << endl;
            Close(ht);
            ht = NULL;
        }
    }

    cout << "\nДемонстрация завершена." << endl;
    return 0;
}