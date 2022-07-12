using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Botmito.Commands 
{
    public class TeamCommands : BaseCommandModule
    {
        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            var blueButton = new DiscordButtonComponent(ButtonStyle.Primary, "blue_button", "BlueTeam", false);
            var redButton = new DiscordButtonComponent(ButtonStyle.Danger, "red_button", "RedTeam", false);
            var greenButton = new DiscordButtonComponent(ButtonStyle.Success, "green_button", "GreenTeam", false);
            var noneButton = new DiscordButtonComponent(ButtonStyle.Secondary, "none_button", "None", false);

            var buttonEmbed = new DiscordEmbedBuilder
            {
                Title = "Which team would you like to join?"
            };

            var message = new DiscordMessageBuilder()
                .AddEmbed(buttonEmbed)
                .AddComponents(blueButton, redButton, greenButton, noneButton);
                     
            var sentMessage = await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            var buttonResult = await interactivity.WaitForButtonAsync(sentMessage,
                x => x.User == ctx.User &&
                (x.Id == "blue_button" || x.Id == "red_button" || x.Id == "green_button" || x.Id == "none_button")).ConfigureAwait(false);

            switch (buttonResult.Result.Id)
            {
                case "blue_button":
                    await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                    await sentMessage.DeleteAsync().ConfigureAwait(false);
                    await ctx.Channel.SendMessageAsync(buttonResult.Result.User.Mention + " joined BlueTeam!").ConfigureAwait(false);
                    break;

                case "red_button":
                    await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                    await sentMessage.DeleteAsync().ConfigureAwait(false);
                    await ctx.Channel.SendMessageAsync(buttonResult.Result.User.Mention + " joined RedTeam!").ConfigureAwait(false);
                    break;

                case "green_button":
                    await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                    await sentMessage.DeleteAsync().ConfigureAwait(false);
                    await ctx.Channel.SendMessageAsync(buttonResult.Result.User.Mention + " joined GreenTeam!").ConfigureAwait(false);
                    break;

                case "none_button":
                    await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                    await sentMessage.DeleteAsync().ConfigureAwait(false);
                    break;

                default:
                    await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                    await ctx.Channel.SendMessageAsync("Interaction failed").ConfigureAwait(false);
                    break;
            }
        }


        [Command("poll")] //triggers preemptive rate limit because of adding reactions too fast
        public async Task Poll(CommandContext ctx, TimeSpan duration, params DiscordEmoji[] emojiOptions) //Rate limit error when sending emojis
        {
            var interactivity = ctx.Client.GetInteractivity();
            var options = emojiOptions.Select(x => x.ToString());

            var embed = new DiscordEmbedBuilder
            {
                Title = "Poll",
                Description = String.Join(" ", options)
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            foreach(var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);
            var distinctResult = result.Distinct();

            var results = distinctResult.Select(x => $"{x.Emoji}: {x.Total}");

            await ctx.Channel.SendMessageAsync(String.Join("\n", results)).ConfigureAwait(false);
        }      

    }
}

