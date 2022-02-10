using UnityEngine;

namespace Megalith
{
    public static class MegalithIO
    {
        public static bool ConvertToRelativePath(string absolutePath, out string relativePath)
        {
            relativePath = absolutePath;
            if (!absolutePath.StartsWith(Application.dataPath) && !absolutePath.Replace("/", "\\").StartsWith(Application.dataPath))
                return false;
            relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
            return true;
        }
    }
}