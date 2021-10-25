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
            AddTab(icon, type, name, title, page, -1);
        }

        public static void AddTab(Sprite icon, int type, string name, string title, Type page, int index)
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
                page = page,
                index = index
            };
            list.Add(tab);
            byType.Add(type, tab);
            byName.Add(name, tab);
            Main.AddOrDeleteTab(tab, true);
            InspectorPanel settingsPanel = scnEditor.instance?.settingsPanel;
            if (settingsPanel == null)
                return;
            GameObject gameObject = UnityEngine.Object.Instantiate(settingsPanel.gc.prefab_propertiesPanel);
            gameObject.name = tab.name;
            PropertiesPanel component = gameObject.GetComponent<PropertiesPanel>();
            component.levelEventType = (LevelEventType)tab.type;
            component.gameObject.SetActive(false);
            GameObject gameObject2 = UnityEngine.Object.Instantiate(settingsPanel.gc.prefab_tab);
            InspectorTab component2 = gameObject2.GetComponent<InspectorTab>();
            component2.Init((LevelEventType)tab.type, settingsPanel);
            component2.SetSelected(false);
            lock (GCS.levelEventsInfo)
                component.Init(settingsPanel, GCS.levelEventsInfo[tab.name]);

            if (index == -1)
            {
                component2.GetComponent<RectTransform>().AnchorPosY(8f - 68f * (settingsPanel.tabs.childCount - 1));
                gameObject2.transform.SetParent(settingsPanel.tabs, false);
            }
            else
            {
                if (settingsPanel.tabs.childCount <= index)
                    index = settingsPanel.tabs.childCount - 1;
                List<InspectorTab> tabs = new List<InspectorTab>();
                for (int i = index; i < settingsPanel.tabs.childCount; i++)
                {
                    InspectorTab tab2 = settingsPanel.tabs.GetChild(i).GetComponent<InspectorTab>();
                    if (tab2 == null || tab2.levelEventType == (LevelEventType)tab.type)
                        continue;
                    tabs.Add(tab2);
                }
                tabs.ForEach(tab2 => tab2.transform.SetParent(null, false));
                component2.GetComponent<RectTransform>().AnchorPosY(8f - 68f * index);
                gameObject2.transform.SetParent(settingsPanel.tabs, false);
                foreach (InspectorTab tab2 in tabs)
                {
                    tab2.GetComponent<RectTransform>().AnchorPosY(8f - 68f * ++index);
                    tab2.transform.SetParent(settingsPanel.tabs, false);
                }
            }
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

        internal static void SortTab()
        {
            InspectorPanel settingsPanel = scnEditor.instance?.settingsPanel;
            if (settingsPanel == null)
                return;
            Dictionary<int, InspectorTab> dict = new Dictionary<int, InspectorTab>();
            List<InspectorTab> tabs = new List<InspectorTab>();
            for (int i = 0; i < settingsPanel.tabs.childCount; i++) {
                InspectorTab tab = settingsPanel.tabs.GetChild(i).GetComponent<InspectorTab>();
                if (tab == null)
                    continue;
                if (byType.ContainsKey((int)tab.levelEventType))
                {
                    dict.Add((int)tab.levelEventType, tab);
                    continue;
                }
                tabs.Add(tab);
            }
            settingsPanel.tabs.DetachChildren();
            foreach (CustomTab tab in list)
            {
                if (!dict.TryGetValue(tab.type, out InspectorTab component))
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate(settingsPanel.gc.prefab_tab);
                    component = gameObject.GetComponent<InspectorTab>();
                    component.Init((LevelEventType)tab.type, settingsPanel);
                    component.SetSelected(false);
                }
                if (tab.index == -1 || tab.index >= tabs.Count)
                    tabs.Add(component);
                else
                    tabs.Insert(tab.index, component);
            }
            for (int i = 0; i < tabs.Count; i++)
            {
                InspectorTab tab = tabs[i];
                tab.GetComponent<RectTransform>().AnchorPosY(8f - 68f * i);
                tab.gameObject.transform.SetParent(settingsPanel.tabs, false);
            }
        }

        internal class CustomTab
        {

            public Sprite icon;
            public int type;
            public string name;
            public string title;
            public Type page;
            public int index;
            internal CustomTab()
            {
            }
        }
    }
}
