import random
from typing import List

class IterativeCode:
    @staticmethod
    def msg_to_2dim_matrix(msg: List[int], height: int, width: int) -> List[List[int]]:
        """Преобразует сообщение в двумерную матрицу (список столбцов)"""
        if len(msg) != width * height:
            raise ValueError("Размеры матрицы не соответствуют размерам сообщения")
        matrix = []
        for i in range(width):
            row = []
            for j in range(height):
                row.append(msg[i * height + j])
            matrix.append(row)
        return matrix

    @staticmethod
    def calculate_check_bits(matrix: List[List[int]]) -> List[int]:
        """Вычисляет контрольные биты: сначала вертикальные (Xv), затем горизонтальные (Xh), затем суперпаритет (Xhv)"""
        width = len(matrix)
        height = len(matrix[0])
        bits = [0] * (width + height + 1)

        # Вертикальные паритеты (по столбцам)
        for i in range(height):
            col_sum = sum(matrix[j][i] for j in range(width))
            bits[i] = col_sum % 2

        # Горизонтальные паритеты (по строкам)
        total_sum = 0
        for i in range(width):
            row_sum = sum(matrix[i][j] for j in range(height))
            total_sum += row_sum
            bits[i + height] = row_sum % 2

        # Суперпаритет
        for i in range(len(bits) - 1):
            total_sum += bits[i]
        bits[-1] = total_sum % 2

        return bits

    @staticmethod
    def find_error_positions(matrix: List[List[int]], check_bits: List[int]) -> List[int]:
        """Находит позиции ошибок в исходном сообщении по несоответствию паритетов"""
        check_bits_for_matrix = IterativeCode.calculate_check_bits(matrix)
        width = len(matrix)
        height = len(matrix[0])

        row_mismatch = []
        col_mismatch = []

        # Несовпадения вертикальных паритетов (столбцы)
        for i in range(height):
            if check_bits[i] != check_bits_for_matrix[i]:
                col_mismatch.append(i)

        # Несовпадения горизонтальных паритетов (строки)
        for i in range(width):
            if check_bits[i + height] != check_bits_for_matrix[i + height]:
                row_mismatch.append(i)

        # Индексы ошибок в исходном сообщении
        result = []
        for row in row_mismatch:
            for col in col_mismatch:
                result.append(row * height + col)

        return result


class Printer:
    @staticmethod
    def print_matrix(msg: str, matrix: List[List[int]], reverse: bool = False) -> None:
        print(msg)
        if reverse:
            for i in range(len(matrix[0])):
                print("    ", end="")
                for j in range(len(matrix)):
                    print(matrix[j][i], end="")
                print()
        else:
            for i in range(len(matrix)):
                print("    ", end="")
                for j in range(len(matrix[i])):
                    print(matrix[i][j], end="")
                print()

    @staticmethod
    def print_bits(msg: str, bits: List[int]) -> None:
        print(f"{msg}", end="")
        for bit in bits:
            print(bit, end="")
        print()


def build_encoded_message(msg: List[int], check_bits: List[int], height: int, width: int) -> List[int]:
    """Формирует закодированное сообщение: msg + Xv + Xh + Xhv"""
    Xv = check_bits[:height]
    Xh = check_bits[height:height + width]
    Xhv = [check_bits[-1]]
    return msg + Xv + Xh + Xhv


def main():
    # Параметры матрицы
    height = 4
    width = 6

    # Исходное сообщение (24 бита = 4*6)
    msg = [1, 1, 0, 0, 1, 1, 0, 1, 0, 1, 1, 0,
           1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1]

    # 1) Исходное сообщение
    Printer.print_bits("Исходное сообщение: ", msg)

    # 2) Матрица
    matrix = IterativeCode.msg_to_2dim_matrix(msg, height, width)
    Printer.print_matrix("Двумерная матрица:", matrix, False)
    print()

    # 3) Паритеты
    check_bits = IterativeCode.calculate_check_bits(matrix)

    print("Горизонтальные паритеты (Xh): ", end="")
    for i in range(height, height + width):
        print(check_bits[i], end="")
    print()

    print("Вертикальные паритеты (Xv): ", end="")
    for i in range(height):
        print(check_bits[i], end="")
    print()

    print(f"Суперпаритет (Xhv): {check_bits[-1]}")
    print()

    Printer.print_bits("Все контрольные биты Xr= ", check_bits)
    print("=========================================\n")

    # 4) Закодированное сообщение
    encoded = build_encoded_message(msg, check_bits, height, width)
    Printer.print_bits("Закодированное сообщение (msg + Xv + Xh + Xhv): ", encoded)
    print("=========================================\n")

    # === Демонстрационный случай при 0 ошибках ===
    print("СЛУЧАЙ БЕЗ ОШИБОК:")
    no_error_matrix = IterativeCode.msg_to_2dim_matrix(msg, height, width)
    Printer.print_matrix("Матрица без ошибок:", no_error_matrix, False)

    no_error_check_bits = IterativeCode.calculate_check_bits(no_error_matrix)

    print("Горизонтальные паритеты (Xh): ", end="")
    for i in range(height, height + width):
        print(no_error_check_bits[i], end="")
    print()

    print("Вертикальные паритеты (Xv): ", end="")
    for i in range(height):
        print(no_error_check_bits[i], end="")
    print()

    print(f"Суперпаритет (Xhv): {no_error_check_bits[-1]}")
    print()

    Printer.print_bits("Все контрольные биты Xr= ", no_error_check_bits)

    found_errors = IterativeCode.find_error_positions(no_error_matrix, check_bits)
    if found_errors:
        print("Найдены ошибки в позициях:", found_errors)
    else:
        print("Ошибки не найдены.")
    print("=========================================\n")

    # === Автоматические эксперименты N=100 ===
    N = 100
    N1 = N
    N2 = 0
    N3 = 0
    error_distribution = {0: 0, 1: 0, 2: 0, 3: 0}

    for _ in range(N):
        test_msg = msg.copy()

        num_errors = random.randint(0, 3)
        error_distribution[num_errors] += 1
        error_positions = random.sample(range(len(test_msg)), num_errors)

        for pos in error_positions:
            test_msg[pos] ^= 1

        new_matrix = IterativeCode.msg_to_2dim_matrix(test_msg, height, width)
        found_errors = IterativeCode.find_error_positions(new_matrix, check_bits)

        if len(found_errors) == len(error_positions):
            N2 += 1

        all_correct = all(e in error_positions for e in found_errors) and len(found_errors) == len(error_positions)
        if all_correct:
            N3 += 1

    print("РЕЗУЛЬТАТЫ ЭКСПЕРИМЕНТОВ (N=100):")
    print(f"N1 = {N1}  (всего экспериментов)")
    print(f"N2 = {N2}  (правильно определена кратность)")
    print(f"N3 = {N3}  (все ошибки корректно определены)")
    if N1 > 0:
        print(f"N2/N1 = {N2 / N1:.2f}")
        print(f"N3/N1 = {N3 / N1:.2f}")

    print("\nРаспределение по числу ошибок:")
    for k, v in error_distribution.items():
        print(f"  Ошибок {k}: {v} случаев")

    print("\nРабота завершена.")


if __name__ == "__main__":
    main()
