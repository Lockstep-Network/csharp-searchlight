#pragma warning disable CS1591
namespace Searchlight
{
    /// <summary>
    /// A filter statement contained an unterminated JSON key.  An opening double quote was observed but the remainder
    /// of the string did not contain a closing quote.
    ///
    /// Example: (dimensions."address is not null)
    /// </summary>
    public class UnterminatedJsonKey : SearchlightException
    {
        public string OriginalFilter { get; internal set; }
        public int StartPosition { get; internal set; }
        public ParsingType ParsingType { get; internal set; }

        public string ErrorMessage
        {
            get =>
                $"The query {(ParsingType == ParsingType.Filter ? "filter" : "order by")}, {OriginalFilter}, contained an unterminated JSON Key that starts at {StartPosition}. JSON Keys should be in the format .\"{{KeyName}}\"";
        }
    }

    public enum ParsingType
    {
        Filter,
        OrderBy
    }
}