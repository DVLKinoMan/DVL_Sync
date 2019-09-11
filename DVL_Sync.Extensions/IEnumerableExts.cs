using System;
using System.Collections.Generic;
using System.Linq;

namespace DVL_Sync.Extensions
{
    public static class IEnumerableExts
    {
        public static void RemoveAllExceptLast<T>(this IList<T> source, Func<T,bool> func, int exceptLast)
        {
            var indexes = source.Select((s, i) => new { s, i }).Where(anonym => func(anonym.s)).Select(anonym => anonym.i).ToList().ExceptLast(exceptLast);
            int count = 0;
            foreach (var index in indexes)
            {
                source.RemoveAt(index - count);
                count++;
            }
        }

        public static IList<T> ExceptLast<T>(this IList<T> source, int exceptLast) => source.Take(source.Count - exceptLast).ToList();

    }
}
