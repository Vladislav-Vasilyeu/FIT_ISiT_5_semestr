// Lab-07d.cpp
#include <windows.h>
#include <iostream>
#include <string>

int main() {
    setlocale(LC_ALL, "Ru");
    std::wcout << L"Lab-07d: Запуск двух дочерних процессов Lab-07x...\n\n";

    std::wstring exePath = L"lab07x.exe";  // Убедись, что файл в той же папке

    STARTUPINFO si1 = { sizeof(si1) };
    STARTUPINFO si2 = { sizeof(si2) };
    PROCESS_INFORMATION pi1 = {};
    PROCESS_INFORMATION pi2 = {};

    // Первый дочерний: 60 секунд (1 минута)
    std::wstring cmd1 = exePath + L" 60";

    if (!CreateProcessW(nullptr, (wchar_t*)cmd1.data(), nullptr, nullptr, FALSE,
        CREATE_NEW_CONSOLE, nullptr, nullptr, &si1, &pi1)) {
        std::wcerr << L"Ошибка запуска первого процесса! Код: " << GetLastError() << L"\n";
        return 1;
    }
    std::wcout << L"Первый процесс запущен (PID: " << pi1.dwProcessId << L") — работа 1 минута.\n";

    // Второй дочерний: 120 секунд (2 минуты)
    std::wstring cmd2 = exePath + L" 120";

    if (!CreateProcessW(nullptr, (wchar_t*)cmd2.data(), nullptr, nullptr, FALSE,
        CREATE_NEW_CONSOLE, nullptr, nullptr, &si2, &pi2)) {
        std::wcerr << L"Ошибка запуска второго процесса! Код: " << GetLastError() << L"\n";
        return 1;
    }
    std::wcout << L"Второй процесс запущен (PID: " << pi2.dwProcessId << L") — работа 2 минуты.\n\n";

    std::wcout << L"Родительский процесс ожидает завершения дочерних...\n";

    // Ждём завершения обоих процессов
    HANDLE handles[2] = { pi1.hProcess, pi2.hProcess };
    WaitForMultipleObjects(2, handles, TRUE, INFINITE);

    std::wcout << L"\nОба дочерних процесса завершились корректно.\n";
    std::wcout << L"Родительский процесс завершает работу.\n";

    // Закрываем дескрипторы
    CloseHandle(pi1.hThread); CloseHandle(pi1.hProcess);
    CloseHandle(pi2.hThread); CloseHandle(pi2.hProcess);

    std::wcout << L"Нажмите Enter для выхода...\n";
    std::cin.get();

    return 0;
}