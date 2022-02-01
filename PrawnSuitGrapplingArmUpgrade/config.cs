﻿using Common;
using Common.Configuration;

namespace PrawnSuitGrapplingArmUpgrade
{
	[Field.BindConsole("ps_ga")]
	class ModConfig: Config
	{
		[Field.Range(0f, 5f)]
		public readonly float armCooldown = 0.5f; // default: 2.0f

		[Field.Range(35f, 100f)]
		public readonly float hookMaxDistance = 100f; // default: 35f

		[Field.Range(25f, 70f)]
		public readonly float hookSpeed = 60f; // default: 25f

		[Field.Range(15f, 50f)]
		public readonly float acceleration = 25f; // default: 15f

		[Field.Range(400f, 1000f)]
		public readonly float force = 600f; // default: 400f

		[Field.Range(0, 30)]
		public readonly int fragmentCountToUnlock = Mod.Consts.isGameSN? 4: 2; // unlock with vanilla arm if zero
	}
}
