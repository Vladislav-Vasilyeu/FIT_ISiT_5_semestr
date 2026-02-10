import time

def lz77_compress(text, dict_size, buffer_size):
    """LZ77 сжатие с подробным выводом и возвратом триплетов"""
    print(f"\n{'='*80}")
    print("НАЧАЛО СЖАТИЯ LZ77")
    print(f"Размер словаря (n1): {dict_size}")
    print(f"Размер буфера просмотра (n2): {buffer_size}")
    print(f"Длина исходного текста: {len(text)} символов")
    print(f"{'='*80}\n")
    
    i = 0
    triplets = []
    step = 0
    max_steps = 30  # показываем первые 30 шагов
    
    while i < len(text):
        if step < max_steps:
            print(f"Шаг {step + 1:2d} | Позиция: {i:4d}")
            dict_preview = text[max(0, i-dict_size):i]
            print(f"   Словарь: ...{dict_preview[-30:]}")
            print(f"   Буфер:   {text[i:i+buffer_size]}")
        
        search_start = max(0, i - dict_size)
        search_window = text[search_start:i]
        lookahead = text[i:i + buffer_size]
        
        best_length = 0
        best_distance = 0
        
        for length in range(min(len(lookahead), len(search_window)), 0, -1):
            pattern = lookahead[:length]
            pos = search_window.rfind(pattern)
            if pos != -1:
                best_length = length
                best_distance = i - (search_start + pos)
                break
        
        next_char = text[i + best_length] if i + best_length < len(text) else ''
        
        triplet = (best_distance, best_length, next_char if next_char else '')
        triplets.append(triplet)
        
        if step < max_steps:
            if best_distance == 0:
                print(f"   → Нет совпадения → выводим '{next_char}'")
            else:
                print(f"   → Найдено совпадение: длина = {best_length}, расстояние = {best_distance}")
                print(f"     Повторяется: '{text[i:i+best_length]}'")
                if next_char:
                    print(f"     + следующий символ: '{next_char}'")
            print(f"   → Триплет: {triplet}\n")
        
        step += 1
        i += best_length + (1 if next_char else 0)
    
    if step > max_steps:
        print(f"... (всего выполнено {step} шагов, показано {max_steps})\n")
    
    print(f"Сжатие завершено!")
    print(f"Всего триплетов: {len(triplets)}")
    
    # Вывод запакованной строки
    print(f"\nЗАПАКОВАННАЯ ПОСЛЕДОВАТЕЛЬНОСТЬ (триплеты):")
    print("-" * 80)
    triplet_strs = [f"({d},{l},'{c}')" if c else f"({d},{l},'') " for d, l, c in triplets]
    print(" ".join(triplet_strs))
    print("-" * 80)
    
    print(f"{'='*80}")
    
    return triplets

def lz77_decompress(triplets):
    """LZ77 распаковка с подробным выводом"""
    print(f"\n{'='*80}")
    print("НАЧАЛО РАСПАКОВКИ LZ77")
    print(f"Количество триплетов: {len(triplets)}")
    print(f"{'='*80}\n")
    
    result = []
    step = 0
    max_steps = 30
    
    for dist, length, char in triplets:
        if step < max_steps:
            print(f"Шаг {step + 1:2d} | Триплет: ({dist}, {length}, '{char}')")
        
        if dist == 0:
            result.append(char)
            if step < max_steps:
                print(f"   → Добавляем символ: '{char}'")
        else:
            start = len(result) - dist
            copied = result[start:start + length]
            result.extend(copied)
            if step < max_steps:
                print(f"   → Копируем {length} символов с расстояния {dist}: '{''.join(copied)}'")
            if char:
                result.append(char)
                if step < max_steps:
                    print(f"   → Добавляем символ: '{char}'")
        
        if step < max_steps:
            preview = ''.join(result)[-60:]
            print(f"   Текущий результат: ...{preview}\n")
        
        step += 1
    
    if step > max_steps:
        print(f"... (всего обработано {step} триплетов, показано {max_steps})\n")
    
    decompressed = ''.join(result)
    print(f"Распаковка завершена!")
    print(f"Длина восстановленного текста: {len(decompressed)} символов")
    print(f"{'='*80}")
    
    return decompressed

def calculate_compression(original_len, triplets, dict_size, buffer_size):
    """Расчёт результата сжатия"""
    original_bits = original_len * 8
    
    # Оценка бит на триплет
    bits_distance = (dict_size + 1).bit_length() if dict_size > 0 else 1
    bits_length = (buffer_size + 1).bit_length()
    bits_char = 8  # упрощённо, для кириллицы можно 16
    
    bits_per_triplet = bits_distance + bits_length + bits_char
    compressed_bits = len(triplets) * bits_per_triplet
    
    ratio = original_bits / compressed_bits if compressed_bits > 0 else 0
    savings = (1 - compressed_bits / original_bits) * 100 if original_bits > 0 else 0
    
    return original_bits, compressed_bits, ratio, savings

def main():
    print("ЛАБОРАТОРНАЯ РАБОТА №10")
    print("СЖАТИЕ/РАСПАКОВКА ДАННЫХ МЕТОДОМ ЛЕМПЕЛЯ-ЗИВА (LZ77)")
    print("=" * 80)
    
    # Ввод текста
    print("Введите текст для сжатия (завершите пустой строкой):")
    lines = []
    while True:
        try:
            line = input()
            if line == "":
                break
            lines.append(line)
        except EOFError:
            break
    
    text = "\n".join(lines)
    if not text.strip():
        text = "абракадабра " * 200  # тестовый текст с повторами
    
    print(f"\nИсходный текст ({len(text)} символов):")
    print(text[:500] + ("..." if len(text) > 500 else ""))
    print(f"{'='*80}\n")
    
    results = []
    
    for test_num in range(1, 6):
        print(f"{'*'*80}")
        print(f"ТЕСТ {test_num}/5")
        print(f"{'*'*80}")
        
        try:
            n1 = int(input("Введите размер словаря (n1): "))
            n2 = int(input("Введите размер буфера просмотра (n2): "))
        except:
            print("Ошибка ввода. Используем n1=1000, n2=50")
            n1, n2 = 1000, 50
        
        # Сжатие
        start_time = time.perf_counter()
        triplets = lz77_compress(text, n1, n2)
        compress_time = time.perf_counter() - start_time
        
        # Распаковка
        start_time = time.perf_counter()
        recovered = lz77_decompress(triplets)
        decompress_time = time.perf_counter() - start_time
        
        # Результат сжатия
        orig_bits, comp_bits, ratio, savings = calculate_compression(len(text), triplets, n1, n2)
        
        correct = recovered == text
        
        results.append({
            'test': test_num,
            'n1': n1,
            'n2': n2,
            'triplets': len(triplets),
            'orig_bits': orig_bits,
            'comp_bits': comp_bits,
            'ratio': ratio,
            'savings': savings,
            'compress_time': compress_time,
            'decompress_time': decompress_time,
            'correct': correct
        })
        
        print(f"\nРЕЗУЛЬТАТ СЖАТИЯ ТЕСТА {test_num}")
        print("-" * 60)
        print(f"  Размер до:     {orig_bits} бит")
        print(f"  Оценка после:  {comp_bits} бит ({len(triplets)} триплетов)")
        print(f"  Коэффициент:   {ratio:.2f}")
        print(f"  Экономия:      {savings:.1f}%")
        print(f"  Время сжатия:  {compress_time:.4f} с")
        print(f"  Время распаковки: {decompress_time:.4f} с")
        print(f"  Корректность:  {'ДА' if correct else 'НЕТ'}")
        print("-" * 60)
    
    # Итоговая таблица
    print(f"\n{'='*100}")
    print("ИТОГОВАЯ ТАБЛИЦА")
    print(f"{'='*100}")
    print(f"{'Тест':<6} {'n1':<10} {'n2':<10} {'Триплетов':<12} {'До, бит':<12} {'После, бит':<12} {'Коэфф.':<10} {'Экономия %':<12} {'Корректно'}")
    print("-" * 100)
    for r in results:
        print(f"{r['test']:<6} {r['n1']:<10} {r['n2']:<10} {r['triplets']:<12} {r['orig_bits']:<12} {r['comp_bits']:<12} {r['ratio']:<10.2f} {r['savings']:<12.1f} {'Да' if r['correct'] else 'Нет'}")
    
    best = max(results, key=lambda x: x['savings'])
    print(f"\nЛучшее сжатие: тест {best['test']} — экономия {best['savings']:.1f}%")

if __name__ == "__main__":
    main()