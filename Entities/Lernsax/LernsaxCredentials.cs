using System;
using System.Text;
using VpServiceAPI.Tools;

namespace VpServiceAPI.Entities.Lernsax
{
    public sealed record LernsaxCredentials
    {
        private const string HashSeparator = "--|--";
        public LernsaxCredentials(string encryptedCredentials)
        {
            (Address, Password) = Decrypt(encryptedCredentials);            
        }
        public LernsaxCredentials(string mail, string password)
        {
            Address = mail;
            Password = password;
        }

        public string Address { get; set; }
        public string Password { get; set; }

        public string Encrypt()
        {
            var addressHash = StringCipher.Encrypt(Address, EncryptionKey.LERNSAX);
            var passwordHash = StringCipher.Encrypt(Password, EncryptionKey.LERNSAX);

            return addressHash + HashSeparator + passwordHash;
        }
        private (string address, string password) Decrypt(string encrypted)
        {
            var splitted = encrypted.Split(HashSeparator);

            var addressHash = splitted[0];
            var passwordHash = splitted[1];


            return (StringCipher.Decrypt(addressHash, EncryptionKey.LERNSAX), 
                StringCipher.Decrypt(passwordHash, EncryptionKey.LERNSAX));
        }
        

    }
}
