using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace blakserv_logbot
{
   public class DiscordClient
   {
      private readonly string CONSOLE_ERROR = "{0} {1}: {2} {3}: {4}";
      private readonly string COMMAND_ERROR = "Invalid command.";
      private readonly string CWORD_IGNORE = "ignore";
      private readonly string CWORD_UNIGNORE = "unignore";
      private readonly string DISCORD_LOGENTRY = "{0} {1} - {2}:\n{3}";

      // Channel IDs
      private readonly DiscordSocketClient discordSocketClient;
      private readonly CommandService commandService;
      private readonly IServiceProvider iServiceProvider;
      private readonly ulong channelID;
      private readonly string botPrefix = "logbot";

      public DiscordClient()
      {
         var config = new DiscordSocketConfig
         {
            GatewayIntents = GatewayIntents.DirectMessages | GatewayIntents.GuildMembers
                 | GatewayIntents.GuildMessages | GatewayIntents.Guilds,
            AlwaysDownloadUsers = true
         };
         discordSocketClient = new DiscordSocketClient(config);
         commandService = new CommandService();
         iServiceProvider = new ServiceCollection()
            .AddSingleton(discordSocketClient)
            .AddSingleton(commandService)
            .BuildServiceProvider();

         discordSocketClient.MessageReceived += OnMessageReceivedAsync;

         channelID = ulong.Parse(Program.ConfigHandler.ConfigFile.DiscordChannel);
         if (Program.ConfigHandler.ConfigFile.BotPrefix != string.Empty)
            botPrefix = Program.ConfigHandler.ConfigFile.BotPrefix;

         string token = Program.ConfigHandler.ConfigFile.BotToken;
         discordSocketClient.LoginAsync(Discord.TokenType.Bot, token);
         discordSocketClient.StartAsync();

         discordSocketClient.Log += OnLogAsync;
      }

      /// <summary>
      /// Log errors to console.
      /// </summary>
      /// <param name="message"></param>
      /// <returns></returns>
      private Task OnLogAsync(LogMessage message)
      {
         Console.WriteLine(String.Format(CONSOLE_ERROR, DateTime.Now.ToShortDateString(),
             DateTime.Now.ToLongTimeString(), message.Severity.ToString(), message.Source,
             message.Exception?.ToString() ?? message.Message));

         return Task.CompletedTask;
      }

      /// <summary>
      /// Handle received messages either from DMs or from the #guide channel.
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      private async Task OnMessageReceivedAsync(SocketMessage s)
      {
         var msg = s as SocketUserMessage;

         // Ignore messages from self.
         if (msg == null || msg.Author?.Id == discordSocketClient.CurrentUser.Id)
            return;

         // Only listen to correct channel.
         if (msg.Channel.Id != channelID)
            return;

         // Check if the message has a valid command prefix.
         int argPos = 0;
         if (!msg.HasStringPrefix($"!{botPrefix}", ref argPos))
            return;

         int cmdStartPos = msg.Content.IndexOf(' ');
         if (cmdStartPos == -1
             || cmdStartPos - 1 >= msg.Content.Length)
         {
            await SendChannelMessage(COMMAND_ERROR);
            return;
         }

         string text = msg.Content.Substring(cmdStartPos);
         // Split message into [ command_word, arguments ]
         string[] commands = text.Trim().Split(' ', 2);
         if (commands.Length == 0)
         {
            await SendChannelMessage(COMMAND_ERROR);
            return;
         }
         Console.WriteLine($"Executing command {commands[0]}");

         // Add an ignore string - log entries containing this will be discarded.
         if (commands[0].ToLower() == CWORD_IGNORE)
         {
            if (Program.ConfigHandler.ConfigFile.IgnoreStrings.Contains(commands[1]))
            {
               await SendChannelMessage($"Already ignoring string: {commands[1]}");
               return;
            }
            Program.ConfigHandler.ConfigFile.IgnoreStrings.Add(commands[1]);
            Console.WriteLine($"Ignoring string: {commands[1]}");
            await SendChannelMessage($"Added to ignore strings: {commands[1]}");
         }
         // Unignore a string.
         else if (commands[0].ToLower() == CWORD_UNIGNORE)
         {
            if (Program.ConfigHandler.ConfigFile.IgnoreStrings.Contains(commands[1]))
            {
               Program.ConfigHandler.ConfigFile.IgnoreStrings.Remove(commands[1]);
               Console.WriteLine($"Unignoring string: {commands[1]}");
               await SendChannelMessage($"Unignoring string: {commands[1]}");
            }
         }
      }

      /// <summary>
      /// Sends a LogEntry to the Discord channel. 
      /// </summary>
      /// <param name="Message"></param>
      /// <returns></returns>
      public async Task SendLogEntry(LogEntry LogEntry)
      {
         if (LogEntry == null)
         {
            Console.WriteLine("Got invalid LogEntry in SendLogEntry");

            return;
         }

         await SendChannelMessage(string.Format(DISCORD_LOGENTRY,
            LogEntry.ServerNumber,
            LogEntry.LogType.ToString(),
            LogEntry.Timestamp.ToLongTimeString(),
            LogEntry.Message));
      }

      /// <summary>
      /// Sends a message to the Discord channel. Used for feedback
      /// i.e. message sent to user, user not found.
      /// </summary>
      /// <param name="Message"></param>
      /// <returns></returns>
      public async Task SendChannelMessage(string Message)
      {
         if (Message == null)
         {
            Console.WriteLine("Got null message in SendChannelMessage");

            return;
         }

         var channel = discordSocketClient.GetChannel(channelID) as SocketTextChannel;

         if (channel == null)
         {
            Console.WriteLine("Got null channel in SendChannelMessage");

            return;
         }

         await channel.SendMessageAsync(Message);
      }
   }
}
