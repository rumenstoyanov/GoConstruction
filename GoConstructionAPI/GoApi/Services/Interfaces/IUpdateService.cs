using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Services.Interfaces
{
    public interface IUpdateService<T> where T : class
    {
        Dictionary<string, string> Diff(T preUpdate, T postUpdate);
    }
}
