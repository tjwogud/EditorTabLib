using System.Collections.Generic;

namespace EditorTabLib.Properties
{
    public class Property_Toggle : Property
    {
        private readonly bool value_default;

        public Property_Toggle(string name, bool value_default = false, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
            : base(name, key, canBeDisabled, startEnabled, enableIf, disableIf)
        {
            this.value_default = value_default;
        }

        public override Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>()
                    {
                        { "name", name },
                        { "type", "Bool" },
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
