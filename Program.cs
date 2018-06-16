using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AtomicLock
{
    class Program
    {
        private static ALock _lock = new ALock();

        private static volatile int counter = 0;

        static void Main(string[] args)
        {
            var threads = Enumerable.Range(1, 10).Select(_ => CreateThread()).ToList();

            threads.ForEach(t => t.Join());

            Console.WriteLine($"expected: {threads.Count * 100000}, actual: {counter}");
        }

        private static Thread CreateThread()
        {
            var thread = new Thread(ThreadMain);
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }

        private static void ThreadMain()
        {
            for (int i = 0; i < 100000; i++)
            {
                using (_lock.Lock())
                {
                    counter++;
                }
            }
        }
    }
}
