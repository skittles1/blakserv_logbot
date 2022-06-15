using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace blakserv_logbot
{
   public class ConfigFolder
   {
      public string ServerNumber { get; set; }
      public string Folder { get; set; }
      public List<string> Files { get; set; }
   }

   public class ConfigFile
   {
      public string BotToken { get; set; }
      public string BotPrefix { get; set; }
      public string DiscordChannel { get; set; }
      public List<ConfigFolder> Folders { get; set; }
      public List<string> IgnoreStrings { get; set; }
   }

   public class ConfigHandler
   {
      private readonly string configFileName = "config.json";
      public ConfigFile ConfigFile { get; protected set; }

      public ConfigHandler()
      {
         FileInfo file = new FileInfo(configFileName);
         if (!file.Exists)
         {
            Console.WriteLine("Could not find config.json, creating" +
               "default one. You must edit this file and add your own" +
               "discord bot token, discord channel ID, etc.");
            ConfigFile = new ConfigFile
            {
               BotToken = "discord_bot_token",
               BotPrefix = "logbot",
               DiscordChannel = "discord_channel_id",
               IgnoreStrings = new List<string>() { },
               Folders = new List<ConfigFolder>() { new ConfigFolder
                  {
                     ServerNumber = "100",
                     Folder = @"C:\Meridian59\channel\",
                     Files =  new List<string>() { "error.txt", "debug.txt", "god.txt" }
                  }
               },
            };

            SaveConfig();
            System.Environment.Exit(0);
         }

         string jsonString = File.ReadAllText(configFileName);
         ConfigFile = JsonSerializer.Deserialize<ConfigFile>(jsonString)!;
      }

      public void SaveConfig()
      {
         var options = new JsonSerializerOptions { WriteIndented = true };
         string defaultJson = JsonSerializer.Serialize(ConfigFile, options);
         File.WriteAllText(configFileName, defaultJson + "\n");
      }
   }
}
