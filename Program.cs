using System;
using System.Threading.Tasks;
using Disqord;
using Microsoft.Extensions.DependencyInjection;

namespace MudaePokeFarm
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var container = BuildServiceProvider();
            var bot = container.GetRequiredService<PokeBot>();
            return bot.InitializeAsync();
        }

        private static IServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton<Config>()
                .AddSingleton<PokeBot>()
                .AddSingleton(container => new DiscordClient(TokenType.User, container.GetRequiredService<Config>().Token))
                .BuildServiceProvider();
        }
    }
}
