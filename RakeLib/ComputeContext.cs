using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public class ComputeContext
    {
        private readonly ImmutableDictionary<string, string> _variables;

        #region Constructors
        public ComputeContext(IDictionary<string, string> inputs)
            : this(
                  NormalizeInputs(inputs),
                  ImmutableDictionary<string, string>.Empty)
        {
        }

        private ComputeContext(
            IImmutableDictionary<string, string> inputs,
            ImmutableDictionary<string, string> variables)
        {
            Inputs = inputs;
            _variables = variables;
        }

        private static IImmutableDictionary<string, string> NormalizeInputs(
            IDictionary<string, string> inputs)
        {
            if (inputs == null)
            {
                throw new ArgumentNullException(nameof(inputs));
            }

            return ImmutableDictionary<string, string>.Empty.AddRange(inputs);
        }
        #endregion

        public IImmutableDictionary<string, string> Inputs { get; private set; }

        internal ComputeContext AddVariable(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new ComputeContext(Inputs, _variables.Add(name, value));
        }

        public string GetVariable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!_variables.ContainsKey(name))
            {
                throw new IndexOutOfRangeException(
                    $"Variable '{name}' doesn't exist in the current context");
            }
            else
            {
                return _variables[name];
            }
        }

        public async Task<string> FetchContent(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            await Task.CompletedTask;

            throw new NotImplementedException();
        }
    }
}