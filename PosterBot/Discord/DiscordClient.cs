using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Reddit;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using PosterBot.SaveData;
using System.Net;

namespace PosterBot.Discord
{
	class DiscordClient
	{
		public static DiscordSocketClient socketClient { get; set; } = new DiscordSocketClient();
		public static CommandService _commands { get; set; }
		public static IServiceProvider _services { get; set; }
		public DiscordClient()
		{
			_commands = new CommandService();
			_services = new ServiceCollection()
				.AddSingleton(socketClient)
				.AddSingleton(_commands)
				.BuildServiceProvider();

			socketClient.Log += SocketClient_Log;
			socketClient.MessageReceived += SocketClient_MessageReceived;
			
		}

		public async Task RunAsync()
		{
			await socketClient.LoginAsync(TokenType.Bot, ConfigManager.config.token);
			await socketClient.StartAsync();

			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

			RedditClient redditClient = new RedditClient(
				appId: ConfigManager.config.redditAppId,
				appSecret: ConfigManager.config.redditAppSecret,
				refreshToken: ConfigManager.config.redditRefreshToken);

			foreach (string sub in ConfigManager.config.subredditsWatched)
			{
				Subreddit subreddit = redditClient.Subreddit(sub);
				subreddit.Posts.GetNew(limit:100000);
				subreddit.Posts.NewUpdated += NewRedditPost;
				subreddit.Posts.MonitorNew(monitoringDelayMs: 60000);
				//subreddit.Posts.MonitorNew();
			}

			await socketClient.SetGameAsync(
				name: $"prefix: {ConfigManager.config.botPrefix}", 
				streamUrl: null,
				type: ActivityType.Listening);

			await Task.Delay(-1);
		}

		private async Task SocketClient_MessageReceived(SocketMessage socketMessage)
		{
			try
			{
				if (socketMessage == null)
				{
					Console.WriteLine("[Discord] [!..] A scoket message was null, ignoring");
					return;
				}

				SocketUserMessage message = socketMessage as SocketUserMessage;
				if (message.Author.Id == socketClient.CurrentUser.Id)
					return;
				if (message.Author.IsBot)
					return;

				int ArgPos = 0;

				if (message.HasStringPrefix(ConfigManager.config.botPrefix, ref ArgPos))
				{
					SocketCommandContext context = new SocketCommandContext(socketClient, message);
					if (!context.IsPrivate && ConfigManager.config.guildWhitelist.Contains(context.Guild.Id.ToString()))
					{
						Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] ({context.Guild.Name}) {context.User} : {context.Message}");

						var result = await _commands.ExecuteAsync(context, ArgPos, _services);

						if (!result.IsSuccess)
						{
							Console.WriteLine("Last command was an error! " + "-> " + "Reason:" + result.ErrorReason + " Value: " + result.Error.Value);

							if (ConfigManager.config.verbose)
							{
								await context.Channel.SendMessageAsync($"**Uh-Oh!** *{result.ErrorReason}*");
							}
						}
						else
						{
							try
							{
								//await context.Message.DeleteAsync();
							}
							catch(Exception)
							{
								Console.WriteLine("Could not delete message");
							}
						}
					}
					else
					{
						Console.WriteLine(String.Format($"Un-whitelisted guild requested command: \nID: {context.Guild.Id}\nName: {context.Guild.Name}\nOwner: {context.Guild.Owner}\nOwnerID: {context.Guild.OwnerId}\nChannel Name: {context.Channel.Name}\nChannel ID: {context.Channel.Id}"));
						await context.Channel.SendMessageAsync("I don't know you, I am leaving.");
						await context.Guild.LeaveAsync();
					}
				}
			}
			catch(Exception e)
			{
				Console.WriteLine("[!!.] Error caught during message parsing!");
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

		private async void NewRedditPost(object sender, PostsUpdateEventArgs e)
		{
			foreach (var post in e.Added)
			{
				// Old post... somehow
				if (post.Created.AddHours(1) < DateTime.Now) { continue; }
				foreach (var guildData in SaveDataManager.saveData.guildDataList)
				{
					try
					{
						if (ConfigManager.config.guildWhitelist.Contains(guildData.guildId.ToString()))
						{
							LinkPost linkPost = post as LinkPost;
							
							EmbedBuilder builder = new EmbedBuilder()
							{
								Title = $"[r/{post.Subreddit}] {post.Title}",
								Url = $"https://www.reddit.com{post.Permalink}"
							};
							if(linkPost != null)
							{
								if (linkPost.URL.Contains("i.redd.it"))
								{
									builder.ImageUrl = linkPost.URL;
								}
								else if (linkPost.URL.Contains("v.redd.it"))
								{
									builder.ImageUrl = linkPost.URL;
									builder.Title += "[Video]";
								}
								else
								{
									builder.ImageUrl = linkPost.Thumbnail;
									builder.Url = linkPost.URL;
									builder.Description = $"[Permalink to post]({$"https://www.reddit.com{linkPost.Permalink}"}){Environment.NewLine}";
								}
							}
							else
							{
								if(post.Listing.SelfText.Length >= 301)
								{ 
									builder.Description = WebUtility.HtmlDecode(post.Listing.SelfText.Substring(0,300)) + "...";
								}
								else
								{
									builder.Description = WebUtility.HtmlDecode(post.Listing.SelfText);
								}
							}
							builder.Footer = new EmbedFooterBuilder() {
								 Text = $"by u/{post.Author}"
							};
							string mention = guildData.useMention ? $"<@&{guildData.mentionRoleId}>" : "";
							await socketClient.GetGuild(guildData.guildId).GetTextChannel(guildData.channelId).SendMessageAsync(text:mention,embed: builder.Build());
						}
						else
						{
							await socketClient.GetGuild(guildData.guildId).LeaveAsync();
						}
					}
					catch(Exception ex)
					{
						Console.WriteLine("[!!.] Had an error while trying to handle new post." + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
					}
				}
			}
		}

		private Task SocketClient_Log(LogMessage arg)
		{
			Console.WriteLine("[Discord] " + arg.ToString());
			return Task.CompletedTask;
		}
	}
}
