using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public static class ExtensionClasses
    {
        //public static TSource First<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        //{
        //    using (IEnumerator<TSource> sourceIterator = source.GetEnumerator())
        //    {
        //        if (!sourceIterator.MoveNext())
        //        {
        //            TSource temp = default(TSource); //Instead of returning an exception trying to get a default value of a particular type
        //            return temp;
        //        }
        //        else
        //        {
        //            return sourceIterator.Current;
        //        }
        //    }
        //}

        public static TSource Firstt<TSource>(this IEnumerable<TSource> source)
        {
            using (IEnumerator<TSource> sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    TSource temp = default(TSource);
                    return temp;
                }
                else
                {
                    return sourceIterator.Current;
                }
            }
        }
    }
}
