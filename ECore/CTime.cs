using System;
using System.Linq;

namespace ECore
{
    public class CTime
    {
        TimeSpan time_span;

        DateTime old_time;
        DateTime now_time;

        public CTime()
        {
            Reset();
        }

        public bool Get(double timeMS)
        {
            this.now_time = DateTime.Now;
            time_span = this.now_time - this.old_time;
            if (time_span.TotalMilliseconds > timeMS)
            {
                this.old_time = DateTime.Now;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            this.old_time = DateTime.Now;
            this.now_time = DateTime.Now;
        }
    }
}






