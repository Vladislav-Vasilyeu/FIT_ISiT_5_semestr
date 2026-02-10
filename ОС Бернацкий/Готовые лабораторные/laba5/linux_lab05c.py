# linux_lab05c.py
import os
import sys
import threading
import time
import psutil

# Потоковая функция — аналог Lab-05x
def worker_thread(thread_num, target_priority):
    start_time = time.perf_counter()

    pid = os.getpid()
    tid = threading.get_ident()

    total_iterations = 1_000_000
    report_interval = 1000

    # Устанавливаем приоритет потока (в Linux TID трактуется как PID для setpriority)
    try:
        os.setpriority(os.PRIO_PROCESS, tid, target_priority)
        print(f"[Поток {thread_num}] Установлен nice = {target_priority}")
    except PermissionError:
        print(f"[Поток {thread_num}] Ошибка: для nice < 0 нужны права root")
    except Exception as e:
        print(f"[Поток {thread_num}] Ошибка установки приоритета: {e}")

    print(f"[Поток {thread_num}] Запущен (PID {pid}, TID {tid})")

    i = 0
    while i <= total_iterations:
        if i % report_interval == 0:
            print(f"[Поток {thread_num}] --- Итерация {i} ---")
            print(f"[Поток {thread_num}] PID: {pid}")
            print(f"[Поток {thread_num}] TID: {tid}")

            # Текущий nice
            try:
                current_nice = os.nice(0)
                print(f"[Поток {thread_num}] Текущий nice: {current_nice}")
            except:
                pass

            # Текущий CPU
            try:
                cpu = os.sched_getcpu()
                print(f"[Поток {thread_num}] Текущий CPU: {cpu}")
            except:
                pass

            print(f"[Поток {thread_num}] ---------------------------")
            time.sleep(0.2)

        i += 1

    elapsed = time.perf_counter() - start_time
    print(f"[Поток {thread_num}] === ЗАВЕРШЕНИЕ ===")
    print(f"[Поток {thread_num}] Итераций: {total_iterations}")
    print(f"[Поток {thread_num}] Время: {elapsed:.2f} сек")

def main():
    if len(sys.argv) != 4:
        print("Использование: sudo python3 linux_lab05c.py <P1> <P2> <P3>")
        print("  P1: 0 = все CPU, 1 = только CPU0, иначе = маска (десятичная)")
        print("  P2: nice для потока 1 (-20..19)")
        print("  P3: nice для потока 2 (-20..19)")
        sys.exit(1)

    p1 = int(sys.argv[1])
    nice1 = int(sys.argv[2])
    nice2 = int(sys.argv[3])

    # Определение маски affinity
    num_cpus = os.cpu_count()
    if p1 == 0:
        mask = list(range(num_cpus))  # все процессоры
        print(f"P1 = 0 → Все процессоры ({num_cpus} шт.)")
    elif p1 == 1:
        mask = [0]  # только CPU 0
        print("P1 = 1 → Только CPU 0")
    else:
        mask = [i for i in range(num_cpus) if (p1 & (1 << i))]
        print(f"P1 = {p1} → Маска: CPUs {mask}")

    print(f"P2 (nice поток 1): {nice1}")
    print(f"P3 (nice поток 2): {nice2}")
    print()

    # Установка affinity для текущего процесса
    try:
        p = psutil.Process(os.getpid())
        p.cpu_affinity(mask)
        print(f"Установлена маска affinity процесса: CPUs {mask}")
    except Exception as e:
        print(f"Ошибка установки affinity: {e}")

    # Создание и запуск потоков
    thread1 = threading.Thread(target=worker_thread, args=(1, nice1))
    thread2 = threading.Thread(target=worker_thread, args=(2, nice2))

    thread1.start()
    thread2.start()

    print("Оба потока запущены. Вывод чередуется в одном окне.\n")

    # Ожидание завершения
    thread1.join()
    thread2.join()

    print("\nОба потока завершились.")

if __name__ == "__main__":
    main()