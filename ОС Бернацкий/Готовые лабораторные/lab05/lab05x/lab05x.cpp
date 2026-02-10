#include <windows.h>
#include <iostream>
#include <chrono>

using namespace std::chrono;

int main() {
    setlocale(LC_ALL, "RU");
    // Засекаем время начала
    auto start_time = high_resolution_clock::now();

    const long long TOTAL_ITERATIONS = 1'000'000;
    const long long PRINT_EVERY = 1000;

    // Получаем дескрипторы процесса и потока
    HANDLE hProcess = GetCurrentProcess();
    HANDLE hThread = GetCurrentThread();

    for (long long i = 1; i <= TOTAL_ITERATIONS; ++i) {
        // Каждые 1000 итераций — вывод и задержка
        if (i % PRINT_EVERY == 0) {
            // 1. Номер итерации
            std::cout << "Итерация: " << i << "\n";

            // 2. Идентификатор процесса
            DWORD processId = GetCurrentProcessId();
            std::cout << "  PID (Process ID): " << processId << "\n";

            // 3. Идентификатор потока
            DWORD threadId = GetCurrentThreadId();
            std::cout << "  TID (Thread ID):  " << threadId << "\n";

            // 4. Класс приоритета процесса
            DWORD priorityClass = GetPriorityClass(hProcess);
            std::string priorityClassStr;
            switch (priorityClass) {
            case IDLE_PRIORITY_CLASS:          priorityClassStr = "Idle"; break;
            case BELOW_NORMAL_PRIORITY_CLASS:  priorityClassStr = "Below Normal"; break;
            case NORMAL_PRIORITY_CLASS:        priorityClassStr = "Normal"; break;
            case ABOVE_NORMAL_PRIORITY_CLASS:  priorityClassStr = "Above Normal"; break;
            case HIGH_PRIORITY_CLASS:          priorityClassStr = "High"; break;
            case REALTIME_PRIORITY_CLASS:      priorityClassStr = "Realtime"; break;
            default:                           priorityClassStr = "Unknown"; break;
            }
            std::cout << "  Класс приоритета процесса: " << priorityClassStr << " (" << priorityClass << ")\n";

            // 5. Приоритет потока
            int threadPriority = GetThreadPriority(hThread);
            std::string threadPriorityStr;
            switch (threadPriority) {
            case THREAD_PRIORITY_IDLE:              threadPriorityStr = "Idle"; break;
            case THREAD_PRIORITY_LOWEST:            threadPriorityStr = "Lowest"; break;
            case THREAD_PRIORITY_BELOW_NORMAL:      threadPriorityStr = "Below Normal"; break;
            case THREAD_PRIORITY_NORMAL:            threadPriorityStr = "Normal"; break;
            case THREAD_PRIORITY_ABOVE_NORMAL:      threadPriorityStr = "Above Normal"; break;
            case THREAD_PRIORITY_HIGHEST:           threadPriorityStr = "Highest"; break;
            case THREAD_PRIORITY_TIME_CRITICAL:     threadPriorityStr = "Time Critical"; break;
            default:                                threadPriorityStr = "Unknown"; break;
            }
            std::cout << "  Приоритет потока: " << threadPriorityStr << " (" << threadPriority << ")\n";

            // 6. Номер назначенного процессора (текущий CPU, на котором выполняется поток)
            DWORD_PTR processAffinityMask, systemAffinityMask;
            GetProcessAffinityMask(hProcess, &processAffinityMask, &systemAffinityMask);

            // Текущий процессор, на котором выполняется поток в данный момент
            ULONG_PTR currentProcessor = 0;
            // Начиная с Windows 10 / Server 2016 есть ProcessorNumber
#if _WIN32_WINNT >= 0x0600  // Windows Vista и выше
            PROCESSOR_NUMBER procNum;
            GetCurrentProcessorNumberEx(&procNum);
            currentProcessor = procNum.Group * 64 + procNum.Number;  // Учёт групп (для >64 ядер)
#else
            currentProcessor = GetCurrentProcessorNumber();
#endif

            std::cout << "  Текущий процессор (CPU): " << currentProcessor << "\n";

            std::cout << "------------------------------------------\n";

            // Задержка 200 мс
            Sleep(200);
        }
    }

    // Конец работы — вывод общего времени
    auto end_time = high_resolution_clock::now();
    duration<double, std::milli> elapsed = end_time - start_time;

    std::cout << "\n=== ЗАВЕРШЕНИЕ РАБОТЫ ===\n";
    std::cout << "Всего итераций: " << TOTAL_ITERATIONS << "\n";
    std::cout << "Общее время выполнения: " << elapsed.count() / 1000.0 << " секунд\n";

    // Пауза, чтобы окно не закрылось сразу (удобно при запуске из Lab-05b)
    std::cout << "Нажмите Enter для выхода...\n";
    std::cin.get();

    return 0;
}