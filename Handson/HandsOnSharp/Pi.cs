
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;

public class Pi
{
    const int num_steps = 1000000000;

    /// <summary>Main method to time various implementations of computing PI.</summary>
    public static void TestPi(string[] args)
    {
        while (true)
        {
            Time(() => SerialLinqPi(), "SerialLinq");
            Time(() => ParallelLinqPi(), "parallelLinq");
            var s = Time(() => SerialPi(), "serialPI");
            Time(() => ParallelPi(), "parallePi");
            var p = Time(() => ParallelPartitionerPi(), "parallelPiPartioner");

            var f = Time(() => ParallelPiFalseSharing(), "falseSharing");
            Console.WriteLine("speedup: " + s / p);

            Console.WriteLine("----");
            Console.ReadLine();
        }
    }

    /// <summary>Times the execution of a function and outputs both the elapsed time and the function's result.</summary>
    static double Time<T>(Func<T> work, string version)
    {
        var sw = Stopwatch.StartNew();
        var result = work();
        Console.WriteLine("[{0}] sw.Elapsed: {1}ms ~> {2}", version, sw.Elapsed.TotalMilliseconds, result);
        return sw.Elapsed.TotalMilliseconds;
    }

    /// <summary>Estimates the value of PI using a LINQ-based implementation.</summary>
    static double SerialLinqPi()
    {
        double step = 1.0 / (double)num_steps;
        return (from i in Enumerable.Range(0, num_steps)
                let x = (i + 0.5) * step
                select 4.0 / (1.0 + x * x)).Sum() * step;
    }

    /// <summary>Estimates the value of PI using a PLINQ-based implementation.</summary>
    static double ParallelLinqPi()
    {
        double step = 1.0 / (double)num_steps;
        return (from i in ParallelEnumerable.Range(0, num_steps)
                let x = (i + 0.5) * step
                select 4.0 / (1.0 + x * x)).Sum() * step;
    }

    /// <summary>Estimates the value of PI using a for loop.</summary>
    static double SerialPi()
    {
        double sum = 0.0;
        double step = 1.0 / (double)num_steps;
        for (int i = 0; i < num_steps; i++)
        {
            double x = (i + 0.5) * step;
            sum = sum + 4.0 / (1.0 + x * x);
        }
        return step * sum;
    }

    /// <summary>Estimates the value of PI using a Parallel.For.</summary>
    static double ParallelPi()
    {
        double sum = 0.0;
        double step = 1.0 / (double)num_steps;
        object monitor = new object();
        Parallel.For(0, num_steps, () => 0.0, (i, state, local) =>
        {
            double x = (i + 0.5) * step;
            return local + 4.0 / (1.0 + x * x);
        }, local => { lock (monitor) sum += local; });
        return step * sum;
    }


    static double ParallelPiFalseSharing()
    {
        double step = 1.0 / (double)num_steps;

        var options = new ParallelOptions();
        var threadCount = 4;
        options.MaxDegreeOfParallelism = threadCount;
        var sums = new double[threadCount];

        Parallel.ForEach(Partitioner.Create(0, 4), options, (threadId) =>
        {
            for (int i = threadId.Item1; i < num_steps; i += threadCount)
            {
                double x = (i + 0.5) * step;
                sums[threadId.Item1] += 4.0 / (1.0 + x * x);
            }
        });

        double result = 0.0;
        for (int i = 0; i < threadCount; i++)
        {
            result = result + sums[i] * step;
        }
        return result;
    }

    /// <summary>Estimates the value of PI using a Parallel.ForEach and a range partitioner.</summary>
    static double ParallelPartitionerPi()
    {
        double sum = 0.0;
        double step = 1.0 / (double)num_steps;
        object monitor = new object();
        Parallel.ForEach(Partitioner.Create(0, num_steps), () => 0.0, (range, state, local) =>
        {
            for (int i = range.Item1; i < range.Item2; i++)
            {
                double x = (i + 0.5) * step;
                local += 4.0 / (1.0 + x * x);
            }
            return local;
        }, local => { lock (monitor) sum += local; });
        return step * sum;
    }

}