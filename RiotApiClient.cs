using System;
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

        public RiotApiClient()
        {
            this.api = RiotApi.GetDevelopmentInstance("RGAPI-1a38476a-67df-4774-a777-f4fc59727e0c");
            this.region = Region.Na;
        }

        public RelevantMatchInfo checkLastMatchWin(string summonerName)
        {
            try {
                // Retrieve information about the last match the summoner played
                var summoner = this.api.Summoner.GetSummonerByNameAsync(Region.Na, summonerName).Result;
                var matchHistory = this.api.Match.GetMatchListAsync(this.region, summoner.AccountId).Result;
                var matchRef = matchHistory.Matches[0];
                var match = this.api.Match.GetMatchAsync(this.region, matchRef.GameId).Result;

                // Only try to find out about it if the match occurred within the last 30 seconds
                DateTime gameEnd = match.GameCreation + match.GameDuration;
                Console.WriteLine("Last game end time: " + gameEnd + " Current time: " + DateTime.Now);
                if (DateTime.Now - gameEnd > TimeSpan.FromSeconds(30)) {
                    return null;
                }
                
                // Figure out which participant the summoner was and check their win state
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
            try {
                var participants = match.Participants;
                var participantIdentities = match.ParticipantIdentities;
                
                foreach (var participant in participants) {
                    foreach(var participantIdentity in participantIdentities) {
                        if (participant.ParticipantId == participantIdentity.ParticipantId &&
                                participantIdentity.Player.SummonerId.Equals(summoner.Id) &&
                                !participant.Stats.Winner) {
                            // Get champion that was played so we can access its name
                            //var champName = this.api.StaticData.Champions.GetAllAsync(match.GameVersion).Result.Keys[participant.ChampionId];
                            return new RelevantMatchInfo()
                            {
                                summonerName = summoner.Name,
                                championName = "Lux",
                                winner = participant.Stats.Winner,
                                kills = participant.Stats.Kills,
                                deaths = participant.Stats.Deaths,
                                assists = participant.Stats.Assists
                            };
                        }
                    }
                }
            }
            catch (RiotSharpException ex) {
                Console.Write(ex);
            }

            // If no losers found or an exception occurs return null
            return null;
        }
    }
}