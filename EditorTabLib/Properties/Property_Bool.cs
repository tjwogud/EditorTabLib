using System.Collections.Generic;

namespace EditorTabLib.Properties
{
    public class Property_Bool : Property
    {
        public Property_Bool(string name, bool value_default = false, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
            : base(name, key, canBeDisabled, startEnabled, enableIf, disableIf)
        {
            data["type"] = "Bool";
            data["default"] = value_default;
        }
    }
}
