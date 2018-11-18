namespace RakeLib
{
    public class CompiledMethodInvoke
    {
        public bool IsProperty { get; set; }

        public string Name { get; set; }

        public CompiledCompute[] Parameters { get; set; }

        public CompiledMethodInvoke Next { get; set; }
    }
}