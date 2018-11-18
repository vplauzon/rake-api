using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public class Compiler
    {
        public async Task<CompiledCompute> CompileExpressionAsync(string expression)
        {
            await Task.CompletedTask;

            throw new NotImplementedException();
        }

        public async Task<CompiledFunction> CompileFunctionAsync(FunctionDescription description)
        {
            await Task.CompletedTask;

            throw new NotImplementedException();
        }
    }
}