using System;
using System.Collections.Generic;
using System.Threading;

namespace GZipConsoleApp
{
    public class ProducerConsumerQueue<T> : IDisposable where T : class
    {
        private readonly object _locker = new object();
        private readonly Thread[] _workers;
        private readonly Queue<T> _tasks = new Queue<T>();

        public ProducerConsumerQueue(int workersCount)
        {
            _workers = new Thread[workersCount];

            for (int i = 0; i < _workers.Length; i++)
                (_workers[i] = new Thread(Consume)).Start();
        }

        private void Consume()
        {
            while (true)
            {
                T task;
                lock (_locker)
                {
                    while (_tasks.Count == 0)
                    {
                        Monitor.Wait(_locker);
                    }

                    task = _tasks.Dequeue();
                }

                if (task == null)
                {
                    return;
                }

                //execute
            }
        }

        public void EnqueueTask(T task)
        {
            lock (_locker)
            {
                _tasks.Enqueue(task);
                Monitor.PulseAll(_locker);
            }
        }

        public void Dispose()
        {
            foreach (Thread _ in _workers)
            {
                EnqueueTask(null);
            }

            foreach (Thread worker in _workers)
            {
                worker.Join();
            }
        }
    }
}