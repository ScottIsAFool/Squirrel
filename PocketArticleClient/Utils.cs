using System.Collections.Generic;

namespace PocketArticle
{
    public static class Utils
    {
        /// <summary>
        /// Convert a dictionary to a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static List<T> ToList<T>(this Dictionary<string, T> items) where T : new()
        {
            if (items == null)
            {
                return null;
            }

            var itemEnumerator = items.GetEnumerator();
            var list = new List<T>();

            while (itemEnumerator.MoveNext())
            {
                list.Add(itemEnumerator.Current.Value);
            }

            return list;
        }
    }
}
