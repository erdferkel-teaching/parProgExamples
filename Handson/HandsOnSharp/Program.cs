using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace HandsOnSharp
{
    class Program
    {
        class OOP
        {
            private int member;


            public void DoOOP()
            {
                for(int i=0;i<100;i++)
                member += 1;
            }
        }

        private static int id = -1;
        private static System.Threading.ThreadLocal<int> threadId = new System.Threading.ThreadLocal<int>(() => System.Threading.Interlocked.Increment(ref id));

        public static void ClassOOP()
        {
            var cnt = 10000000;
            var sw = new System.Diagnostics.Stopwatch();
            var cores = System.Environment.ProcessorCount;
            var leaky = new System.Collections.Generic.List<object>();
            var oops = new OOP[cores];
            for (int i = 0; i < oops.Length; i++)
            {
                oops[i] = new OOP();
                for (int garbagge = 0; garbagge < 0; garbagge++) leaky.Add(new OOP[100]);
            }
            sw.Start();
            for(int i=0;i<cnt;i++)
            {
                oops[i%cores].DoOOP();
            }
            sw.Stop();
            var singleThreaded = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine("single threaded took: {0}ms", sw.Elapsed.TotalMilliseconds);

            sw.Restart();
            var p = new ParallelOptions();
            p.MaxDegreeOfParallelism = cores;
            Parallel.ForEach(Partitioner.Create(0, cnt),p, range =>
              {
                  var id = threadId.Value % cores;
                  for (int i=range.Item1;i<range.Item2;i++)
                  {
                      oops[id].DoOOP();
                  }
              });

            sw.Stop();

            var speedup = singleThreaded / sw.Elapsed.TotalMilliseconds;

            Console.WriteLine($"multi threaded took: {sw.Elapsed.TotalMilliseconds}ms (speedup: {speedup}");
            Console.WriteLine(leaky.Count); // prevent optimization
        }

        static void Oversubscription()
        {
            for (int i = 0; i < (Environment.ProcessorCount * 4); i++)
            {
                new Thread(() => {            
                    // Do work             
                    for (int j = 0; j < 1000000000; j++) ;
                }).Start();
            }
        }

        static void Main(string[] args)
        {
            //ClassOOP();
            //Oversubscription();
            TaskParallelism.Test();
        }
    }
}
