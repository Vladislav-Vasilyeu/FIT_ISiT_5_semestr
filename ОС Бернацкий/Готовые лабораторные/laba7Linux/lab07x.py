# Lab-07x.py
import sys
import time
import math

def is_prime(n):
    if n < 2: return False
    if n == 2: return True
    if n % 2 == 0: return False
    for i in range(3, int(math.sqrt(n)) + 1, 2):
        if n % i == 0: return False
    return True

# Если передан аргумент — время работы в секундах
max_seconds = 0
if len(sys.argv) > 1:
    max_seconds = int(sys.argv[1])

start_time = time.time()
number = 2
count = 0

print("Генерация простых чисел..." + (f" (автозавершение через {max_seconds} сек)" if max_seconds else ""))

while True:
    if max_seconds and (time.time() - start_time >= max_seconds):
        break

    if is_prime(number):
        count += 1
        print(f"{count:8}: {number}")

    number += 1

elapsed = time.time() - start_time
print(f"\n=== ЗАВЕРШЕНИЕ ===")
print(f"Найдено простых чисел: {count}")
print(f"Время работы: {elapsed:.3f} секунд")