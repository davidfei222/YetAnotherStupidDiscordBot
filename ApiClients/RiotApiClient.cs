using System;
using System.Threading;
using System.Collections.Generic;
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
            this.api = RiotApi.GetDevelopmentInstance("RGAPI-3afede4e-f745-4b0b-befe-f2ab1b845c38");
            this.region = Region.Na;
            this.champsClient = new LolChampionsClient();
            // Retrieve all of the champion data on startup
            this.champsClient.retrieveChampionData().GetAwaiter().GetResult();
        }

        // This method runs in its own thread to continually check for a recently finished game. 
        // When a recently finished game is detected, the gameFinished event is fired off.
        public void matchHistoryCheckLoop()
        {
            while (true) {
                Console.WriteLine("checking last match state for monitored summoners."); // Discord client ready: " + this.discordStarted);

                foreach (string summoner in StaticData.summonerToDiscordMappings.Keys) {
                    var lastMatchInfo = this.retrieveLastMatchData(summoner);
                    Console.WriteLine("Last game end time: " + lastMatchInfo.finishTime + " Current time: " + DateTime.Now);

                    // Only fire off the gameFinished event for games that happened recently, and only if the discord client is ready to handle it
                    if (lastMatchInfo == null) {
                        Console.WriteLine("Could not retrieve info about last match for " + summoner + ".");
                    } else if (DateTime.Now - lastMatchInfo.finishTime > TimeSpan.FromMinutes(15)) {
                        Console.WriteLine("Summoner " + lastMatchInfo.summonerName + " has not lost a game recently enough to warrant a loss check.");
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
                // Retrieve information about the last match the summoner played
                var summoner = this.api.Summoner.GetSummonerByNameAsync(Region.Na, summonerName).Result;
                var matchHistory = this.api.Match.GetMatchListAsync(this.region, summoner.AccountId).Result;
                var matchRef = matchHistory.Matches[0];
                var match = this.api.Match.GetMatchAsync(this.region, matchRef.GameId).Result;
 
                // Figure out which participant the summoner was and gather relevant information from the match details
                return this.parseMatchData(match, summoner);
            }
            catch (RiotSharpException ex) {
                Console.Write(ex);
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