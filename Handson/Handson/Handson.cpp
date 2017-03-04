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

double piSerial(long num_steps)
{
	int i;
	double x, pi, sum = 0.0;
	
	auto step = 1.0 / (double)num_steps;

	for (i = 0; i < num_steps; i++)
	{
		x = (i + 0.5)*step;
		sum = sum + 4.0 / (1.0 + x*x);
	}
	pi = step * sum;
	return pi;
}


void testPi()
{
	double pi, start;
	start = omp_get_wtime();
	pi = piSerial(10000000);
	printf("serial pi: %f in %f\n", pi, omp_get_wtime() - start);
}


int main()
{
	//helloWorld();
	testPi();
    return 0;
}

