from collections import Counter
import math
import heapq


message = "владиславвасильев".lower()

print(f"ЛАБОРАТОРНАЯ РАБОТА №9")
print("СЖАТИЕ ДАННЫХ: ШЕННОНА-ФАНО И ХАФФМАНА")
print("=" * 80)
print(f"Исходное сообщение: '{message}'")
print(f"Длина: {len(message)} символов")
print(f"ASCII: {len(message) * 8} бит\n")


freq = Counter(message)
symbols = sorted(freq.items(), key=lambda x: (-x[1], x[0]))

print("Частоты символов:")
for sym, count in symbols:
    prob = count / len(message)
    print(f"  '{sym}': {count} (p = {prob:.4f})")
print()


def shannon_fano_custom(items, code="", codes=None):
    if codes is None:
        codes = {}
    if len(items) == 1:
        codes[items[0][0]] = code if code else "0"
        return codes
    
    sorted_items = sorted(items, key=lambda x: (-x[1], x[0]))
    
    
    total = sum(c for _, c in sorted_items)
    half = total / 2
    cum = 0
    split = 0
    for i in range(len(sorted_items)):
        cum += sorted_items[i][1]
        if cum > half:  
            split = i
            break
    if split == 0:
        split = len(sorted_items) // 2  
    
    top = sorted_items[:split]      
    bottom = sorted_items[split:]   
    
    #print(f"Деление группы (код '{code}'): ")
    #print(f"  Верхняя (чаще, +1): {[(s, c) for s, c in top]}")
    #print(f"  Нижняя  (реже, +0): {[(s, c) for s, c in bottom]}")
    
    shannon_fano_custom(top, code + "1", codes)
    shannon_fano_custom(bottom, code + "0", codes)
    
    return codes

print("=== МЕТОД ШЕННОНА-ФАНО (пирамидка: чаще = 1, реже = 0) ===")
sf_codes = {}
shannon_fano_custom(symbols, "", sf_codes)

print("\nТаблица кодов Шеннона-Фано:")
print(f"{'Символ':<6} {'Частота':<8} {'Код'}")
for sym, count in symbols:
    print(f"{sym:<6} {count:<8} {sf_codes[sym]}")

sf_encoded = ''.join(sf_codes[c] for c in message)
sf_bits = len(sf_encoded)
print(f"\nЗакодированное сообщение: {sf_encoded}")
print(f"Длина: {sf_bits} бит")


def is_prefix_free(codes_dict):
    codes = list(codes_dict.values())
    for i in range(len(codes)):
        for j in range(len(codes)):
            if i != j and codes[j].startswith(codes[i]):
                return False
    return True

print(f"Условие префикса выполнено: {is_prefix_free(sf_codes)}")
print(f"Все коды уникальны: {len(sf_codes) == len(set(sf_codes.values()))}")

# Декодирование
def decode_sf(encoded, codes):
    rev = {v: k for k, v in codes.items()}
    res = ""
    cur = ""
    for b in encoded:
        cur += b
        if cur in rev:
            res += rev[cur]
            cur = ""
    return res

print(f"Декодировано обратно: '{decode_sf(sf_encoded, sf_codes)}'\n")


class Node:
    def __init__(self, char=None, freq=0, left=None, right=None):
        self.char = char
        self.freq = freq
        self.left = left
        self.right = right
    
    def __lt__(self, other):
        return self.freq < other.freq

def build_huffman_tree(freq_dict):
    heap = []
    for char, f in freq_dict.items():
        heapq.heappush(heap, Node(char, f))
    
    step = 1
    print(f"Шаг {step}: Исходные узлы:")
    temp = sorted(heap, key=lambda x: x.freq)
    for node in temp:
        print(f"  '{node.char}' ({node.freq})")
    step += 1
    
    while len(heap) > 1:
        left = heapq.heappop(heap)
        right = heapq.heappop(heap)
        merged = Node(None, left.freq + right.freq, left, right)
        heapq.heappush(heap, merged)
        
        print(f"\nШаг {step}: Объединение")
        print(f"  Левый:  {left.char or '*'} ({left.freq})")
        print(f"  Правый: {right.char or '*'} ({right.freq})")
        print(f"  Узел ({merged.freq})")
        step += 1
    
    return heap[0]

#def print_tree(node, prefix="", is_left=True):
    if node is not None:
        char_str = f"'{node.char}'" if node.char else "*"
        freq_str = f"({node.freq})"
        line = prefix + ("└── " if is_left else "├── ") + char_str + freq_str
        print(line)
        
        if node.left or node.right:
            new_prefix = prefix + ("    " if is_left else "│   ")
            if node.left:
                print_tree(node.left, new_prefix, True)
            if node.right:
                print_tree(node.right, new_prefix, False)

def generate_codes(node, code="", codes=None):
    if codes is None:
        codes = {}
    if node.char:
        codes[node.char] = code if code else "0"
    if node.left:
        generate_codes(node.left, code + "0", codes)
    if node.right:
        generate_codes(node.right, code + "1", codes)
    return codes

print("=== МЕТОД ХАФФМАНА ===")
root = build_huffman_tree(dict(symbols))

#print("\nДерево Хаффмана (ASCII-арт):")
#print_tree(root)

huff_codes = generate_codes(root)

print("\nТаблица кодов Хаффмана:")
print(f"{'Символ':<6} {'Частота':<8} {'Код'}")
for sym, count in symbols:
    print(f"{sym:<6} {count:<8} {huff_codes[sym]}")

huff_encoded = ''.join(huff_codes[c] for c in message)
huff_bits = len(huff_encoded)
print(f"\nЗакодированное сообщение: {huff_encoded}")
print(f"Длина: {huff_bits} бит")

print(f"Условие префикса выполнено: {is_prefix_free(huff_codes)}")
print(f"Все коды уникальны: {len(huff_codes) == len(set(huff_codes.values()))}")

def decode_huff(encoded, root):
    res = ""
    node = root
    for b in encoded:
        node = node.left if b == '0' else node.right
        if node.char:
            res += node.char
            node = root
    return res

print(f"Декодировано обратно: '{decode_huff(huff_encoded, root)}'\n")


print("=" * 80)
print("СРАВНЕНИЕ ЭФФЕКТИВНОСТИ")
print("=" * 80)
ascii_bits = len(message) * 8
print(f"ASCII (8 бит/символ): {ascii_bits} бит")
print(f"Шеннон-Фано:          {sf_bits} бит (сжатие {(1 - sf_bits/ascii_bits)*100:.2f}%)")
print(f"Хаффман:              {huff_bits} бит (сжатие {(1 - huff_bits/ascii_bits)*100:.2f}%)")

entropy = -sum(p * math.log2(p) for p in [c/len(message) for c in freq.values() if c > 0])
#print(f"\nТеоретическая энтропия: {entropy:.3f} бит/символ")
#print(f"Средняя длина кода Шеннон-Фано: {sf_bits/len(message):.3f} бит/символ")
#print(f"Средняя длина кода Хаффман:     {huff_bits/len(message):.3f} бит/символ")

