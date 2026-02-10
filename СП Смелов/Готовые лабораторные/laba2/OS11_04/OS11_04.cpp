#include "HT.h"
#include <iostream>
#include <string>
#include <cstdlib>  // rand
#include <ctime>    // time for srand
#include <windows.h>  // Sleep

using namespace std;
using namespace HT;

int main(int argc, char* argv[]) {  // Arg — имя файла? Но в задании нет, предполагаем hardcode or arg.
    const char* file = argv[1];  // Или argv[1]
    HTHANDLE* ht = Open(file);
    if (!ht) { cout << "Error opening" << endl; return 1; }

    srand(time(NULL));  // Seed for random.
    while (true) {
        int key_num = rand() % 50 + 1;  // Random 1-50.
        string key = "key" + to_string(key_num);  // "key1".."key50"
        int value = 0;  // 32-bit = 0

        Element* el_get = new Element(key.c_str(), key.length());
        Element* res = Get(ht, el_get);
        if (res) {
            int old_value = *(int*)res->payload;
            int new_value = old_value + 1;
            if (Update(ht, el_get, &new_value, sizeof(int))) {
                cout << "Updated key " << key << ": from " << old_value << " to " << new_value << endl;
            }
            else {
                // error
            }
            delete res;
        }
        else {
            cout << "Key " << key << " not found for update" << endl;
        }
        delete el_get;
        Sleep(1000);  // 1 сек.
    }
    Close(ht);  // Не достигнет, т.к. infinite.
    return 0;
}