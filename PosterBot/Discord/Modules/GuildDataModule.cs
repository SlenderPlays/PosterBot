using Discord.Commands;
using PosterBot.Discord.Preconditions;
using PosterBot.SaveData;
using System.Threading.Tasks;

namespace PosterBot.Discord.Modules
{
	public class GuildDataModule : ModuleBase<SocketCommandContext>
	{
		[Command("setChannel")]
		[Summary("Set the channel you want your new posts delivered to you. Bot must have permission to write to it.")]
		[RequireBotManager()]
		public async Task UpdateChannelAsync([Summary("The channel. Make sure you mention the channel with '#'.")] string channelMention)
		{

			ulong id;
			if (ulong.TryParse(channelMention.Replace("<", "").Replace(">", "").Replace("#", ""), out id))
			{
				foreach (var guildData in SaveDataManager.saveData.guildDataList)
				{
					if (guildData.guildId == Context.Guild.Id)
					{
						guildData.channelId = id;
						SaveDataManager.Save();
						await ReplyAsync("Updated channel!");
						return;
					}
				}

				SaveDataManager.saveData.guildDataList.Add(new GuildData(Context.Guild.Id, id));
				SaveDataManager.Save();
				await ReplyAsync("Updated channel!");
			}
			else
			{
				await ReplyAsync("Can't get channel. Be sure to mention it with '#'.");
			}
		}
		[Command("setBotManager")]
		[Summary("Set the role you want to be able to manage the bot.")]
		[RequireBotManager()]
		public async Task UpdateBotManagerAsync([Summary("The role. Make sure you mention it.")] string roleMention)
		{
			ulong id;
			if (ulong.TryParse(roleMention.Replace("<", "").Replace(">", "").Replace("&", "").Replace("@", ""), out id))
			{
				foreach (var guildData in SaveDataManager.saveData.guildDataList)
				{
					if (guildData.guildId == Context.Guild.Id)
					{
						guildData.botManagerRoleId = id;
						SaveDataManager.Save();
						await ReplyAsync("Updated settings!");
						return;
					}
				}
				await ReplyAsync("To ensure consistency and accuracy, please first set the channel where you want the updates.");
			}
			else
			{
				await ReplyAsync("Can't get role. Be sure to mention it.");
			}
		}
		[Command("setMentionRole")]
		[Summary("Set the role you want to be mentioned by the bot.")]
		[RequireBotManager()]
		public async Task UpdateMentionRoleAsync([Summary("The role. Make sure you mention it.")] string roleMention)
		{
			ulong id;
			if (ulong.TryParse(roleMention.Replace("<", "").Replace(">", "").Replace("&", "").Replace("@", ""), out id))
			{
				foreach (var guildData in SaveDataManager.saveData.guildDataList)
				{
					if (guildData.guildId == Context.Guild.Id)
					{
						guildData.mentionRoleId = id;
						SaveDataManager.Save();
						await ReplyAsync("Updated settings!");
						return;
					}
				}
				await ReplyAsync("To ensure consistency and accuracy, please first set the channel where you want the updates.");
			}
			else
			{
				await ReplyAsync("Can't get role. Be sure to mention it.");
			}
		}
		[Command("setMention")]
		[Summary("Set if you want the bot to mention people when there is a new post.")]
		[RequireBotManager()]
		public async Task UpdateMentionRoleAsync([Summary("True if you want the bot to mention / False if you don't")] bool mention)
		{

			foreach (var guildData in SaveDataManager.saveData.guildDataList)
			{
				if (guildData.guildId == Context.Guild.Id)
				{
					guildData.useMention = mention;
					SaveDataManager.Save();
					await ReplyAsync("Updated settings!");
					return;
				}
			}
			await ReplyAsync("To ensure consistency and accuracy, please first set the channel where you want the updates.");
		}
	}
}
