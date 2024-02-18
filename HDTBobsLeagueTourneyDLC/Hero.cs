using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using System.Text;

namespace HDTBobsLeagueTourneyDLC
{
    /**
     * Represents a BattleGrounds hero snapshot at a given time
     */

    internal class Hero
    {
        internal int HeroEntityId { get; private set; }
        internal string CardId { get; private set; }

        internal string HeroName { get; private set; }

        internal string Battletag;

        internal int Position { get; private set; }

        internal int Health;

        internal bool IsDead;

        public Hero(int heroEntityId, string cardId, string heroName, string battletag, int position, Entity heroEntity)
        {
            HeroEntityId = heroEntityId;
            CardId = cardId;
            HeroName = heroName;
            Battletag = battletag;
            Position = position;
            Health = heroEntity.Health;
            IsDead = heroEntity.Health <= 0;
        }

        public Hero(Entity heroEntity)
        {
            HeroEntityId = heroEntity.Id;
            CardId = heroEntity.CardId;

            Card cardFromId = Database.GetCardFromId(CardId);
            string heroName = ((cardFromId != null) ? cardFromId.Name : "");
            HeroName = heroName;

            Battletag = "";

            // Checking turn number in case the history is being initialized while already ingame
            if (Core.Game.GetTurnNumber() > 1)
            {
                Position = heroEntity.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE);
            }
            else
            {
                Position = 0;
            }

            Health = heroEntity.Health;

            IsDead = heroEntity.Health <= 0;
        }

        public Hero(Hero hero, Entity heroEntity, int? Position = null)
        {
            HeroEntityId = hero.HeroEntityId;
            CardId = hero.CardId;
            HeroName = hero.HeroName;
            Battletag = hero.Battletag;

            this.Position = Position ?? hero.Position;

            Health = heroEntity.Health;
            IsDead = heroEntity.Health <= 0;
        }

        public override string ToString()
        {
            return new StringBuilder().Append($"HeroEntityId: {HeroEntityId}, ")
                                      .Append($"CardID: {CardId}, ")
                                      .Append($"HeroName: {HeroName}, ")
                                      .Append($"Battletag: {Battletag}, ")
                                      .Append($"Position: {Position}, ")
                                      .Append($"Health: {Health}, ")
                                      .Append($"IsDead: {IsDead}")
                                      .ToString();
        }
    }
}
