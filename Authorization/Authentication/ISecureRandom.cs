namespace Starcounter.Authorization.Authentication
{
    internal interface ISecureRandom
    {
        /// <summary>
        /// Generates a string of hex characters ([0-9a-fA-F]) with cryptographically strong PRNG.
        /// </summary>
        /// <param name="bytesLength">number of bytes to generate. The generated string will have twice that number of characters</param>
        /// <returns>A random string of length twice the <paramref name="bytesLength"/>, consisting of [0-9a-fA-F] characters</returns>
        string GenerateRandomHexString(int bytesLength);
    }
}