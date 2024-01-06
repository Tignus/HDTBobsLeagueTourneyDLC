using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace HDTBobsLeagueTourneyDLC
{
    internal class Hero
    {
        internal int HeroEntityId { get; private set; }
        internal string CardID { get; private set; }

        private string HeroName;

        internal string Battletag;

        internal int Position { get; private set; }

        private bool IsDead; // What if 2 dead in the same turn ? Is the position correctly fetched ?

        public Hero(int heroEntityId, string cardId, string heroName, string battletag, int position, bool isDead)
        {
            HeroEntityId = heroEntityId;
            CardID = cardId;
            HeroName = heroName;
            Battletag = battletag;
            Position = position;
            IsDead = isDead;
        }

        public Hero(Entity heroEntity)
        {
            HeroEntityId = heroEntity.Id; // TODO fix data saving (hero name, ...)
            CardID = heroEntity.CardId;

            Card cardFromId = Database.GetCardFromId(CardID);
            string heroName = ((cardFromId != null) ? cardFromId.Name : "");
            HeroName = heroName;

            Battletag = "";
            Position = 0;
            IsDead = false;
        }

        public Hero(Hero hero, int? Position = null, bool? IsDead = null)
        {
            HeroEntityId = hero.HeroEntityId;
            CardID = hero.CardID;
            HeroName = hero.HeroName;
            Battletag = hero.Battletag;
            this.Position = Position ?? hero.Position;
            this.IsDead = IsDead ?? hero.IsDead;
        }

        public Hero Clone(int? Position = null, bool? IsDead = null)
        {
            return new Hero(heroEntityId: HeroEntityId,
                            cardId: CardID,
                            heroName: HeroName,
                            battletag: Battletag,
                            position: Position ?? this.Position,
                            isDead: IsDead ?? this.IsDead);
        }
    }
}
