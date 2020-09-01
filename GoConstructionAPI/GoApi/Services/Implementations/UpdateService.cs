using GoApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Services.Implementations
{
    public class UpdateService
    {
        public Dictionary<string, string> Diff<T>(T preUpdate, T postUpdate) where T : class
        {
            var diffDict = new Dictionary<string, string>();
            var properties = typeof(T).GetProperties();
            foreach (var pi in properties)
            {
                if (pi.GetValue(preUpdate).ToString() != pi.GetValue(postUpdate).ToString())
                {
                    diffDict.Add(pi.Name, pi.GetValue(postUpdate).ToString());
                }
            }
            return diffDict;

        }
    }
}
