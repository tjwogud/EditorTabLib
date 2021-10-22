using ADOFAI;
using System;
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
        }

        public static void DeleteTab(int type)
        {
            if (!byType.TryGetValue(type, out CustomTab tab))
                return;
            list.Remove(tab);
            byType.Remove(tab.type);
            byName.Remove(tab.name);
            Main.AddOrDeleteTab(tab, false);
        }

        public static void DeleteTab(string name)
        {
            if (!byName.TryGetValue(name, out CustomTab tab))
                return;
            list.Remove(tab);
            byType.Remove(tab.type);
            byName.Remove(tab.name);
            Main.AddOrDeleteTab(tab, false);
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
