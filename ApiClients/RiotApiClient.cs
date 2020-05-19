using System;
using System.Threading;
using System.Threading.Tasks;
using RiotSharp;
using RiotSharp.Misc;
using RiotSharp.Endpoints.MatchEndpoint;
using RiotSharp.Endpoints.SummonerEndpoint;
using Objects;

namespace ApiClients
{
    public class RiotApiClient
    {
        private RiotApi api;
        private Region region;
        public event Func<RelevantMatchInfo, int> gameFinished = delegate{ return 0; };
        private LolChampionsClient champsClient;

        public RiotApiClient()
        {
            this.api = RiotApi.GetDevelopmentInstance(Environment.GetEnvironmentVariable("RiotToken"));
            this.region = Region.Na;
            this.champsClient = new LolChampionsClient();
            // Retrieve all of the champion data on startup
            this.champsClient.retrieveChampionData().GetAwaiter().GetResult();
        }

        //public static void Main(string[] args) => new RiotApiClient().matchHistoryCheckLoop();

        // This method runs the core functionality of this component - checking match history and sending info over to the 
        // Discord client whenever it detects a game loss.
        public async void checkMatchHistories(Object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("checking last match state for monitored summoners at {0}.", e.SignalTime);

            foreach (string summoner in StaticData.summonerToDiscordMappings.Keys) {
                RelevantMatchInfo lastMatchInfo = null;
                CancellationTokenSource timeoutCancelTokenSource = new CancellationTokenSource();

                try {
                    Task<RelevantMatchInfo> retrieveTask = this.retrieveLastMatchData(summoner);
                    var completedTask = await Task.WhenAny(retrieveTask, Task.Delay(10000, timeoutCancelTokenSource.Token));

                    if (completedTask == retrieveTask) {
                        timeoutCancelTokenSource.Cancel();
                        lastMatchInfo = retrieveTask.Result;
                    } else {
                        Console.WriteLine("The operation has timed out.");
                        continue;
                    }
                } catch(Exception ex) {
                    Console.WriteLine("The operation has failed: {0}. Skipping to next summoner...", ex.Message);
                    continue;
                }
                
                this.handleLastMatchEvent(lastMatchInfo);
            }
        }

        private void handleLastMatchEvent(RelevantMatchInfo lastMatchInfo)
        {
            Console.WriteLine("Last game end time: {0} Current time: {1}", lastMatchInfo.finishTime, DateTime.Now);

            // Only fire off the gameFinished event for games that happened recently, and only if the discord client is ready to handle it
            if (lastMatchInfo == null) {
                Console.WriteLine("Could not retrieve info about last match for {0}.", lastMatchInfo.summonerName);
            } else if (DateTime.Now - lastMatchInfo.finishTime > TimeSpan.FromMinutes(15)) {
                Console.WriteLine("Summoner {0} has not played a game recently enough to warrant a loss check.", lastMatchInfo.summonerName);
            } else {
                gameFinished(lastMatchInfo);
            }
        }

        public async Task<RelevantMatchInfo> retrieveLastMatchData(string summonerName)
        {
            try {
                Console.WriteLine("Retrieving summoner info...");
                // Retrieve information about the last match the summoner played
                var summoner = await this.api.Summoner.GetSummonerByNameAsync(Region.Na, summonerName);
                Console.WriteLine("Retrieving match list...");
                var matchHistory = await this.api.Match.GetMatchListAsync(this.region, summoner.AccountId);
                var matchRef = matchHistory.Matches[0];
                Console.WriteLine("Retrieving last match info...");
                var match = await this.api.Match.GetMatchAsync(this.region, matchRef.GameId);
                
                Console.WriteLine("Parsing last match info...");
                // Figure out which participant the summoner was and gather relevant information from the match details
                return this.parseMatchData(match, summoner);
            }
            catch (RiotSharpException ex) {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        private RelevantMatchInfo parseMatchData(Match match, Summoner summoner)
        {
            var participants = match.Participants;
            var participantIdentities = match.ParticipantIdentities;
            
            foreach (var participant in participants) {
                foreach(var participantIdentity in participantIdentities) {
                    if (participant.ParticipantId == participantIdentity.ParticipantId && participantIdentity.Player.SummonerId.Equals(summoner.Id)) {
                        // Get champion that was played so we can access its name
                        var champName = this.champsClient.retrieveChampionByKey(participant.ChampionId.ToString()).name;
                        
                        return new RelevantMatchInfo()
                        {
                            finishTime = match.GameCreation.ToLocalTime() + match.GameDuration,
                            summonerName = summoner.Name,
                            championName = champName,
                            winner = participant.Stats.Winner,
                            kills = participant.Stats.Kills,
                            deaths = participant.Stats.Deaths,
                            assists = participant.Stats.Assists
                        };
                    }
                }
            }

            // If summoner not found in game return null.  The should not occur under normal circumstances.
            return null;
        }
    }
}