using System;

namespace Searchlight.Parsing
{
    /// <summary>
    /// Represents a column that can be used in the database
    /// </summary>
    public class ColumnInfo
    {
        /// <summary>
        /// Constructor for a column that should be renamed in the parser
        /// </summary>
        /// <param name="filterName">The name supplied in the raw filter</param>
        /// <param name="columnName">The name of the column in the database</param>
        /// <param name="aliases">If this field is known by other names, list them here</param>
        /// <param name="columnType">The raw type of the column in the database</param>
        /// <param name="enumType">The type of the enum that the column is mapped to</param>
        /// <param name="description">A description of the column for autocomplete</param>
        /// <param name="isJson"></param>
        /// <param name="isEncrypted">Is the column an encrypted column</param>
        public ColumnInfo(string filterName, string columnName, string[] aliases, Type columnType, Type enumType, string description, bool isJson, bool isEncrypted)
        {
            FieldName = filterName;
            OriginalName = columnName;
            FieldType = columnType;
            Aliases = aliases;
            if (enumType != null && !enumType.IsEnum)
            {
                throw new ArgumentException("Must specify an enum type", nameof(enumType));
            }

            EnumType = enumType;
            Description = description;
            IsJson = isJson;

            if(isEncrypted && columnType != typeof(string))
            {
                throw new ArgumentException($"Field {FieldName} is marked as encrypted but is not of type string. Encrypted columns must be of type string", nameof(isEncrypted));
            }
            IsEncrypted = isEncrypted;
        }

        /// <summary>
        /// This is the name of the field that the user provides in the filter text
        /// </summary>
        public string FieldName { get; private set; }

        /// <summary>
        /// This is the name of the field that the user provides in the filter text
        /// </summary>
        public string[] Aliases { get; private set; }

        /// <summary>
        /// This is the name of the field as it is emitted into SQL expressions
        /// </summary>
        public string OriginalName { get; private set; }

        /// <summary>
        /// When the user compares a field to a parameter, the parameter must be convertable to this type
        /// </summary>
        public Type FieldType { get; private set; }
        
        /// <summary>
        /// When the user specifies a field to be an enum, the parameter must be mapped by this type
        /// ex. The field type is int and enum is CarType { Sedan = 0, SUV = 1 } so "Sedan" should translate to 0
        /// </summary>
        public Type EnumType { get; private set; }

        /// <summary>
        /// Detailed field documentation for autocomplete, if provided.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// (optional) Set to true if the database column is storing JSON.
        /// </summary>
        public bool IsJson { get; set; } = false;
        
        /// <summary>
        /// (optional) Set to true if the database column is encrypted.
        /// </summary>
        public bool IsEncrypted { get; set; } = false;
    }
}