#include <windows.h>
#include <iostream>
#include <string>
#include <sstream>

DWORD GetPriorityClassFromInt(int prio) {
    switch (prio) {
    case 0: return IDLE_PRIORITY_CLASS;
    case 1: return BELOW_NORMAL_PRIORITY_CLASS;
    case 2: return NORMAL_PRIORITY_CLASS;
    case 3: return ABOVE_NORMAL_PRIORITY_CLASS;
    case 4: return HIGH_PRIORITY_CLASS;
    case 5: return REALTIME_PRIORITY_CLASS;
    default: return NORMAL_PRIORITY_CLASS;
    }
}

int main(int argc, char* argv[]) {
    setlocale(LC_ALL, "RU");
    // Проверка аргументов
    if (argc != 4) {
        std::cerr << "Использование: Lab-05b.exe <P1> <P2> <P3>\n";
        std::cerr << "  P1: 0 = все процессоры, 1 = только CPU0, иначе = маска в десятичном виде\n";
        std::cerr << "  P2 и P3: приоритет (0=Idle, 1=BelowNormal, 2=Normal, 3=AboveNormal, 4=High, 5=Realtime)\n";
        return 1;
    }

    // Чтение P1 и определение маски affinity
    int p1_input = std::stoi(argv[1]);
    DWORD_PTR affinityMask;

    if (p1_input == 0) {
        // Получаем маску всех доступных процессоров
        SYSTEM_INFO sysinfo;
        GetSystemInfo(&sysinfo);
        affinityMask = (1ULL << sysinfo.dwNumberOfProcessors) - 1;
        std::cout << "P1 = 0 → Используются ВСЕ процессоры (" << sysinfo.dwNumberOfProcessors << " шт.). Маска: 0x"
            << std::hex << affinityMask << std::dec << "\n";
    }
    else if (p1_input == 1) {
        affinityMask = 1;  // Только первый процессор (бит 0)
        std::cout << "P1 = 1 → Используется ТОЛЬКО ОДИН процессор (CPU 0). Маска: 0x1\n";
    }
    else {
        // Прямое указание маски (например, 3 = CPU0 и CPU1)
        affinityMask = static_cast<DWORD_PTR>(p1_input);
        std::cout << "P1 = " << p1_input << " → Прямая маска affinity: 0x" << std::hex << affinityMask << std::dec << "\n";
    }

    int prio1 = std::stoi(argv[2]);
    int prio2 = std::stoi(argv[3]);

    std::cout << "P2 (приоритет первого дочернего): " << prio1 << "\n";
    std::cout << "P3 (приоритет второго дочернего): " << prio2 << "\n\n";

    // Путь к дочернему приложению
    std::wstring childPath = L"lab05x.exe";

    STARTUPINFO si1 = { sizeof(si1) };
    PROCESS_INFORMATION pi1 = {};
    STARTUPINFO si2 = { sizeof(si2) };
    PROCESS_INFORMATION pi2 = {};

    // Запуск первого дочернего процесса
    if (!CreateProcessW(childPath.c_str(), nullptr, nullptr, nullptr, FALSE,
        CREATE_NEW_CONSOLE, nullptr, nullptr, &si1, &pi1)) {
        std::wcerr << L"Ошибка создания первого процесса! Код: " << GetLastError() << L"\n";
        return 1;
    }

    // Запуск второго дочернего процесса
    if (!CreateProcessW(childPath.c_str(), nullptr, nullptr, nullptr, FALSE,
        CREATE_NEW_CONSOLE, nullptr, nullptr, &si2, &pi2)) {
        std::wcerr << L"Ошибка создания второго процесса! Код: " << GetLastError() << L"\n";
        CloseHandle(pi1.hThread);
        CloseHandle(pi1.hProcess);
        return 1;
    }

    // Установка affinity и приоритетов
    SetProcessAffinityMask(pi1.hProcess, affinityMask);
    SetProcessAffinityMask(pi2.hProcess, affinityMask);

    SetPriorityClass(pi1.hProcess, GetPriorityClassFromInt(prio1));
    SetPriorityClass(pi2.hProcess, GetPriorityClassFromInt(prio2));

    std::cout << "Оба дочерних процесса Lab-05x.exe успешно запущены в отдельных окнах.\n";
    std::cout << "Ожидаем завершения хотя бы одного из них...\n\n";

    // Ждём завершения любого из процессов
    HANDLE handles[2] = { pi1.hProcess, pi2.hProcess };
    WaitForMultipleObjects(2, handles, FALSE, INFINITE);

    std::cout << "Один из процессов завершился.\n";
    std::cout << "Нажмите Enter для завершения Lab-05b...\n";
    std::cin.get();

    // Закрываем дескрипторы
    CloseHandle(pi1.hThread); CloseHandle(pi1.hProcess);
    CloseHandle(pi2.hThread); CloseHandle(pi2.hProcess);

    return 0;
}