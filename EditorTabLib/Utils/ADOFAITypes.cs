using System;
using System.Collections.Generic;

namespace EditorTabLib.Utils
{
    internal class ADOFAITypes
    {
        internal static Type control;
        internal static readonly Dictionary<string, Type> controls = new Dictionary<string, Type>();
        internal static readonly string[] supportedControls = new string[] { "Bool", "Color", "Export", "File", "LongText", "Rating", "Text", "Tile", "Toggle", "Vector2" };

        internal static void InitializeTypes()
        {
            control = Reflections.GetType($"ADOFAI.PropertyControl") ?? Reflections.GetType($"ADOFAI.LevelEditor.Controls.PropertyControl"); ;
            if (control == null)
            {
                Main.Logger.Log("Cannot find \"PropertyControl\"");
            }
            foreach (string control in supportedControls)
            {
                Type type = Reflections.GetType($"ADOFAI.PropertyControl_{control}") ?? Reflections.GetType($"ADOFAI.LevelEditor.Controls.PropertyControl_{control}");
                if (type == null)
                {
                    Main.Logger.Log($"Cannot find \"PropertyControl_{control}\"");
                    continue;
                }
                controls.Add(control, type);
                controls.Add(control.ToLower(), type);
                controls.Add(control.ToUpper(), type);
            }
        }
    }
}
