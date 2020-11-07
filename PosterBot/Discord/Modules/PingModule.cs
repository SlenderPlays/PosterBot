using Discord.Commands;
using PosterBot.Discord.Preconditions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace PosterBot.Discord.Modules
{
	public class PingModule : ModuleBase<SocketCommandContext>
	{
		[Command("ping")]
		[Summary("Pong! Check if the bot is alive.")]
		[RequireBotManager()]
		public async Task PingAsync()
		{
			await ReplyAsync(":white_check_mark: **Bot Online**");
		}
	}
}
