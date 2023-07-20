using System.Collections.Generic;

namespace EditorTabLib.Utils
{
    public static class RDStringEx
    {
        public static string GetOrOrigin(string key, Dictionary<string, object> parameters = null)
        {
            string result = RDString.GetWithCheck(key, out bool exists, parameters);
            return exists ? result : key;
        }
    }
}
