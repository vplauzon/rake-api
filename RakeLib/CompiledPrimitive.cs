namespace RakeLib
{
    public class CompiledPrimitive
    {
        public int? Integer { get; set; }

        public string QuotedString { get; set; }

        #region Object Methods
        public override string ToString()
        {
            return Integer != null
                ? $"{Integer}"
                : $"\"{QuotedString}\"";
        }

        public override bool Equals(object obj)
        {
            var primitive = obj as CompiledPrimitive;

            return primitive != null
                && object.Equals(Integer, primitive.Integer)
                && object.Equals(QuotedString, primitive.QuotedString);
        }

        public override int GetHashCode()
        {
            return Integer != null
                ? Integer.GetHashCode()
                : QuotedString.GetHashCode();
        }
        #endregion
    }
}