using System.Threading;
using System.Threading.Tasks;

namespace CulinaryRecipes.API.Mediation
{
    public interface IMediator
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}
