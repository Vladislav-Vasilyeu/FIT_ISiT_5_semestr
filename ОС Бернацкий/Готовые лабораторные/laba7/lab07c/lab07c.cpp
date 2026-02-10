#include <windows.h>
#include <iostream>
#include <iomanip>

int main() {
    setlocale(LC_ALL, "Ru");
    // Создаём ручной (manual-reset) ожидающий таймер
    HANDLE hTimer = CreateWaitableTimer(NULL, FALSE, NULL);
    if (hTimer == NULL) {
        std::cerr << "Ошибка создания таймера! Код: " << GetLastError() << "\n";
        return 1;
    }

    // Период — 3 секунды (в 100-наносекундных интервалах)
    // Отрицательное значение = относительное время
    LARGE_INTEGER dueTime;
    dueTime.QuadPart = -30000000LL;  // -3 секунды (30 000 000 * 100 нс)

    // Устанавливаем таймер: первый запуск через 3 сек, затем каждые 3 сек
    if (!SetWaitableTimer(hTimer, &dueTime, 3000, NULL, NULL, FALSE)) {
        std::cerr << "Ошибка установки таймера! Код: " << GetLastError() << "\n";
        CloseHandle(hTimer);
        return 1;
    }

    volatile LONGLONG iterations = 0;  // volatile — чтобы компилятор не оптимизировал цикл
    int printCount = 0;

    std::cout << "Запуск цикла подсчёта итераций. Вывод каждые 3 секунды...\n\n";

    while (true) {
        ++iterations;

        // Ожидаем срабатывания таймера с нулевым таймаутом (не блокируем CPU надолго)
        DWORD waitResult = WaitForSingleObject(hTimer, 0);

        if (waitResult == WAIT_OBJECT_0) {  // Таймер сработал
            printCount++;

            std::cout << "Прошло " << (printCount * 3) << " секунд. "
                << "Итераций: " << std::setw(12) << iterations << "\n";

            // Если прошло 15 секунд — завершаем
            if (printCount >= 5) {  // 5 × 3 = 15 секунд
                std::cout << "\nИтоговое количество итераций за 15 секунд: "
                    << std::setw(12) << iterations << "\n";
                std::cout << "Программа завершает работу.\n";
                break;
            }
        }
        // Если WAIT_TIMEOUT — продолжаем считать итерации
    }

    // Отменяем таймер и закрываем дескриптор
    CancelWaitableTimer(hTimer);
    CloseHandle(hTimer);

    std::cout << "\nНажмите Enter для выхода...\n";
    std::cin.get();

    return 0;
}