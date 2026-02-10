#define _CRT_SECURE_NO_WARNINGS
#include <iostream>
#include <string>
#include <Windows.h>
#include "../OS11_HTAPI/HT.h"
#pragma comment(lib, "OS11_HTAPI")



/*
* HINSTANCE dll = LoadLibrary("OS11_HTAPI.dll");

*/

using namespace std;
using namespace HT;

int main(int argc, char* argv[])
{
    if (argc != 2) {
        cout << "Usage: os11_02.exe path_to_storage" << endl;
        return -1;
    }
    const size_t cSize = strlen(argv[1]) + 1;
    wchar_t* wc = new wchar_t[cSize];
    mbstowcs(wc, argv[1], cSize);

    cout << "[CLIENT] Starting, waiting for server..." << endl;

    while (true)  // Бесконечный цикл — программа никогда не завершится сама
    {
        // Ждём запуска сервера (наличия мьютекса)
        HANDLE hServerMutex = nullptr;
        while (!hServerMutex)
        {
            hServerMutex = OpenMutex(SYNCHRONIZE, FALSE, L"HT_SERVER_MUTEX");
            if (!hServerMutex)
            {
                cout << "[CLIENT] Server not running, retrying in 1 second..." << endl;
                Sleep(1000);
            }
        }
        CloseHandle(hServerMutex);

        // Пытаемся открыть существующее хранилище
        HTHandle* ht = OpenExist(wc);
        if (!ht)
        {
            cout << "[CLIENT] Cannot open storage, retrying in 1 second..." << endl;
            Sleep(1000);
            continue;  // снова ждём сервер/хранилище
        }

        cout << "[CLIENT] Connected to storage" << endl;

        // Основная работа: вставляем 50 элементов
        string payload = "0";
        for (int i = 0; i < 50; ++i)
        {
            // Проверяем, жив ли сервер во время вставки
            hServerMutex = OpenMutex(SYNCHRONIZE, FALSE, L"HT_SERVER_MUTEX");
            if (!hServerMutex)
            {
                cout << "[CLIENT] Server stopped during insertion!" << endl;
                break;
            }
            CloseHandle(hServerMutex);

            string key = to_string(rand() % 50);
            Element* element = new Element(key.c_str(), (int)key.length() + 1,
                payload.c_str(), (int)payload.length() + 1);

            cout << "Inserting element: ";
            Print(element);

            if (Insert(ht, element))
            {
                cout << "[CLIENT] SUCCESS: Inserted element with key: " << key << endl;
            }
            else
            {
                cout << "[CLIENT] ERROR: Failed to insert element with key: " << key
                    << " - " << ht->LastErrorMessage << endl;
            }

            delete element;
            Sleep(1000);
        }

        //Close(ht);
        cout << "[CLIENT] Finished 50 insertions. Storage closed, waiting for server restart..." << endl;

        // После закрытия снова будем ждать сервер в начале цикла
        Sleep(1000);
    }

    delete[] wc;
    return 0;  // Эта строка никогда не выполнится
}