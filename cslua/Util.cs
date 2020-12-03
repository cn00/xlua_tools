using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XLua;
using RealStatePtr = System.IntPtr;

using TSource = System.Object;
using TLocal = System.Object;

namespace xlua
{
    public static class Util
    {
        public static void PushAny(this RealStatePtr L, object o)
        {
            ObjectTranslatorPool.Instance.Find(L).PushAny(L, o);
        }

        public static List<object> ToList(IEnumerable<object> source)
        {
            return source.ToList();
        }

        public static List<IGrouping<object, object>> GroupBy(
            IEnumerable<object> source,
            Func<object, object> keySelector)
        {
            return source.GroupBy(keySelector).ToList();
        }

        public static List<object> Select(
            IEnumerable<object> source,
            Func<object, object> selector)
        {
            return source.Select(g => selector(g)).ToList();
        }

        public static List<object> SelectMany(
            IEnumerable<object> source,
            Func<object, IEnumerable<object>> selector)
        {
            return source.SelectMany(g => selector(g)).ToList();
        }

        public static object First(IEnumerable<object> source)
        {
            return source.First();
        }

        public static string Replace(string self, string ol, string nl)
        {
            return self.Replace(ol, nl);
        }

        static object ParallelLock = new object();
        public static ParallelLoopResult ForEach(IEnumerable<TSource> source, Func<TLocal> init,
            Func<TSource, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            /*
            public static ParallelLoopResult ForEach<TSource, TLocal>(
                IEnumerable<TSource> source,
                Func<TLocal> localInit,
                Func<TSource, ParallelLoopState, TLocal, TLocal> body,
                Action<TLocal> localFinally)
            */
            
            return Parallel.ForEach(source
                , () =>
                {
                    // lock(ParallelLock)
                    {
                        return init();
                    }
                }
                ,(ts, pls, l1)=>
                {
                    // lock(ParallelLock)
                    {
                        return body(ts, pls, l1);
                    }
                }, lfinal =>
                {
                    lock (ParallelLock)
                    {
                        Debug.WriteLine($"ManagedThreadId={Thread.CurrentThread.ManagedThreadId}");
                        localFinally(lfinal);
                    }
                }
            );
        }
        
        public static T BinarySearch<T, TKey>(this IList<T> list, Func<T, TKey> keySelector, /*Func<T,int>*/TKey compare) where TKey : IComparable<TKey>
        {
            if (list.Count == 0)
                throw new InvalidOperationException("Item not found");

            int min = 0;
            int max = list.Count;
            T midItem;
            while (min < max)
            {
                int mid = min + ((max - min) / 2);
                midItem = list[mid];
                TKey midKey = keySelector(midItem);
                // int comp = compare(midItem);
                int comp = midKey.CompareTo(compare);
                if (comp < 0)
                {
                    min = mid + 1;
                }
                else if (comp > 0)
                {
                    max = mid - 1;
                }
                else
                {
                    return midItem;
                }
            }
            if (min == max 
                && min < list.Count 
                // && compare(list[min]) == 0
                && keySelector(list[min]).CompareTo(compare) == 0
            )
            {
                return list[min];
            }
            // throw new InvalidOperationException("Item not found");
            return default(T);
        }//BinarySearch

        public static void BinarySearchTest()
        {
            var td = new TimeDebug("BinarySearchTest");
            var l = File.ReadAllLines("/Users/cn/a3/c/log-ios-ft.txt")
                .Where(i=>i.Length > 1)
                .Select(i => i.Trim())
                .ToList();
            l.Sort((a,b)=>a.CompareTo(b));
            td.Step("Sort");
            File.WriteAllLines("/Users/cn/a3/c/log-ios-sort.txt", l);

            int ll = l.Count;
            var r = new Random();
            var times = 100;
            for (int j = 0; j < times; j++)
            {
                int i = r.Next()%ll;
                var s = l[i];
                var t = l.BinarySearch(ik => ik, s);
                Debug.WriteLine($"{j}:{i}:{s}:{t}");
            }
            td.Step($"bsearch:{times}s");
        }
    }
    public class TimeDebug
    {
        private string Tag = "";

        private Int64 StartTime0 = 0;
        private Int64 StartTime = 0;
        public TimeDebug(string tag = "")
        {
            StartTime0 = StartTime = DateTime.Now.ToBinary();
            Tag = tag;
        }

        [Conditional("DEBUG")]
        public void Step(string tag = "", Int64 deltaM = 0)
        {
            var delta = DateTime.Now.ToBinary() - StartTime;
            StartTime = DateTime.Now.ToBinary();
            if(delta > deltaM)
                Debug.WriteLine($"{Tag}.{tag}:{delta/1000000:F}s");
        }
    }
}