using System.Collections.Generic;

namespace EditorTabLib.Properties
{
    public class Property_TextArea : Property
    {
        private readonly string value_default;

        public Property_TextArea(string name, string value_default = null, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
            : base(name, key, canBeDisabled, startEnabled, enableIf, disableIf)
        {
            this.value_default = value_default ?? string.Empty;
        }

        public override Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>()
                    {
                        { "name", name },
                        { "type", "Text" },
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
