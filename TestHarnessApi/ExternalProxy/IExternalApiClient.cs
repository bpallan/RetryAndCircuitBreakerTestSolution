using System.Threading.Tasks;

namespace TestHarnessApi.ExternalProxy
{
    public interface IExternalApiClient
    {
        Task<string> GetSuccessfulResponse();
        Task<string> GetUnreliableResponse();
        Task<string> GetFailureResponse();
    }
}