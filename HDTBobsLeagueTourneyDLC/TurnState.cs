using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System.Collections.Generic;
using System.Linq;

namespace HDTBobsLeagueTourneyDLC
{
    internal class TurnState
    {
        internal List<Hero> Heroes { get; private set; }
        public TurnState()
        {
            Heroes = new List<Hero>();

            for (int rank = 1; rank < 8; rank++)
            {
                // TODO log hero count
                Entity heroEntity = getHeroAtRank(rank);
                if (heroEntity != null)
                {
                    Card cardFromId = Database.GetCardFromId(heroEntity.CardId);
                    string heroName = ((cardFromId != null) ? cardFromId.Name : "");
                    Log.Info($"Adding hero {heroName} at rank {rank}");

                    Heroes.Add(new Hero(heroEntity.Id, rank));
                    Log.Info($"Hero added");
                }
                else
                {
                    Log.Info("Missing hero");

                    Log.Info($"Adding disconnected hero at rank {rank}");
                    Heroes.Add(new Hero(0, rank, "Disconnected ?"));
                }


            }
        }
        public TurnState(TurnState previousTurn, int opponentEntityId)
        {
            for (int i = 1; i < 8; i++)
            {
                Entity entity = getHeroAtRank(i);

                Hero previousHero = previousTurn.Heroes.Find(hero =>
                {
                    return hero.HeroEntityId == entity.Id;
                });

                Heroes.Add(new Hero(previousHero, i, false)); // TODO deal with death
            }
        }

        private Entity getHeroAtRank(int rank)
        {
            return Core.Game.Entities.Values.Where(x => x.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE) == rank).FirstOrDefault();
        }



    }
}
