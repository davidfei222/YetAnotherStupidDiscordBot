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
        public void matchHistoryCheckLoop()
        {
            while (true) {
                Console.WriteLine("checking last match state for monitored summoners."); // Discord client ready: " + this.discordStarted);

                foreach (string summoner in StaticData.summonerToDiscordMappings.Keys) {
                    CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                    RelevantMatchInfo lastMatchInfo = null;
                    var lastMatchRetrieval = Task.Factory.StartNew(() => this.retrieveLastMatchData(summoner), cancelTokenSource.Token);

                    if (lastMatchRetrieval.Wait(TimeSpan.FromSeconds(30))) {
                        lastMatchInfo = lastMatchRetrieval.Result;
                    } else {
                        cancelTokenSource.Cancel();
                        Console.WriteLine("A request has timed out.");
                        continue;
                    }

                    Console.WriteLine("Last game end time: " + lastMatchInfo.finishTime + " Current time: " + DateTime.Now);

                    // Only fire off the gameFinished event for games that happened recently, and only if the discord client is ready to handle it
                    if (lastMatchInfo == null) {
                        Console.WriteLine("Could not retrieve info about last match for " + summoner + ".");
                    } else if (DateTime.Now - lastMatchInfo.finishTime > TimeSpan.FromMinutes(15)) {
                        Console.WriteLine("Summoner " + lastMatchInfo.summonerName + " has not played a game recently enough to warrant a loss check.");
                    } else {
                        gameFinished(lastMatchInfo);
                    }
                }

                Thread.Sleep(30000);
            }
        }

        public RelevantMatchInfo retrieveLastMatchData(string summonerName)
        {
            try {
                Console.WriteLine("Retrieving summoner info...");
                // Retrieve information about the last match the summoner played
                var summoner = this.api.Summoner.GetSummonerByNameAsync(Region.Na, summonerName).Result;
                Console.WriteLine("Retrieving match list...");
                var matchHistory = this.api.Match.GetMatchListAsync(this.region, summoner.AccountId).Result;
                var matchRef = matchHistory.Matches[0];
                Console.WriteLine("Retrieving last match info...");
                var match = this.api.Match.GetMatchAsync(this.region, matchRef.GameId).Result;
                
                Console.WriteLine("Parsing last match info...");
                // Figure out which participant the summoner was and gather relevant information from the match details
                return this.parseMatchData(match, summoner);
            }
            catch (RiotSharpException ex) {
                Console.WriteLine(ex.Message);
            }
            
            // If an exception occurs return null
            return null;
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