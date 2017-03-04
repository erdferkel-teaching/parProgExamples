#include <omp.h>
#include <stdio.h>

void helloWorld()
{
	omp_set_num_threads(4);
#pragma omp parallel
	{
		int ID = omp_get_thread_num();
		printf("hello(%d)", ID);
		printf(" world(%d)", ID);
	}
}


int main()
{
	//helloWorld();
    return 0;
}

