# Lab-07a.py
import time
import datetime
import os

def format_two_digits(n):
    return f"{n:02d}"

# Получаем локальное время как struct_time
local_time = time.localtime()

# Получаем UTC время как struct_time
utc_time = time.gmtime()

# Вычисляем разницу в секундах между локальным и UTC
# localtime - gmtime = bias в секундах
bias_seconds = time.mktime(local_time) - time.mktime(utc_time)

# Переводим в часы и минуты
bias_hours = int(bias_seconds // 3600)
bias_minutes = int(abs(bias_seconds % 3600) // 60)

sign = '+' if bias_seconds >= 0 else '-'
abs_bias_hours = abs(bias_hours)

# Формируем строку в формате YYYY-MM-DDThh:mm:ss±hhmm
dt = datetime.datetime.fromtimestamp(time.mktime(local_time))
formatted = (
    f"{dt.year}-{format_two_digits(dt.month)}-{format_two_digits(dt.day)}T"
    f"{format_two_digits(dt.hour)}:{format_two_digits(dt.minute)}:{format_two_digits(dt.second)}"
    f"{sign}{format_two_digits(abs_bias_hours)}{format_two_digits(bias_minutes)}"
)

print(formatted)

input("\nНажмите Enter для выхода...")