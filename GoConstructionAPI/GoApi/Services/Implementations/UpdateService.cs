using GoApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Services.Implementations
{
    public class UpdateService<T> : IUpdateService<T> where T : class
    {
        public Dictionary<string, string> Diff(T preUpdate, T postUpdate)
        {
            var diffDict = new Dictionary<string, string>();
            var properties = typeof(T).GetProperties();
            foreach (var pi in properties)
            {
                if (pi.GetValue(preUpdate) != pi.GetValue(postUpdate))
                {
                    diffDict.Add(pi.Name, pi.GetValue(postUpdate).ToString());
                }
            }
            return diffDict;

        }
    }
}
