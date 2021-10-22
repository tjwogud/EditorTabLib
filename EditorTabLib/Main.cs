using ADOFAI;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using static EditorTabLib.CustomTabManager;

namespace EditorTabLib
{
    public static class Main
    {
        private static UnityModManager.ModEntry.ModLogger Logger;
        private static Harmony harmony;
        private static bool IsEnabled = false;

        private static void Setup(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            //modEntry.OnGUI = OnGUI;

        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            IsEnabled = value;
            if (value)
            {
                harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                AddOrDeleteAllTabs(true);
            }
            else
            {
                harmony.UnpatchAll(modEntry.Info.Id);
                AddOrDeleteAllTabs(false);
            }
            return true;
        }

        internal static void AddOrDeleteAllTabs(bool flag)
        {
            if (GCS.settingsInfo == null)
                return;
            CustomTabManager.list.ForEach(tab =>
            {
                AddOrDeleteTab(tab, flag);
            });
        }

        internal static void AddOrDeleteTab(CustomTab tab, bool flag)
        {
            if (GCS.settingsInfo == null)
                return;
            GCS.levelEventTypeString[(LevelEventType)tab.type] = tab.name;
            GCS.levelEventsInfo[tab.name] = new LevelEventInfo
            {
                categories = new List<LevelEventCategory>(),
                executionTime = LevelEventExecutionTime.Special,
                name = tab.name,
                pro = false,
                propertiesInfo = new Dictionary<string, ADOFAI.PropertyInfo>(),
                type = (LevelEventType)tab.type
            };
            GCS.levelEventIcons[(LevelEventType)tab.type] = tab.icon;
            GCS.settingsInfo[tab.name] = new LevelEventInfo
            {
                categories = new List<LevelEventCategory>(),
                executionTime = LevelEventExecutionTime.Special,
                name = tab.name,
                propertiesInfo = new Dictionary<string, ADOFAI.PropertyInfo>(),
                type = (LevelEventType)tab.type
            };
        }

        /*
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
        }
        */
    }
}