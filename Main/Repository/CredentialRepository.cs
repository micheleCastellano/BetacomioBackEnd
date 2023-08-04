using Main.Data;
using Main.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using UtilityLibrary;


namespace Main.Repository
{
    public class CredentialRepository : ICredentialRepository
    {
        private readonly BetacomioContext _context;

        public CredentialRepository(BetacomioContext context)
        {
            _context = context;
        }

        public Credential? GetCredentialsByEmailAddress(string emailAddress)
        {
            try
            {
                return _context.Credentials.FirstOrDefault(c => c.EmailAddress == emailAddress);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public (bool, Credential?) CheckLogin(string emailAddress, string password)
        {

            var credentials = GetCredentialsByEmailAddress(emailAddress);

            if (credentials == null)
            {
                return (false, null);
            }

            bool valid = MyEncryptor.LoginPwdAndSaltNotHashedTogether(password, credentials.PasswordHash, credentials.PasswordSalt);

            if (valid)
            {
                return (true, credentials);
            }
            return (false, null);

        }

        public async Task<Credential> AddCredentialAsync(string emailAddress, string password)
        {
            (string encryptedPwd, string salt) = MyEncryptor.EncryptStringSaltInsideHashing(password);


            Credential cred = new Credential();
            cred.PasswordSalt = salt;
            cred.EmailAddress = emailAddress;
            cred.PasswordHash = encryptedPwd;

            _context.Credentials.Add(cred);


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("errore: " + ex.Message);
            }
            return cred;


        }

    }
}