using System;
using System.Collections.Generic;
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
                        Console.WriteLine($"ManagedThreadId={Thread.CurrentThread.ManagedThreadId}");
                        localFinally(lfinal);
                    }
                }
            );
        }
    }
}