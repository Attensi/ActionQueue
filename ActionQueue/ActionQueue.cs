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
    }
}
