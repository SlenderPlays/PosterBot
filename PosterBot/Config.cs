using System;
using System.Collections.Generic;

namespace PosterBot
{
	public class Config
	{
		public string token { get; set; }
		public string botPrefix { get; set; }
		public List<string> guildWhitelist { get; set; }
		public List<string> subredditsWatched  { get; set; }
		public bool verbose { get; set; } = false;
		public string redditAppId { get; set; }
		public string redditAppSecret { get; set; }
		public string redditRefreshToken { get; set; }
	}

	public static class ConfigManager
	{
		public static Config config { get; set; }
	}

	public class ConfigException : Exception
	{
		public ConfigException()
			: base("The config file has invalid data or is missing one or more required values such as 'token','botPrefix','guildWhitelist'(List), " +
				"'subredditsWatched (List)'. You may also want to use 'verbose' for more debug information.")
		{ }

		public ConfigException(string message) : base(message)
		{
		}

		public ConfigException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
