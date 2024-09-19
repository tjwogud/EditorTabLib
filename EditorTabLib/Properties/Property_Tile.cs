using System.Collections.Generic;

namespace EditorTabLib.Properties
{
    public class Property_Tile : Property
    {
        public const int THIS_TILE = 0x001;
        public const int START     = 0x010;
        public const int END       = 0x100;

        public Property_Tile(string name, (int, TileRelativeTo)? value_default = null, int hideButtons = 0, int min = int.MinValue, int max = int.MaxValue, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
            : base(name, key, canBeDisabled, startEnabled, enableIf, disableIf)
        {
            data["type"] = "Tile";
            data["default"] = value_default != null ? new List<object> { value_default.Value.Item1, value_default.Value.Item2 } : new List<object> { 0, TileRelativeTo.ThisTile };
            data["hideButtons"] = hideButtons;
            data["min"] = min;
            data["max"] = max;
        }
    }
}
