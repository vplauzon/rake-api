using RakeLib.Parsing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public class Compiler
    {
        private readonly Parser _parser;

        public async Task<CompiledFunction> CompileAsync(FunctionDescription description)
        {
            var parsed = await _parser.ParseFunctionAsync(description);

            throw new NotImplementedException();
        }
    }
}