#define _CRT_SECURE_NO_WARNINGS
#include <windows.h>
#include <stdio.h>
#include <stdlib.h>

int main(int argc, char* argv[]) {
    int iterations = 0;
    
    if (argc > 1) {
        iterations = atoi(argv[1]);
    }
    else {
        char* env = getenv("ITER_NUM");
        if (env) iterations = atoi(env);
    }
    if (iterations <= 0) {
        printf("Error: No iterations specified.\n");
        ExitProcess(1);
    }
    printf("Iterations: %d\n", iterations);
    for (int i = 0; i < iterations; i++) {
        printf("Iteration %d, PID: %d\n", i + 1, GetCurrentProcessId());
        Sleep(500);
    }
    return 0;
}