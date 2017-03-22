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

        public static void Walk1<T>(Tree<T> root, Action<T> action)
        {
            if (root == null) return;
            Parallel.Invoke(
                    () => action(root.Data),
                    () => Walk1(root.Left, action),
                    () => Walk1(root.Right, action)
            );
        }

        public static void Walk2<T>(Tree<T> root, Action<T> action)
        {
            if (root == null) return;

            var t1 = Task.Factory.StartNew(() => action(root.Data));
            var t2 = Task.Factory.StartNew(() => Walk2(root.Left, action));
            var t3 = Task.Factory.StartNew(() => Walk2(root.Right, action));

            Task.WaitAll(t1, t2, t3);
        }

        public static void Walk3<T>(Tree<T> root, Action<T> action)
        {
            if (root == null) return;

            var t1 = Task.Factory.StartNew(() => action(root.Data), TaskCreationOptions.AttachedToParent);
            var t2 = Task.Factory.StartNew(() => Walk3(root.Left, action), TaskCreationOptions.AttachedToParent);
            var t3 = Task.Factory.StartNew(() => Walk3(root.Right, action), TaskCreationOptions.AttachedToParent);

            Task.WaitAll(t1, t2, t3);
        }

        public static Task Walk4<T>(Tree<T> root, Action<T> action)
        {
            return Task.Factory.StartNew(() =>
            {
                if (root == null) return;
                action(root.Data);
                WalkSequential(root.Left, action);
                WalkSequential(root.Right, action);
            }, TaskCreationOptions.AttachedToParent);
        }

        private static Task _completedTask = ((Func<Task>)(() => { var tcs = new TaskCompletionSource<object>(); tcs.SetResult(null); return tcs.Task; }))();

        public static Task Walk5<T>(Tree<T> root, Action<T> action)
        {
            if (root == null) return _completedTask;
            Task t1 = Task.Factory.StartNew(() => action(root.Data));
            Task<Task> t2 = Task.Factory.StartNew(() => Walk5(root.Left, action));
            Task<Task> t3 = Task.Factory.StartNew(() => Walk5(root.Right, action));
            return Task.Factory.ContinueWhenAll(
                new Task[] { t1, t2.Unwrap(), t3.Unwrap() },
                tasks => Task.WaitAll(tasks));
        }

        public static Task ContinueWhenAll(this TaskFactory factory, params Task[] tasks)
        {
            return factory.ContinueWhenAll(tasks, completed => Task.WaitAll(completed));
        }

        public static Task Walk6<T>(Tree<T> root, Action<T> action)
        {
            if (root == null) return _completedTask;
            var t1 = Task.Factory.StartNew(() => action(root.Data));
            var t2 = Task.Factory.StartNew(() => Walk6(root.Left, action));
            var t3 = Task.Factory.StartNew(() => Walk6(root.Right, action));
            return ContinueWhenAll(Task.Factory, t1, t2.Unwrap(), t3.Unwrap());
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

            sw.Restart();
            Walk1(t, v => Fib(v));
            sw.Stop();
            Console.WriteLine($"Parallel.Invoke took: {sw.Elapsed.TotalMilliseconds}");

            sw.Restart();
            Walk2(t, v => Fib(v));
            sw.Stop();
            Console.WriteLine($"StartNew took: {sw.Elapsed.TotalMilliseconds}");

            sw.Restart();
            Walk3(t, v => Fib(v));
            sw.Stop();
            Console.WriteLine($"StartNew (*3) per level: {sw.Elapsed.TotalMilliseconds}");

            sw.Restart();
            Walk4(t, v => Fib(v)).Wait();
            sw.Stop();
            Console.WriteLine($"One Task per level: {sw.Elapsed.TotalMilliseconds}");

            sw.Restart();
            Walk5(t, v => Fib(v)).Wait();
            sw.Stop();
            Console.WriteLine($"Nested continuation: {sw.Elapsed.TotalMilliseconds}");

            sw.Restart();
            Walk6(t, v => Fib(v)).Wait();
            sw.Stop();
            Console.WriteLine($"Nested continuation 2: {sw.Elapsed.TotalMilliseconds}");
        }
    }
}
