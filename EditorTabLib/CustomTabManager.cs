using ADOFAI;
using EditorTabLib.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EditorTabLib
{
    public class CustomTabManager
    {
        internal static List<CustomTab> list = new List<CustomTab>();
        internal static Dictionary<int, CustomTab> byType = new Dictionary<int, CustomTab>();
        internal static Dictionary<string, CustomTab> byName = new Dictionary<string, CustomTab>();

        public static void AddTab(Sprite icon, int type, string name, Dictionary<SystemLanguage, string> title, List<Properties.Property> properties, bool saveSetting = false, Action onFocused = null, Action onUnFocused = null, Func<LevelEvent, string, object, object, bool> onChange = null, int index = -1)
        {
            if (icon == null)
                throw new ArgumentNullException("icon cannot be null!");
            if (name == null)
                throw new ArgumentNullException("name cannot be null!");
            if (properties == null)
                throw new ArgumentNullException("properties cannot be null!");
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
                properties = properties.Select(property => property.ToData()).ToList(),
                saveSetting = saveSetting,
                onFocused = onFocused,
                onUnFocused = onUnFocused,
                onChange = onChange,
                index = index
            };
            AddTab(tab);
        }

        public static void AddTab(Sprite icon, int type, string name, Dictionary<SystemLanguage, string> title, List<Dictionary<string, object>> properties, bool saveSetting = false, Action onFocused = null, Action onUnFocused = null, Func<LevelEvent, string, object, object, bool> onChange = null, int index = -1)
        {
            if (icon == null)
                throw new ArgumentNullException("icon cannot be null!");
            if (name == null)
                throw new ArgumentNullException("name cannot be null!");
            if (properties == null)
                throw new ArgumentNullException("properties cannot be null!");
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
                properties = properties,
                saveSetting = saveSetting,
                onFocused = onFocused,
                onUnFocused = onUnFocused,
                onChange = onChange,
                index = index
            };
            AddTab(tab);
        }

        public static void AddTab<T>(Sprite icon, int type, string name, Dictionary<SystemLanguage, string> title, int index = -1) where T : CustomTabBehaviour
        {
            if (icon == null)
                throw new ArgumentNullException("icon cannot be null!");
            if (name == null)
                throw new ArgumentNullException("name cannot be null!");
            if (typeof(T) == null)
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
                page = typeof(T),
                index = index
            };
            AddTab(tab);
        }

        private static void AddTab(CustomTab tab)
        {
            list.Add(tab);
            byType.Add(tab.type, tab);
            byName.Add(tab.name, tab);
            Main.AddOrDeleteTab(tab, true);
            InspectorPanel settingsPanel = scnEditor.instance?.settingsPanel;
            if (settingsPanel == null)
                return;
            GameObject gameObject = UnityEngine.Object.Instantiate(RDConstants.data.prefab_propertiesPanel);
            gameObject.name = tab.name;
            PropertiesPanel component = gameObject.GetComponent<PropertiesPanel>();
            component.levelEventType = (LevelEventType)tab.type;
            component.gameObject.SetActive(false);
            GameObject gameObject2 = UnityEngine.Object.Instantiate(RDConstants.data.prefab_tab);
            InspectorTab component2 = gameObject2.GetComponent<InspectorTab>();
            component.Init(settingsPanel, GCS.settingsInfo[tab.name]);
            component2.Init((LevelEventType)tab.type, settingsPanel);
            component2.SetSelected(false);

            if (tab.index == -1)
            {
                component2.GetComponent<RectTransform>().AnchorPosY(8f - 68f * settingsPanel.tabs.childCount);
                gameObject2.transform.SetParent(settingsPanel.tabs, false);
            }
            else
            {
                if (settingsPanel.tabs.childCount <= tab.index)
                    tab.index = settingsPanel.tabs.childCount - 1;
                List<InspectorTab> tabs = new List<InspectorTab>();
                for (int i = tab.index; i < settingsPanel.tabs.childCount; i++)
                {
                    InspectorTab tab2 = settingsPanel.tabs.GetChild(i).GetComponent<InspectorTab>();
                    if (tab2 == null || tab2.levelEventType == (LevelEventType)tab.type)
                        continue;
                    tabs.Add(tab2);
                }
                tabs.ForEach(tab2 => tab2.transform.SetParent(null, false));
                component2.GetComponent<RectTransform>().AnchorPosY(8f - 68f * tab.index);
                gameObject2.transform.SetParent(settingsPanel.tabs, false);
                foreach (InspectorTab tab2 in tabs)
                {
                    tab2.GetComponent<RectTransform>().AnchorPosY(8f - 68f * ++tab.index);
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
                    UnityEngine.Object.DestroyImmediate(rect.gameObject);
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
            for (int i = 0; i < settingsPanel.tabs.childCount; i++)
            {
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
                    /*
                    GameObject gameObject = UnityEngine.Object.Instantiate(RDConstants.data.prefab_tab);
                    component = gameObject.GetComponent<InspectorTab>();
                    component.Init((LevelEventType)tab.type, settingsPanel);
                    component.SetSelected(false);
                    */
                    component = null;
                }
                if (tab.index == -1 || tab.index >= tabs.Count)
                    tabs.Add(component);
                else
                    tabs.Insert(tab.index, component);
            }
            for (int i = 0; i < tabs.Count; i++)
            {
                InspectorTab tab = tabs[i];
                if (!tab)
                    continue;
                tab.GetComponent<RectTransform>().AnchorPosY(8f - 68f * i);
                tab.gameObject.transform.SetParent(settingsPanel.tabs, false);
            }
        }

        public static LevelEvent GetEvent(LevelEventType type)
        {
            if (scnEditor.instance == null || !byType.TryGetValue((int)type, out CustomTab tab) || (!tab.saveSetting && scnEditor.instance.settingsPanel.selectedEventType != type))
                return null;
            if (scnEditor.instance.settingsPanel.selectedEventType == type)
                return scnEditor.instance.settingsPanel.selectedEvent;
            return Patches.InspectorPanelShowPanelPatch.saves.TryGetValue(type, out LevelEvent value) ? value : null;
        }

        internal class CustomTab : CustomEventManager.CustomEvent
        {
            internal Type page;
            internal int index;
            internal bool saveSetting;
            internal CustomTab()
            {
            }
        }
    }
}
