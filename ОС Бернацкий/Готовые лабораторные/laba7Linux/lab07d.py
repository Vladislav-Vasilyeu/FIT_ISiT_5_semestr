# Lab-07d.py — версия для Linux с отдельными окнами терминала
import subprocess
import time
import sys
import os

def run_in_new_terminal(title, command):
    """
    Запускает команду в новом окне терминала.
    Автоматически определяет доступный эмулятор терминала.
    """
    terminal_emulators = [
        # GNOME (Ubuntu, Fedora, Debian и др.)
        ["gnome-terminal", "--title", title, "--", "bash", "-c", f"{command}; exec bash"],
        
        # KDE
        ["konsole", "--hold", "--title", title, "-e", "bash", "-c", f"{command}; exec bash"],
        
        # Xfce
        ["xfce4-terminal", "--title", title, "--hold", "-x", "bash", "-c", f"{command}; exec bash"],
        
        # Mate
        ["mate-terminal", "--title", title, "--", "bash", "-c", f"{command}; exec bash"],
        
        # LXDE / LXQt
        ["lxterminal", "--title", title, "-e", "bash", "-c", f"{command}; exec bash"],
        
        # Общий fallback — xterm (почти всегда есть)
        ["xterm", "-hold", "-title", title, "-e", "bash", "-c", f"{command}; exec bash"],
    ]

    for term_cmd in terminal_emulators:
        try:
            # Пробуем запустить
            proc = subprocess.Popen(term_cmd)
            if proc.poll() is None:  # Процесс успешно стартовал
                return proc
        except FileNotFoundError:
            continue  # Этот терминал не установлен — пробуем следующий
        except Exception:
            continue

    print("Ошибка: не найден ни один поддерживаемый эмулятор терминала!", file=sys.stderr)
    sys.exit(1)

print("Lab-07d (Linux): Запуск двух дочерних процессов Lab-07x в отдельных окнах терминала...\n")

# Команда для первого процесса — 60 секунд
cmd1 = "python3 Lab-07x.py 60"

# Команда для второго процесса — 120 секунд
cmd2 = "python3 Lab-07x.py 120"

# Запускаем в отдельных окнах
proc1 = run_in_new_terminal("Lab-07x: 1 минута", cmd1)
print(f"Первое окно терминала открыто — работа 1 минуту (PID терминала: {proc1.pid})")

proc2 = run_in_new_terminal("Lab-07x: 2 минуты", cmd2)
print(f"Второе окно терминала открыто — работа 2 минуты (PID терминала: {proc2.pid})\n")

print("Родительский процесс ожидает завершения окон терминала...")

# Ждём завершения обоих терминалов (т.е. когда пользователь закроет окна или процессы внутри завершатся)
proc1.wait()
print("Первое окно терминала закрыто")

proc2.wait()
print("Второе окно терминала закрыто")

print("\nОба дочерних процесса завершились. Родитель завершает работу.")
input("Нажмите Enter для выхода...")