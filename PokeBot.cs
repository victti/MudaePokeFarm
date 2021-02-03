using Disqord;
using System;
using System.Threading.Tasks;

namespace MudaePokeFarm
{
    internal class PokeBot
    {
        private readonly DiscordClient _client;

        private readonly Config _config;

        private DateTime lastRollTime;

        public PokeBot(DiscordClient client, Config config)
        {
            _client = client;
            _config = config;
        }

        public async Task InitializeAsync()
        {
            _client.Ready += async args =>
            {
                try
                {
                    Console.WriteLine($"Logged in as {_client.CurrentUser}.\n");

                    Console.WriteLine(
                        "MudaePokeFarm is up and running!\n" +
                        "MudaePokeFarm will shut down when you close this window.\n"
                    );

                    _ = Task.Run(() => RunCommand());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to log in. Exception:\n" + e);
                }
            };

            await _client.RunAsync();
        }

        private async void RunCommand()
        {
            while (true)
            {
                bool success = false;

                try
                {
                    await ((IMessageChannel)_client.GetChannel(_config.ChannelId)).SendMessageAsync("$p");
                    success = true;
                } 
                catch
                {
                    Console.WriteLine(string.Format("An error occurred while trying to roll a pokemon."));
                }

                lastRollTime = DateTime.UtcNow;

                TimeSpan nextRollTime = timeToNextRoll(success, lastRollTime);

                DateTime localTime = lastRollTime.ToLocalTime();

                Console.WriteLine(string.Format("Rolled at {0} | Next roll at {1}", localTime.TimeOfDay, localTime.Add(nextRollTime).TimeOfDay));

                await Task.Delay(nextRollTime);
            }
        }

        private TimeSpan timeToNextRoll(bool success, DateTime now)
        {
            if (success)
            {
                int nextHour = now.Hour < 23 ? now.Hour % 2 == 0 ? now.Hour + 2 : now.Hour + 1 : 1;
                TimeSpan nextRoll = new TimeSpan(nextHour, 0, 5).Subtract(now.TimeOfDay);
                return nextRoll;
            }

            return now.TimeOfDay.Add(new TimeSpan(0, 1, 0));
        }
    }
}
