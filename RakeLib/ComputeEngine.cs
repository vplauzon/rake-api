using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public class ComputeEngine
    {
        public ComputeEngine(Quotas quotas)
        {
        }

        public async Task<IDictionary<string, string>> ComputeAsync(
            IDictionary<string, string> inputs)
        {
            await Task.CompletedTask;

            throw new NotImplementedException();
        }
    }
}