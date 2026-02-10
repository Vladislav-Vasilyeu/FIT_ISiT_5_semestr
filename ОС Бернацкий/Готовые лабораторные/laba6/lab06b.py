import subprocess
import sys
import os
import time
import win32event
import win32api
import winerror

def worker_process(process_name, username, iterations=90):
    """Функция рабочего процесса с именованным мьютексом"""
    letters = [char for char in username if char.isalpha()]
    letter_index = 0
    
    mutex_name = "Global\\Lab06b_CriticalSection"
    mutex = win32event.CreateMutex(None, False, mutex_name)
    
    print(f"=== {process_name} запущен (PID: {os.getpid()}) ===")
    print(f"Используемые буквы: {''.join(letters)}")
    print("-" * 50)
    
    i = 1
    while i <= iterations:
        letter = letters[letter_index]
        letter_index = (letter_index + 1) % len(letters)
        
        if 30 <= i <= 60:
            print(f"\n--- {process_name} ОЖИДАЕТ КРИТИЧЕСКУЮ СЕКЦИЮ ---")
            
            win32event.WaitForSingleObject(mutex, win32event.INFINITE)
            try:
                print(f"--- {process_name} ВОШЕЛ В КРИТИЧЕСКУЮ СЕКЦИЮ ---")
                
                while i <= 60 and i <= iterations:
                    critical_mark = " [CRITICAL]"
                    print(f"{process_name}: итерация {i:2d}, буква '{letter}'{critical_mark}")
                    time.sleep(0.1)
                    i += 1
                    if i <= iterations:
                        letter = letters[letter_index]
                        letter_index = (letter_index + 1) % len(letters)
                
                print(f"--- {process_name} ВЫШЕЛ ИЗ КРИТИЧЕСКОЙ СЕКЦИИ ---\n")
            finally:
                win32event.ReleaseMutex(mutex)
        else:
            critical_mark = ""
            print(f"{process_name}: итерация {i:2d}, буква '{letter}'{critical_mark}")
            time.sleep(0.1)
            i += 1
    
    win32api.CloseHandle(mutex)
    print(f"=== {process_name} завершен ===")
    input("Нажмите Enter для выхода...")

def main():
    """Главная функция для запуска процессов в отдельных консо-лях"""
    username = "User-64cc68ae"
    
    print("Запуск приложения Lab-06b")
    print(f"Полное имя пользователя: {username}")
    print(f"Критический диапазон: итерации 30-60")
    print("=" * 60)
    
    current_file = os.path.abspath(__file__)
    
    processes = []
    
    print("Запуск Процесса A в отдельной консоли...")
    cmd_a = [sys.executable, current_file, "Процесс A", username]
    process_a = subprocess.Popen(cmd_a, creationflags=subprocess.CREATE_NEW_CONSOLE)
    processes.append(process_a)
    
    print("Запуск Процесса B в отдельной консоли...")
    cmd_b = [sys.executable, current_file, "Процесс B", username]
    process_b = subprocess.Popen(cmd_b, creationflags=subprocess.CREATE_NEW_CONSOLE)
    processes.append(process_b)
    
    print("Запуск главного процесса в этой консоли...")
    worker_process("Главный процесс", username)
    
    for process in processes:
        process.wait()
    
    print("=" * 60)
    print("Все процессы завершили выполнение")
    input("Нажмите Enter для выхода...")

if __name__ == "__main__":
    if len(sys.argv) >= 3:
        worker_process(sys.argv[1], sys.argv[2])
    else:
        main()
