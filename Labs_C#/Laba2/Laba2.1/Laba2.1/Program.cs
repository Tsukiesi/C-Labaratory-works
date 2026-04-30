using System;
using System.Diagnostics;
using System.Threading;

// Код выполнен студентом: Возбранюк Артем

namespace PrimeCounterThreads
{
    class Program
    {
        const int RangeEnd = 10000;
        const int ThreadCount = 4;
        static int totalPrimeCount = 0;

        static readonly object monitorLock = new object();
        static Mutex mutex = new Mutex();
        static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        static bool IsPrime(int n)
        {
            if (n < 2) return false;
            for (int i = 2; i <= Math.Sqrt(n); i++)
                if (n % i == 0) return false;
            return true;
        }

        static void SyncMonitor(Action action) { lock (monitorLock) action(); }

        static void SyncMutex(Action action) { mutex.WaitOne(); action(); mutex.ReleaseMutex(); }

        static void SyncSemaphore(Action action) { semaphore.Wait(); action(); semaphore.Release(); }

        static void ThreadWorker(int start, int end, int threadId, Action<Action> syncWrapper)
        {
            for (int i = start; i <= end; i++)
            {
                bool found = IsPrime(i);
                if (found || i % 100 == 0)
                {
                    syncWrapper(() =>
                    {
                        Console.SetCursorPosition(0, threadId);
                        Console.Write($"Поток {threadId} | Число: {i,-5}");
                        if (found)
                        {
                            totalPrimeCount++;
                            Console.SetCursorPosition(30, threadId);
                            Console.Write($"| Последнее простое число: {i,-5}");
                        }
                    });
                }
            }
        }

        static void Execute(string title, Action<Action> syncWrapper)
        {
            Console.Clear();
            Console.WriteLine($"=================={title}=================");
            Console.WriteLine("Статус потоков:");

            totalPrimeCount = 0;
            Thread[] threads = new Thread[ThreadCount];
            int step = RangeEnd / ThreadCount;
            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < ThreadCount; i++)
            {
                int start = i * step + 1;
                int end = (i == ThreadCount - 1) ? RangeEnd : (i + 1) * step;
                int id = i + 1;
                threads[i] = new Thread(() => ThreadWorker(start, end, id, syncWrapper));
                threads[i].Start();
            }

            foreach (var t in threads) t.Join();
            sw.Stop();
            Console.SetCursorPosition(0, ThreadCount + 1);
            Console.WriteLine(new string('=', 61));
            Console.WriteLine($"Результат: {totalPrimeCount} простых чисел");
            Console.WriteLine($"Время выполнения: {sw.ElapsedMilliseconds} мс");
            Console.WriteLine("Нажмите любую клавишу для перехода к следующей версии...");
            Console.ReadKey();
        }

        static void Main()
        {
            Execute(" Версия 1 (Monitor/Lock) =", SyncMonitor);
            Execute(" Версия 2 (Mutex) ========", SyncMutex);
            Execute(" Версия 3 (SemaphoreSlim) ", SyncSemaphore);
        }
    }
}
