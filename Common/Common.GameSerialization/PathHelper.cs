﻿using System.IO;

namespace Common
{
	static partial class Paths
	{
		static public string savesPath
		{
			get
			{
#if DEBUG && !GAME_SAVEPATH
				string path = modRootPath + "{dbgsave}";
#else
				string path = Path.Combine(SaveLoadManager.GetTemporarySavePath(), Strings.modName);
#endif
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);														$"Using save path '{path}'".logDbg();

				return path;
			}
		}
	}
}