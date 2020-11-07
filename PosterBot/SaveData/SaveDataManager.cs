using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PosterBot.SaveData
{
	public static class SaveDataManager
	{
		public static SaveData saveData { get; set; }
		public static string saveDataPath { get; set; }

		public static void Initialize(string path)
		{
			if(!path.EndsWith("save-data.dat")) {
				throw new Exception("Trying to read save data from the wrong file! This can cause permanent damage to files! Aborting...");
			}
			saveDataPath = path;
			if (!File.Exists(path))
			{
				saveData = new SaveData();
				Save();
			}
			else
			{
				FileStream fs = new FileStream(path, FileMode.Open);
				BinaryFormatter formatter = new BinaryFormatter();
				saveData = (SaveData)formatter.Deserialize(fs);
			}
		}

		public static void Save()
		{
			FileStream fs = new FileStream(saveDataPath, FileMode.OpenOrCreate);
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(fs, saveData);
		}
	}
}
