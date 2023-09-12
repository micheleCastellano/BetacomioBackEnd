using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLibrary
{
    public static class MyEncryptor
    {
        public static string EncryptString256(string sValue)
        {
            string encryptedString = string.Empty;
            try
            {
                SHA256 sha256 = SHA256. Create();
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sValue));
                StringBuilder sb = new();
                for (int i = 0; i < bytes.Length; i++)
                    sb.Append(bytes[i].ToString("x2"));
                encryptedString = sb.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in encryption: " + e.Message);
            }
            return encryptedString;
        }

        public static (string encryptedPass, string salt) EncryptStringSaltInsideHashing(string sValue)
        {
            (string, string) result = ("", "");

            byte[] bytesSalt = new byte[16];

            try
            {
                
                bytesSalt = RandomNumberGenerator.GetBytes(bytesSalt.Length);

                string sHashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: sValue,
                    salt: bytesSalt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 32));

                result = (sHashed, Convert.ToBase64String(bytesSalt));
            }

            catch (Exception ex)
            {

                Console.WriteLine("Error during encryption: " + ex.Message);
            }
            return result;
        }

        public static (string encryptedPass, string salt) EncryptStringSaltOutsideHashing(string sValue)
        {
            string salt =MyRandomGenerator.GenerateAlphanumericValue(16);
            StringBuilder hashedPwd = new();
            hashedPwd.Append(EncryptString256(sValue));
            hashedPwd.Append(EncryptString256(salt));
            return (hashedPwd.ToString(), salt);
        }


        public static bool LoginPwdAndSaltHashedTogether(string pwd, string encryptedPwd, string salt)
        {
            byte[] byteSalt = Convert.FromBase64String(salt);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: pwd,
                salt: byteSalt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32));

            return (hashed == encryptedPwd);
        }

        public static bool LoginPwdAndSaltNotHashedTogether(string pwd, string encryptedPwd, string salt)
        {

            if (string.IsNullOrEmpty(pwd) || string.IsNullOrEmpty(encryptedPwd) || string.IsNullOrEmpty(salt)) return false;

            StringBuilder sb = new();
            sb.Append(EncryptString256(pwd));
            sb.Append(EncryptString256(salt));

            return (sb.ToString() == encryptedPwd);
        }
    }
}
