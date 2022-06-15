using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace blakserv_logbot
{
   public class LogEntry
   {
      public LogType LogType;
      public DateTime Timestamp;
      public string Message;
      public string ServerNumber;
      public static string[] DateFormats = { "MMM  d yyyy HH:mm:ss", "MMM dd yyyy HH:mm:ss" };

      public LogEntry(LogType LogType, string ServerNumber, string Message)
      {
         this.LogType = LogType;
         this.ServerNumber = ServerNumber;

         int splitIndex = Message.IndexOf('|');
         // Some log entries (e.g. stack trace) won't have a time/date added.
         if (splitIndex < 0)
         {
            this.Message = Message;
            Timestamp = DateTime.Now;
         }
         else
         {
            string tdString = Message[..splitIndex];
            this.Message = Message.Substring(splitIndex + 1).Trim();

            // Get a datetime from blakserv string
            bool res = DateTime.TryParseExact(tdString, DateFormats, new CultureInfo("en-US"),
               DateTimeStyles.AllowWhiteSpaces, out Timestamp);
            if (!res)
            {
               Console.WriteLine($"Error parsing date: {tdString} in LogEntry: {Message}");
               Timestamp = DateTime.Now;
            }
         }
      }
   }
}
