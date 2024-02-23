using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Searchlight.Exceptions
{
    public class InvalidOperation : SearchlightException
    {
        public string OriginalFilter { get; internal set; }

        public string FieldName { get; internal set; }

        public string Operation {get; internal set; }

        public string ErrorMessage
        {
            get =>
                $"The query filter, {OriginalFilter}, uses {Operation} on {FieldName} which is not supported.";
        }
    }
}