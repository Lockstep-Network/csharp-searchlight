﻿using System;

namespace Searchlight
{
    /// <summary>
    /// Represents a field that is permitted to be used as a filter or sort-by column in the SafeParser
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SearchlightField : Attribute
    {
        /// <summary>
        /// If this column is named differently in the API, this is the official SQL name of the column
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        /// If this field can potentially be known by other names, list them here
        /// </summary>
        public string[] Aliases { get; set; }

        /// <summary>
        /// If this field is a different type in the database, this is the actual field type in the DB
        /// </summary>
        public Type FieldType { get; set; }

        /// <summary>
        /// If this field is presented to the user as an enum, use this source enum to parse the value before converting to fieldType for querying
        /// </summary>
        public Type EnumType { get; set; }
        
        /// <summary>
        /// (optional) If you wish to use Searchlight autocomplete, you can provide a documentation block here.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// (optional) Set to true if the database column is storing JSON.
        ///
        /// Current limitations of using JSON columns include:
        /// - No filtering/sorting on JSON arrays
        /// - Supported operators are:
        ///   - (Not) Equals
        ///   - (Not) In
        ///   - Is (Not) Null
        /// </summary>
        public bool IsJson { get; set; } = false;
        
        /// <summary>
        /// (optional) Set to true if the database column is encrypted.
        /// 
        /// If the column is encrypted, the Searchlight engine will use the provided ISearchlightStringEncryptor to encrypt the value before querying.
        /// The column must be of type string.
        /// Encrypted columns can only use equality, nullity, or in operators.
        /// </summary>
        public bool IsEncrypted { get; set; } = false;
    }
}
