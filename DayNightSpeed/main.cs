﻿using Common;
using Common.Configuration;

namespace DayNightSpeed
{
	public static class Main
	{
		internal static ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();

			Options.init();
			
			DayNightSpeedControl.init();
			DayNightCyclePatches.init();
		}
	}
}