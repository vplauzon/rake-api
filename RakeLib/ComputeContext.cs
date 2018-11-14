using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public class ComputeContext
    {
        private readonly ImmutableDictionary<string, object> _variables;

        #region Constructors
        public ComputeContext(IDictionary<string, object> inputs)
            : this(
                  NormalizeInputs(inputs),
                  ImmutableDictionary<string, object>.Empty)
        {
        }

        private ComputeContext(
            IImmutableDictionary<string, object> inputs,
            ImmutableDictionary<string, object> variables)
        {
            Inputs = inputs;
            _variables = variables;
        }

        private static IImmutableDictionary<string, object> NormalizeInputs(
            IDictionary<string, object> inputs)
        {
            if (inputs == null)
            {
                throw new ArgumentNullException(nameof(inputs));
            }

            return ImmutableDictionary<string, object>.Empty.AddRange(inputs);
        }
        #endregion

        public IImmutableDictionary<string, object> Inputs { get; private set; }

        internal ComputeContext AddVariable(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new ComputeContext(Inputs, _variables.Add(name, value));
        }

        public object GetVariable(string name)
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