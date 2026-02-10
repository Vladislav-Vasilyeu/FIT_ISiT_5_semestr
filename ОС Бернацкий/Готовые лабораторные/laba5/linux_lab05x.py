# linux_lab05x.py
import os
import threading
import time

def main():
    start_time = time.perf_counter()

    pid = os.getpid()
    tid = threading.get_ident()

    total_iterations = 1_000_000
    report_interval = 1000

    print(f"Процесс {pid}, поток {tid} запущен")

    i = 0
    while i <= total_iterations:
        if i % report_interval == 0:
            print(f"--- Итерация {i} ---")
            print(f"PID: {pid}")
            print(f"TID: {tid}")

            # Уровень nice (приоритет)
            try:
                nice = os.nice(0)
                print(f"Nice (приоритет): {nice}")
            except Exception:
                print("Nice: Недоступно")

            # Текущий CPU
            try:
                cpu = os.sched_getcpu()
                print(f"Текущий процессор (CPU): {cpu}")
            except Exception:
                print("CPU: Недоступно")

            print("---------------------------")
            time.sleep(0.2)  # 200 мс задержка

        i += 1

    end_time = time.perf_counter()
    elapsed = end_time - start_time

    print(f"=== ЗАВЕРШЕНИЕ РАБОТЫ ===")
    print(f"Всего итераций: {total_iterations}")
    print(f"Прошедшее время: {elapsed:.2f} секунд")

if __name__ == "__main__":
    main()