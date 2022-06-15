using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace blakserv_logbot
{
   class QueueWorker
   {
      protected readonly Thread thread;
      protected readonly LogQueue queue;
      protected volatile bool IsRunning;
      private DiscordClient DiscordClient;

      public QueueWorker(LogQueue Queue)
      {
         thread = new Thread(ThreadProc);
         queue = Queue;
      }

      public void Start()
      {
         if (IsRunning)
            return;

         IsRunning = true;
         thread.Start();
      }
      public void Stop()
      {
         IsRunning = false;
      }

      /// <summary>
      /// Internal thread loop
      /// </summary>
      protected async void ThreadProc()
      {
         DiscordClient = new DiscordClient();
         while (IsRunning)
         {
            Thread.Sleep(1000);
            if (!IsRunning)
               break;

            if (queue.TryDequeue(out LogEntry message))
            {
               await DiscordClient.SendLogEntry(message);
               continue;
            }
         }
      }
   }
}
