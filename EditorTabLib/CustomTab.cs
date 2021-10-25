using ADOFAI;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorTabLib
{
    public class CustomTabManager
    {
        internal static List<CustomTab> list = new List<CustomTab>();
        internal static Dictionary<int, CustomTab> byType = new Dictionary<int, CustomTab>();
        internal static Dictionary<string, CustomTab> byName = new Dictionary<string, CustomTab>();

        public static void AddTab(Sprite icon, int type, string name, string title, Type page)
        {
            if (icon == null)
                throw new ArgumentNullException("icon cannot be null!");
            if (name == null)
                throw new ArgumentNullException("name cannot be null!");
            if (page == null)
                throw new ArgumentNullException("page cannot be null!");
            if (byType.ContainsKey(type))
                throw new ArgumentException("customtab with type " + type + " already exists!");
            if (byName.ContainsKey(name))
                throw new ArgumentException("customtab named " + name + " already exists!");
            int max = 0;
            foreach (LevelEventType let in Enum.GetValues(typeof(LevelEventType)))
                if ((int)let > max)
                    max = (int)let;
            if (type <= max)
                throw new ArgumentException("type must be bigger than " + max);
            CustomTab tab = new CustomTab
            {
                icon = icon,
                type = type,
                name = name,
                title = title,
                page = page
            };
            list.Add(tab);
            byType.Add(type, tab);
            byName.Add(name, tab);
            Main.AddOrDeleteTab(tab, true);
            InspectorPanel settingsPanel = scnEditor.instance?.settingsPanel;
            if (settingsPanel == null)
                return;
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(settingsPanel.gc.prefab_propertiesPanel);
            gameObject.transform.SetParent(settingsPanel.panels, false);
            gameObject.name = tab.name;
            PropertiesPanel component = gameObject.GetComponent<PropertiesPanel>();
            component.levelEventType = (LevelEventType)tab.type;
            component.gameObject.SetActive(false);
            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(settingsPanel.gc.prefab_tab);
            gameObject2.transform.SetParent(settingsPanel.tabs, false);
            InspectorTab component2 = gameObject2.GetComponent<InspectorTab>();
            component2.Init((LevelEventType)tab.type, settingsPanel);
            component2.GetComponent<RectTransform>().AnchorPosY(8f - 68f * (settingsPanel.tabs.childCount - 1));
            component2.SetSelected(false);
            lock (GCS.levelEventsInfo)
                component.Init(settingsPanel, GCS.levelEventsInfo[tab.name]);
        }

        public static void DeleteTab(int type)
        {
            if (!byType.TryGetValue(type, out CustomTab tab))
                return;
            list.Remove(tab);
            byType.Remove(tab.type);
            byName.Remove(tab.name);
            Main.AddOrDeleteTab(tab, false);
            InspectorPanel settingsPanel = scnEditor.instance?.settingsPanel;
            if (settingsPanel == null)
                return;
            bool deleted = false;
            bool firstAfterDelete = true;
            LevelEventType selectAfterDelete = LevelEventType.SongSettings;
            for (int i = 0; i < settingsPanel.tabs.childCount; i++)
            {
                RectTransform rect = (RectTransform)settingsPanel.tabs.GetChild(i);
                InspectorTab component = rect.gameObject.GetComponent<InspectorTab>();
                if (component?.levelEventType == (LevelEventType)tab.type)
                {
                    UnityEngine.Object.Destroy(rect.gameObject);
                    deleted = true;
                    continue;
                }
                if (deleted)
                {
                    if (firstAfterDelete)
                    {
                        selectAfterDelete = component.levelEventType;
                        firstAfterDelete = false;
                    }
                    rect.AnchorPosY(8f - 68f * (i - 1));
                }
            }
            if (settingsPanel.selectedEventType == (LevelEventType)tab.type)
                settingsPanel.ShowPanel(selectAfterDelete);
        }

        public static void DeleteTab(string name)
        {
            if (!byName.TryGetValue(name, out CustomTab tab))
                return;
            DeleteTab(tab.type);
        }

        internal class CustomTab
        {

            public Sprite icon;
            public int type;
            public string name;
            public string title;
            public Type page;

            internal CustomTab()
            {
            }
        }
    }
}
