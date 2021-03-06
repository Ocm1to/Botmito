using Botmito.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Botmito
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public async Task RunAsync()
        {
            var configJson = JsonSerializer.Deserialize<ConfigJson>(File.ReadAllText("config.json"));

            var clientConfig = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true, 
                Intents = DiscordIntents.All,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            };

            Client = new DiscordClient(clientConfig);

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                DmHelp = false,
                EnableDefaultHelp = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            
            Commands.RegisterCommands<BasicCommands>();
            Commands.RegisterCommands<TeamCommands>();
            Commands.RegisterCommands<ModCommands>();

            Client.MessageCreated += OnMessageAdded;

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(1),
            });

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        public async Task OnMessageAdded(DiscordClient client, MessageCreateEventArgs e)
        {
            var blacklistedWords = File.ReadAllLines("blacklisted.txt");

            if (blacklistedWords.Any(x => e.Message.Content.Contains(x)))
            {              
                await e.Message.RespondAsync("Watch your language!").ConfigureAwait(false);
            }
        }
    }
}
