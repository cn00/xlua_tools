using System;
using System.Diagnostics;

namespace app.Services
{
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