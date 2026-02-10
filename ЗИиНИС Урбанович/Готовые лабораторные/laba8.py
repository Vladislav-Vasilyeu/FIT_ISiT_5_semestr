import time

def burrows_wheeler_transform(s):
    print(f"\n=== ПРЯМОЕ ПРЕОБРАЗОВАНИЕ BWT ===")
    print(f"Исходная строка: '{s}' (длина: {len(s)})")
    
    s += '$'
    n = len(s)
    
    rotations = [s[i:] + s[:i] for i in range(n)]
    
    print("\nВсе циклические сдвиги:")
    for i, rot in enumerate(rotations):
        print(f"{i:2d}: {rot}")
    
    start_time = time.perf_counter()
    sorted_rotations = sorted(rotations)
    sort_time = time.perf_counter() - start_time
    
    print("\nОтсортированные циклические сдвиги:")
    for i, rot in enumerate(sorted_rotations):
        print(f"{i:2d}: {rot}")
    
    original_index = sorted_rotations.index(s)
    L = ''.join(rot[-1] for rot in sorted_rotations)
    
    print(f"\nРезультат прямого BWT:")
    print(f"  L = '{L}'")
    print(f"  I (индекс) = {original_index}")
    print(f"  Время сортировки: {sort_time:.6f} сек")
    
    return L, original_index, sort_time

def inverse_burrows_wheeler(L, I):
    print(f"\n=== ОБРАТНОЕ ПРЕОБРАЗОВАНИЕ BWT (ПОЛНЫЙ ПОШАГОВЫЙ ВЫВОД) ===")
    print(f"L = '{L}', I = {I}")
    
    n = len(L)
    table = [""] * n
    
    start_time = time.perf_counter()
    
    for iteration in range(1, n + 1):
        print(f"\nИТЕРАЦИЯ {iteration}:")
        
        # Шаг 1: Добавляем L слева к текущим строкам
        unsorted_table = [L[i] + table[i] for i in range(n)]
        
        print("  После добавления L слева (неотсортировано):")
        for j in range(n):
            print(f"    {j:2d}: {unsorted_table[j]}")
        
        # Шаг 2: Сортируем таблицу
        table = sorted(unsorted_table)
        
        print("  После сортировки:")
        for j in range(n):
            marker = " ← ИСХОДНАЯ СТРОКА" if j == I else ""
            print(f"    {j:2d}: {table[j]}{marker}")
    
    restore_time = time.perf_counter() - start_time
    
    restored = table[I].rstrip('$')
    
    print(f"\nФинальная восстановленная строка (без $): '{restored}'")
    print(f"Время обратного преобразования: {restore_time:.6f} сек")
    
    return restored, restore_time

def text_to_binary(text):
    binary = ''.join(format(ord(c), '08b') for c in text)
    print(f"\nТекст: '{text}'")
    print(f"ASCII коды: {' '.join(str(ord(c)) for c in text)}")
    print(f"Бинарная последовательность: {binary}")
    return binary

def main():
    print("ЛАБОРАТОРНАЯ РАБОТА №8")
    print("СЖАТИЕ/РАСПАКОВКА ДАННЫХ МЕТОДОМ БАРРОУЗА-УИЛЕРА")
    print("=" * 80)
    
    blocks = [
        "Владислав",
        "Васильев",
        "мультимиллионер"
    ]
    
    results = []
    
    for idx, block in enumerate(blocks, 1):
        print(f"\n{'='*80}")
        print(f"БЛОК {idx}: '{block}'")
        print(f"{'='*80}")
        
        L, I, fwd_time = burrows_wheeler_transform(block)
        restored, rev_time = inverse_burrows_wheeler(L, I)
        
        results.append({
            'text': block,
            'len': len(block),
            'fwd_time': fwd_time,
            'rev_time': rev_time
        })
        
        print(f"\nПроверка: {block == restored}")
    
    # Задание 3
    print(f"\n{'='*80}")
    print("ЗАДАНИЕ 3: Первые 3 символа 'мультимиллионер' → 'мул'")
    print(f"{'='*80}")
    
    binary_text = "мул"
    binary_str = text_to_binary(binary_text)
    
    print(f"\nBWT для бинарной строки:")
    L_bin, I_bin, fwd_bin = burrows_wheeler_transform(binary_str)
    restored_bin, rev_bin = inverse_burrows_wheeler(L_bin, I_bin)
    
    print(f"\nПроверка: {binary_str == restored_bin}")
    
    # Анализ времени
    print(f"\n{'='*80}")
    print("АНАЛИЗ ВРЕМЕНИ")
    print(f"{'='*80}")
    print(f"{'Текст':<20} {'Длина':<8} {'Прямое, сек':<15} {'Обратное, сек':<15} {'Соотношение'}")
    print("-" * 70)
    for res in results:
        ratio = res['rev_time'] / res['fwd_time'] if res['fwd_time'] > 0 else 0
        print(f"{res['text']:<20} {res['len']:<8} {res['fwd_time']:.6f}      {res['rev_time']:.6f}      {ratio:.2f}")
    
    ratio_bin = rev_bin / fwd_bin if fwd_bin > 0 else 0
    print(f"\nБинарный блок 'мул':")
    print(f"  Прямое: {fwd_bin:.6f} сек")
    print(f"  Обратное: {rev_bin:.6f} сек")
    print(f"  Соотношение: {ratio_bin:.6f}")

if __name__ == "__main__":
    main()