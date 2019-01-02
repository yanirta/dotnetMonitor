using System.Collections.Generic;

namespace Monitor.Extentions
{
    public static class DictionaryExtentions
    {
        public static object getSafeValue(this Dictionary<string, object> dictionary, string key, object fallback = null)
        {
            object retval = dictionary.GetValueOrDefault(key, fallback);
            string srtval = retval as string;
            if (srtval != null)
                retval = string.IsNullOrEmpty(srtval) ? fallback : retval;
            return retval;
        }

    }
}