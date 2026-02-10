#include "HT.h"
#include <iostream>
#include <string>

using namespace std;
using namespace HT;


int main(int argc, char* argv[]) {  // Аргументы командной строки.

    if (argc < 2) {
        cout << "Usage: OS11_CREATE file [interval] [capacity] [keylen] [payloadlen]" << endl;
        return 1;
    }

    
    const int DEFAULT_INTERVAL = 5;    
    const int DEFAULT_CAPACITY = 1000;
    const int DEFAULT_KEYLEN = 32;
    const int DEFAULT_PAYLEN = 256;

    const char* file = argv[1];  

    
    int interval = DEFAULT_INTERVAL;
    int cap = DEFAULT_CAPACITY;
    int keylen = DEFAULT_KEYLEN;
    int paylen = DEFAULT_PAYLEN;

    try {
        if (argc >= 3 && argv[2] && argv[2][0] != '\0') interval = stoi(argv[2]);  // Sec.
        if (argc >= 4 && argv[3] && argv[3][0] != '\0') cap = stoi(argv[3]);       // Capacity.
        if (argc >= 5 && argv[4] && argv[4][0] != '\0') keylen = stoi(argv[4]);   // MaxKey.
        if (argc >= 6 && argv[5] && argv[5][0] != '\0') paylen = stoi(argv[5]);   // MaxPayload.
    }
    catch (const std::exception& e) {
        cerr << "Invalid numeric argument: " << e.what() << endl;
        return 1;
    }

    cout << "Using parameters: file=" << file
         << ", interval=" << interval
         << ", capacity=" << cap
         << ", keylen=" << keylen
         << ", payloadlen=" << paylen << endl;

    HTHANDLE* ht = Create(cap, interval, keylen, paylen, file);  // Создаём.
    if (ht) {
        cout << "HT-StorageCreated filename=" << file << ", snapshotinterval=" << interval << endl;
        cout << "capacity=" << cap << ", maxkeylength=" << keylen << ", maxdatalength=" << paylen << endl;
        Close(ht);  // Закрываем.
        return 0;
    }
    else {
        std::cout << "Error creating HT" << std::endl;
        return 1;
    }
}