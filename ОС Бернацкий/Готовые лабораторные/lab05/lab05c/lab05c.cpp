#include <windows.h>
#include <iostream>
#include <string>
#include <sstream>
#include <chrono>

using namespace std::chrono;

// Глобальные переменные для передачи параметров в потоки
struct ThreadParams {
    int threadNum;       // 1 или 2
    int threadPriority;  // числовое значение приоритета
};

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

int GetThreadPriorityFromInt(int prio) {
    switch (prio) {
    case 0: return THREAD_PRIORITY_IDLE;
    case 1: return THREAD_PRIORITY_LOWEST;
    case 2: return THREAD_PRIORITY_BELOW_NORMAL;
    case 3: return THREAD_PRIORITY_NORMAL;
    case 4: return THREAD_PRIORITY_ABOVE_NORMAL;
    case 5: return THREAD_PRIORITY_HIGHEST;
    case 6: return THREAD_PRIORITY_TIME_CRITICAL;
    default: return THREAD_PRIORITY_NORMAL;
    }
}

// Потоковая функция (аналог Lab-05x)
DWORD WINAPI ChildThread(LPVOID lpParam) {
    ThreadParams* params = static_cast<ThreadParams*>(lpParam);
    int threadNum = params->threadNum;

    auto start_time = high_resolution_clock::now();

    const long long TOTAL_ITERATIONS = 1'000'000;
    const long long PRINT_EVERY = 1000;

    HANDLE hProcess = GetCurrentProcess();
    HANDLE hThread = GetCurrentThread();

    std::wstringstream prefix;
    prefix << L"[Поток " << threadNum << L"] ";

    for (long long i = 1; i <= TOTAL_ITERATIONS; ++i) {
        if (i % PRINT_EVERY == 0) {
            std::wcout << prefix.str() << L"Итерация: " << i << L"\n";

            DWORD processId = GetCurrentProcessId();
            std::wcout << prefix.str() << L"  PID: " << processId << L"\n";

            DWORD threadId = GetCurrentThreadId();
            std::wcout << prefix.str() << L"  TID: " << threadId << L"\n";

            DWORD priorityClass = GetPriorityClass(hProcess);
            std::wcout << prefix.str() << L"  Класс приоритета процесса: " << priorityClass << L"\n";

            int threadPriority = GetThreadPriority(hThread);
            std::wcout << prefix.str() << L"  Приоритет потока: " << threadPriority << L"\n";

#if _WIN32_WINNT >= 0x0600
            PROCESSOR_NUMBER procNum;
            GetCurrentProcessorNumberEx(&procNum);
            ULONG currentProcessor = procNum.Group * 64 + procNum.Number;
#else
            ULONG currentProcessor = GetCurrentProcessorNumber();
#endif
            std::wcout << prefix.str() << L"  Текущий процессор (CPU): " << currentProcessor << L"\n";

            std::wcout << prefix.str() << L"------------------------------------------\n";

            Sleep(200);  // 200 мс задержка
        }
    }

    auto end_time = high_resolution_clock::now();
    duration<double, std::milli> elapsed = end_time - start_time;

    std::wcout << prefix.str() << L"=== ЗАВЕРШЕНИЕ ПОТОКА ===\n";
    std::wcout << prefix.str() << L"Всего итераций: " << TOTAL_ITERATIONS << L"\n";
    std::wcout << prefix.str() << L"Время выполнения потока: " << elapsed.count() / 1000.0 << L" секунд\n\n";

    return 0;
}

int main(int argc, char* argv[]) {
    setlocale(LC_ALL, "Ru");
    if (argc != 5) {
        std::cerr << "Использование: Lab-05c.exe <P1> <P2> <P3> <P4>\n";
        std::cerr << "  P1: 0 = все процессоры, 1 = только CPU0, иначе = маска\n";
        std::cerr << "  P2: приоритет процесса (0=Idle ... 4=High)\n";
        std::cerr << "  P3: приоритет первого потока (1=Lowest, 3=Normal, 5=Highest и т.д.)\n";
        std::cerr << "  P4: приоритет второго потока\n";
        return 1;
    }

    int p1_input = std::stoi(argv[1]);
    int prioProcess = std::stoi(argv[2]);
    int prioThread1 = std::stoi(argv[3]);
    int prioThread2 = std::stoi(argv[4]);

    // Определяем маску affinity
    DWORD_PTR affinityMask;
    if (p1_input == 0) {
        SYSTEM_INFO sysinfo;
        GetSystemInfo(&sysinfo);
        affinityMask = (1ULL << sysinfo.dwNumberOfProcessors) - 1;
        std::cout << "P1 = 0 → Все процессоры (" << sysinfo.dwNumberOfProcessors << " шт.)\n";
    }
    else if (p1_input == 1) {
        affinityMask = 1;
        std::cout << "P1 = 1 → Только один процессор (CPU 0)\n";
    }
    else {
        affinityMask = static_cast<DWORD_PTR>(p1_input);
        std::cout << "P1 = " << p1_input << " → Прямая маска affinity\n";
    }

    // Вывод параметров
    std::cout << "Параметры:\n";
    std::cout << "  P2 (приоритет процесса): " << prioProcess << "\n";
    std::cout << "  P3 (приоритет потока 1): " << prioThread1 << "\n";
    std::cout << "  P4 (приоритет потока 2): " << prioThread2 << "\n\n";

    // Установка приоритета и affinity для текущего процесса
    SetPriorityClass(GetCurrentProcess(), GetPriorityClassFromInt(prioProcess));
    SetProcessAffinityMask(GetCurrentProcess(), affinityMask);

    // Параметры потоков
    ThreadParams params1{ 1, prioThread1 };
    ThreadParams params2{ 2, prioThread2 };

    // Создание потоков
    HANDLE hThread1 = CreateThread(nullptr, 0, ChildThread, &params1, 0, nullptr);
    HANDLE hThread2 = CreateThread(nullptr, 0, ChildThread, &params2, 0, nullptr);

    if (!hThread1 || !hThread2) {
        std::cerr << "Ошибка создания потока!\n";
        return 1;
    }

    // Установка приоритетов потоков
    SetThreadPriority(hThread1, GetThreadPriorityFromInt(prioThread1));
    SetThreadPriority(hThread2, GetThreadPriorityFromInt(prioThread2));

    std::cout << "Два дочерних потока запущены. Вывод идёт в это окно.\n";
    std::cout << "Ожидаем завершения потоков...\n\n";

    // Ждём завершения обоих потоков
    WaitForSingleObject(hThread1, INFINITE);
    WaitForSingleObject(hThread2, INFINITE);

    std::cout << "Оба потока завершены. Нажмите Enter для выхода.\n";
    std::cin.get();

    CloseHandle(hThread1);
    CloseHandle(hThread2);

    return 0;
}