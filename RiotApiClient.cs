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
        public bool hasAnnounced;

        public RiotApiClient()
        {
            this.api = RiotApi.GetDevelopmentInstance("RGAPI-5cef5870-c2a8-48fa-8ebe-0c940f62fc64");
            this.region = Region.Na;
            this.hasAnnounced = false;
        }

        public RelevantMatchInfo checkLastMatchWin(string summonerName)
        {
            try {
                // Retrieve information about the last match the summoner played
                var summoner = this.api.Summoner.GetSummonerByNameAsync(Region.Na, summonerName).Result;
                var matchHistory = this.api.Match.GetMatchListAsync(this.region, summoner.AccountId).Result;
                var matchRef = matchHistory.Matches[0];
                var match = this.api.Match.GetMatchAsync(this.region, matchRef.GameId).Result;

                // Only give data back if the match occurred within the last 15 minutes
                DateTime gameEnd = match.GameCreation.ToLocalTime() + match.GameDuration;
                Console.WriteLine("Last game end time: " + gameEnd + " Current time: " + DateTime.Now);
                if (DateTime.Now - gameEnd > TimeSpan.FromMinutes(15)) {
                    // Release the lock if it's been more than 15 minutes since the last game
                    this.hasAnnounced = false;
                    Console.WriteLine("Announced state: " + this.hasAnnounced);
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
            try {
                var participants = match.Participants;
                var participantIdentities = match.ParticipantIdentities;
                
                foreach (var participant in participants) {
                    foreach(var participantIdentity in participantIdentities) {
                        if (participant.ParticipantId == participantIdentity.ParticipantId &&
                                participantIdentity.Player.SummonerId.Equals(summoner.Id)) {
                            // Get champion that was played so we can access its name
                            //var champName = this.api.StaticData.Champions.GetAllAsync(match.GameVersion).Result.Keys[participant.ChampionId];
                            return new RelevantMatchInfo()
                            {
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
            }
            catch (RiotSharpException ex) {
                Console.Write(ex);
            }

            // If summoner not found in game or an exception occurs return null.  The first case should not occur
            return null;
        }
    }
}