using ADOFAI;
using EditorTabLib.Utils;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TinyJson;
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
            CustomEventManager.categories.ToList().ForEach(pair => GCS.eventCategoryIcons.Add((LevelEventCategory)pair.Key, pair.Value));
            CustomEventManager.list.ForEach(ev => AddOrDeleteTab(ev, flag));
            CustomTabManager.list.ForEach(tab => AddOrDeleteTab(tab, flag));
        }

        internal static void AddOrDeleteTab(CustomEventManager.CustomEvent ev, bool flag)
        {
            if (!typeof(ADOStartup).Get<bool>("startup"))
                return;
            if (flag)
            {
                LevelEventInfo levelEventInfo = new LevelEventInfo
                {
                    categories = ev.categories ?? new List<LevelEventCategory>(),
                    executionTime = LevelEventExecutionTime.Special,
                    name = ev.name,
                    propertiesInfo = new Dictionary<string, ADOFAI.PropertyInfo>(),
                    type = (LevelEventType)ev.type
                };
                if (ev.properties != null)
                    foreach (var dictionary in ev.properties)
                    {
                        ADOFAI.PropertyInfo propertyInfo = new ADOFAI.PropertyInfo(dictionary, levelEventInfo);
                        if (dictionary.TryGetValue("type", out object obj) && obj is string str && str == "Export" && dictionary.TryGetValue("default", out obj) && obj is UnityAction action)
                            propertyInfo.value_default = action;
                        propertyInfo.order = 0;
                        levelEventInfo.propertiesInfo.Add(propertyInfo.name, propertyInfo);
                    }
                GCS.levelEventTypeString[(LevelEventType)ev.type] = ev.name;
                GCS.levelEventIcons[(LevelEventType)ev.type] = ev.icon;
                if (ev is CustomTabManager.CustomTab)
                    GCS.settingsInfo[ev.name] = levelEventInfo;
                else
                {
                    GCS.levelEventsInfo[ev.name] = levelEventInfo;
                }
            } else
            {
                GCS.levelEventTypeString.Remove((LevelEventType)ev.type);
                GCS.levelEventIcons.Remove((LevelEventType)ev.type);
                GCS.settingsInfo.Remove(ev.name);
                GCS.levelEventsInfo.Remove(ev.name);
            }
        }
    }
}