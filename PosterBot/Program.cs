using CommandLine;
using Newtonsoft.Json;
using PosterBot.Discord;
using System;
using System.IO;
using Reddit;
using PosterBot.SaveData;

namespace PosterBot
{
	class Program
	{
		public class CLOptions
		{
			[Option('c', "config", Required = true, HelpText = "The path to the config.json FILE.")]
			public string configPath { get; set; }
			[Option('d', "save-data", Required = false, Default = "", HelpText = "The path to the save-data.dat FILE.")]
			public string saveDataPath { get; set; }
		}
		static void Main(string[] args)
		{
			Parser.Default.ParseArguments<CLOptions>(args).WithParsed(options =>
			{
				Config config = GetConfig(options.configPath);
				if (config == null)
				{
					return;
				}
				CheckConfig(config);
				ConfigManager.config = config;

				SaveDataManager.Initialize(options.saveDataPath);

				DiscordClient discordClient = new DiscordClient();
				// TODO: To be put into a single thread if execution is still needed here
				discordClient.RunAsync().GetAwaiter().GetResult();
			});
		}

		private static void CheckConfig(Config config)
		{
			if (String.IsNullOrWhiteSpace(config.token) ||
				String.IsNullOrWhiteSpace(config.botPrefix) ||
				config.guildWhitelist == null ||
				config.subredditsWatched == null ||
				String.IsNullOrWhiteSpace(config.redditAppId) ||
				String.IsNullOrWhiteSpace(config.redditAppSecret) ||
				String.IsNullOrWhiteSpace(config.redditRefreshToken))
			{
				throw new ConfigException();
			}
		}

		private static Config GetConfig(string configPath)
		{
			string json = "";
			try
			{
				using (StreamReader sr = File.OpenText(configPath))
				{
					json = sr.ReadToEnd();
				}
			}
			catch (FileNotFoundException)
			{
				Console.WriteLine("[!!!] No config.json file found! Either create a config.json file inside the same folder as the executable or run with argument the absolute path of the config file. Shuttting down...");
				return null;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);

				return null;
			}

			return JsonConvert.DeserializeObject<Config>(json);
		}
	}
}
