using System;
using System.Collections.Generic;
using System.Threading;

namespace GZipConsoleApp
{
    public class ProducerConsumerQueue<T> where T : class
    {
        private readonly Action<T> _consumeAction;
        private readonly object _locker = new object();
        private readonly Thread[] _workers;
        private readonly Queue<T> _tasks = new Queue<T>();

        public ProducerConsumerQueue(int workersCount, Action<T> consumeAction)
        {
            _consumeAction = consumeAction;
            _workers = new Thread[workersCount];

            for (var i = 0; i < _workers.Length; i++)
            {
                (_workers[i] = new Thread(Consume)).Start();
            }
        }

        private void Consume()
        {
            while (true)
            {
                T task;
                var lockWasTaken = false;
                try
                {
                    Monitor.Enter(_locker, ref lockWasTaken);
                    while (_tasks.Count == 0)
                    {
                        Monitor.Wait(_locker);
                    }

                    task = _tasks.Dequeue();
                }
                finally
                {
                    if (lockWasTaken)
                    {
                        Monitor.Exit(_locker);
                    }
                }

                if (task == null)
                {
                    return;
                }

                _consumeAction.Invoke(task);
            }
        }

        public void Enqueue(T task)
        {
            var lockWasTaken = false;
            try
            {
                Monitor.Enter(_locker, ref lockWasTaken);
                _tasks.Enqueue(task);
                Monitor.Pulse(_locker);
            }
            finally
            {
                if (lockWasTaken)
                {
                    Monitor.Exit(_locker);
                }
            }
        }

        public void Stop()
        {
            foreach (Thread _ in _workers)
            {
                Enqueue(null);
            }

            foreach (Thread worker in _workers)
            {
                worker.Join();
            }
        }
    }
}