import math
from collections import Counter

# Твои сообщения (вариант 3)
message1 = "мультимиллионер"
message2 = "мультимиллионерсеменохранилище"

messages = [
    ("1 часть: мультимиллионер", message1),
    ("2 часть: мультимиллионерсеменохранилище", message2)
]

SCALE_LENGTH = 80  # длина визуальной шкалы

def draw_interval(low, high, step):
    """Визуализация отрезка [low, high) на шкале 0..1"""
    start = int(low * SCALE_LENGTH)
    end = int(high * SCALE_LENGTH)
    if end > SCALE_LENGTH:
        end = SCALE_LENGTH
    if start >= end:
        # если отрезок слишком маленький — показываем точку
        pos = int(low * SCALE_LENGTH)
        line = [' '] * SCALE_LENGTH
        line[pos] = '●'
    else:
        line = ['-'] * start + ['█'] * (end - start) + ['-'] * (SCALE_LENGTH - end)
    print(f"Шаг {step:2d}: |" + ''.join(line) + "|  [{low:.15f}, {high:.15f})")

print("ЛАБОРАТОРНАЯ РАБОТА №11")
print("АРИФМЕТИЧЕСКОЕ КОДИРОВАНИЕ — НАГЛЯДНАЯ ВИЗУАЛИЗАЦИЯ ОТРЕЗКА")
print("=" * 80)

for idx, (title, msg) in enumerate(messages, 1):
    print(f"\n{'='*80}")
    print(f"{title.upper()} (длина: {len(msg)} символов)")
    print(f"{'='*80}")
    print(f"Исходное сообщение: {msg}\n")
    
    freq = Counter(msg)
    total = len(msg)
    
    print("1. МОДЕЛЬ ВЕРОЯТНОСТЕЙ")
    print("-" * 60)
    print(f"{'Символ':<10} {'Частота':<10} {'Вероятность':<15} {'Диапазон [low, high)'}")
    print("-" * 60)
    
    symbols = sorted(freq.keys())
    cum_low = 0.0
    cum_ranges = {}
    
    for sym in symbols:
        prob = freq[sym] / total
        cum_high = cum_low + prob
        cum_ranges[sym] = (cum_low, cum_high)
        print(f"{sym:<10} {freq[sym]:<10} {prob:<15.6f} [{cum_low:.12f}, {cum_high:.12f})")
        cum_low = cum_high
    
    print("-" * 60)
    print()
    
    entropy = -sum((count / total) * math.log2(count / total) for count in freq.values())
    
    print("2. ПРОЦЕСС КОДИРОВАНИЯ С ВИЗУАЛИЗАЦИЕЙ ОТРЕЗКА")
    print("-" * 80)
    print("Шкала: |" + "-" * SCALE_LENGTH + "|  (0.0 — 1.0)\n")
    
    low = 0.0
    high = 1.0
    
    draw_interval(low, high, 0)
    print()
    
    for step, char in enumerate(msg, 1):
        range_width = high - low
        char_low, char_high = cum_ranges[char]
        
        new_low = low + range_width * char_low
        new_high = low + range_width * char_high
        
        print(f"Символ {step:2d}: '{char}' (диапазон символа: [{char_low:.12f}, {char_high:.12f}))")
        draw_interval(new_low, new_high, step)
        print(f"   Ширина отрезка: {range_width:.18f} → {new_high - new_low:.18f}\n")
        
        low = new_low
        high = new_high
    
    print("-" * 80)
    print(f"Финальный отрезок: [{low:.20f}, {high:.20f})")
    print(f"Ширина: {high - low:.20f}")
    
    code = (low + high) / 2
    
    print(f"\n3. ФИНАЛЬНЫЙ КОД")
    print("-" * 50)
    print(f"Код (середина отрезка): {code:.20f}")
    
    if high - low > 0:
        required_bits = -math.log2(high - low)
        print(f"Теоретическая длина кода: {required_bits:.3f} бит")
        print(f"Средняя на символ: {required_bits / len(msg):.3f} бит/символ")
        print(f"Энтропия Шеннона: {entropy:.3f} бит/символ")
        #print(f"Эффективность: {entropy / (required_bits / len(msg)) * 100:.1f}%")
    else:
        print("Ширина отрезка = 0 — достигнут предел точности float")
        print(f"Энтропия Шеннона: {entropy:.3f} бит/символ")
    
    print("-" * 50)
    print()
    
    print("4. ДЕКОДИРОВАНИЕ")
    print("-" * 80)
    print("Шкала: |" + "-" * SCALE_LENGTH + "|")
    print()
    
    value = code
    current_low = 0.0
    current_high = 1.0
    decoded = []
    
    for step in range(1, len(msg) + 1):
        range_width = current_high - current_low
        if range_width <= 0:
            print(f"Шаг {step}: отрезок нулевой — декодирование невозможно дальше")
            break
        
        normalized = (value - current_low) / range_width
        
        found = None
        for sym in symbols:
            sym_low, sym_high = cum_ranges[sym]
            if sym_low <= normalized < sym_high:
                found = sym
                break
        
        if found is None:
            print(f"Шаг {step}: символ не найден (потеря точности)")
            break
        
        decoded.append(found)
        
        new_low = current_low + range_width * cum_ranges[found][0]
        new_high = current_low + range_width * cum_ranges[found][1]
        
        print(f"Шаг {step:2d}: код {value:.18f} → нормализ. {normalized:.15f} → символ '{found}'")
        draw_interval(new_low, new_high, step)
        print()
        
        current_low = new_low
        current_high = new_high
    
    decoded_msg = ''.join(decoded)
    
    print("-" * 80)
    print(f"Восстановленное сообщение: {decoded_msg}")
    print(f"Совпадение: {'ДА' if decoded_msg == msg else 'НЕТ (из-за потери точности на последних символах)'}")
    print()
    
    print("5. ОЦЕНКА ТОЧНОСТИ")
    print("-" * 50)
    print(f"Финальная ширина отрезка: {high - low:.20f}")
    if high - low > 0:
        bits_needed = math.ceil(-math.log2(high - low))
        print(f"Требуемая точность: ~{bits_needed} бит")
    else:
        print("Отрезок сузился до нуля — потеря точности")
    

