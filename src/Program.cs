using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace blakserv_logbot
{
   class Program
   {
      static LogWorker logWorker;
      static QueueWorker queueWorker;
      static LogQueue logQueue;
      public static ConfigHandler ConfigHandler { get; protected set; }

      static void Main()
      {
         List<LogFile> logFiles = new List<LogFile>();

         // Contains the log files.
         ConfigHandler = new ConfigHandler();

         // Load files.
         foreach (ConfigFolder folder in ConfigHandler.ConfigFile.Folders)
         {
            foreach (string file in folder.Files)
            {
               try
               {
                  logFiles.Add(new LogFile(folder.ServerNumber, folder.Folder, file));
               }
               catch (FileNotFoundException)
               {
                  Console.WriteLine($"Could not find {folder.Folder + file}");
               }
               catch (Exception e)
               {
                  Console.WriteLine($"Error while loading {folder.Folder + file}: {e.Message}");
               }
            }
         }

         if (logFiles.Count == 0)
         {
            Console.WriteLine($"Could not load any log files, exiting.");
            Environment.Exit(1);
         }
         logQueue = new LogQueue();

         // Workers - one thread to process log files and put messages on the
         // log queue one thread to process the log queue and dispatch messages.
         logWorker = new LogWorker(logFiles, logQueue);
         logWorker.Start();
         queueWorker = new QueueWorker(logQueue);
         queueWorker.Start();

         Console.WriteLine("Welcome to Blakserv logbot. Use 'quit' to quit.");
         while (true)
         {
            // Check for input.
            string input = Console.ReadLine();
            if (input == "quit")
            {
               logWorker.Stop();
               queueWorker.Stop();
               ConfigHandler.SaveConfig();
               Environment.Exit(0);
            }
            Thread.Sleep(500);
         }
      }
   }

   public static class HttpClientExtensions
   {
      public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient httpClient, string url, object body)
      {
         var bodyJson = JsonSerializer.Serialize(body);
         var stringContent = new StringContent(bodyJson, Encoding.UTF8, "application/json");
         return httpClient.PostAsync(url, stringContent);
      }
   }
}

