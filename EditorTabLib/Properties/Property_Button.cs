using System.Collections.Generic;
using UnityEngine.Events;

namespace EditorTabLib.Properties
{
    public class Property_Button : Property
    {
        private readonly UnityAction action;

        public Property_Button(string name, UnityAction action, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
            : base(name, key, canBeDisabled, startEnabled, enableIf, disableIf)
        {
            this.action = action;
        }

        public override Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>()
                    {
                        { "name", name },
                        { "type", "Export" },
                        { "default", action },
                        { "canBeDisabled", canBeDisabled },
                        { "startEnabled", startEnabled },
                        { "enableIf", enableIf },
                        { "disableIf", disableIf },
                        { "key", key }
                    };
        }
    }
}
