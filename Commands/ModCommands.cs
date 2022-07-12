using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Botmito.Commands
{
    public class ModCommands : BaseCommandModule
    {
        public List<string> Blacklisted = new List<string>();

        [Command("mute")]
        //[RequirePermissions(DSharpPlus.Permissions.MuteMembers)] //commands do not work even when the user triggering them has the permission
        [Description("Mutes a specified channel member.")]
        public async Task Mute(CommandContext ctx,
           [Description("Member username")] DiscordMember member)
        {
            await member.SetMuteAsync(true).ConfigureAwait(false);
            await ctx.Message.RespondAsync("Successfully muted " + member.Mention).ConfigureAwait(false);
        }

        [Command("unmute")]
        //[RequirePermissions(DSharpPlus.Permissions.MuteMembers)]
        [Description("Unmutes a specified channel member.")]
        public async Task Unmute(CommandContext ctx,
           [Description("Member username")] DiscordMember member)
        {
            await member.SetMuteAsync(false).ConfigureAwait(false);
        }

        [Command("deafen")]
        //[RequirePermissions(DSharpPlus.Permissions.DeafenMembers)]
        [Description("Deafens a specified channel member.")]
        public async Task Deafen(CommandContext ctx,
          [Description("Member username")] DiscordMember member)
        {
            await member.SetDeafAsync(true).ConfigureAwait(false);
            await ctx.Message.RespondAsync("Successfully deafened " + member.Mention).ConfigureAwait(false);
        }

        [Command("undeafen")]
        //[RequirePermissions(DSharpPlus.Permissions.DeafenMembers)]
        [Description("Undeafens a specified channel member.")]
        public async Task Undeafen(CommandContext ctx,
           [Description("Member username")] DiscordMember member)
        {
            await member.SetDeafAsync(false).ConfigureAwait(false);
        }

        [Command("timeout")] //TODO - doesnt work for some reason
        //[RequirePermissions(DSharpPlus.Permissions.ModerateMembers)]
        [Description("Times out a specified channel member for a given duration.")]
        public async Task Timeout(CommandContext ctx,
           [Description("Member username")] DiscordMember member,
           [Description("Timeout duration (eg. 5m)")] TimeSpan time)
        {
            var duration = DateTime.Now + time;
            if(duration > DateTime.Now.AddMinutes(15))
            {
                await ctx.RespondAsync("You can't timeout a member for more than 15 minutes.").ConfigureAwait(false);
            }
            else
            await member.TimeoutAsync(duration).ConfigureAwait(false);
        }

        [Command("memberinfo")]
        [Description("Displays information about a specified member.")]
        public async Task MemberInfo(CommandContext ctx, 
            [Description("Member username")] DiscordMember member)
        {
            var nick = member.Nickname;
            var roles = member.Roles; //the bot doesnt see roles for some reason
            var roleNames = new List<string>();

            foreach(var role in roles)
            {
                roleNames.Add(role.Name);
            }

            if (String.IsNullOrWhiteSpace(member.Nickname))
                nick = "<none>";

            var memberEmbed = new DiscordEmbedBuilder
            {
                Title = "Some information about " + member.Username,
                Description =
                "\nUsername: " + member.Username +
                "\nNickname: " + nick +
                "\nJoined: " + member.JoinedAt.ToString("d") +
                "\nRoles: " + String.Join(", ", roleNames), //TODO - doesnt work
                ImageUrl = member.AvatarUrl,
                Color = DiscordColor.White
            };

            await ctx.Channel.SendMessageAsync(embed: memberEmbed).ConfigureAwait(false);
        }

        [Command("nick")] 
        //[RequirePermissions(DSharpPlus.Permissions.ManageNicknames)]
        [Description("Changes a member's nickname.")]
        public async Task Nick(CommandContext ctx,
           [Description("Member username")] DiscordMember member,
           [RemainingText, Description("New nickname")] string newNickname)
        {
            await member.ModifyAsync(x => x.Nickname = newNickname);
        }

        [Command("grantrole")] //TODO - doesnt work for some reason
        //[RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Description("Grants a specified role to a chosen member.")]
        public async Task GrantRole(CommandContext ctx,
           [Description("Member username")] DiscordMember member,
           [Description("Role name")] DiscordRole role)          
        {
            await member.GrantRoleAsync(role);
        }

        [Command("revokerole")] //TODO - doesnt work for some reason
        //[RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Description("Revokes a specified role from a chosen member.")]
        public async Task RevokeRole(CommandContext ctx,
           [Description("Member username")] DiscordMember member,
           [Description("Role name")] DiscordRole role)
        {
            await member.RevokeRoleAsync(role);
        }

        [Command("kick")]
        //[RequirePermissions(DSharpPlus.Permissions.KickMembers)]
        [Description("Kicks a specified member from the server.")]
        public async Task Kick(CommandContext ctx,
           [Description("Member username")] DiscordMember member)
        {
            var yesButton = new DiscordButtonComponent(ButtonStyle.Success, "yes_button", "Yes", false);
            var noButton = new DiscordButtonComponent(ButtonStyle.Danger, "no_button", "No", false);

            var kickEmbed = new DiscordEmbedBuilder
            {
                Title = $"Are you sure you want to kick {member.Username}?"
            };

            var message = new DiscordMessageBuilder()
                .AddEmbed(kickEmbed)
                .AddComponents(yesButton, noButton);

            var kickMessage = await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            var buttonResult = await interactivity.WaitForButtonAsync(kickMessage,
                x => x.User == ctx.User &&
                (x.Id == "yes_button" || x.Id == "no_button")).ConfigureAwait(false);

            if(buttonResult.Result.Id == "yes_button")
            {
                await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                await kickMessage.DeleteAsync().ConfigureAwait(false); //deleting the message makes it so the bot stops responding to anything
                //await member.RemoveAsync().ConfigureAwait(false);
                await ctx.Channel.SendMessageAsync("Aaaaaaand they're gone!").ConfigureAwait(false);
            }

            else if(buttonResult.Result.Id == "no_button")
            {
                await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                await kickMessage.DeleteAsync().ConfigureAwait(false); //deleting the message makes it so the bot stops responding to anything
                await ctx.Channel.SendMessageAsync(member.Mention + " has been spared... For now.").ConfigureAwait(false);
            }
            else
            {
                await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                await kickMessage.DeleteAsync().ConfigureAwait(false); //deleting the message makes it so the bot stops responding to anything
                await ctx.Channel.SendMessageAsync("Something went wrong").ConfigureAwait(false);
            }

            //await ctx.Channel.SendMessageAsync($"Are you sure you want to kick {member.Mention}? [yes/no]");
            //var answer = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member);

            //if (answer.Result.Content.ToLower() == "yes")
            //    await member.RemoveAsync();
            //else if (answer.Result.Content.ToLower() == "no")
            //    await ctx.Channel.SendMessageAsync(member.Mention + " has been spared... For now.");
            //else
            //    await ctx.Channel.SendMessageAsync("Invalid answer.");
        }

        [Command("ban")]
        //[RequirePermissions(DSharpPlus.Permissions.BanMembers)]
        [Description("Bans a specified member from the server and deletes all their messeges from this day.")]
        public async Task Ban(CommandContext ctx,
            [Description("Member username")] DiscordMember member,
            [Description("Reason for the ban")] params string[] reason)
        {
            var interactivity = ctx.Client.GetInteractivity();

            await ctx.Channel.SendMessageAsync($"Are you sure you want to ban {member.Mention}? [yes/no]");
            var answer = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member);

            if (answer.Result.Content.ToLower() == "yes")
                await member.BanAsync(1, String.Join(' ', reason));
            else if (answer.Result.Content.ToLower() == "no")
                await ctx.Channel.SendMessageAsync(member.Mention + " has been spared... For now.");
            else
                await ctx.Channel.SendMessageAsync("Invalid answer.");
        }

        [Command("move")] //TODO - doesnt work, the bot doesnt see channels
        //[RequirePermissions(DSharpPlus.Permissions.MoveMembers)]
        [Description("Moves a specified member to a chosen voice channel.")]
        public async Task Move(CommandContext ctx,
           [Description("Member username")] DiscordMember member,
           [Description("Channel name")] DiscordChannel channel)
        {
            await member.PlaceInAsync(channel).ConfigureAwait(false);
        }

        [Command("channelinfo")] //bot doesnt see channels and roles 
        public async Task ChannelInfo(CommandContext ctx)
        {
            var channels = ctx.Guild.Channels; //gets empty dictionary for some reason
            var roles = ctx.Guild.Roles; //gets empty dictionary for some reason

            await ctx.Channel.SendMessageAsync("channels: " + channels.Count.ToString() + " roles: " + roles.Count.ToString()).ConfigureAwait(false);
        }

        [Command("blacklist")] //TODO - doesnt work, tried writing words into a file
        //[RequirePermissions(DSharpPlus.Permissions.MoveMembers)]
        [Description("Blacklists a word.")]
        public async Task Blacklist(CommandContext ctx,
           [Description("Word to blacklist")] string word)
        {
            using var file = new StreamWriter("blacklisted.txt", append: true);
            await file.WriteLineAsync(word);

            await ctx.RespondAsync("Word has been blacklisted successfully.");

            ctx.Client.MessageCreated += async (s, e) =>
            {
                var blacklistedWords = File.ReadAllText("blacklisted.txt");

                if (blacklistedWords.Contains(e.Message.Content.ToLower()))
                {
                    await e.Message.DeleteAsync();
                    await e.Channel.SendMessageAsync(e.Message.Author.Mention + " watch your language!").ConfigureAwait(false);
                }
            };
        }
    }
}
