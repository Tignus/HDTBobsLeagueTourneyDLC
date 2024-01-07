using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System.Collections.Generic;
using System.Linq;
using static HDTBobsLeagueTourneyDLC.constants.PluginConstants;

namespace HDTBobsLeagueTourneyDLC
{
    internal class TurnState
    {
        internal List<Hero> Heroes { get; private set; }

        /* This constructor is used only at game start
         * */

        public TurnState()
        {
            int selectedHeroesCount = Core.Game.Entities.Values.Where(x => x.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE) != 0).Count();

            if (selectedHeroesCount != PLAYER_COUNT)
            {
                Log.Error($"Available heroes count is not {PLAYER_COUNT} ({selectedHeroesCount})");
            }

            Player player = Core.Game.Player;
            string playerHeroCardId = player.Board.FirstOrDefault(x => x.IsHero).CardId;

            Heroes = new List<Hero>();
            foreach (Entity heroEntity in Core.Game.Entities.Values.Where(x => x.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE) != 0))
            {
                Hero hero = new Hero(heroEntity);

                if (hero.CardID == playerHeroCardId)
                {
                    hero.Battletag = player.Name;
                }

                Heroes.Add(hero);
                Log.Error($"Hero {heroEntity} added.");
            }
        }

        public TurnState(TurnState previousTurn, int opponentEntityId)
        {
            Heroes = new List<Hero>();

            foreach (Entity heroEntity in Core.Game.Entities.Values.Where(x => x.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE) != 0).Take(PLAYER_COUNT))
            {
                int currentPosition = GetHeroPosition(heroEntity);

                if (currentPosition == 0)
                {
                    Log.Error($"Hero {heroEntity} is at position 0 in leaderboard");
                }

                Hero previousHero = previousTurn.Heroes.Find(hero =>
                {
                    return hero.CardID == heroEntity.CardId;
                });

                Heroes.Add(new Hero(previousHero, currentPosition, false)); // TODO deal with death
            }
        }

        private int GetHeroPosition(Entity heroEntity)
        {
            return Core.Game.Entities.Values.Where(x => x.Id == heroEntity.Id).FirstOrDefault().GetTag(GameTag.PLAYER_LEADERBOARD_PLACE);
        }
    }
}
