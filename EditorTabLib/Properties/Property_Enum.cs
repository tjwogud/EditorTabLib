using System;
using System.Collections.Generic;

namespace EditorTabLib.Properties
{
    public class Property_Enum<T> : Property where T : Enum
    {
        private readonly string value_default;
        private readonly Type type;

        public Property_Enum(string name, T value_default = default, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
            : base(name, key, canBeDisabled, startEnabled, enableIf, disableIf)
        {
            this.value_default = value_default.ToString();
            type = typeof(T);
        }

        public override Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>()
                    {
                        { "name", name },
                        { "type", "Enum:" + (type.Assembly.FullName == typeof(ADOBase).Assembly.FullName ? type.Name : type.AssemblyQualifiedName) },
                        { "default", value_default },
                        { "canBeDisabled", canBeDisabled },
                        { "startEnabled", startEnabled },
                        { "enableIf", enableIf },
                        { "disableIf", disableIf },
                        { "key", key }
                    };
        }
    }
}
