using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Botmito.Commands 
{
    public class BasicCommands : BaseCommandModule
    {
        const string WebApiHeader = "X-Api-Key";

        [Command("ping")]
        [Description("Responds with \"Pong\" and displays bot's reaction time.")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Message.RespondAsync($"Pong! {ctx.Client.Ping}ms").ConfigureAwait(false);
        }

        [Command("greet")]
        [Description("Greet a specified member.")]
        public async Task Greet(CommandContext ctx,
            [Description("Member username")] DiscordMember member)
        {
            await ctx.Channel.SendMessageAsync($"Hello {member.Mention}! Welcome onboard!").ConfigureAwait(false);
        }

        [Command("add")]
        [Description("Adds input numbers and displays result.")]
        public async Task Add(CommandContext ctx, 
            [Description("Numers to add")] params int[] numbers)
        {
            await ctx.Channel.SendMessageAsync(numbers.Sum().ToString()).ConfigureAwait(false);
        }

        [Command("echo")]
        [Description("Echoes the next message you send to the channel.")]
        public async Task Echo(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Content);
        }


        [Command("coinflip")]
        [Description("Perform a virtual coin flip. The result is random and is either \"Heads\" or \"Tails\".")]
        public async Task CoinFlip(CommandContext ctx)
        {
            var rng = new Random();
            var result = string.Empty;

            _ = rng.Next(2) == 0 ? result = "Heads" : result = "Tails";

            var coinEmbed = new DiscordEmbedBuilder
            {
                Title = result,     
                Color = DiscordColor.Gold
            };
                
            await ctx.Channel.SendMessageAsync(embed: coinEmbed).ConfigureAwait(false);
        }

        [Command("diceroll")]
        [Description("Perform a virtual dice roll. Specify type and number of dice.")]
        public async Task DiceRoll(CommandContext ctx,
           [Description("Specify dice type (number of sides).")] int diceType,
           [Description("Specify number of dice you'd like to roll.")] int numberOfDice)
        {
            if(diceType > 100 || diceType <= 0)
            {
                await ctx.Message.RespondAsync("Invalid dice type. Sorry!").ConfigureAwait(false);
            }
            else if(numberOfDice > 20 || numberOfDice <= 0)
            {
                await ctx.Message.RespondAsync("Number of dice should be between 1 and 20.").ConfigureAwait(false);
            }
            else
            {
                var rng = new Random();
                var outcomes = new List<int>();
                var result = string.Empty;
                var sum = 0;

                for (var i = 0; i < numberOfDice; i++)
                {
                    var roll = rng.Next(1, diceType + 1);
                    outcomes.Add(roll);
                    sum += roll;
                }

                result = string.Join(" ", outcomes);

                await ctx.Channel.SendMessageAsync("Roll result: " + result + "\nSum: " + sum).ConfigureAwait(false);
            }  
        }

        [Command("reminder")]
        [Description("Sets a reminder for the user.")] 
        public async Task Reminder(CommandContext ctx,
           [Description("Specify a time for the reminder (eg. \"1d 1h 1s\").")] TimeSpan reminderTime,
           [Description("Write a note for the reminder (eg. \"turn off the oven\").")] params string[] reminderNote)
        {
            var note = string.Join(' ', reminderNote);
            await ctx.Message.RespondAsync("Reminder set for " + (DateTime.Now + reminderTime));
          
            var reminderEmbed = new DiscordEmbedBuilder
            {
                Title = "Reminder " + ":bell:",
                Description = note,
                Color = DiscordColor.White,
            };

            var timer = new Timer(_ => ctx.Channel.SendMessageAsync(ctx.User.Mention, embed: reminderEmbed).ConfigureAwait(false));
            timer.Change((int)(reminderTime).TotalMilliseconds, Timeout.Infinite);  
        }

        [Command("fact")] 
        [Description("Displays a random, interesting fact.")]
        public async Task Fact(CommandContext ctx)
        {
            var httpClient = new HttpClient();
            var apiUrl = "https://api.api-ninjas.com/v1/facts?limit=1";

            var apiKey = await GetApiKey ("keystorage.txt");

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add(WebApiHeader, apiKey); 

            var fact = await httpClient.GetStringAsync(apiUrl);

            var factJson = JsonSerializer.Deserialize<List<FactJson>>(fact);

            var factEmbed = new DiscordEmbedBuilder
            {
                Title = "Fact",
                Description = factJson.First().Fact
            };

            await ctx.Channel.SendMessageAsync(embed: factEmbed).ConfigureAwait(false);
        }

        [Command("joke")]
        [Description("The bot tells you a joke.")]
        public async Task Joke(CommandContext ctx)
        {
            var httpClient = new HttpClient();
            var apiUrl = "https://api.api-ninjas.com/v1/jokes?limit=1";

            var apiKey = await GetApiKey("keystorage.txt");

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add(WebApiHeader, apiKey); 

            var joke = await httpClient.GetStringAsync(apiUrl);

            var jokeJson = JsonSerializer.Deserialize<List<JokeJson>>(joke);

            var jokeEmbed = new DiscordEmbedBuilder
            {
                Title = "Joke",
                Description = jokeJson.First().Joke
            };

            await ctx.Channel.SendMessageAsync(embed: jokeEmbed).ConfigureAwait(false);
        }
        
        private async Task<string> GetApiKey(string path)
        {
            var apiKey = await File.ReadAllTextAsync(path);
            return apiKey;
        }
    }
}

