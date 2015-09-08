using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ActionQueueSpace
{
    public class ActionQueue
    {
        /*
        Usage:
        
            ActionQueue queue = new ActionQueue(1);

            queue.addAction(() => {
                // Code to run
            });
        
        */
        private TaskFactory factory;
        private CancellationTokenSource cts = new CancellationTokenSource();
        

        /* Constructors */
        public ActionQueue() : this(1) { }

        public ActionQueue(int ConcurrancyLevel)
        {
            var lcts = new LimitedConcurrencyLevelTaskScheduler(ConcurrancyLevel);
            factory = new TaskFactory(lcts);
        }

        /* Destructor */
        ~ActionQueue()
        {
            cts.Dispose();
        }

        /* Public interface */
        public Task AddAction(Action action)
        {
            return factory.StartNew(action, cts.Token);
        }

        public List<Task> AddActions(List<Action> actions)
        {
            var tasks = new List<Task>();
            foreach (var action in actions)
            {
                var task = this.AddAction(action);
                tasks.Add(task);
            }
            return tasks;
        }

        /* Private class */
        private class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
        {
            [ThreadStatic]
            private static bool _currentThreadIsProcessingItems;
            private readonly LinkedList<Task> _tasks = new LinkedList<Task>();
            private readonly int _maxDegreeOfParallelism;
            private int _delegatesQueuedOrRunning = 0;

            public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
            {
                if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
                _maxDegreeOfParallelism = maxDegreeOfParallelism;
            }

            protected sealed override void QueueTask(Task task)
            {
                lock (_tasks)
                {
                    _tasks.AddLast(task);
                    if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                    {
                        ++_delegatesQueuedOrRunning;
                        NotifyThreadPoolOfPendingWork();
                    }
                }
            }

            private void NotifyThreadPoolOfPendingWork()
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ =>
                {
                    _currentThreadIsProcessingItems = true;
                    try
                    {
                        while (true)
                        {
                            Task item;
                            lock (_tasks)
                            {
                                if (_tasks.Count == 0)
                                {
                                    --_delegatesQueuedOrRunning;
                                    break;
                                }

                                item = _tasks.First.Value;
                                _tasks.RemoveFirst();
                            }

                            base.TryExecuteTask(item);
                        }
                    }
                    finally { _currentThreadIsProcessingItems = false; }
                }, null);
            }

            protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                if (!_currentThreadIsProcessingItems) return false;

                if (taskWasPreviouslyQueued)
                    if (TryDequeue(task))
                        return base.TryExecuteTask(task);
                    else
                        return false;
                else
                    return base.TryExecuteTask(task);
            }

            protected sealed override bool TryDequeue(Task task)
            {
                lock (_tasks) return _tasks.Remove(task);
            }

            public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

            protected sealed override IEnumerable<Task> GetScheduledTasks()
            {
                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(_tasks, ref lockTaken);
                    if (lockTaken) return _tasks;
                    else throw new NotSupportedException();
                }
                finally
                {
                    if (lockTaken) Monitor.Exit(_tasks);
                }
            }
        }
    }
}
