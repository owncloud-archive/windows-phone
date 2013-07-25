namespace OwnCloud.Extensions
{
    /// <summary>
    /// Erweiterungen für Strings
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Such den nächsten Index in Value aus seachStrings
        /// </summary>
        /// <param name="value">Der Wert, in dem gesucht wird</param>
        /// <param name="searchStrings">DIeses Werte werden gesucht</param>
        /// <param name="minIndex">Der anfänglich Suchindex</param>
        public static int NextIndexOf(this string value, string[] searchStrings, int minIndex, out string foundString)
        {
            foundString = null;

            int i = -1;
            foreach (var searchString in searchStrings)
            {
                int cI = value.IndexOf(searchString, System.StringComparison.InvariantCulture);

                if (cI <= i) continue;
                i = cI;
                foundString = searchString;
            }

            return i;
        }

        /// <summary>
        /// Translates a string
        /// </summary>
        /// <param name="value">Translation index key</param>
        /// <param name="param">String.Format parameters</param>
        /// <returns></returns>
        public static string Translate(this string key, params object[] param)
        {
            return string.Format(LocalizedStrings.Get(key), param);
        }
    }
}
