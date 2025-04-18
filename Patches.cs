using __Project__.Scripts.FloorPaintSystem;
using __Project__.Scripts.Managers;
using HarmonyLib;
using MyBox;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements.UIR;

namespace StoreExpansion
{
    [HarmonyPatch]
    class GrowthTabPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GrowthTab), "Start")]
        public static void StartPrefix()
        {
            Plugin.Log.LogInfo("GrowthTab Start");
        }
    }

    [HarmonyPatch]
    class PlayerInteractionPatches
    {
        static bool hiddenBuildings = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerInteraction), "Update")]
        public static void UpdatePostfix()
        {
            bool isInside = Singleton<PlayerController>.Instance.IsInside;
            if (isInside != hiddenBuildings)
            {
                // show these buildings only when outside, they collide with inside the expanded store/storage
                GameObject.Find("---ENVIRONMENT---/NewYorkCity/Houses/NYC_Building_15_1/SM_NYC_Building_15")?.SetActive(!isInside);
                GameObject.Find("---ENVIRONMENT---/NewYorkCity/Houses/NYC_Building_15_1/P_Spa/SM_BushSpa")?.SetActive(!isInside);
                GameObject.Find("---ENVIRONMENT---/NewYorkCity/Houses/NYC_Building_8_1 (10)/SM_NYC_Building_8")?.SetActive(!isInside);
                GameObject.Find("---ENVIRONMENT---/NewYorkCity/Houses/NYC_Building_8_2 (6)/SM_NYC_Building_8")?.SetActive(!isInside);
                GameObject.Find("---ENVIRONMENT---/NewYorkCity/Houses/NYC_Building_6_1 1 (1)/SM_NYC_Building_6")?.SetActive(!isInside);
                GameObject.Find("---ENVIRONMENT---/NewYorkCity/Sidewalk &&/SideWalks/Sidewalk_4x4_A_01 (289)")?.SetActive(!isInside);

                hiddenBuildings = isInside;
            }
        }

    }

    [HarmonyPatch]
    class SectionManagerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SectionManager), "Awake")]
        public static void AwakePrefix(SectionManager __instance)
        {
            Plugin.CloneSections();
        }
    }

    [HarmonyPatch]
    class StorageSectionManagerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StorageSectionManager), "Awake")]
        public static void AwakePrefix(StorageSectionManager __instance)
        {
            Plugin.CloneStorageSections();
        }
    }
}