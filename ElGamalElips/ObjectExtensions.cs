using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectExtensionsNameSpace
{
    public static class ObjectExtensions
    {
        public static bool IsNull<T>(this T obj) where T : class
        {
            return null == obj;
        }

        public static bool IsNotNull<T>(this T obj) where T : class
        {
            return null != obj;
        }
    }
}
