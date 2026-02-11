using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Horizon.Database.Plugins
{
    public interface IPlugin
    {
        void Register(IConfiguration configuration, IServiceCollection services);
    }
}
