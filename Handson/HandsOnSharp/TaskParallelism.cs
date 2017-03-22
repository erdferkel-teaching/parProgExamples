using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HandsOnSharp
{
    static class TaskParallelism
    {
        public class Tree<T>
        {
            public T Data;
            public Tree<T> Left, Right;
        }

        public static Tree<int> CreateTree(int depth, System.Random r)
        {
            if (depth <= 0)
            {
                var t = new Tree<int>();
                t.Left = null;
                t.Right = null;
                t.Data = r.Next(8, 18);
                return t;
            }
            else
            {
                var t = new Tree<int>();
                t.Left = CreateTree(depth - 1, r);
                t.Right = CreateTree(depth - 1, r);
                t.Data = r.Next(5, 15);
                return t;
            }
        }

        public static int Fib(int n)
        {
            if (n <= 1) return n;
            return Fib(n - 1) + Fib(n - 2);
        }

        public static void WalkSequential<T>(Tree<T> root, Action<T> action)
        {
            if (root == null) return;
            action(root.Data);
            WalkSequential(root.Left, action);
            WalkSequential(root.Right, action);
        }


        public static void Test()
        {
            for (int i = 0; i < 15; i++) Console.WriteLine("fib({0}):{1}", i, Fib(i));
            var t = CreateTree(15, new System.Random());
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            WalkSequential(t, v => Fib(v));
            sw.Stop();
            Console.WriteLine($"Sequential took: {sw.Elapsed.TotalMilliseconds}");


        }
    }
}
