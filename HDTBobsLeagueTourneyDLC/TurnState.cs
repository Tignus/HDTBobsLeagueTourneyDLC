using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

                if (hero.CardId == playerHeroCardId)
                {
                    hero.Battletag = player.Name;
                }

                Heroes.Add(hero);
                Log.Error($"Hero {Database.GetCardFromId(heroEntity.CardId).Name} added.");
            }
        }

        public TurnState(TurnState previousTurn)
        {
            Heroes = new List<Hero>();

            foreach (Entity heroEntity in Core.Game.Entities.Values.Where(x => x.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE) != 0).Take(PLAYER_COUNT))
            {
                int currentPosition = GetHeroPosition(heroEntity);
                bool isDead = heroEntity.Health <= 0;

                Hero previousHero = previousTurn.Heroes.Find(hero =>
                {
                    // Wil not works with skins since TransformableHeroCardidTable is incomplete
                    return hero.CardId == BattlegroundsUtils.GetOriginalHeroId(heroEntity.CardId);
                });

                Heroes.Add(new Hero(previousHero, heroEntity, currentPosition));
            }
        }

        private int GetHeroPosition(Entity heroEntity)
        {
            if (Core.Game.GetTurnNumber() == 1)
            {
                return 0;
            }
            return Core.Game.Entities.Values.Where(x => x.Id == heroEntity.Id).FirstOrDefault().GetTag(GameTag.PLAYER_LEADERBOARD_PLACE);
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            foreach (var hero in Heroes)
            {
                stringBuilder.AppendLine(hero.ToString());
            }
            return stringBuilder.ToString();
        }
    }
}
