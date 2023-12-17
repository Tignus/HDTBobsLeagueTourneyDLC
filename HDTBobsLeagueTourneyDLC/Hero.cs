using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;

namespace HDTBobsLeagueTourneyDLC
{
    internal class Hero
    {
        internal int HeroEntityId { get; private set; }

        private string HeroName;

        internal string Battletag { get; set; }

        private int Position;

        private bool IsDead;
        public Hero(int heroEntityId, int position, string battletag = "")
        {
            HeroEntityId = heroEntityId; // TODO fix data saving (hero name, ...)
            Battletag = battletag;
            Position = position;
        }
        public Hero(int heroEntityId, string heroName, string battletag, int position, bool isDead)
        {
            HeroEntityId = heroEntityId;
            HeroName = heroName;
            Battletag = battletag;
            Position = position;
            IsDead = isDead;
        }
        public Hero(Hero hero, int? Position = null, bool? IsDead = null)
        {
            HeroEntityId = hero.HeroEntityId;
            HeroName = hero.HeroName;
            Battletag = hero.Battletag;
            this.Position = Position ?? hero.Position;
            this.IsDead = IsDead ?? hero.IsDead;
        }

        public Hero Clone(int? Position = null, bool? IsDead = null)
        {
            return new Hero(heroEntityId: HeroEntityId,
                            heroName: HeroName,
                            battletag: Battletag,
                            position: Position ?? this.Position,
                            isDead: IsDead ?? this.IsDead);
        }
    }
}
