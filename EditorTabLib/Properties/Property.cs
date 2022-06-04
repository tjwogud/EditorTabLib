using System.Collections.Generic;
using System.Linq;

namespace EditorTabLib.Properties
{
    public abstract class Property
    {
        protected readonly string name;
        protected readonly string key;
        protected readonly bool canBeDisabled;
        protected readonly bool startEnabled;
        protected readonly List<object> enableIf;
        protected readonly List<object> disableIf;

        internal Property() {
        }

        public Property(string name, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
        {
            this.name = name;
            this.key = key;
            this.canBeDisabled = canBeDisabled;
            this.startEnabled = startEnabled;
            this.enableIf = enableIf?.SelectMany(pair => new object[] { pair.Key, pair.Value }).ToList() ?? new List<object>();
            this.disableIf = disableIf?.SelectMany(pair => new object[] { pair.Key, pair.Value }).ToList() ?? new List<object>();
        }

        public abstract Dictionary<string, object> ToData();
    }
}
