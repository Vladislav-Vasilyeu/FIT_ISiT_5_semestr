# Lab-07c.py
import signal
import time

iterations = 0
print_count = 0

def handler(signum, frame):
    global iterations, print_count
    print_count += 1
    print(f"Прошло {print_count * 3} секунд. Итераций: {iterations:,}")

    if print_count >= 5:  # 15 секунд
        elapsed_wall = time.time() - start_time
        print(f"\nИтоговое количество итераций за 15 секунд: {iterations:,}")
        print(f"Прошло реального времени: {elapsed_wall:.3f} секунд")
        print("Программа завершает работу.")
        exit(0)

    # Перезапускаем таймер на следующие 3 секунды
    signal.alarm(3)

# Устанавливаем обработчик и первый таймер
signal.signal(signal.SIGALRM, handler)
signal.alarm(3)

print("Запуск цикла. Вывод каждые 3 секунды...\n")
start_time = time.time()

# Интенсивный цикл
while True:
    iterations += 1