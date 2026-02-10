#include <stdio.h>
#include <windows.h>
#include <processthreadsapi.h>
#include <locale.h>

int global_init = 123456;
int global_uninit;

static int static_global_init = 777;
static int static_global_uninit;

void some_function() { printf("Я просто функция\n"); }

int main(int argc, char* argv[])
{
	setlocale(LC_ALL, "Russian");

    printf("PID процесса: %lu\n\n", GetCurrentProcessId());

    int local_init = 42;
    int local_uninit;
    static int static_local_init = 999;
    static int static_local_uninit;

    printf("global_init          = %p\n", &global_init);
    printf("global_uninit        = %p\n", &global_uninit);
    printf("static_global_init   = %p\n", &static_global_init);
    printf("static_global_uninit = %p\n\n", &static_global_uninit);

    printf("static_local_init    = %p\n", &static_local_init);
    printf("static_local_uninit  = %p\n\n", &static_local_uninit);

    printf("local_init           = %p\n", &local_init);
    printf("local_uninit         = %p\n\n", &local_uninit);

    printf("some_function        = %p\n", some_function);
    printf("main                 = %p\n\n", main);

    printf("argc                 = %p\n", &argc);
    printf("argv                 = %p   → %p\n", &argv, argv[0]);

    printf("\nНажмите Enter для выхода...\n");
    getchar();
    return 0;
}