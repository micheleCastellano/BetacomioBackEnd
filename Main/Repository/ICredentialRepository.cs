using Main.Models;

namespace Main.Repository
{
    public interface ICredentialRepository
    {
        Task<Credential> AddCredentialAsync(string emailAddress, string password);
        (bool, Credential) CheckLogin(string emailAddress, string password);
        Credential GetCredentialsByEmailAddress(string emailAddress);
    }
}