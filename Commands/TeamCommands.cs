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
        const string BlueButtonId = "blue_button";
        const string RedButtonId = "red_button";
        const string GreenButtonId = "green_button";
        const string NoneButtonId = "none_button";
        const ulong BlueTeamId = 994600135982055464;
        const ulong RedTeamId = 994600244757147659;
        const ulong GreenTeamId = 990648266997776464;

        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            var blueButton = new DiscordButtonComponent(ButtonStyle.Primary, BlueButtonId, "BlueTeam", false);
            var redButton = new DiscordButtonComponent(ButtonStyle.Danger, RedButtonId, "RedTeam", false);
            var greenButton = new DiscordButtonComponent(ButtonStyle.Success, GreenButtonId, "GreenTeam", false);
            var noneButton = new DiscordButtonComponent(ButtonStyle.Secondary, NoneButtonId, "None", false);

            var blueTeamRole = ctx.Guild.GetRole(BlueTeamId);
            var redTeamRole = ctx.Guild.GetRole(RedTeamId);
            var greenTeamRole = ctx.Guild.GetRole(GreenTeamId);

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
                (x.Id == BlueButtonId || x.Id == RedButtonId || x.Id == GreenButtonId || x.Id == NoneButtonId)).ConfigureAwait(false);

            switch (buttonResult.Result.Id)
            {
                case BlueButtonId:
                    await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                    await ctx.Channel.SendMessageAsync(buttonResult.Result.User.Mention + " joined BlueTeam!").ConfigureAwait(false);
                    break;

                case RedButtonId:
                    await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                    await ctx.Channel.SendMessageAsync(buttonResult.Result.User.Mention + " joined RedTeam!").ConfigureAwait(false);
                    break;

                case GreenButtonId:
                    await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                    await ctx.Channel.SendMessageAsync(buttonResult.Result.User.Mention + " joined GreenTeam!").ConfigureAwait(false);
                    break;

                case NoneButtonId:
                    await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                    break;

                default:
                    await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                    await ctx.Channel.SendMessageAsync("Interaction failed").ConfigureAwait(false);
                    break;
            }
        }


        [Command("poll")] //discord api triggers preemptive rate limit because of adding reactions too fast
        public async Task Poll(CommandContext ctx, TimeSpan duration, params DiscordEmoji[] emojiOptions) 
        {
            var interactivity = ctx.Client.GetInteractivity();
            var options = emojiOptions.Select(x => x.ToString());

            var embed = new DiscordEmbedBuilder
            {
                Title = "Poll",
                Description = string.Join(" ", options)
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            foreach(var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);
            var distinctResult = result.Distinct();

            var results = distinctResult.Select(x => $"{x.Emoji}: {x.Total}");

            await ctx.Channel.SendMessageAsync(string.Join("\n", results)).ConfigureAwait(false);
        }      

    }
}

