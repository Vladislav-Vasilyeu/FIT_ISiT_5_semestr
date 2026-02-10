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

        Element* el = new Element(key.c_str(), key.length(), &value, sizeof(int));
        if (Delete(ht, new Element(key.c_str(), key.length()))) {
            cout << "Deleted key " << key << ": success" << endl;
        }
        else {
            char* err = HT::GetLastError(ht);
            cout << "Delete key " << key << ": error - " << (err ? err : "unknown") << endl;
        }
        delete el;
        Sleep(1000);  // 1 сек.
    }
    Close(ht);  // Не достигнет, т.к. infinite.
    return 0;
}