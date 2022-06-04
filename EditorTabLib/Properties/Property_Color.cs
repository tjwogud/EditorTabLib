using System.Collections.Generic;
using UnityEngine;

namespace EditorTabLib.Properties
{
    public class Property_Color : Property
    {
        private readonly string value_default;
        private readonly bool usesAlpha;

        public Property_Color(string name, Color? value_default = null, bool usesAlpha = true, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
            : base(name, key, canBeDisabled, startEnabled, enableIf, disableIf)
        {
            this.value_default = ToHex(value_default.GetValueOrDefault(Color.white), usesAlpha);
            this.usesAlpha = usesAlpha;
        }

        private static string ToHex(Color c, bool alpha)
        {
            return (alpha
                ? string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a))
                : string.Format("{0:X2}{1:X2}{2:X2}", ToByte(c.r), ToByte(c.g), ToByte(c.b))).ToLower();
        }

        private static byte ToByte(float f)
        {
            return (byte)(Mathf.Clamp01(f) * 255f);
        }

        public override Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>()
                    {
                        { "name", name },
                        { "type", "Color" },
                        { "default", value_default },
                        { "usesAlpha", usesAlpha },
                        { "canBeDisabled", canBeDisabled },
                        { "startEnabled", startEnabled },
                        { "enableIf", enableIf },
                        { "disableIf", disableIf },
                        { "key", key }
                    };
        }
    }
}
