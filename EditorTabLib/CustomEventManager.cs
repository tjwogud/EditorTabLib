using ADOFAI;
using EditorTabLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace EditorTabLib
{
    public class CustomEventManager
    {
        internal static List<CustomEvent> list = new List<CustomEvent>();
        internal static Dictionary<int, CustomEvent> byType = new Dictionary<int, CustomEvent>();
        internal static Dictionary<string, CustomEvent> byName = new Dictionary<string, CustomEvent>();
        internal static object let_valuesAndNames = typeof(Enum).Method("GetCachedValuesAndNames", new object[] { typeof(LevelEventType), true });
        internal static object lec_valuesAndNames = typeof(Enum).Method("GetCachedValuesAndNames", new object[] { typeof(LevelEventCategory), true });
        internal static Dictionary<int, Sprite> categories = new Dictionary<int, Sprite>();

        public static void AddEvent(Sprite icon, int type, string name, Dictionary<SystemLanguage, string> title, List<Properties.Property> properties, List<LevelEventCategory> categories, Action onFocused = null, Action onUnFocused = null, Func<LevelEvent, string, object, object, bool> onChange = null)
        {
            AddEvent(icon, type, name, title, properties.Select(property => property.ToData()).ToList(), categories, onFocused, onUnFocused, onChange);
        }

        public static void AddEvent(Sprite icon, int type, string name, Dictionary<SystemLanguage, string> title, List<Dictionary<string, object>> properties, List<LevelEventCategory> categories, Action onFocused = null, Action onUnFocused = null, Func<LevelEvent, string, object, object, bool> onChange = null)
        {
            if (icon == null)
                throw new ArgumentNullException("icon cannot be null!");
            if (name == null)
                throw new ArgumentNullException("name cannot be null!");
            if (properties == null)
                throw new ArgumentNullException("properties cannot be null!");
            if (byType.ContainsKey(type))
                throw new ArgumentException("customevent with type " + type + " already exists!");
            if (byName.ContainsKey(name))
                throw new ArgumentException("customevent named " + name + " already exists!");
            int max = 0;
            foreach (LevelEventType let in Enum.GetValues(typeof(LevelEventType)))
                if ((int)let > max)
                    max = (int)let;
            if (type <= max)
                throw new ArgumentException("type must be bigger than " + max);
            CustomEvent ev = new CustomEvent
            {
                icon = icon,
                type = type,
                name = name,
                title = title,
                properties = properties,
                categories = categories,
                onFocused = onFocused,
                onUnFocused = onUnFocused,
                onChange = onChange
            };
            list.Add(ev);
            byType.Add(type, ev);
            byName.Add(name, ev);

            let_valuesAndNames.Set("Names", let_valuesAndNames.Get<string[]>("Names").ToList().Append(name).ToArray());
            let_valuesAndNames.Set("Values", let_valuesAndNames.Get<ulong[]>("Values").ToList().Append((ulong)type).ToArray());
        }

        public static void AddCategory(Sprite icon, int type, string name)
        {
            if (icon == null)
                throw new ArgumentNullException("icon cannot be null!");
            if (name == null)
                throw new ArgumentNullException("name cannot be null!");

            List<string> names = lec_valuesAndNames.Get<string[]>("Names").ToList();
            names.Insert(names.Count - 0, name);
            List<ulong> values = lec_valuesAndNames.Get<ulong[]>("Values").ToList();
            values.Insert(values.Count - 0, (ulong)type);
            lec_valuesAndNames.Set("Names", names.ToArray());
            lec_valuesAndNames.Set("Values", values.ToArray());
            categories.Add(type, icon);
        }

        internal class CustomEvent
        {
            internal Sprite icon;
            internal int type;
            internal string name;
            internal Dictionary<SystemLanguage, string> title;
            internal List<Dictionary<string, object>> properties;
            internal List<LevelEventCategory> categories;
            internal Action onFocused;
            internal Action onUnFocused;
            internal Func<LevelEvent, string, object, object, bool> onChange;
            internal CustomEvent()
            {
            }
        }
    }
}
