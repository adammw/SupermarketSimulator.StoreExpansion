using __Project__.Scripts.FloorPaintSystem;
using __Project__.Scripts.Managers;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MyBox;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StoreExpansion
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Plugin.Log = Logger;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Plugin.Log.LogInfo($"Scene loaded: {scene.name}");
            if (scene.name == "Main Scene")
            {
                List<SectionSO> sections = Singleton<IDManager>.Instance.Sections;
                SectionSO sectionToClone = sections[sections.Count - 1];

                for (int i = 0; i < 15; i++)
                {
                    SectionSO newSection = ScriptableObject.CreateInstance<SectionSO>();
                    newSection.name = "SectionClone";
                    newSection.ID = sectionToClone.ID + i + 1;
                    newSection.Cost = sectionToClone.Cost;
                    newSection.LocalizedName = sectionToClone.LocalizedName;
                    newSection.RequiredStoreLevel = sectionToClone.RequiredStoreLevel;
                    newSection.RequiredSectionID = sectionToClone.ID + i;
                    newSection.DailyRentAddition = sectionToClone.DailyRentAddition;
                    sections.Add(newSection);
                }
            }
        }

        public static void CloneSections()
        {
            SectionManager sectionManager = Singleton<SectionManager>.Instance;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            // Clone Section
            Section[] sections = sectionManager.transform.GetComponentsInChildren<Section>();
            GameObject sectionToClone = sections[sections.Length - 1].gameObject;

            GameObject floorsContainer = GameObject.Find("/---GAME---/Store/Store &&/Floors");
            GameObject floorToClone = floorsContainer.transform.GetChild(floorsContainer.transform.childCount - 1).gameObject;

            GameObject ceilingsContainer = GameObject.Find("/---GAME---/Store/Store &&/Ceiling");
            GameObject ceilingToClone = ceilingsContainer.transform.GetChild(5).gameObject;

            for (int index = 0; index < 15; index++)
            {
                GameObject newSection = GameObject.Instantiate(sectionToClone, sectionToClone.transform.parent);
                newSection.name = $"SectionClone ({index})";

                for (int i = 0; i < newSection.transform.childCount; i++)
                {
                    Transform child = newSection.transform.GetChild(i);
                    for (int j = 0; j < child.childCount; j++)
                    {
                        Transform grandChild = child.GetChild(j);
                        grandChild.transform.localPosition = grandChild.transform.localPosition.OffsetXZ(-4 * ((index/5)+1), (4-index%5) * 4);
                    }
                }
                newSection.transform.SetParent(sectionManager.transform);

                // Clone Floor
                Plugin.Log.LogInfo($"cloning floor {floorToClone} - {floorToClone.transform.localPosition}");
                GameObject newFloor = GameObject.Instantiate(floorToClone, floorToClone.transform.parent);
                newFloor.name = $"FloorClone ({index})";
                newFloor.transform.localPosition = newFloor.transform.localPosition.OffsetXZ(-4 * ((index / 5) + 1), (4 - index%5) * 4);
                FloorTextureData defaultFloorTexData = Singleton<PaintManager>.Instance.GetTextureDataFromList(FloorTextureType.TYPE1_DEFAULT);
                newFloor.GetComponentsInChildren<PaintableFloor>().ForEach(pf => { if (pf.TextureData == null) { pf.Initialize(defaultFloorTexData); } });


                // Clone Ceiling
                Plugin.Log.LogInfo($"cloning ceiling {ceilingToClone} - {ceilingToClone.transform.parent}");
                GameObject newCeiling = GameObject.CreatePrimitive(PrimitiveType.Plane); //GameObject.Instantiate(ceilingToClone, ceilingToClone.transform.parent);
                newCeiling.name = $"Fake Ceiling ({index})";
                newCeiling.tag = ceilingToClone.tag;
                newCeiling.GetComponent<MeshRenderer>().material = ceilingToClone.GetComponent<MeshRenderer>().materials[1];
                newCeiling.transform.localRotation = Quaternion.Euler(0, 90, 180);
                newCeiling.transform.localScale = new Vector3(0.4f, 1, 0.4f);
                Vector3 offset = new Vector3(-2 + -4 * ((index / 5) + 1), 0,  6 + -4 * (index % 5));
                newCeiling.transform.localPosition = ceilingToClone.transform.localPosition + offset + new Vector3(0, -0.1f, 0); // ceiling is slightly sunken to architrave level
                newCeiling.transform.SetParent(ceilingsContainer.transform);

                // pointing up to block shadows
                GameObject newShadowBlocker = GameObject.CreatePrimitive(PrimitiveType.Plane); //GameObject.Instantiate(ceilingToClone, ceilingToClone.transform.parent);
                newShadowBlocker.name = $"Fake Ceiling Shadow Blocker ({index})";
                newShadowBlocker.GetComponent<MeshRenderer>().material = ceilingToClone.GetComponent<MeshRenderer>().materials[0];
                newShadowBlocker.transform.localRotation = Quaternion.Euler(0, 90, 0);
                newShadowBlocker.transform.localScale = new Vector3(0.42f, 1, 0.42f); // slightly larger than needed and higher to stop lighting up the architrave
                newShadowBlocker.transform.localPosition = ceilingToClone.transform.localPosition + offset + new Vector3(0, 0.1f, 0); 
                newShadowBlocker.transform.SetParent(ceilingsContainer.transform);
            }
        }

        public static void ListGameObjects(GameObject obj, string tree = "-")
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                Transform child = obj.transform.GetChild(i);
                Plugin.Log.LogMessage($"|{tree} {child.name} - ${child.transform.localPosition}");
                ListGameObjects(child.gameObject, tree + "-");
            }
        }
    }
}
