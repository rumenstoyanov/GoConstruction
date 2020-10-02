using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Services.Interfaces
{
    public interface IMessagePublisher
    {
        Task Publish<T>(T obj) where T : class;
    }
}
