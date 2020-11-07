using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace PosterBot.SaveData
{
	[Serializable]
	public class SaveData
	{
		public List<GuildData> guildDataList { get; set; }
		public SaveData()
		{
			guildDataList = new List<GuildData>();
		}
	}

	[Serializable]
	public class GuildData
	{
		public ulong guildId;
		public ulong channelId;
		public ulong botManagerRoleId = 0;
		public ulong mentionRoleId = 0;
		public bool useMention = false;

		public GuildData(ulong guildId, ulong channelId)
		{
			this.guildId = guildId;
			this.channelId = channelId;
		}

		public GuildData(ulong guildId, ulong channelId, ulong botManagerRoleId, ulong mentionRoleId, bool useMention) : this(guildId, channelId)
		{
			this.botManagerRoleId = botManagerRoleId;
			this.mentionRoleId = mentionRoleId;
			this.useMention = useMention;
		}
	}
}
