using System;
using System.Security.Cryptography;

namespace Starcounter.Authorization.Authentication
{
    internal class SecureRandom : ISecureRandom
    {
        private readonly RandomNumberGenerator _randomNumberGenerator;

        public SecureRandom()
        {
            _randomNumberGenerator = RandomNumberGenerator.Create();
        }

        public string GenerateRandomHexString(int bytesLength)
        {
            var bytes = new byte[bytesLength];
            _randomNumberGenerator.GetBytes(bytes);
            return BitConverter.ToString(bytes).Replace("-","");
        }
    }
}