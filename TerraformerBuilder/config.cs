﻿using Common.Configuration;

namespace TerraformerBuilder
{
	class ModConfig: Config
	{
		public readonly bool bigInInventory = true;

#if DEBUG
		[AddToConsole]
		public readonly float forcedBuildTime = 1.0f;
#endif
	}
}