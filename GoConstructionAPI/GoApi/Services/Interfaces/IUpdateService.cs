using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Services.Interfaces
{
    public interface IUpdateService
    {
        Dictionary<string, string> Diff<T>(T preUpdate, T postUpdate) where T : class;
    }
}
