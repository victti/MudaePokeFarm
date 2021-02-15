using Disqord;
using Disqord.Events;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MudaePokeFarm
{
    internal class PokeBot
    {
        static readonly ulong[] _ids =
{
            432610292342587392, // main Mudae bot
            479206206725160960  // the first maid "Mudamaid" which doesn't match _nameRegex
        };

        static readonly Regex _nameRegex = new Regex(@"^Mudae?(maid|butler)\s*\d+$", RegexOptions.Singleline | RegexOptions.Compiled);

        private bool _initialized;

        private readonly DiscordClient _client;

        private readonly Config _config;

        private DateTime _lastRollTime;

        private bool _justRolled;

        private object _rerollLock;

        public PokeBot(DiscordClient client, Config config)
        {
            _client = client;
            _config = config;
            _rerollLock = new object();

            Console.Title = "MudaePokeBot";
        }

        public async Task InitializeAsync()
        {
            _client.Ready += async args =>
            {
                try
                {
                    if (!_initialized)
                    {
                        Console.WriteLine($"Logged in as {_client.CurrentUser}.\n");

                        Console.WriteLine(
                            "MudaePokeFarm is up and running!\n" +
                            "MudaePokeFarm will shut down when you close this window.\n"
                        );

                        _client.MessageReceived += HandleMessageReceived;

                        _ = Task.Run(() => RunCommand2(true));

                        _initialized = true;
                    } else
                    {
                        Console.WriteLine($"Reconnected as {_client.CurrentUser}.\n");

                        _client.MessageReceived += HandleMessageReceived;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to log in. Exception:\n" + e);
                }
            };

            await _client.RunAsync();
        }

#pragma warning disable 1998
        private async Task HandleMessageReceived(MessageReceivedEventArgs e)
#pragma warning restore 1998
        {
            if (!(e.Message is IUserMessage message && IsMudae(message.Author)) || message.ChannelId != _config.ChannelId)
                return;

            string content = message.Content;
            if (content[0] == '<')
                return;

            if (_justRolled)
            {
                string[] splitContent = content.Split(":");
                
                if(splitContent[0] == _client.CurrentUser.Name)
                {
                    string outputMessage = "You rolled: ";
                    if (!splitContent[1].Contains("**"))
                    {
                        string[] pokeLines = content.Split("\n");
                        bool first = true;

                        for(int i = 0; i < pokeLines.Length; i++)
                        {
                            if (pokeLines[i].Contains("Shiny chain"))
                                continue;

                            if (!first)
                            {
                                outputMessage += ", ";
                            }

                            string[] splitPokeLines = pokeLines[i].Split(":");
                            int initialJ = i == 0 ? 1 : 0;

                            string pokeInfo = "(";
                            if (splitPokeLines[initialJ + 1] == "pokenew")
                            {
                                pokeInfo += "New";
                            }
                            if (pokeLines[i].Contains("shinySparkles"))
                            {
                                if (pokeInfo != "(")
                                    pokeInfo += ", ";
                                pokeInfo += "Shiny";
                            }
                            pokeInfo += ")";

                            string[] splitPokeLines2 = pokeLines[i].Split("**");

                            string pokeName = splitPokeLines2[1];

                            if (pokeInfo == "()")
                                pokeInfo = "";

                            outputMessage += pokeName + pokeInfo;

                            if (first)
                            {
                                first = false;
                            }
                        }
                    } else
                    {
                        outputMessage += "Nothing";
                    }

                    outputMessage += "\n";

                    Console.WriteLine(outputMessage);

                    _justRolled = false;
                } else
                {
                    if (splitContent[0].Contains("$p"))
                    {
                        _justRolled = false;
                    }
                }
                return;
            }
            ReRunCommand();
        }

        private async void ReRunCommand()
        {
            await Task.Delay(3500);
            lock (_rerollLock)
            {
                if (_justRolled)
                {
                    Console.WriteLine(string.Format("Failed to roll a pokemon. Rerolling..."));

                    _ = Task.Run(() => RunCommand2(false));
                }
            }
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

                _lastRollTime = DateTime.UtcNow;

                TimeSpan nextRollTime = timeToNextRoll(success, _lastRollTime);

                DateTime localTime = _lastRollTime.ToLocalTime();

                Console.WriteLine(string.Format("Rolled at {0} | Next roll at {1}", localTime.TimeOfDay, localTime.Add(nextRollTime).TimeOfDay));

                _justRolled = true;

                await Task.Delay(nextRollTime);
            }
        }

        private async void RunCommand2(bool repeat)
        {
            do
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

                _lastRollTime = DateTime.UtcNow;

                TimeSpan nextRollTime = timeToNextRoll(success, _lastRollTime);

                DateTime localTime = _lastRollTime.ToLocalTime();

                Console.WriteLine(string.Format("Rolled at {0} | Next roll at {1}", localTime.TimeOfDay, localTime.Add(nextRollTime).TimeOfDay));

                _justRolled = true;

                await Task.Delay(nextRollTime);
            } while (repeat);
        }

        private TimeSpan timeToNextRoll(bool success, DateTime now)
        {
            if (success)
            {
                int nextHour = now.Hour < 23 ? now.Hour % 2 == 0 ? now.Hour + 2 : now.Hour + 1 : 1;
                TimeSpan nextRoll = new TimeSpan(nextHour, 0, 0).Add(_config.Delay).Subtract(now.TimeOfDay);
                if(nextHour == 1)
                {
                    nextRoll = new TimeSpan(24, 0, 0).Add(_config.Delay).Subtract(now.TimeOfDay);
                }
                return nextRoll;
            }

            return now.TimeOfDay.Add(new TimeSpan(0, 1, 0));
        }

        public bool IsMudae(IUser user) => user.IsBot && (Array.IndexOf(_ids, user.Id) != -1 || _nameRegex.IsMatch(user.Name));
    }
}
