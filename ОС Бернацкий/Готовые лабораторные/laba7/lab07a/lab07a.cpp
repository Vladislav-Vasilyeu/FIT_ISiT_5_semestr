#include <windows.h>
#include <iostream>
#include <iomanip>
#include <sstream>

// Функция для добавления ведущего нуля
std::wstring FormatTwoDigits(int value) {
    std::wstringstream ss;
    ss << std::setw(2) << std::setfill(L'0') << value;
    return ss.str();
}

// Функция для добавления четырёх цифр (год)
std::wstring FormatFourDigits(int value) {
    std::wstringstream ss;
    ss << std::setw(4) << std::setfill(L'0') << value;
    return ss.str();
}

int main() {
    SYSTEMTIME localTime;
    SYSTEMTIME utcTime;

    // Получаем локальное время
    GetLocalTime(&localTime);

    // Получаем UTC-время
    GetSystemTime(&utcTime);

    // Вычисляем разницу в минутах между локальным и UTC
    // Для этого переводим оба времени в количество миллисекунд с 1601 года
    FILETIME localFileTime, utcFileTime;
    SystemTimeToFileTime(&localTime, &localFileTime);
    SystemTimeToFileTime(&utcTime, &utcFileTime);

    // Преобразуем в 64-битные целые
    ULARGE_INTEGER localInt, utcInt;
    localInt.LowPart = localFileTime.dwLowDateTime;
    localInt.HighPart = localFileTime.dwHighDateTime;
    utcInt.LowPart = utcFileTime.dwLowDateTime;
    utcInt.HighPart = utcFileTime.dwHighDateTime;

    // Разница в 100-наносекундных интервалах
    LONGLONG difference = static_cast<LONGLONG>(localInt.QuadPart - utcInt.QuadPart);

    // Переводим в минуты (1 минута = 600 000 000 * 100 нс)
    long biasMinutes = static_cast<long>(difference / (600000000LL));

    // Получаем знак и абсолютное значение
    wchar_t sign = (biasMinutes >= 0) ? L'+' : L'-';
    long absBiasMinutes = (biasMinutes >= 0) ? biasMinutes : -biasMinutes;

    int biasHours = static_cast<int>(absBiasMinutes / 60);
    int biasMins = static_cast<int>(absBiasMinutes % 60);

    // Формируем строку в формате YYYY-MM-DDThh:mm:ss±hhmm
    std::wcout << FormatFourDigits(localTime.wYear) << L"-"
        << FormatTwoDigits(localTime.wMonth) << L"-"
        << FormatTwoDigits(localTime.wDay) << L"T"
        << FormatTwoDigits(localTime.wHour) << L":"
        << FormatTwoDigits(localTime.wMinute) << L":"
        << FormatTwoDigits(localTime.wSecond)
        << sign
        << FormatTwoDigits(biasHours)<<"."
        << FormatTwoDigits(biasMins)
        << std::endl;

    // Пауза, чтобы окно не закрылось сразу
    system("pause");

    return 0;
}