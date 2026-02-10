# Lab-07b.py
import time
import os

print("Запуск интенсивного цикла на 15 секунд процессорного времени...")

start_process = time.process_time()
start_wall = time.time()  # реальное (wall clock) время

iterations = 0
target_process_time = 15.0

while True:
    iterations += 1

    elapsed_process = time.process_time() - start_process
    if elapsed_process >= target_process_time:
        break

    # Печать каждые ~5 и ~10 секунд процессорного времени
    if 5.0 <= elapsed_process < 5.1 and 'printed_5' not in locals():
        print(f"Через ~5 сек CPU: {iterations:,} итераций")
        printed_5 = True
    if 10.0 <= elapsed_process < 10.1 and 'printed_10' not in locals():
        print(f"Через ~10 сек CPU: {iterations:,} итераций")
        printed_10 = True

elapsed_wall = time.time() - start_wall

print(f"\nЗа 15 секунд процессорного времени выполнено: {iterations:,} итераций")
print(f"Прошло реального (wall clock) времени: {elapsed_wall:.3f} секунд")

input("\nНажмите Enter для выхода...")