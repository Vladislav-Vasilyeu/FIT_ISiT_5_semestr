# linux_lab05b.py — версия специально для Konsole (KDE) и универсальная
import os
import sys
import subprocess
import time
import psutil
import shutil

def run_in_new_terminal(title, nice_value, script_path):
    """
    Запускает скрипт с заданным nice в новом окне терминала.
    Приоритет: Konsole → другие терминалы.
    """
    full_command = ["nice", "-n", str(nice_value), "python3", script_path]

    # Сначала пробуем Konsole (твой терминал)
    if shutil.which("konsole"):
        try:
            proc = subprocess.Popen([
                "konsole",
                "--hold",                  # Окно остаётся открытым после завершения
                "--caption", title,        # Заголовок окна
                "-e"                       # Выполнить команду
            ] + full_command)
            print(f"Запущен в Konsole: {title} (nice {nice_value})")
            return proc
        except Exception as e:
            print(f"Ошибка запуска konsole: {e}")

    # Fallback на другие терминалы (на всякий случай)
    fallback_terminals = [
        ["gnome-terminal", "--title", title, "--"] + full_command,
        ["xfce4-terminal", "--title", title, "--hold", "-x"] + full_command,
        ["xterm", "-hold", "-title", title, "-e"] + full_command,
    ]

    for cmd in fallback_terminals:
        if shutil.which(cmd[0]):
            try:
                proc = subprocess.Popen(cmd)
                print(f"Запущен в {cmd[0]}: {title}")
                return proc
            except Exception as e:
                print(f"Ошибка запуска {cmd[0]}: {e}")

    print("Ошибка: не найден ни один терминал (konsole, gnome-terminal, xterm и т.д.)")
    print("Установи konsole: sudo pacman -S konsole  (на Arch/Manjaro)")
    sys.exit(1)

def main():
    if len(sys.argv) != 4:
        print("Использование: python3 linux_lab05b.py <P1> <P2> <P3>")
        print("  P1: 0 = все CPU, 1 = только CPU0, иначе = маска")
        print("  P2, P3: nice (-20..19), для значений <0 может потребоваться sudo")
        sys.exit(1)

    p1 = int(sys.argv[1])
    nice1 = int(sys.argv[2])
    nice2 = int(sys.argv[3])

    num_cpus = os.cpu_count()
    if p1 == 0:
        mask = list(range(num_cpus))
        print(f"P1 = 0 → Все процессоры ({num_cpus} шт.)")
    elif p1 == 1:
        mask = [0]
        print("P1 = 1 → Только CPU 0")
    else:
        mask = [i for i in range(num_cpus) if (p1 & (1 << i))]
        print(f"P1 = {p1} → CPUs {mask}")

    print(f"P2 (nice первого дочернего): {nice1}")
    print(f"P3 (nice второго дочернего): {nice2}")
    print()

    # Установка affinity для дочерних процессов (применяется после запуска)
    # В Linux проще установить affinity через taskset в командной строке
    # Но мы сделаем через psutil после запуска — поэтому добавим вывод PID в lab05x

    script_path = os.path.abspath("linux_lab05x.py")

    print("Запуск двух дочерних процессов в отдельных окнах Konsole...\n")

    # Первый процесс
    proc1 = run_in_new_terminal(f"Lab-05x #1 (nice {nice1})", nice1, script_path)

    # Небольшая пауза, чтобы первое окно открылось
    time.sleep(1.5)

    # Второй процесс
    proc2 = run_in_new_terminal(f"Lab-05x #2 (nice {nice2})", nice2, script_path)

    print("\nОба окна Konsole открыты.")
    print("Теперь ты видишь два отдельных окна с бегущими итерациями.")
    print("Родительский процесс ждёт закрытия окон...")

    # Ждём завершения терминалов
    proc1.wait()
    proc2.wait()

    print("\nОба дочерних процесса завершились.")

if __name__ == "__main__":
    main()