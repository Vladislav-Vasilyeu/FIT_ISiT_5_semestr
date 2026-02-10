#include <windows.h>
#include <iostream>
#include <iomanip>

int main() {
    setlocale(LC_ALL, "Ru");
    // Получаем частоту счётчика производительности (тиков в секунду)
    LARGE_INTEGER frequency;
    QueryPerformanceFrequency(&frequency);

    // Получаем начальное значение счётчика
    LARGE_INTEGER start;
    QueryPerformanceCounter(&start);

    // Цели по времени в секундах
    const double TIME_5_SEC = 5.0;
    const double TIME_10_SEC = 10.0;
    const double TIME_15_SEC = 15.0;

    // Переводим секунды в тики
    LONGLONG target5 = start.QuadPart + static_cast<LONGLONG>(TIME_5_SEC * frequency.QuadPart);
    LONGLONG target10 = start.QuadPart + static_cast<LONGLONG>(TIME_10_SEC * frequency.QuadPart);
    LONGLONG target15 = start.QuadPart + static_cast<LONGLONG>(TIME_15_SEC * frequency.QuadPart);

    bool printed5 = false;
    bool printed10 = false;

    LONGLONG iterations = 0;
    LARGE_INTEGER current;

    std::cout << "Запуск бесконечного цикла. Измерение итераций...\n";

    // Бесконечный цикл без задержек
    while (true) {
        ++iterations;

        // Получаем текущее время
        QueryPerformanceCounter(&current);

        // Проверка на 5 секунд
        if (!printed5 && current.QuadPart >= target5) {
            std::cout << "Через 5 секунд выполнено итераций:  "
                << std::setw(12) << iterations << "\n";
            printed5 = true;
        }

        // Проверка на 10 секунд
        if (!printed10 && current.QuadPart >= target10) {
            std::cout << "Через 10 секунд выполнено итераций: "
                << std::setw(12) << iterations << "\n";
            printed10 = true;
        }

        // Проверка на 15 секунд — завершение
        if (current.QuadPart >= target15) {
            std::cout << "Через 15 секунд выполнено итераций: "
                << std::setw(12) << iterations << "\n";
            std::cout << "Итоговое количество итераций:       "
                << std::setw(12) << iterations << "\n";
            std::cout << "Программа завершает работу.\n";
            break;
        }
    }

    std::cout << "Нажмите Enter для выхода...\n";
    std::cin.get();

    return 0;
}