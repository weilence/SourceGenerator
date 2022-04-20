using System.Collections.Generic;

namespace SourceGenerator.Library
{
    public static class ExtensionMethods
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
        {
            return dic.ContainsKey(key) ? dic[key] : default;
        }
    }
}