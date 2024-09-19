using ADOFAI;
using System.Collections.Generic;

namespace EditorTabLib.Properties
{
    public class Property_File : Property
    {
        public Property_File(string name, string value_default = null, FileType fileType = FileType.Audio, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
            : base(name, key, canBeDisabled, startEnabled, enableIf, disableIf)
        {
            data["type"] = "File";
            data["default"] = value_default ?? string.Empty;
            data["fileType"] = fileType;
        }
    }
}
