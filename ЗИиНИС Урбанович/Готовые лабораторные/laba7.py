import random
import numpy as np

# Параметры
MESSAGE_BYTES = 15
TOTAL_INFO_BITS = MESSAGE_BYTES * 8  # 120 бит
K = 6
INFO_ROWS = 3
INFO_COLS = 2
INTERLEAVE_COLUMNS = 6
BURST_LENGTHS = [3, 5, 6]
NUM_TESTS = 35
DETAILED_TESTS = 10

def parity(bits):
    return sum(bits) % 2

class SmallProductCode:
    def __init__(self):
        self.original_matrices = []

    def encode_word(self, info6, word_num):
        matrix = np.array(info6).reshape(INFO_ROWS, INFO_COLS)
        
        print(f"  Инфо-матрица 3x2 для слова {word_num}:")
        for row in matrix:
            print("    " + " ".join(map(str, row)))
        
        # Горизонтальные паритеты
        row_parities = [parity(matrix[i]) for i in range(INFO_ROWS)]
        matrix = np.hstack((matrix, np.array(row_parities, dtype=int).reshape(-1, 1)))
        
        print("  + горизонтальные паритеты:")
        for row in matrix:
            print("    " + " ".join(map(str, row)))
        
        # Вертикальные паритеты только по инфо-столбцам
        col_parities_info = [parity(matrix[:, j]) for j in range(INFO_COLS)]
        
        # Паритет по строке горизонтальных паритетов = overall parity
        overall_parity = parity(row_parities)
        
        # Формируем нижнюю строку
        bottom_row = col_parities_info + [overall_parity]
        matrix = np.vstack((matrix, np.array(bottom_row, dtype=int)))
        
        print("  + вертикальные паритеты и паритет паритетов:")
        for row in matrix:
            print("    " + " ".join(map(str, row)))
        print()
        
        coded12 = info6 + row_parities + col_parities_info + [overall_parity]
        
        self.original_matrices.append(matrix.copy())
        return coded12

    def decode_word_detailed(self, received12, word_num, print_detail=False):
        info_rec = received12[:6]
        row_p = received12[6:9]
        col_p_info = received12[9:11]
        overall = received12[11]
        
        mat = np.zeros((4, 3), dtype=int)
        mat[:3, :2] = np.array(info_rec).reshape(3, 2)
        mat[:3, 2] = row_p
        mat[3, :2] = col_p_info
        mat[3, 2] = overall
        
        original_mat = self.original_matrices[word_num - 1]
        
        if print_detail:
            print(f"    Полученная матрица 4x3 для слова {word_num} (ошибки в \"\"):")
            for r in range(4):
                row_str = []
                for c in range(3):
                    bit = mat[r, c]
                    orig = original_mat[r, c]
                    if bit != orig:
                        row_str.append(f'"{bit}"')
                    else:
                        row_str.append(str(bit))
                print("      " + " ".join(row_str))
        
        corrections = 0
        for iter_num in range(1, 4):
            row_syndromes = [parity(mat[i]) for i in range(4)]
            col_syndromes = [parity(mat[:, j]) for j in range(3)]
            
            if print_detail:
                print(f"    Итерация {iter_num}:")
                print(f"      Синдромы строк: {' '.join(map(str, row_syndromes))}")
                print(f"      Синдромы столбцов: {' '.join(map(str, col_syndromes))}")
            
            error_found = False
            for i in range(4):
                for j in range(3):
                    if row_syndromes[i] == 1 and col_syndromes[j] == 1:
                        old = mat[i, j]
                        mat[i, j] ^= 1
                        corrections += 1
                        error_found = True
                        if print_detail:
                            pos_type = "инфо" if i < 3 and j < 2 else "гор. паритет" if j == 2 and i < 3 else "верт. паритет" if i == 3 and j < 2 else "overall"
                            print(f"      Исправлена ошибка в ({i+1},{j+1}) [{pos_type}]: {old} → {mat[i, j]}")
            if not error_found:
                if print_detail:
                    print("      Одиночных ошибок больше нет.")
                break
        
        if print_detail and corrections > 0:
            print("    Матрица после исправления:")
            for r in range(4):
                print("      " + " ".join(map(str, mat[r])))
            print()
        
        return mat[:3, :2].flatten().tolist(), corrections

class BlockInterleaver:
    def __init__(self, columns):
        self.columns = columns
    
    def interleave(self, bits):
        orig_len = len(bits)
        pad_len = (self.columns - orig_len % self.columns) % self.columns
        padded = bits + [0] * pad_len
        rows = len(padded) // self.columns
        matrix = np.array(padded).reshape(rows, self.columns)
        
        print(f"  Полная матрица перемежения ({rows} строк x {self.columns} столбцов):")
        for i in range(rows):
            print("    " + " ".join(map(str, matrix[i])))
        print()
        
        interleaved = matrix.T.flatten().tolist()
        print(f"  Последовательность после перемежения (длина {len(interleaved)}):")
        print("  " + "".join(map(str, interleaved)))
        print()
        return interleaved, orig_len, pad_len
    
    def deinterleave_detailed(self, bits, orig_len, pad_len, print_detail=False):
        rows = len(bits) // self.columns
        matrix = np.array(bits).reshape(self.columns, rows).T
        deinterleaved_padded = matrix.flatten().tolist()
        deinterleaved = deinterleaved_padded[:orig_len]
        
        if print_detail:
            print(f"    Полная матрица деперемежения ({rows} строк x {self.columns} столбцов):")
            for i in range(rows):
                print("      " + " ".join(map(str, matrix[i])))
            print()
            print(f"    Последовательность после деперемежения (длина {len(deinterleaved)}):")
            print("    " + "".join(map(str, deinterleaved)))
            print()
            
            print("    Разбиение на закодированные слова по 12 бит:")
            for w in range(20):
                word = deinterleaved[w*12:(w+1)*12]
                print(f"      Слово {w+1}: {' '.join(map(str, word))}")
            print()
        
        return deinterleaved

def main():
    random.seed(42)
    
    message_bits = [random.randint(0, 1) for _ in range(TOTAL_INFO_BITS)]
    print(f"Исходное сообщение (120 бит):\n{''.join(map(str, message_bits))}\n")
    
    info_words = [message_bits[i:i+K] for i in range(0, TOTAL_INFO_BITS, K)]
    print("Информационные слова (по 6 бит):")
    for i, word in enumerate(info_words, 1):
        print(f"{i:2d}: {' '.join(map(str, word))}")
    print()
    
    coder = SmallProductCode()
    all_coded_bits = []
    
    print("Кодирование каждого слова итеративным кодом:")
    for i, word in enumerate(info_words, 1):
        print(f"--- Слово {i} ---")
        coded12 = coder.encode_word(word, i)
        all_coded_bits.extend(coded12)
        print(f"Закодированное слово {i} (12 бит): {' '.join(map(str, coded12))}\n")
    
    print(f"Общий закодированный поток: {len(all_coded_bits)} бит (20 × 12 = 240)\n")
    
    interleaver = BlockInterleaver(INTERLEAVE_COLUMNS)
    interleaved, orig_len, pad_len = interleaver.interleave(all_coded_bits)
    
    stats = {length: {"success": 0} for length in BURST_LENGTHS}
    
    for burst_len_idx, burst_len in enumerate(BURST_LENGTHS):
        # Подробно для первых 10 тестов 3 бит + по одному тесту для 5 и 6 бит
        detailed_tests = DETAILED_TESTS if burst_len == 3 else 1
        
        print(f"{'='*80}")
        print(f"ТЕСТЫ ДЛЯ ПАКЕТОВ ОШИБОК ДЛИНЫ {burst_len} бит")
        
        print(f"{'='*80}")
        
        for test in range(1, NUM_TESTS + 1):
            is_detailed = test <= detailed_tests
            
            received_int = interleaved.copy()
            start = random.randint(0, len(received_int) - burst_len)
            error_positions = list(range(start, start + burst_len))
            for pos in error_positions:
                received_int[pos] ^= 1
            
            if is_detailed:
                print(f"\n{'-'*80}")
                print(f"ТЕСТ {test}: пакет ошибок длины {burst_len} с позиции {start}")
                highlight = []
                for i, bit in enumerate(received_int):
                    if i in error_positions:
                        highlight.append(f'"{bit}"')
                    else:
                        highlight.append(str(bit))
                print("  Перемежённая последовательность с ошибками:")
                print("  " + "".join(highlight))
                print("\n  Деперемежение:")
            
            received_coded = interleaver.deinterleave_detailed(received_int, orig_len, pad_len, print_detail=is_detailed)
            
            if is_detailed:
                print("  Декодирование слов:")
            
            recovered_bits = []
            total_corrections = 0
            
            for w in range(20):
                coded12_rec = received_coded[w*12:(w+1)*12]
                has_error = any(coded12_rec[j] != all_coded_bits[w*12 + j] for j in range(12))
                print_detail_word = is_detailed and has_error
                if print_detail_word:
                    print(f"  --- Декодирование слова {w+1} ---")
                decoded6, corrections = coder.decode_word_detailed(coded12_rec, w+1, print_detail=print_detail_word)
                recovered_bits.extend(decoded6)
                total_corrections += corrections
            
            errors = sum(a != b for a, b in zip(message_bits, recovered_bits))
            success = errors == 0
            if success:
                stats[burst_len]["success"] += 1
            
            status = "УСПЕХ" if success else "НЕУДАЧА"
            if is_detailed:
                print(f"  Всего исправлено: {total_corrections}")
                print(f"  Результат: {status} (остаточно ошибок: {errors})\n")
            else:
                print(f"Тест {test}: {status} (ошибок: {errors})")
        
        eff = stats[burst_len]["success"] / NUM_TESTS * 100
        print(f"\n>>> Эффективность для пакета {burst_len} бит: {eff:.1f}%")
    
    print("\nИТОГОВАЯ ЭФФЕКТИВНОСТЬ:")
    for length in BURST_LENGTHS:
        eff = stats[length]["success"] / NUM_TESTS * 100
        print(f"  Пакет длины {length} бит: {eff:.1f}%")

if __name__ == "__main__":
    main()