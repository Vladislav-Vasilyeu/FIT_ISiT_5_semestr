#include "HT.h"
#include <iostream>
#include <string>

using namespace std;
using namespace HT;

int main(int argc, char* argv[]) {

    if (argc < 2) { cout << "Usage: OS11_START file" << endl; return 1; }

    const char* file = argv[1];

    HTHANDLE* ht = Open(file);  // Открываем.
    if (ht) {
        cout << "HT-StorageStart filename=" << file << ", snapshotinterval=" << ht->SecSnapshotInterval << endl;
        cout << "capacity=" << ht->Capacity << ", maxkeylength=" << ht->MaxKeyLength << ", maxdatalength=" << ht->MaxPayloadLength << endl;
        cin.get();  // Ждём ввода.
        Snap(ht);  // Финальный snap.
        Close(ht);  // Закрываем.
        return 0;
    }
    else {
        cout << "Error opening HT" << endl;
        return 1;
    }
}