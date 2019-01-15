using System.Threading.Tasks;

namespace ExternalApiProxy
{
    public interface IExternalApiClient
    {
        Task<string> GetSuccessfulResponse();
    }
}