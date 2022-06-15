using System;
using System.Collections.Generic;
using System.Threading;

namespace blakserv_logbot
{
   class LogWorker
   {
      protected readonly Thread thread;
      protected readonly LogQueue queue;
      protected List<LogFile> logFiles;
      protected volatile bool IsRunning;

      public LogWorker(List<LogFile> LogFiles, LogQueue Queue)
      {
         thread = new Thread(ThreadProc);
         queue = Queue;
         logFiles = LogFiles;
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
      protected void ThreadProc()
      {
         while (IsRunning)
         {
            foreach (LogFile logFile in logFiles)
            {
               Thread.Sleep(1000);
               if (!IsRunning)
                  break;

               if (!logFile.IsOpen)
               {
                  logFile.TryOpen();
                  continue;
               }

               if (!logFile.Exists())
               {
                  Console.WriteLine($"Can no longer find file {logFile.FileName}");
                  logFile.Close();
                  continue;
               }

               if (logFile.HasUpdated)
               {
                  string[] changes = logFile.GetChanges();
                  logFile.UpdateLength();
                  foreach (string s in changes)
                     if (s != "\0")
                        ProcessLineChange(logFile, s);
               }
            }
         }
      }

      private void ProcessLineChange(LogFile LogFile, string Line)
      {
         foreach (string ignore in Program.ConfigHandler.ConfigFile.IgnoreStrings)
            if (Line.Contains(ignore))
               return;
         queue.Enqueue(LogFile.LogType, LogFile.ServerNumber, Line);
      }
   }
}
