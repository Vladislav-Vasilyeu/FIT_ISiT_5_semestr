#include <windows.h>
#include <stdio.h>
#include <processthreadsapi.h>
#include <locale.h>

int main()
{
	setlocale(LC_ALL, "Russian");
    printf("PID процесса: %lu\n\n", GetCurrentProcessId());

    SYSTEM_INFO si; GetSystemInfo(&si);
    SIZE_T page = si.dwPageSize;
    const SIZE_T TOTAL = 256, COMMIT = 128;
    SIZE_T total_size = TOTAL * page, commit_size = COMMIT * page;

    printf("Страница: %zu байт\n\n", page);

    printf("[1] Резерв 256 страниц... Enter\n"); getchar();
    void* base = VirtualAlloc(NULL, total_size, MEM_RESERVE, PAGE_NOACCESS);
    if (!base) { printf("Ошибка: %lu\n", GetLastError()); return 1; }
    printf("Адрес: %p\n\n", base);

    printf("[2] Коммит 128 страниц... Enter\n"); getchar();
    void* ca = (char*)base + commit_size;
    if (!VirtualAlloc(ca, commit_size, MEM_COMMIT, PAGE_READWRITE)) {
        printf("COMMIT fail: %lu\n", GetLastError()); return 1;
    }

    printf("[3] Заполняем... Enter\n"); getchar();
    for (SIZE_T i = 0; i < commit_size / 4; i++) ((int*)ca)[i] = i;

    printf("[4] Только чтение... Enter\n"); getchar();
    DWORD old; VirtualProtect(ca, commit_size, PAGE_READONLY, &old);

    printf("[5] Decommit второй половины... Enter\n"); getchar();
    VirtualFree(ca, commit_size, MEM_DECOMMIT);

    printf("[6] Освободить всё... Enter\n"); getchar();
    VirtualFree(base, 0, MEM_RELEASE);

    printf("Готово\n"); getchar();
    return 0;
}