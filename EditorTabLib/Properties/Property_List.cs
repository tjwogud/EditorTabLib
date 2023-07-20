using System.Collections.Generic;

namespace EditorTabLib.Properties
{
    public class Property_List : Property
    {
        private readonly List<string> list;
        private readonly string value_default;

        public Property_List(string name, List<string> list, string value_default = default, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
            : base(name, key, canBeDisabled, startEnabled, enableIf, disableIf)
        {
            this.list = list;
            this.value_default = value_default.IsNullOrEmpty() ? list[0] : value_default;
        }

        public override Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>()
                    {
                        { "name", name },
                        { "type", "Enum:" + typeof(Dummy).AssemblyQualifiedName },
                        { "unit", $"{string.Join(";", list)}" },
                        { "default", value_default },
                        { "canBeDisabled", canBeDisabled },
                        { "startEnabled", startEnabled },
                        { "enableIf", enableIf },
                        { "disableIf", disableIf },
                        { "key", key }
                    };
        }

        internal enum Dummy { }
    }
}
