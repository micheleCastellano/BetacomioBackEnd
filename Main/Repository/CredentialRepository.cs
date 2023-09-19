using Main.Controllers;
using Main.Data;
using Main.EmailSender;
using Main.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using UtilityLibrary;


namespace Main.Repository
{
    public class CredentialRepository : ICredentialRepository
    {
        private readonly BetacomioContext _contextBetacomio;
        private readonly AdventureWorksLt2019Context _contextAdventure;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CredentialRepository> _logger;


        public CredentialRepository(BetacomioContext contextBetacomio, AdventureWorksLt2019Context contextAdventure, IEmailSender emailSender, ILogger<CredentialRepository> logger)
        {
            _contextBetacomio = contextBetacomio;
            _contextAdventure = contextAdventure;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<bool> EmailAddressExist(string emailAddress)
        {
            try
            {
                var customer =await _contextAdventure.Customers.FirstOrDefaultAsync(c => c.EmailAddress == emailAddress);

                if (customer != null)
                {
                    return true;
                }

                var credentials = await _contextBetacomio.Credentials.FirstOrDefaultAsync(c=> c.EmailAddress == emailAddress);

                if (credentials != null)
                {
                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return true;
            }
        }

        public async Task<Credential?> GetCredentialByEmailAddressBetacomioDBAsync(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                return null;
            }

            try
            {
                return await _contextBetacomio.Credentials.FirstOrDefaultAsync(c => c.EmailAddress == emailAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }

        public Credential? CheckLoginBetacomio(string emailAddress, string password)
        {
            try
            {
                Credential credential = _contextBetacomio.Credentials.First(c => c.EmailAddress == emailAddress);

                bool valid = MyEncryptor.LoginPwdAndSaltHashedTogether(password, credential.PasswordHash, credential.PasswordSalt);

                if (!valid)
                {
                    return null;
                }

                return credential;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }
        // NEW
        public Credential? CheckAdminBetacomio(string emailAddress, string password)
        {
            try
            {
                Credential credential = _contextBetacomio.Credentials.First(c => c.EmailAddress == emailAddress && c.AdminAccess);

                bool valid = MyEncryptor.LoginPwdAndSaltHashedTogether(password, credential.PasswordHash, credential.PasswordSalt);

                if (!valid)
                {
                    return null;
                }

                return credential;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }

        public async Task<Credential?> CheckLoginAdventure(string emailAddress)
        {
            try
            {
                Customer customer = _contextAdventure.Customers.First(c => c.EmailAddress == emailAddress);

                string pwd = MyRandomGenerator.GenerateAlphanumericValue(10);

                await _emailSender.SendEmailAsync(emailAddress, customer.FirstName, pwd);

                return await AddCredentialAsync(emailAddress, pwd);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }

        public async Task<Credential?> AddCredentialAsync(string emailAddress, string password)
        {
            (string encryptedPwd, string salt) = MyEncryptor.EncryptStringSaltInsideHashing(password);

            Credential cred = new Credential()
            {
                PasswordSalt = salt,
                EmailAddress = emailAddress,
                PasswordHash = encryptedPwd
            };

            _contextBetacomio.Credentials.Add(cred);

            try
            {
                await _contextBetacomio.SaveChangesAsync();
                return cred;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }

    }
}