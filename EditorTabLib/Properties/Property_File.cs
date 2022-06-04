using ADOFAI;
using System.Collections.Generic;

namespace EditorTabLib.Properties
{
    public class Property_File : Property
    {
        private readonly string value_default;
        private readonly FileType fileType;

        public Property_File(string name, string value_default = null, FileType fileType = FileType.Audio, string key = null, bool canBeDisabled = false, bool startEnabled = false, Dictionary<string, string> enableIf = null, Dictionary<string, string> disableIf = null)
            : base(name, key, canBeDisabled, startEnabled, enableIf, disableIf)
        {
            this.value_default = value_default ?? string.Empty;
            this.fileType = fileType;
        }

        public override Dictionary<string, object> ToData()
        {
            return new Dictionary<string, object>()
                    {
                        { "name", name },
                        { "type", "File" },
                        { "default", value_default },
                        { "fileType", fileType },
                        { "canBeDisabled", canBeDisabled },
                        { "startEnabled", startEnabled },
                        { "enableIf", enableIf },
                        { "disableIf", disableIf },
                        { "key", key }
                    };
        }
    }
}
