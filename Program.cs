using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
            var threads = Enumerable.Range(1, 10).Select(threadId => CreateThread(threadId)).ToList();

            Stopwatch stopWatch = new Stopwatch();

            stopWatch.Start();

            threads.ForEach(t => t.Join());

            stopWatch.Stop();

            Console.WriteLine($"expected: {threads.Count * 100000}, actual: {counter}, elapsed: {stopWatch.ElapsedMilliseconds} ms");
        }

        private static Thread CreateThread(int threadId)
        {
            var thread = new Thread(ThreadMain);
            thread.IsBackground = true;
            thread.Start(threadId);
            return thread;
        }

        private static void ThreadMain(object _threadId)
        {
            Thread.BeginThreadAffinity();

            try
            {
                int threadId = (int)_threadId;

                var processorsCount = Environment.ProcessorCount;

                if(processorsCount > 1)
                {
                    foreach (ProcessThread thread in Process.GetCurrentProcess().Threads)
                    {
                        if (thread.Id == GetCurrentThreadId())
                        {
                            var processorNum = threadId >= processorsCount ? threadId % processorsCount : threadId;
                            thread.ProcessorAffinity = (IntPtr)(1L << processorNum);
                        }
                    }
                }

                for (int i = 0; i < 100000; i++)
                {
                    using (_lock.Lock(threadId))
                    {
                        counter++;
                    }
                }
            }
            finally
            {
                Thread.EndThreadAffinity();
            }
        }

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();
    }
}
