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
                // show these buildings only when outside, they collide with inside the expanded store
                GameObject.Find("---ENVIRONMENT---/NewYorkCity/Houses/NYC_Building_15_1/SM_NYC_Building_15")?.SetActive(!isInside);
                GameObject.Find("---ENVIRONMENT---/NewYorkCity/Houses/NYC_Building_8_1 (10)/SM_NYC_Building_8")?.SetActive(!isInside);
                GameObject.Find("---ENVIRONMENT---/NewYorkCity/Houses/NYC_Building_8_2 (6)/SM_NYC_Building_8")?.SetActive(!isInside);
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
            // move walls
            GameObject wallsContainer = GameObject.Find("/---GAME---/Store/Store &&/Walls");
            foreach (int i in new int[] { 1, 3, 6, 7, 8 })
            {
                wallsContainer.transform.GetChild(i).localPosition = wallsContainer.transform.GetChild(i).localPosition.OffsetX(-12);
            }

            var CloneWall = (int idx, Vector3 offset) =>
            {
                GameObject newWall = GameObject.Instantiate(wallsContainer.transform.GetChild(idx).gameObject);
                newWall.transform.localPosition = newWall.transform.localPosition + offset;
                newWall.transform.SetParent(wallsContainer.transform);
            };

            CloneWall(5, new Vector3(-4, 0, 8));
            CloneWall(5, new Vector3(-8, 0, 8));
            CloneWall(5, new Vector3(-12, 0, 8));

            CloneWall(9, new Vector3(-4, 0, 8));
            CloneWall(9, new Vector3(-8, 0, 8));
            CloneWall(9, new Vector3(-12, 0, 8));

            Plugin.CloneSections();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SectionManager), "Start")]
        public static void StartPrefix()
        {
            ListSections();
        }

        public static void ListSections()
        {
            SectionManager sectionManager = Singleton<SectionManager>.Instance;
            Plugin.Log.LogMessage("Sections: ");
            foreach (var section in sectionManager.sections)
            {
                Plugin.Log.LogMessage($"{section} - ${section.gameObject} - ${section.gameObject.transform.localPosition}");
                Traverse t = new Traverse(section);

                GameObject[] toBeDisabledObjs = t.Field("m_ToBeDisabled").GetValue<GameObject[]>();
                Plugin.Log.LogMessage("ToBeDisabled: ");
                foreach (var obj in toBeDisabledObjs)
                {
                    Plugin.Log.LogMessage($"{obj} - ${obj.transform.localPosition}");
                    Plugin.ListGameObjects(obj);
                }

                GameObject[] toBeEnabledObjs = t.Field("m_ToBeEnabled").GetValue<GameObject[]>();
                Plugin.Log.LogMessage("ToBeEnabled: ");
                foreach (var obj in toBeEnabledObjs)
                {
                    Plugin.Log.LogMessage($"{obj} - ${obj.transform.localPosition}");
                    Plugin.ListGameObjects(obj);
                }
            }
        }
    }
}