#include <omp.h>
#include <stdio.h>

#define NUM_THREADS 4

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

double parallelPi1(long num_steps)
{
	auto step = 1.0 / (double)num_steps;

	omp_set_num_threads(NUM_THREADS);
	double sum[NUM_THREADS];
	int globalNumThreads;
#pragma omp parallel 
	{
		auto id = omp_get_thread_num();
		auto numThreads = omp_get_num_threads();
		if (id == 0) globalNumThreads = numThreads;
		double x;
		sum[id] = 0.0;
		for (int i = id; i < num_steps; i += numThreads)
		{
			x = (i + 0.5)*step;
			sum[id] = sum[id] + 4.0 / (1.0 + x * x);
		}
	}

	double pi = 0.0;
	for (int i = 0; i < globalNumThreads; i++)
	{
		pi = pi + sum[i] * step;
	}
	return pi;
}

double parallelPi2(long num_steps)
{
	auto step = 1.0 / (double)num_steps;

	omp_set_num_threads(NUM_THREADS);
	double sum[NUM_THREADS][8];
	int globalNumThreads;
#pragma omp parallel 
	{
		auto id = omp_get_thread_num();
		auto numThreads = omp_get_num_threads();
		if (id == 0) globalNumThreads = numThreads;
		double x;
		sum[id][0] = 0.0;
		for (int i = id; i < num_steps; i += numThreads)
		{
			x = (i + 0.5)*step;
			sum[id][0] = sum[id][0] + 4.0 / (1.0 + x * x);
		}
	}

	double pi = 0.0;
	for (int i = 0; i < globalNumThreads; i++)
	{
		pi = pi + sum[i][0] * step;
	}
	return pi;
}

double parallelPi3(long num_steps)
{
	auto step = 1.0 / (double)num_steps;

	omp_set_num_threads(NUM_THREADS);
	double pi = 0.0;
	int globalNumThreads;
#pragma omp parallel 
	{
		auto id = omp_get_thread_num();
		auto numThreads = omp_get_num_threads();
		if (id == 0) globalNumThreads = numThreads;
		double x;
		for (int i = id; i < num_steps; i += numThreads)
		{
			x = (i + 0.5)*step;
#pragma omp critical
			pi +=4.0 / (1.0 + x * x);
		}
	}

	return pi*=step;
}

double parallelPi4(long num_steps)
{
	auto step = 1.0 / (double)num_steps;

	omp_set_num_threads(NUM_THREADS);
	double globalSum = 0.0;
	int globalNumThreads;
#pragma omp parallel 
	{
		auto id = omp_get_thread_num();
		auto numThreads = omp_get_num_threads();
		if (id == 0) globalNumThreads = numThreads;
		double x;
		double localSum = 0.0;
		for (int i = id; i < num_steps; i += numThreads)
		{
			x = (i + 0.5)*step;
			localSum = localSum + 4.0 / (1.0 + x * x);
		}
#pragma omp critical
		globalSum += localSum * step;
	}

	return globalSum;
}

double parallelPi5(long num_steps)
{
	auto step = 1.0 / (double)num_steps;

	omp_set_num_threads(NUM_THREADS);
	double globalSum = 0.0;
	int globalNumThreads;
#pragma omp parallel 
	{
		auto id = omp_get_thread_num();
		auto numThreads = omp_get_num_threads();
		if (id == 0) globalNumThreads = numThreads;
		double x;
		double localSum = 0.0;
		for (int i = id; i < num_steps; i += numThreads)
		{
			x = (i + 0.5)*step;
			localSum = localSum + 4.0 / (1.0 + x * x);
		}
		auto normalized = localSum * step;
#pragma omp atomic
		globalSum += normalized;
	}

	return globalSum;
}

double piSerialReduction(long num_steps)
{
	double x, pi, sum = 0.0;

	auto step = 1.0 / (double)num_steps;

#pragma omp parallel for reduction (+:sum)
	for (int i = 0; i < num_steps; i++)
	{
		x = (i + 0.5)*step;
		sum = sum + 4.0 / (1.0 + x*x);
	}
	pi = step * sum;
	return pi;
}

double piSerialReduction2(long num_steps)
{
	double x,pi, sum = 0.0;

	auto step = 1.0 / (double)num_steps;

#pragma omp parallel for reduction (+:sum) private(x)
	for (int i = 0; i < num_steps; i++)
	{
		x = (i + 0.5)*step;
		sum = sum + 4.0 / (1.0 + x*x);
	}
	pi = step * sum;
	return pi;
}

double piSerialReduction3(long num_steps)
{
	double x, pi, sum = 0.0;

	auto step = 1.0 / (double)num_steps;

#pragma omp parallel
	{
		double x = 0;
#pragma omp for reduction (+:sum) schedule(static)
		for (int i = 0; i < num_steps; i++)
		{
			x = (i + 0.5)*step;
			sum = sum + 4.0 / (1.0 + x*x);
		}
	}
	pi = step * sum;
	return pi;
}

void testPi()
{
	const int cnt = 100000000;
	double pi, start;
	start = omp_get_wtime();
	pi = piSerial(cnt);
	printf("serial pi: %f in %f\n", pi, omp_get_wtime() - start);

	start = omp_get_wtime();
	pi = parallelPi1(cnt);
	printf("parallel pi: %f in %f\n", pi, omp_get_wtime() - start);

	start = omp_get_wtime();
	pi = parallelPi2(cnt);
	printf("parallel pi: %f in %f\n", pi, omp_get_wtime() - start);

	//start = omp_get_wtime();
	//pi = parallelPi3(cnt);
	//printf("parallel pi critical: %f in %f\n", pi, omp_get_wtime() - start);

	start = omp_get_wtime();
	pi = parallelPi4(cnt);
	printf("parallel pi critical: %f in %f\n", pi, omp_get_wtime() - start);

	start = omp_get_wtime();
	pi = parallelPi5(cnt);
	printf("parallel pi atomic: %f in %f\n", pi, omp_get_wtime() - start);

	start = omp_get_wtime();
	pi = piSerialReduction(cnt);
	printf("parallel pi reduction: %f in %f\n", pi, omp_get_wtime() - start);

	start = omp_get_wtime();
	pi = piSerialReduction2(cnt);
	printf("parallel pi reduction 2: %f in %f\n", pi, omp_get_wtime() - start);

	start = omp_get_wtime();
	pi = piSerialReduction3(cnt);
	printf("parallel pi reduction 3: %f in %f\n", pi, omp_get_wtime() - start);
}


int main()
{
	//helloWorld();
	testPi();
	testPi();
    return 0;
}

