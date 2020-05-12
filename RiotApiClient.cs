using System;
using RiotSharp;
using RiotSharp.Misc;
using RiotSharp.Endpoints.MatchEndpoint;
using RiotSharp.Endpoints.SummonerEndpoint;

namespace YetAnotherStupidDiscordBot 
{
    class RiotApiClient
    {
        private RiotApi api;
        private Region region;

        public RiotApiClient()
        {
            this.api = RiotApi.GetDevelopmentInstance("RGAPI-43457f36-af2b-4ec6-9188-a27575c0a7a1");
            this.region = Region.Na;
        }

        private bool checkLastMatchWin(Summoner summoner)
        {
            try {
                // Retrieve information about the last match the summoner played
                var matchHistory = this.api.Match.GetMatchListAsync(this.region, summoner.AccountId).Result;
                var matchRef = matchHistory.Matches[0];
                var match = this.api.Match.GetMatchAsync(this.region, matchRef.GameId).Result;
                
                // Figure out which participant the summoner was and check their win state
                var participants = match.Participants;
                var participantIdentities = match.ParticipantIdentities;
                
                foreach (var participant in participants) {
                    foreach(var participantIdentity in participantIdentities) {
                        if (participant.ParticipantId == participantIdentity.ParticipantId &&
                            participantIdentity.Player.SummonerId.Equals(summoner.Id)) {
                            return participant.Stats.Winner;
                        }
                    }
                }
            }
            catch (RiotSharpException ex) {
                // Handle the exception however you want.
                Console.Write(ex);
            }
            
            // If exception occurs just assume false
            return false;
        }

        private void riotApiTest()
        {
            try {
                var summoner = this.api.Summoner.GetSummonerByNameAsync(Region.Na, "AnimePenguin").Result;
                var name = summoner.Name;
                var level = summoner.Level;
                var accountId = summoner.AccountId;
                Console.WriteLine("Name: " + name + ", Level: " + level + ", accountId: " + accountId);

                var matchHistory = this.api.Match.GetMatchListAsync(Region.Na, accountId).Result;
                MatchReference lastMatch = matchHistory.Matches[matchHistory.EndIndex-1];
                Console.WriteLine(lastMatch.ChampionID + " " + lastMatch.Lane + " " + lastMatch.Role + " " + lastMatch.Queue);
            }
            catch (RiotSharpException ex) {
                // Handle the exception however you want.
                Console.Write(ex);
            }
        }
    }
}