using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace HandsOnSharp
{
    class TowardsParallelLoop
    {
        public static void SerialFor(int count)
        {
            var arr = new double[count];
            var target = new double[count];
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < arr.Length; i++)
            {
                target[i] = arr[i] + 2;
            }
            sw.Stop();
            Console.WriteLine($"loop took: {sw.Elapsed.TotalMilliseconds}");
        }

        public static void ParalellFor(int count)
        {
            var arr = new double[count];
            var target = new double[count];
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Parallel.For(0, arr.Length, i =>
            {
                target[i] = arr[i] + 2;
            });
            sw.Stop();
            Console.WriteLine($"loop took: {sw.Elapsed.TotalMilliseconds}");
        }

        public static void MyParallelFor1(int inclusiveLowerBound, int exclusiveUpperBound, Action<int> body)
        {
            throw new NotImplementedException();
        }

        public static void MyParallelFor(int inclusiveLowerBound, int exclusiveUpperBound, Action<int> body)
        {
            // Determine the number of iterations to be processed, the number of     
            // cores to use, and the approximate number of iterations to process      
            // in each thread.     
            int size = exclusiveUpperBound - inclusiveLowerBound;
            int numProcs = Environment.ProcessorCount;
            int range = size / numProcs;

            // Use a thread for each partition. Create them all,     
            // start them all, wait on them all.     
            var threads = new List<Thread>(numProcs);
            for (int p = 0; p < numProcs; p++)
            {
                int start = p * range + inclusiveLowerBound;
                int end = (p == numProcs - 1) ? exclusiveUpperBound : start + range;
                threads.Add(new Thread(() =>
                {
                    for (int i = start; i < end; i++) body(i);
                }));
            }
            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join();
        }

        public static void MyParallelFor2(int inclusiveLowerBound, int exclusiveUpperBound, Action<int> body)
        {
            // Determine the number of iterations to be processed, the number of     
            // cores to use, and the approximate number of iterations to process in      
            // each thread.     
            int size = exclusiveUpperBound - inclusiveLowerBound;
            int numProcs = Environment.ProcessorCount;
            int range = size / numProcs;

            // Keep track of the number of threads remaining to complete.     
            int remaining = numProcs;
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                // Create each of the threads.         
                for (int p = 0; p < numProcs; p++)
                {
                    int start = p * range + inclusiveLowerBound;
                    int end = (p == numProcs - 1) ? exclusiveUpperBound : start + range;
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        for (int i = start; i < end; i++) body(i);
                        if (Interlocked.Decrement(ref remaining) == 0) mre.Set();
                    });
                }
                // Wait for all threads to complete.         
                mre.WaitOne();
            }
        }

        public static void MyParallelFor3(int inclusiveLowerBound, int exclusiveUpperBound, Action<int> body)
        {
            // Get the number of processors, initialize the number of remaining       
            // threads, and set the starting point for the iteration.     
            int numProcs = Environment.ProcessorCount;
            int remainingWorkItems = numProcs;
            int nextIteration = inclusiveLowerBound;

            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                // Create each of the work items.        
                for (int p = 0; p < numProcs; p++)
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        int index;
                        while ((index = Interlocked.Increment(ref nextIteration) - 1) < exclusiveUpperBound)
                        {
                            body(index);
                        }
                        if (Interlocked.Decrement(ref remainingWorkItems) == 0)
                            mre.Set();
                    });
                }

                // Wait for all threads to complete.         
                mre.WaitOne();
            }
        }


        public static void MyParallelFor4(int inclusiveLowerBound, int exclusiveUpperBound, Action<int> body)
        {
            // Get the number of processors, initialize the number of remaining      
            // threads, and set the starting point for the iteration.     
            int numProcs = Environment.ProcessorCount;
            int remainingWorkItems = numProcs;
            int nextIteration = inclusiveLowerBound;
            const int batchSize = 3;

            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                // Create each of the work items.         
                for (int p = 0; p < numProcs; p++)
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        int index;
                        while ((index = Interlocked.Add(ref nextIteration, batchSize) - batchSize) < exclusiveUpperBound)
                        {
                            // In a real implementation, we’d need to handle                     
                            // overflow on this arithmetic.                     
                            int end = index + batchSize;
                            if (end >= exclusiveUpperBound) end = exclusiveUpperBound;
                            for (int i = index; i < end; i++) body(i);
                        }
                        if (Interlocked.Decrement(ref remainingWorkItems) == 0)
                            mre.Set();
                    });
                }

                // Wait for all threads to complete         
                mre.WaitOne();
            }
        }
    }
}