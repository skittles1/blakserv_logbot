namespace blakserv_logbot.tests
{
   [TestClass]
   public class TestLogEntry
   {
      private string noDateTime = "User::UserGo(#bCancel=INT 0) [user.bof (8881)]";
      private string singleDateTime = "Jun  4 2022 01:24:44|LoadBof loaded 1799 of 1799 found .bof files";
      private string doubleDateTime = "Jun 14 2022 19:21:00|LoadBof loaded 1799 of 1799 found .bof files";

      private string includesDT = "Jun 14 2022 19:21:00|LoadBof loaded 1799 of 1799 found .bof files";
      private string wsIncludesDT = "Jun 14 2022 19:21:00|   LoadBof loaded 1799 of 1799 found .bof files   ";
      private string strippedDT = "LoadBof loaded 1799 of 1799 found .bof files";


      /// <summary>
      /// Check if the LogEntry timestamp is correct for the 3 kinds
      /// of timestamp encountered in a log message.
      /// 1. No timestamp.
      /// 2. Timestamp with 2 spaces between short month & single digit day.
      /// 3. Timestamp with 1 space between short month & double digit day.
      /// </summary>
      [TestMethod]
      public void TestDateString()
      {
         LogEntry l = new LogEntry(LogType.Error, "105", noDateTime);
         // Won't be exact, but should be close.
         Assert.IsTrue((l.Timestamp - DateTime.Now).TotalSeconds <= 2);

         // Jun  4 2022 01:24:44
         l = new LogEntry(LogType.Error, "105", singleDateTime);
         Assert.IsTrue((new DateTime(2022, 6, 4, 1, 24, 44) - l.Timestamp).TotalSeconds < 1);

         // Jun 14 2022 19:21:00
         l = new LogEntry(LogType.Error, "105", doubleDateTime);
         Assert.IsTrue((new DateTime(2022, 6, 14, 19, 21, 00) - l.Timestamp).TotalSeconds < 1);
      }

      /// <summary>
      /// Tests whether the LogEntry message is correct.
      /// </summary>
      [TestMethod]
      public void TestMessage()
      {
         LogEntry l = new LogEntry(LogType.Error, "105", includesDT);
         Assert.IsTrue(l.Message.Equals(strippedDT));

         l = new LogEntry(LogType.Error, "105", wsIncludesDT);
         Assert.IsTrue(l.Message.Equals(strippedDT));

         l = new LogEntry(LogType.Error, "105", noDateTime);
         Assert.IsTrue(l.Message.Equals(noDateTime));
      }
   }
}
