using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KS3
{
    public class Util
    {
        /*
         * public static ParallelLoopResult ForEach(IEnumerable<TSource> source, Func<TLocal> localInit, Func<TSource, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
         */
        public static ParallelLoopResult ForEachStr(IEnumerable<string> source,  Func<int> init,         Func<string, ParallelLoopState, int,     int> body,    Action<int> localFinally)
        {
            return Parallel.ForEach(source, init, body, localFinally);
        }
    }
}