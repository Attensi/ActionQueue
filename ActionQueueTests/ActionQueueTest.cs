using ActionQueueSpace;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ActionQueueTests
{
    [TestClass]
    public class ActionQueueTest
    {
        [TestMethod]
        public void TestCanExecuteAction()
        {
            bool isSet = false;
            
            ActionQueue queue = new ActionQueue();
            Task t = queue.AddAction(() =>
            {
                Thread.Sleep(200);
                isSet = true;
                Thread.Sleep(200);
            });

            Assert.AreEqual(TaskStatus.WaitingToRun, t.Status);
            Assert.AreEqual(false, isSet);

            Task.WaitAll(t);

            Assert.AreEqual(TaskStatus.RanToCompletion, t.Status);
            Assert.AreEqual(true, isSet);
        }
    }
}
