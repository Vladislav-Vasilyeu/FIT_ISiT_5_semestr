// Lab-07x.cpp — обновлённая версия с поддержкой аргумента
#include <windows.h>
#include <iostream>
#include <iomanip>
#include <cmath>
#include <conio.h>
#include <string>

bool IsPrime(long long n) {
    if (n < 2) return false;
    if (n == 2) return true;
    if (n % 2 == 0) return false;
    long long sqrt_n = static_cast<long long>(sqrt(static_cast<double>(n))) + 1;
    for (long long i = 3; i <= sqrt_n; i += 2) {
        if (n % i == 0) return false;
    }
    return true;
}

int main(int argc, char* argv[]) {
    setlocale(LC_ALL, "Ru");
    LARGE_INTEGER frequency, start;
    QueryPerformanceFrequency(&frequency);
    QueryPerformanceCounter(&start);

    // Если передан аргумент — это время работы в секундах
    int maxSeconds = 0;
    if (argc > 1) {
        maxSeconds = std::stoi(argv[1]);
    }

    LONGLONG targetTicks = 0;
    if (maxSeconds > 0) {
        targetTicks = start.QuadPart + static_cast<LONGLONG>(maxSeconds * frequency.QuadPart);
    }

    std::cout << "Генерация простых чисел ";
    if (maxSeconds > 0) std::cout << "(автозавершение через " << maxSeconds << " сек)";
    std::cout << ".\n\n";

    long long number = 2;
    long long count = 0;

    while (true) {
        // Проверка нажатия клавиши (для ручного завершения, если нет таймера)
        if (_kbhit()) {
            _getch();
            break;
        }

        // Проверка автоматического завершения по времени
        if (maxSeconds > 0) {
            LARGE_INTEGER current;
            QueryPerformanceCounter(&current);
            if (current.QuadPart >= targetTicks) {
                break;
            }
        }

        if (IsPrime(number)) {
            ++count;
            std::cout << std::setw(8) << count << ": " << number << "\n";
        }
        ++number;
    }

    LARGE_INTEGER end;
    QueryPerformanceCounter(&end);
    double elapsed = static_cast<double>(end.QuadPart - start.QuadPart) / frequency.QuadPart;

    std::cout << "\n=== ЗАВЕРШЕНИЕ РАБОТЫ ===\n";
    std::cout << "Найдено простых чисел: " << count << "\n";
    std::cout << "Время работы: " << std::fixed << std::setprecision(3) << elapsed << " секунд\n";
    std::wcout << L"Нажмите Enter для выхода...\n";
    std::cin.get();

    return 0;
}