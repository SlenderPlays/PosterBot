using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace PosterBot.Discord.Modules
{
	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		private readonly CommandService _service;
		
		public HelpModule(CommandService service)
		{
			_service = service;
		}

		[Command("help")]
		[Summary("Get info about the commands available to use.")]
		public async Task HelpAsync()
		{
			string prefix = ConfigManager.config.botPrefix;
			EmbedBuilder builder = new EmbedBuilder()
			{
				Description = "These are the commands you can use:"
			};

			foreach (ModuleInfo module in _service.Modules)
			{
				string description = "";
				foreach (CommandInfo cmd in module.Commands)
				{
					PreconditionResult result = await cmd.CheckPreconditionsAsync(Context);
					if (result.IsSuccess)
					{
						foreach(string alias in cmd.Aliases)
						{
							string paramInfo = "";
							foreach(ParameterInfo param in cmd.Parameters)
							{
								paramInfo += $"<{param.Name}> ";
							}
							description += $"{prefix}{alias} {paramInfo}- {cmd.Summary}\n";
						}
					}
				}

				if (!string.IsNullOrWhiteSpace(description))
				{
					builder.Description += $"\n{description}";
				}
			}

			await ReplyAsync("", false, builder.Build());
		}

		[Command("help")]
		[Summary("Get info about a specific command.")]
		public async Task HelpAsync([Summary("The command you wish to know more about")]string command)
		{
			SearchResult result = _service.Search(Context, command);

			if (!result.IsSuccess)
			{
				await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
				return;
			}

			string prefix = ConfigManager.config.botPrefix;
			EmbedBuilder builder = new EmbedBuilder()
			{
				Description = "Here are some matching results:\n"
			};

			foreach (CommandMatch match in result.Commands)
			{
				CommandInfo cmd = match.Command;

				builder.Description += $"**{cmd.Name}**\n";
				
				var aliases = cmd.Aliases.Except(new string[] { cmd.Name });
				if (aliases.Count() != 0)
				{
					builder.Description += $"Also known as: {string.Join(", ", aliases)}\n";
				}

				builder.Description += "\n";
				builder.Description += cmd.Summary;
				builder.Description += "\n";
				string commandUsage = $"Usage: `{prefix}{cmd.Name} ";
				foreach (ParameterInfo param in cmd.Parameters)
				{
					commandUsage += $"<{param.Name}> ";
				}
				commandUsage = commandUsage.Trim() + "`";
				builder.Description += $"{commandUsage}\n";
				builder.Description += "\n";
				foreach (ParameterInfo param in cmd.Parameters)
				{
					builder.Description += $"`<{param.Name}>` - {param.Summary}\n";
				}
			}

			await ReplyAsync("", false, builder.Build());
		}
	}
}

