import os
import threading
import psutil
import multiprocessing
import ctypes

def get_process_affinity_mask(process):
    """Получить маску родственности процесса в двоичном виде"""
    try:
        affinity = process.cpu_affinity()
        max_cpus = os.cpu_count()
        mask = ['0'] * max_cpus
        for cpu in affinity:
            if cpu < max_cpus:
                mask[cpu] = '1'
        return ''.join(mask[::-1])
    except Exception:
        return "Недоступно"

def get_system_affinity_mask():
    """Получить системную маску родственности в двоичном виде"""
    try:
        max_cpus = os.cpu_count()
        mask = ['1'] * max_cpus
        return ''.join(mask[::-1])
    except Exception:
        return "Недоступно"

def get_process_priority_class(process):
    """Получить класс приоритетов процесса"""
    try:
        priority = process.nice()
        priority_classes = {
            psutil.REALTIME_PRIORITY_CLASS: "REALTIME_PRIORITY_CLASS",
            psutil.HIGH_PRIORITY_CLASS: "HIGH_PRIORITY_CLASS",
            psutil.ABOVE_NORMAL_PRIORITY_CLASS: "ABOVE_NORMAL_PRIORITY_CLASS",
            psutil.NORMAL_PRIORITY_CLASS: "NORMAL_PRIORITY_CLASS",
            psutil.BELOW_NORMAL_PRIORITY_CLASS: "BE-LOW_NORMAL_PRIORITY_CLASS",
            psutil.IDLE_PRIORITY_CLASS: "IDLE_PRIORITY_CLASS"
        }
        return priority_classes.get(process.nice(), f"Unknown: {priority}")
    except Exception as e:
        return f"Недоступно ({e})"

def get_current_processor():
    """Получить номер текущего процессора"""
    try:
        current_cpu = os.sched_getaffinity(0)
        return min(current_cpu) if current_cpu else 'Не определен'
    except Exception:
        return "Невозможно определить"

def get_thread_priority():
    """Получить приоритет текущего потока"""
    try:
        thread_id = threading.current_thread().ident
        kernel32 = ctypes.windll.kernel32
        handle = kernel32.OpenThread(0x0040, False, thread_id)
        if handle:
            priority = kernel32.GetThreadPriority(handle)
            kernel32.CloseHandle(handle)
            
            priority_names = {
                -15: "THREAD_PRIORITY_LOWEST",
                -2: "THREAD_PRIORITY_BELOW_NORMAL",
                0: "THREAD_PRIORITY_NORMAL",
                2: "THREAD_PRIORITY_ABOVE_NORMAL",
                1: "THREAD_PRIORITY_HIGHEST",
                15: "THREAD_PRIORITY_TIME_CRITICAL"
            }
            return priority_names.get(priority, f"Unknown: {priority}")
        return "Невозможно определить"
    except Exception as e:
        return f"Недоступно ({e})"

def main():
    current_process = psutil.Process()
    
    print(f"Идентификатор текущего процесса: {current_process.pid}")
    print(f"Идентификатор текущего потока: {threading.get_ident()}")
    print(f"Класс приоритетов текущего процесса: {get_process_priority_class(current_process)}")
    print(f"Приоритет текущего потока: Thread ID: {get_thread_priority()}")
    print(f"Маска родственности процесса: {get_process_affinity_mask(current_process)}")
    print(f"Системная маска родственности: {get_system_affinity_mask()}")
    print(f"Доступных процессоров: {multiprocessing.cpu_count()}")
    print(f"Текущий процессор: {get_current_processor()}")

if __name__ == "__main__":
    main()
