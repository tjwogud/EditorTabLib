using ADOFAI;
using EditorTabLib.Utils;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;
using UnityModManagerNet;

namespace EditorTabLib
{
    internal static class Main
    {
        internal static UnityModManager.ModEntry.ModLogger Logger;
        private static Harmony harmony;

        private static void Setup(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
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
            if (!typeof(ADOStartup).Get<bool>("startup"))
                return;
            CustomTabManager.list.ForEach(tab =>
            {
                AddOrDeleteTab(tab, flag);
            });
        }

        internal static void AddOrDeleteTab(CustomTabManager.CustomTab tab, bool flag)
        {
            if (!typeof(ADOStartup).Get<bool>("startup"))
                return;
            if (flag)
            {
                LevelEventInfo levelEventInfo = new LevelEventInfo
                {
                    categories = new List<LevelEventCategory>(),
                    executionTime = LevelEventExecutionTime.Special,
                    name = tab.name,
                    propertiesInfo = new Dictionary<string, ADOFAI.PropertyInfo>(),
                    type = (LevelEventType)tab.type
                };
                if (tab.properties != null)
                    foreach (var dictionary in tab.properties)
                    {
                        ADOFAI.PropertyInfo propertyInfo = new ADOFAI.PropertyInfo(dictionary, levelEventInfo);
                        if (dictionary.TryGetValue("type", out object obj) && obj is string str && str == "Export" && dictionary.TryGetValue("default", out obj) && obj is UnityAction action)
                            propertyInfo.value_default = action;
                        propertyInfo.order = 0;
                        levelEventInfo.propertiesInfo.Add(propertyInfo.name, propertyInfo);
                    }
                GCS.levelEventTypeString[(LevelEventType)tab.type] = tab.name;
                GCS.levelEventIcons[(LevelEventType)tab.type] = tab.icon;
                GCS.settingsInfo[tab.name] = levelEventInfo;
            } else
            {
                GCS.levelEventTypeString.Remove((LevelEventType)tab.type);
                GCS.levelEventIcons.Remove((LevelEventType)tab.type);
                GCS.settingsInfo.Remove(tab.name);
            }
        }
    }
}