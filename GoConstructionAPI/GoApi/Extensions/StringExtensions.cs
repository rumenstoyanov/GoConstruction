using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoApi.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Forces string into conventions for cache keys.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToCacheKeyFormat(this string s)
        {
            var sb = new StringBuilder();
            
            // Lowecase path.
            sb.Append(s.ToLower());

            // Ending in a backslash.
            if (s.Last() != '/')
            {
                sb.Append('/');
            }

            return sb.ToString();

        }
    }
}
