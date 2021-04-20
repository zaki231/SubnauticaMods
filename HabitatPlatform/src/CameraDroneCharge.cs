﻿using Common.Crafting;

namespace HabitatPlatform
{
	class MapRoomCameraCharge: PoolCraftableObject
	{
		protected override TechInfo getTechInfo()
		{
			TechInfo techInfo = new(new TechInfo.Ing(TechType.MapRoomCamera));
			techInfo.linkedItems.Add(TechType.MapRoomCamera);

			return techInfo;
		}

		public override void patch()
		{
			if (!Main.config.chargeCameras)
				return;

			register(L10n.ids_ChargeCamera, L10n.ids_ChargeCameraDesc, TechType.MapRoomCamera);

			addCraftingNodeTo(CraftTree.Type.MapRoom, "", TechType.MapRoomCamera);
			setTechTypeForUnlock(TechType.MapRoomCamera);
		}

		protected override void initPrefabPool() => addPrefabToPool(TechType.MapRoomCamera);
	}
}