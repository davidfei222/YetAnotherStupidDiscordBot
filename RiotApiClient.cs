using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using RiotSharp;
using RiotSharp.Misc;
using RiotSharp.Endpoints.MatchEndpoint;
using RiotSharp.Endpoints.SummonerEndpoint;
using Objects;

namespace YetAnotherStupidDiscordBot 
{
    class RiotApiClient
    {
        private RiotApi api;
        private Region region;
        public bool hasAnnounced;
        public event Func<RelevantMatchInfo, int> gameFinished = delegate{ return 0; };
        public List<string> monitoredSummoners;

        public RiotApiClient()
        {
            this.api = RiotApi.GetDevelopmentInstance("RGAPI-60224a15-404c-4c7c-a655-475a1d263d32");
            this.region = Region.Na;
            this.hasAnnounced = false;
            this.monitoredSummoners = new List<string>()
            {
                "AnimePenguin"
            };
        }

        // This method runs in its own thread to continually check for a recently finished game. 
        // When a recently finished game is detected, the gameFinished event is fired off.
        public void matchHistoryCheckLoop()
        {
            while (true) {
                Console.WriteLine("checking last match state for monitored summoners");

                foreach (string summoner in this.monitoredSummoners) {
                    var lastMatchInfo = this.checkLastMatchWin(summoner);
                    if (lastMatchInfo != null) {
                        gameFinished(lastMatchInfo);
                    }
                }

                Thread.Sleep(30000);
            }
        }

        private RelevantMatchInfo checkLastMatchWin(string summonerName)
        {
            try {
                // Retrieve information about the last match the summoner played
                var summoner = this.api.Summoner.GetSummonerByNameAsync(Region.Na, summonerName).Result;
                var matchHistory = this.api.Match.GetMatchListAsync(this.region, summoner.AccountId).Result;
                var matchRef = matchHistory.Matches[0];
                var match = this.api.Match.GetMatchAsync(this.region, matchRef.GameId).Result;

                // Only return data about games that happened recently
                DateTime gameEnd = match.GameCreation.ToLocalTime() + match.GameDuration;
                Console.WriteLine("Last game end time: " + gameEnd + " Current time: " + DateTime.Now);
                if (DateTime.Now - gameEnd > TimeSpan.FromMinutes(15)) {
                    Console.WriteLine("Summoner " + summonerName + " has not lost a game recently enough to warrant a loss check.");
                    return null;
                }
                
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
                        //var champName = this.api.StaticData.Champions.GetAllAsync(match.GameVersion).Result.Keys[participant.ChampionId];
                        return new RelevantMatchInfo()
                        {
                            finishTime = match.GameCreation.ToLocalTime() + match.GameDuration,
                            summonerName = summoner.Name,
                            championName = participant.ChampionId.ToString(),
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