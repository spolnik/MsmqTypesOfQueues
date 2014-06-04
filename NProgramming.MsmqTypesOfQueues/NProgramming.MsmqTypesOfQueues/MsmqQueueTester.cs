using System;
using System.Diagnostics;
using System.Messaging;

namespace NProgramming.MsmqTypesOfQueues
{
    class MsmqQueueTester
    {
        private const string DefaultQueuePath = @".\private$\default-queue";
        private const string DurableQueuePath = @".\private$\durable-queue";
        private const string TransactionalQueuePath = @".\private$\transactional-queue";

        private const int Count = 1000;

        static void Main()
        {
            Setup(DefaultQueuePath);
            Setup(DurableQueuePath);
            Setup(TransactionalQueuePath, true);

            var queue = new MessageQueue(DefaultQueuePath);
            Process(queue, DefaultQueuePath);

            queue = new MessageQueue(DurableQueuePath)
            {
                DefaultPropertiesToSend = {Recoverable = true}
            };

            Process(queue, DurableQueuePath);

            MessageQueueTransaction transaction;

            queue = new MessageQueue(TransactionalQueuePath);

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < Count; i++)
            {
                transaction = new MessageQueueTransaction();
                transaction.Begin();
                queue.Send("Message: " + i);
                transaction.Commit();
            }
            Console.WriteLine(TransactionalQueuePath + ": " + stopwatch.ElapsedMilliseconds + " ms.");

            stopwatch = Stopwatch.StartNew();
            transaction = new MessageQueueTransaction();
            transaction.Begin();
            for (var i = 0; i < Count; i++)
            {
                queue.Send("Message: " + i);
            }
            transaction.Commit();
            Console.WriteLine(TransactionalQueuePath + " (batched): " + stopwatch.ElapsedMilliseconds + " ms.");
        }

        private static void Process(MessageQueue queue, string queueName)
        {
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < Count; i++)
                queue.Send("Message: " + i);
            Console.WriteLine(queueName + ": " + stopwatch.ElapsedMilliseconds + " ms.");
        }

        private static void Setup(string queueName, bool transactional = false)
        {
            if (!MessageQueue.Exists(queueName))
                MessageQueue.Create(queueName, transactional);
        }
    }
}
