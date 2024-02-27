#pragma warning disable CS1591
namespace Searchlight.Exceptions
{
    /// <summary>
    /// The operation used in the filter on a given field is not supported.
    ///
    /// Example: `(someField gt 5)` where `someField` is an encrypted field.
    /// </summary>
    public class InvalidOperation : SearchlightException
    {
        public string OriginalFilter { get; internal set; }

        public string FieldName { get; internal set; }

        public string Operation { get; internal set; }

        public string ErrorMessage
        {
            get =>
                $"The query filter, {OriginalFilter}, uses {Operation} on {FieldName} which is not supported.";
        }
    }
}