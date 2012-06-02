using System;

namespace Kunukn.GooglemapsClustering.DataUtility
{
    public static class Util
    {
        public static string GetException(Exception ex)
        {
            return string.Format("Msg:{0}\nStacktrace:{1}\nInnerExc:{2}", ex.Message, ex.StackTrace, ex.InnerException);
        }
    }
}
