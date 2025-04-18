using __Project__.Scripts.FloorPaintSystem;
using __Project__.Scripts.Managers;
using __Project__.Scripts.WallPaintSystem;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MyBox;
using System.Collections.Generic;
using System.Linq;
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
                SectionSO sectionToClone = sections.Last();

                // clone sections for SectionManager
                for (int i = 0; i < 15; i++)
                {
                    SectionSO newSection = ScriptableObject.CreateInstance<SectionSO>();
                    newSection.ID = sectionToClone.ID + i + 1;
                    newSection.name = $"SectionClone {newSection.ID}";
                    newSection.Cost = sectionToClone.Cost;
                    newSection.LocalizedName = sectionToClone.LocalizedName;
                    newSection.RequiredStoreLevel = sectionToClone.RequiredStoreLevel;
                    newSection.RequiredSectionID = sectionToClone.ID + i;
                    newSection.DailyRentAddition = sectionToClone.DailyRentAddition;
                    sections.Add(newSection);
                }

                // move rear walls
                GameObject wallsContainer = GameObject.Find("/---GAME---/Store/Store &&/Walls");
                foreach (int i in new int[] { 1, 3, 6, 7, 8 })
                {
                    wallsContainer.transform.GetChild(i).localPosition = wallsContainer.transform.GetChild(i).localPosition.OffsetX(-12);
                }

                // clone side walls
                int lastWallID = Singleton<PaintManager>.Instance.walls.Last().paintData.wallID;

                var CloneWall = (int idx, GameObject wallsContainer, Vector3 offset) =>
                {
                    GameObject newWall = GameObject.Instantiate(wallsContainer.transform.GetChild(idx).gameObject);
                    newWall.transform.localPosition = newWall.transform.localPosition + offset;
                    newWall.transform.SetParent(wallsContainer.transform);

                    newWall.GetComponentsInChildren<PaintableWall>().ForEach(pw =>
                    {
                        int wallID = lastWallID++;
                        pw.paintData = new PaintData(wallID, 1);
                        Singleton<PaintManager>.Instance.walls.Add(pw);
                    });                    
                };

                CloneWall(5, wallsContainer, new Vector3(-4, 0, 8));
                CloneWall(5, wallsContainer, new Vector3(-8, 0, 8));
                CloneWall(5, wallsContainer, new Vector3(-12, 0, 8));

                CloneWall(9, wallsContainer, new Vector3(-4, 0, 8));
                CloneWall(9, wallsContainer, new Vector3(-8, 0, 8));
                CloneWall(9, wallsContainer, new Vector3(-12, 0, 8));

                // clone storage sections for StorageSectionManager
                List<StorageSO> storageSections = Singleton<IDManager>.Instance.StorageSections;
                StorageSO storageSectionToClone = storageSections.Last();
                for (int i = 0; i < 9; i++)
                {
                    StorageSO newSection = ScriptableObject.CreateInstance<StorageSO>();
                    newSection.ID = storageSectionToClone.ID + i + 1;
                    newSection.name = $"Storage Section {newSection.ID}";
                    newSection.Cost = storageSectionToClone.Cost;
                    newSection.LocalizedName = storageSectionToClone.LocalizedName;
                    newSection.RequiredStoreLevel = storageSectionToClone.RequiredStoreLevel;
                    newSection.RequiredSectionID = storageSectionToClone.ID + i;
                    newSection.DailyRentAddition = storageSectionToClone.DailyRentAddition;
                    storageSections.Add(newSection);
                }

                // move rear walls
                GameObject storageWallsContainer = GameObject.Find("/---GAME---/Store/Storage &&/Walls");
                foreach (int i in new int[] { 13, 14, 15 })
                {
                    storageWallsContainer.transform.GetChild(i).localPosition = storageWallsContainer.transform.GetChild(i).localPosition.OffsetX(-4 * 3);
                }

                // clone side walls
                CloneWall(7, storageWallsContainer, new Vector3(-4, 0, 8));
                CloneWall(7, storageWallsContainer, new Vector3(-8, 0, 8));
                CloneWall(7, storageWallsContainer, new Vector3(-12, 0, 8));

                CloneWall(12, storageWallsContainer, new Vector3(-4, 0, 8));
                CloneWall(12, storageWallsContainer, new Vector3(-8, 0, 8));
                CloneWall(12, storageWallsContainer, new Vector3(-12, 0, 8));
            }
        }

        public static void CloneSections()
        {
            SectionManager sectionManager = Singleton<SectionManager>.Instance;
            
            Section[] sections = sectionManager.transform.GetComponentsInChildren<Section>();
            GameObject sectionToClone = sections[sections.Length - 1].gameObject;

            GameObject floorsContainer = GameObject.Find("/---GAME---/Store/Store &&/Floors");
            GameObject floorToClone = floorsContainer.transform.GetChild(floorsContainer.transform.childCount - 1).gameObject;

            GameObject ceilingsContainer = GameObject.Find("/---GAME---/Store/Store &&/Ceiling");
            GameObject ceilingToClone = ceilingsContainer.transform.GetChild(5).gameObject;

            for (int index = 0; index < 15; index++)
            {
                // Clone Section
                GameObject newSection = GameObject.Instantiate(sectionToClone, sectionToClone.transform.parent);
                newSection.name = $"SectionClone ({index})";
                newSection.transform.SetParent(sectionManager.transform);

                // move something?
                for (int i = 0; i < newSection.transform.childCount; i++)
                {
                    Transform child = newSection.transform.GetChild(i);
                    for (int j = 0; j < child.childCount; j++)
                    {
                        Transform grandChild = child.GetChild(j);
                        grandChild.transform.localPosition = grandChild.transform.localPosition.OffsetXZ(-4 * ((index/5)+1), (4-index%5) * 4);
                    }
                }

                // add walls to the paint manager for loading/saving wall paint
                int lastWallID = Singleton<PaintManager>.Instance.walls.Last().paintData.wallID;
                newSection.GetComponentsInChildren<PaintableWall>().ForEach(pw =>
                {
                    int wallID = lastWallID++;
                    pw.paintData = new PaintData(wallID, 1);
                    Singleton<PaintManager>.Instance.walls.Add(pw);
                });

                // Clone Floor
                Plugin.Log.LogInfo($"cloning floor {floorToClone} - {floorToClone.transform.localPosition}");
                GameObject newFloor = GameObject.Instantiate(floorToClone, floorToClone.transform.parent);
                newFloor.name = $"FloorClone ({index})";
                newFloor.transform.localPosition = newFloor.transform.localPosition.OffsetXZ(-4 * ((index / 5) + 1), (4 - index%5) * 4);
                FloorTextureData defaultFloorTexData = Singleton<PaintManager>.Instance.GetTextureDataFromList(FloorTextureType.TYPE1_DEFAULT);
                int lastFloorID = Singleton<PaintManager>.Instance.floors.Last().floorSaveData.floorId;
                newFloor.GetComponentsInChildren<PaintableFloor>().ForEach(pf => {
                    if (pf.TextureData == null) { pf.Initialize(defaultFloorTexData); }
                    pf.floorSaveData = new FloorSaveData(lastFloorID++, FloorTextureType.TYPE1_DEFAULT);

                    Singleton<PaintManager>.Instance.floors.Add(pf);
                });


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

        // Triggered by prefix patch to StorageSectionManager.Awake, needs to be called before it stores all child components in m_StorageSections
        public static void CloneStorageSections()
        {
            StorageSectionManager sectionManager = Singleton<StorageSectionManager>.Instance;

            
            StorageSection[] sections = sectionManager.transform.GetComponentsInChildren<StorageSection>();
            GameObject sectionToClone = sections.Last().gameObject;

            GameObject floorsContainer = GameObject.Find("/---GAME---/Store/Storage &&/Floors");
            GameObject floorToClone = floorsContainer.transform.GetChild(floorsContainer.transform.childCount - 1).gameObject;

            GameObject ceilingsContainer = GameObject.Find("/---GAME---/Store/Storage &&/Ceiling");
            GameObject ceilingToClone = ceilingsContainer.transform.GetChild(ceilingsContainer.transform.childCount - 1).gameObject;

            for (int index = 0; index < 9; index++)
            {
                // Clone StorageSection
                GameObject newSection = GameObject.Instantiate(sectionToClone, sectionToClone.transform.parent);
                newSection.name = $"SectionClone ({index})";
                newSection.transform.SetParent(sectionManager.transform);
                newSection.transform.localPosition = sectionToClone.transform.localPosition.OffsetXZ(-4 * (1+(index/3)), -4 * (2 - index%3));

                // Clone floor
                GameObject newFloor = GameObject.Instantiate(floorToClone, floorToClone.transform.parent);
                newFloor.name = $"FloorClone ({index})";
                newFloor.transform.localPosition = newFloor.transform.localPosition.OffsetXZ(-4 * (1+(index/3)), -4 * (2 - index%3));
                FloorTextureData defaultFloorTexData = Singleton<PaintManager>.Instance.GetTextureDataFromList(FloorTextureType.TYPE1_DEFAULT);
                int lastFloorID = Singleton<PaintManager>.Instance.floors.Last().floorSaveData.floorId;
                newFloor.GetComponentsInChildren<PaintableFloor>().ForEach(pf => {
                    if (pf.TextureData == null) { pf.Initialize(defaultFloorTexData); }
                    pf.floorSaveData = new FloorSaveData(lastFloorID++, FloorTextureType.TYPE1_DEFAULT);

                    Singleton<PaintManager>.Instance.floors.Add(pf);
                });

                // Create ceiling
                Plugin.Log.LogInfo($"cloning ceiling {ceilingToClone} - {ceilingToClone.transform.parent}");
                GameObject newCeiling = GameObject.CreatePrimitive(PrimitiveType.Plane); //GameObject.Instantiate(ceilingToClone, ceilingToClone.transform.parent);
                newCeiling.name = $"Fake Ceiling ({index})";
                newCeiling.tag = ceilingToClone.tag;
                newCeiling.GetComponent<MeshRenderer>().material = ceilingToClone.GetComponent<MeshRenderer>().materials[1];
                newCeiling.transform.localRotation = Quaternion.Euler(0, 90, 180);
                newCeiling.transform.localScale = new Vector3(0.4f, 1, 0.4f);
                Vector3 offset = new Vector3(-4 * (1 + (index / 3)) + 2, 0,  -4 * (2 - index % 3) + 6);
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
