namespace Searchlight.Encryption
{
    /// <summary>
    /// An interface for encrypting strings prior to being used in a query filter.
    /// </summary>
    public interface ISearchlightStringEncryptor
    {
        /// <summary>
        /// A method to encrypt a string using the same algorithm used to store the encrypted data.
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        string Encrypt(string plainText);
    }
}