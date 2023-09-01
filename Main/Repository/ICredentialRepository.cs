using Main.Models;

namespace Main.Repository
{
    public interface ICredentialRepository
    {
        Task<Credential?> AddCredentialAsync(string emailAddress, string password);
        Task<Credential?> CheckLoginAdventure(string emailAddress);
        Credential? CheckLoginBetacomio(string emailAddress, string password);
        Task<Credential?> GetCredentialByEmailAddressBetacomioDBAsync(string emailAddress);
        Task<bool> EmailAddressExist(string emailAddress);
    }
}