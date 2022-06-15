using System;
using System.Collections.Concurrent;

namespace blakserv_logbot
{
   public class LogQueue
   {
      protected ConcurrentQueue<LogEntry> queue;
      private int maxQueueCount = 20;
      public LogQueue()
      {
         queue = new ConcurrentQueue<LogEntry>();
      }

      public void Enqueue(LogEntry LogMessage)
      {
         queue.Enqueue(LogMessage);
      }

      public void Enqueue(LogType LogType, string ServerNumber, string Message)
      {
         if (Count > maxQueueCount)
         {
            Console.WriteLine($"Too many queue entries, discarding message: {Message}.");
            return;
         }
         Enqueue(new LogEntry(LogType, ServerNumber, Message));
      }

      public bool TryDequeue(out LogEntry LogMessage)
      {
         return queue.TryDequeue(out LogMessage);
      }

      public int Count => queue.Count;
   }
}
