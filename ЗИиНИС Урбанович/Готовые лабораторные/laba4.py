import random

def get_bits_from_input(s: str) -> list[int]:
    """Если ввели чистое число — берём его бинарное представление.
       Если текст — кодируем как UTF-8."""
    if s.isdigit():
        num = int(s)
        bin_str = bin(num)[2:]                  # чистое двоичное без 0b
        bits = [int(b) for b in bin_str]
        print(f"Число {num} → двоичное: {''.join(map(str, bits))} (k = {len(bits)} бит)")
        return bits
    else:
        # Обычный текст → UTF-8
        bits = []
        for ch in s.encode('utf-8'):
            for i in range(7, -1, -1):
                bits.append((ch >> i) & 1)
        print(f"Текст «{s}» → {len(bits)} бит (UTF-8)")
        return bits

def minimal_r_for_k(k: int) -> int:
    r = 3
    while k + r + 1 > (1 << r):    # стандартное условие для расширенного Хемминга тоже покрывается
        r += 1
    return r

def all_nonzero_vectors(r: int):
    return [[(x >> i) & 1 for i in range(r-1, -1, -1)] for x in range(1, 1 << r)]

def build_P_prime(k: int, r: int) -> list[list[int]]:
    candidates = [v for v in all_nonzero_vectors(r) if sum(v) >= 2]
    cols = candidates[:k]
    return [[cols[j][i] for j in range(k)] for i in range(r)]

def identity(r: int):
    return [[1 if i == j else 0 for j in range(r)] for i in range(r)]

def compute_parity(Xk: list[int], P: list[list[int]]) -> list[int]:
    r = len(P)
    return [sum(Xk[j] & P[i][j] for j in range(len(Xk))) % 2 for i in range(r)]

def overall_parity(bits: list[int]) -> int:
    return sum(bits) % 2

def encode(message: str, r_in: int | None, errors: int, extended: bool):
    Xk = get_bits_from_input(message)
    k = len(Xk)

    r = minimal_r_for_k(k) if r_in is None else max(r_in, minimal_r_for_k(k))

    P_prime = build_P_prime(k, r)
    I = identity(r)
    H = [P_prime[i] + I[i] for i in range(r)]

    Xr = compute_parity(Xk, P_prime)
    Xn_core = Xk + Xr
    p = overall_parity(Xn_core) if extended else None
    Xn = Xn_core + [p] if extended else Xn_core

    Yn = Xn[:]
    positions = random.sample(range(len(Yn)), min(errors, len(Yn))) if errors > 0 else []
    for pos in positions:
        Yn[pos] ^= 1

    Yk = Yn[:k]
    Yr = Yn[k:k+r]
    p_recv = Yn[-1] if extended else None

    Yr_prime = compute_parity(Yk, P_prime)
    S = [(Yr[i] ^ Yr_prime[i]) for i in range(r)]

    # Декодирование
    En = [0] * len(Yn)
    mistake = None

    if extended:
        actual_parity = overall_parity(Yn[:-1])
        syndrome_weight = sum(S)

        if syndrome_weight == 0 and actual_parity != p_recv:
            mistake = len(Yn) - 1
            En[mistake] = 1
        elif syndrome_weight != 0 and actual_parity != p_recv:
            col = S[::-1]  # потому что мы строили векторы MSB → LSB
            pos = None
            for i in range(len(H[0])):
                if [H[j][i] for j in range(r)] == col:
                    pos = i
                    break
            if pos is not None:
                mistake = pos
                En[pos] = 1
    else:
        col = S[::-1]
        pos = None
        for i in range(len(H[0])):
            if [H[j][i] for j in range(r)] == col:
                pos = i
                break
        if pos is not None:
            mistake = pos
            En[pos] = 1

    Y_corrected = [a ^ b for a, b in zip(Yn, En)]

    return {
        "k": k, "r": r, "n": len(Xn), "extended": extended,
        "H": H, "Xk": Xk, "Xr": Xr, "Xn": Xn, "Yn": Yn,
        "S": S, "positions": positions, "mistake": mistake,
        "En": En, "Y_corrected": Y_corrected
    }

# ====================== КРАСИВЫЙ ВЫВОД ======================
def pv(name: str, v: list[int]):
    print(f"{name} = [{' '.join(map(str, v))}]")

def pm(name: str, m: list[list[int]]):
    print(f"{name}:")
    for row in m:
        print("  " + " ".join(map(str, row)))
    print()

# ====================== ОСНОВНОЙ ЦИКЛ ======================
if __name__ == "__main__":
    while True:
        msg = input("\nВведите данные (число или текст, exit — выход): ").strip()
        if msg.lower() == "exit" or not msg:
            break

        r_str = input("r (Enter — автоподбор): ").strip()
        r = int(r_str) if r_str else None
        
        err_str = input("Кол-во ошибок (0-2): ").strip()
        errors = int(err_str) if err_str else 0
        
        ext = input("Расширенный код? (y/n, по умолч. y): ").strip().lower() != "n"

        result = encode(msg, r, errors, ext)

        print("\n" + "="*60)
        print(f"k = {result['k']}   r = {result['r']}   n = {result['n']}   extended = {result['extended']}")
        print("="*60)
        pm("Матрица H", result["H"])
        pv("Xk       ", result["Xk"])
        pv("Xr       ", result["Xr"])
        pv("Xn       ", result["Xn"])
        pv("Yn       ", result["Yn"])
        pv("Синдром S", result["S"])
        print(f"Внесённые ошибки в позициях: {result['positions']}")
        print(f"Обнаружена ошибка в позиции: {result['mistake']}")
        pv("Вектор ошибки En", result["En"])
        pv("Исправленное Yn", result["Y_corrected"])
        print("="*60)