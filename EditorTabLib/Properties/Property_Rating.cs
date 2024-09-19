using System.Collections.Generic;

namespace EditorTabLib.Properties
{
    public class Property_Rating : Property
    {
        private readonly int value_default;

        public Property_Rating(string name, int value_default = 1, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
            : base(name, key, canBeDisabled, startEnabled, enableIf, disableIf)
        {
            data["type"] = "Rating";
            data["default"] = value_default;
        }
    }
}
