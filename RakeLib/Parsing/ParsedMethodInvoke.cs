﻿namespace RakeLib.Parsing
{
    internal class ParsedMethodInvoke
    {
        public ParsedExpression Object { get; set; }

        public string Name { get; set; }

        public ParsedExpression[] Parameters { get; set; }
    }
}