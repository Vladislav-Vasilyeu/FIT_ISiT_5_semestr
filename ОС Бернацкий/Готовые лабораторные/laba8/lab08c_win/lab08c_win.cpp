#include <windows.h>
#include <stdio.h>
#include <processthreadsapi.h>
#include <locale.h>

void pause() { printf("\nEnter...\n"); getchar(); system("cls"); }

int main()
{
	setlocale(LC_ALL, "Russian");
    printf("PID процесса: %lu\n\n", GetCurrentProcessId());

    const int N = 10;
    const size_t SZ = 512 * 1024;
    void* blocks[10] = { 0 };

    printf("[1] Создаём кучу 1→8 MiB\n");
    HANDLE h = HeapCreate(0, 1 * 1024 * 1024, 8 * 1024 * 1024);
    if (!h) { printf("HeapCreate fail %lu\n", GetLastError()); return 1; }
    pause();

    for (int i = 0; i < N; i++) {
        printf("[2.%02d] Выделяем %6zu KiB\n", i + 1, SZ / 1024);
        blocks[i] = HeapAlloc(h, 0, SZ);
        if (!blocks[i]) { printf("fail\n"); break; }
        printf("→ %p\n", blocks[i]);
        pause();
    }

    printf("[3] Заполняем\n"); for (int i = 0; i < N; i++) if (blocks[i]) for (size_t j = 0; j < SZ / 4; j++) ((int*)blocks[i])[j] = j;
    pause();

    printf("[4] Освобождаем\n"); for (int i = 0; i < N; i++) if (blocks[i]) HeapFree(h, 0, blocks[i]);
    pause();

    printf("[5] Уничтожаем кучу\n"); HeapDestroy(h);
    pause();

    return 0;
}