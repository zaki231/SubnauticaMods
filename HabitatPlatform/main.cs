﻿using Common;
using Common.Harmony;
using Common.Crafting;

namespace HabitatPlatform
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();
			CraftHelper.patchAll();

			PersistentConsoleCommands.createGameObject<ConsoleCommands>();
		}
	}
}