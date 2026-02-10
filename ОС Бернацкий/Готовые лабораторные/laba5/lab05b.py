import os
import sys
import subprocess
import psutil

def set_processor_affinity(mask):
    """Установить аффинность процессоров"""
    try:
        p = psutil.Process()
        if mask == 0:
            cpus = list(range(os.cpu_count()))
        else:
            cpus = [i for i in range(32) if (mask & (1 << i))]
        p.cpu_affinity(cpus)
        print(f"Аффинность установлена: CPU {cpus}")
        return True
    except Exception as e:
        print(f"Ошибка аффинности: {e}")
        return False

def get_priority_name(code):
    names = {
        0: "IDLE",
        1: "BELOW_NORMAL",
        2: "NORMAL",
        3: "ABOVE_NORMAL",
        4: "HIGH",
        5: "REALTIME"
    }
    return names.get(code, f"UNKNOWN({code})")

def create_child_process(num, prio_code):
    """Создаёт и запускает дочерний процесс"""
    child_code = f'''
import time
import psutil

def get_current_priority():
    """Получение приоритета через числовые коды Windows (актуально для psutil >=5.9)"""
    try:
        priority_value = psutil.Process().nice()
        # Стандартные коды приоритетов Windows
        mapping = {{
            64: "IDLE",
            16384: "BELOW_NORMAL",
            32: "NORMAL",
            32768: "ABOVE_NORMAL",
            128: "HIGH",
            256: "REALTIME"
        }}
        return mapping.get(priority_value, f"Unknown({{priority_value}})")
    except Exception as e:
        return f"Ошибка: {{e}}"

def set_priority(code):
    """Установка приоритета по нашему коду 0-5"""
    try:
        p = psutil.Process()
        # Те же числовые значения
        priority_map = {{
            0: 64,        # IDLE
            1: 16384,     # BELOW_NORMAL
            2: 32,        # NORMAL
            3: 32768,     # ABOVE_NORMAL
            4: 128,       # HIGH
            5: 256        # REALTIME
        }}
        if code in priority_map:
            p.nice(priority_map[code])
    except Exception as e:
        print(f"Не удалось установить приоритет: {{e}}")

# Устанавливаем приоритет
set_priority({prio_code})

start = time.perf_counter()
p = psutil.Process()

print(f"ДОЧЕРНИЙ ПРОЦЕСС {num}")
print(f"PID: {{p.pid}}")
print(f"Заданный приоритет: {{get_priority_name({prio_code})}}")
print(f"Фактический приоритет: {{get_current_priority()}}")
print("=" * 70)

total = 1000000
interval = 100000
results = []
i = 0

for i in range(total + 1):
    x = 0.0
    for j in range(50):
        x += (i * i * j) / 3.14159
        x -= (i + j * j) * 2.71828
        x *= 1.0001
    if i % 100 == 0:
        results.append(x)
    
    if i > 0 and i % interval == 0:
        elapsed = time.perf_counter() - start
        speed = i / elapsed if elapsed > 0 else 0
        print(f"Proc {num} | Итераций: {{i:7,}} | Время: {{elapsed:6.2f}}с | "
              f"Скорость: {{speed:7,.0f}} итер/с | Приоритет: {{get_current_priority()}}")

elapsed_total = time.perf_counter() - start

print("=" * 70)
print(f"Процесс {num} ЗАВЕРШИЛ ВЫЧИСЛЕНИЯ")
print(f"Выполнено итераций: {{i:,}}")
print(f"Затраченное время: {{elapsed_total:.2f}} секунд")
print(f"Средняя скорость: {{i / elapsed_total:,.0f}} итер/сек")
print(f"Контрольная сумма: {{sum(results):.2f}}")
print("=" * 70)
print("=== ОКНО ОСТАЁТСЯ ОТКРЫТЫМ ===")
print("Делайте скриншоты консолей и Process Explorer!")
print("Когда всё снимете — нажмите ENTER в этом окне")
input(">>> Нажмите ENTER для закрытия <<<")

def get_priority_name(code):
    names = {{0: "IDLE", 1: "BELOW_NORMAL", 2: "NORMAL",
              3: "ABOVE_NORMAL", 4: "HIGH", 5: "REALTIME"}}
    return names.get(code, f"UNKNOWN({{code}})")
'''

    temp_file = f"lab05x_temp_{num}.py"
    with open(temp_file, 'w', encoding='utf-8') as f:
        f.write(child_code)

    proc = subprocess.Popen(
        [sys.executable, temp_file],
        creationflags=subprocess.CREATE_NEW_CONSOLE
    )

    return proc, temp_file

def main():
    if len(sys.argv) != 4:
        print("Использование: python lab05b.py <P1> <P2> <P3>")
        print("  P1 — маска аффинности (0 = все CPU)")
        print("  P2 — приоритет 1-го процесса (0-5)")
        print("  P3 — приоритет 2-го процесса (0-5)")
        return

    try:
        p1 = int(sys.argv[1])
        p2 = int(sys.argv[2])
        p3 = int(sys.argv[3])

        print("=" * 70)
        print(f"Параметры:")
        print(f"  Аффинность: {p1} → {{'все CPU' if p1 == 0 else 'ограничено'}}")
        print(f"  Приоритет 1: {get_priority_name(p2)}")
        print(f"  Приоритет 2: {get_priority_name(p3)}")
        print("=" * 70)

        set_processor_affinity(p1)

        proc1, file1 = create_child_process(1, p2)
        proc2, file2 = create_child_process(2, p3)

        print("Дочерние процессы запущены. Ждите завершения вычислений в одном из окон — оно останется открытым для скриншотов!")

        proc1.wait()
        proc2.wait()

        for f in (file1, file2):
            try:
                os.remove(f)
            except:
                pass

        print("Всё завершено.")
        input("Нажмите ENTER для выхода...")

    except ValueError:
        print("Ошибка: аргументы должны быть числами!")
    except Exception as e:
        print(f"Ошибка: {e}")

if __name__ == "__main__":
    main()