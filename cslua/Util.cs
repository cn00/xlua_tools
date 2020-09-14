using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XLua;
using RealStatePtr = System.IntPtr;

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
            return source.Select(g =>  selector(g)).ToList();
        }

        public static object First(IEnumerable<object> source)
        {
            return source.First();
        }
        public static string Replace(string self, string ol, string nl)
        {
            return self.Replace(ol, nl);
        }
        /*
         * public static ParallelLoopResult ForEach(IEnumerable<TSource> source, Func<TLocal> localInit, Func<TSource, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
         */
        public static ParallelLoopResult ForEachStr(IEnumerable<string> source,  Func<int> init,         Func<string, ParallelLoopState, int,     int> body,    Action<int> localFinally)
        {
            return Parallel.ForEach(source, init, body, localFinally);
        }
    }
}