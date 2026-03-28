using Microsoft.AspNetCore.DataProtection;

namespace ECommerceAPI.Services
{
    public class EncryptionService
    {
        private readonly IDataProtector _protector;

        public EncryptionService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("ECommerceAPI.UrlEncryption.v1");
        }

        public string Encrypt(string input)
        {
            return _protector.Protect(input);
        }
        public string Decrypt(string encrypted)
        {
            return _protector.Unprotect(encrypted);
        }
    }
}
