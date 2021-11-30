﻿using UnityEngine;

using Common;
using Common.Crafting;

namespace StasisTorpedo
{
	partial class StasisTorpedo
	{
		class StasisExplosion: MonoBehaviour
		{
			StasisSphere sphere;
			static GameObject stasisSpherePrefab;

			public static void initPrefab(GameObject stasisRiflePrefab)
			{
				if (stasisSpherePrefab)
					return;

				if (!stasisRiflePrefab)
				{
					"StasisExplosion.initPrefab: invalid prefab for StasisRifle!".logError();
					return;
				}

				stasisSpherePrefab = PrefabUtils.storePrefabCopy(stasisRiflePrefab.GetComponent<StasisRifle>().effectSpherePrefab);
				stasisSpherePrefab.transform.position = Vector3.zero;
			}

			void Start()
			{
				if (!stasisSpherePrefab)
				{
					"StasisExplosion.Start: invalid prefab for StasisSphere!".logError();

					Destroy(gameObject);
					return;
				}

				sphere = gameObject.createChild(stasisSpherePrefab).GetComponent<StasisSphere>();

				sphere.time = 10f; // vanilla: min = 4, max = 20
				sphere.radius = 5f; // vanilla: min = 1, max = 10
				sphere.fieldEnergy = 1f;

				sphere.go.SetActive(true); // 'go' is deactivated in Bullet.Awake
				sphere.EnableField();
			}

			void Update()
			{
				if (!sphere.fieldEnabled)
					Destroy(gameObject);
			}
#if DEBUG
			void OnDestroy() => "StasisExplosion.OnDestroy".logDbg();
#endif
		}

		public static TorpedoType torpedoType { get; private set; }

		public static void initPrefab(GameObject gasTorpedoPrefab)
		{
			if (torpedoType != null)
				return;

			if (!gasTorpedoPrefab)
			{
				"StasisTorpedo.initPrefab: invalid prefab for GasTorpedo!".logError();
				return;
			}

			var torpedoPrefab = PrefabUtils.storePrefabCopy(gasTorpedoPrefab);
			var seamothTorpedo = torpedoPrefab.GetComponent<SeamothTorpedo>();

			var explosionPrefab = PrefabUtils.storePrefabCopy(seamothTorpedo.explosionPrefab);
			explosionPrefab.destroyComponent<GasPod>();
			explosionPrefab.AddComponent<StasisExplosion>();

			seamothTorpedo.explosionPrefab = explosionPrefab;

			torpedoType = new() { techType = TechType, prefab = torpedoPrefab };
		}
	}
}