using System.Collections.Generic;

namespace EditorTabLib.Properties
{
    public class Property_Tile : Property
    {
        public const int THIS_TILE = 0x001;
        public const int START     = 0x010;
        public const int END       = 0x100;

        private readonly List<object> value_default;
        private readonly int hideButtons;
        private readonly int min;
        private readonly int max;

        public Property_Tile(string name, (int, TileRelativeTo)? value_default = null, int hideButtons = 0, int min = int.MinValue, int max = int.MaxValue, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
            : base(name, key, canBeDisabled, startEnabled, enableIf, disableIf)
        {
            this.value_default = value_default != null ? new List<object> { value_default.Value.Item1, value_default.Value.Item2 } : new List<object> { 0, TileRelativeTo.ThisTile };
            this.hideButtons = hideButtons;
            this.min = min;
            this.max = max;
        }

        public override Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>()
                    {
                        { "name", name },
                        { "type", "Tile" },
                        { "default", value_default },
                        { "hideButtons", hideButtons },
                        { "min", min },
                        { "max", max },
                        { "canBeDisabled", canBeDisabled },
                        { "startEnabled", startEnabled },
                        { "enableIf", enableIf },
                        { "disableIf", disableIf },
                        { "key", key }
                    };
        }
    }
}
