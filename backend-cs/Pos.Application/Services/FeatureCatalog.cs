using System.Collections.Generic;
using System.Linq;

namespace PosCs.Application.Services
{
    /// <summary>
    /// System-defined tenant features. The same list must be mirrored in the frontend
    /// constants and in docs/FEATURES.md.
    /// </summary>
    public static class FeatureCatalog
    {
        public static readonly System.Collections.Generic.IReadOnlyList<string> Keys = new List<string>
        {
            "dry_clean"
        };

        public static bool IsKnown(string key)
        {
            return Keys.Contains(key);
        }
    }
}
