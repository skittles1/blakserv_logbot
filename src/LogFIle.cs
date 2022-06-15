using System;
using System.IO;
using System.Text;
using System.Linq;

namespace blakserv_logbot
{
   public class LogFile
   {
      public string ServerNumber { get; set; }
      public string Folder { get; set; }
      public string FileName { get; set; }
      public string FullPath => Folder + FileName;
      public long OldLength { get; set; }
      public FileInfo File { get; protected set; }
      public FileStream Stream { get; protected set; }
      public LogType LogType;
      public bool IsOpen => Stream != null;
      public bool HasUpdated => Stream.Length > OldLength;
      public long NumNewBytes => Stream.Length - OldLength;

      public LogFile(string ServerNumber, string Folder, string FileName)
      {
         this.ServerNumber = ServerNumber;
         this.Folder = Folder;
         this.FileName = FileName;
         File = new FileInfo(FullPath);
         if (!File.Exists)
            throw new System.IO.FileNotFoundException($"Could not find {FullPath}");
         Open();
      }

      public void Open()
      {
         Stream = File.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
         OldLength = Stream.Length;
         try
         {
            string fileNoExt = FileName[..FileName.IndexOf('.')];
            LogType = (LogType)Enum.Parse(typeof(LogType), fileNoExt, true);
         }
         catch (Exception)
         {
            Console.WriteLine($"Unable to find log type for file {FileName}");
            LogType = LogType.None;
         }
      }

      // Doesn't throw exception if the file doesn't exist.
      public void TryOpen()
      {
         if (Exists())
         {
            Console.WriteLine($"Trying to reopen {FileName}...");
            Open();
         }
      }

      // Close the stream and set it to null.
      public void Close()
      {
         Stream.Close();
         Stream = null;
      }

      public void UpdateLength() => OldLength = Stream.Length;

      // Get a string array of changes in the file since the last check.
      // Each entry in the array is a separate line of the file.
      public string[] GetChanges()
      {
         // Caller should check file exists and is open prior to calling GetChanges().
         if (Stream == null)
         {
            Console.WriteLine($"Stream for {FullPath} is null when trying to get changes!");
            return Array.Empty<string>();
         }

         // Seek to the last updated position and read new bytes into byte array.
         Stream.Seek(-NumNewBytes, SeekOrigin.End);
         byte[] byteArray = new byte[NumNewBytes + 1];
         int bytesRead = Stream.Read(byteArray, 0, (int)NumNewBytes);
         if (bytesRead <= 0)
            return Array.Empty<string>();

         // Split bytes on newline.
         return Encoding.Default.GetString(byteArray)
             .Split("\n", StringSplitOptions.RemoveEmptyEntries);
      }

      // Whether the file actually exists on disk. FileInfo.Refresh()
      // must be called first so the FileInfo data is current.
      public bool Exists()
      {
         File.Refresh();
         
         // Catch the case where we have a negative length file update
         // which can happen if the file was deleted & recreated, or
         // if something was removed from the file.
         if (NumNewBytes < 0)
            UpdateLength();

         return File.Exists;
      }
   }
}
