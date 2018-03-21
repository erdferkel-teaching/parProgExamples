using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandsOnSharp
{
    public class Quicksort
    {
        public const int THRESHOLD = 256;
        public const int PARALLEL_THRESHOLD = 4096;


        private static void Swap<T>(T[] arr, int i, int j)
        {
            T hold = arr[i];
            arr[i] = arr[j];
            arr[j] = hold;
        }

        private static int Partition<T>(T[] arr, int low, int high) where T : IComparable<T>
        {
            if (low == high)
            {
                return high;
            }

            int pivot = low;
            int i = low + 1;
            int j = high;

            while (true)
            {
                while ((arr[i].CompareTo(arr[pivot]) < 0) && i < high) i++;
                while ((arr[j].CompareTo(arr[pivot]) > 0) && j > low) j--;
                if (j <= i)
                {
                    break;
                }
                Swap(arr, i, j);
            }
            Swap(arr, pivot, j);
            return j;
        }
        private static void QuicksortSequential<T>(T[] arr, int left, int right)
                   where T : IComparable<T>
        {
            if (right > left && right - left < 2048)
            {
                Array.Sort(arr, left, right - left + 1);
            }
            else if (right > left)
            {
                int pivot = Partition(arr, left, right);
                QuicksortSequential(arr, left, pivot - 1);
                QuicksortSequential(arr, pivot + 1, right);
            }
        }

        private static void QuicksortParallel<T>(T[] arr, int left, int right)
            where T : IComparable<T>
        {
            if (right > left)
            {
                if (right > left && right - left < 2048)
                {
                    Array.Sort(arr, left, right - left + 1);
                }
                else if (right - left < PARALLEL_THRESHOLD)
                {
                    QuicksortSequential(arr, left, right);
                }
                else
                {
                    int pivot = Partition(arr, left, right);
                    Parallel.Invoke(new Action[] { delegate {QuicksortParallel(arr, left, pivot - 1); },
                                                   delegate {QuicksortParallel(arr, pivot + 1, right); }
                    });
                }
            }
        }

        public static void Run()
        {
            //var rnd = new System.Random();
            //var input = Enumerable.Range(0, 400000).Select(_ => rnd.Next(500000000)).ToArray();

            var inFile = System.IO.File.OpenText(@"..\..\numbers2");
            var input = new int[390000];
            for (int i = 0; i < 390000; i++)
            {
                var line = inFile.ReadLine();
                input[i] = Int32.Parse(line);
            }

            for (int i=0;i<10;i++)
            {
                var input2 = input.ToArray();
                QuicksortSequential(input2, 0, input2.Length-1);
                for(int j = 0;j<input2.Length-1;j++)
                {
                    if (input2[j] > input2[j + 1]) throw new Exception();
                }
                Console.WriteLine("warming up..");
            }

            var sw = new System.Diagnostics.Stopwatch();
            for (int i = 0; i < 10; i++)
            {
                var input2 = input.ToArray();
                GC.Collect();
                sw.Start();
                QuicksortSequential(input2, 0, input2.Length-1);
                sw.Stop();
            }
            var sequential = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine("sequential took: {0}", sw.Elapsed.TotalMilliseconds / 100.0);

            for (int i = 0; i < 10; i++)
            {
                var input2 = input.ToArray();
                QuicksortParallel(input2, 0, input2.Length-1);
                for (int j = 0; j < input2.Length - 1; j++)
                {
                    if (input2[j] > input2[j + 1]) throw new Exception();
                }
                Console.WriteLine("warming up..");
            }

            sw = new System.Diagnostics.Stopwatch();
            for (int i = 0; i < 10; i++)
            {
                var input2 = input.ToArray();
                GC.Collect();
                sw.Start();
                QuicksortParallel(input2, 0, input2.Length-1);
                sw.Stop();
            }
            Console.WriteLine("paralllel took: {0}", sw.Elapsed.TotalMilliseconds / 100.0);
            Console.WriteLine("speedup: {0}", sequential / sw.Elapsed.TotalMilliseconds);
            Console.ReadLine();

            System.Environment.Exit(0);
        }
    }
}
