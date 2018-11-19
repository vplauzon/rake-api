namespace RakeLib.Parsing
{
    internal class ParsedMethodInvoke
    {
        public bool IsProperty { get; set; }

        public string Name { get; set; }

        public ParsedCompute[] Parameters { get; set; }

        public ParsedMethodInvoke Next { get; set; }
    }
}