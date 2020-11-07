using Discord.Commands;
using Discord.WebSocket;
using PosterBot.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosterBot.Discord.Preconditions
{
	public class RequireBotManager : PreconditionAttribute
	{
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if(context.User is SocketGuildUser guildUser)
			{
				string botManagerRole = "";
				foreach(GuildData guildData in SaveDataManager.saveData.guildDataList)
				{
					if(guildData.guildId == guildUser.Guild.Id)
					{
						botManagerRole = guildData.botManagerRoleId.ToString();
					}
				}

				if (guildUser.Roles.Any(role => role.Permissions.Administrator || role.Id.ToString() == botManagerRole))
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
				else
				{
					return Task.FromResult(PreconditionResult.FromError("You must be a bot manager to run this command!"));
				}
			}
			else
			{
				return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command!"));
			}
		}
	}
}
