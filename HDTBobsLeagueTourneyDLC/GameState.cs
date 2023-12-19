using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HDTBobsLeagueTourneyDLC.constants.PluginConstants;
using static HearthDb.Enums.GameTag;


namespace HDTBobsLeagueTourneyDLC
{
    internal class GameState
    {
        private string Author;
        private int OpponentEntityId = 0;
        private bool IsSpectator;

        private Dictionary<int, TurnState> GameHistory;

        public GameState() { }

        internal async void InitializeGame()
        {
            // TODO Add ourselve in the player list
            await HeroesSelection();
            TurnState newTurn = new TurnState();

            GameHistory = new Dictionary<int, TurnState>
            {
                { Core.Game.GetTurnNumber(), newTurn }
            };

            FetchBobsEntityId();

            // TODO ? Update Battletag if reconnecting during a fight turn ?
        }

        private async Task HeroesSelection()
        {
            const int delayBetweenAttempts = 250;

            int selectedHeroesCount = 0;

            while (selectedHeroesCount != PLAYER_COUNT)
            {
                selectedHeroesCount = Core.Game.Entities.Values.Where(x => x.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE) != 0).Count();

                Log.Info($"Number of heroes selected: {selectedHeroesCount}.");

                // TODO log hero names as they are picked

                if (selectedHeroesCount == 8)
                {
                    Log.Info("All heroes have been selected.");
                    break;
                }
                await Task.Delay(delayBetweenAttempts);
            }
        }

        private void FetchBobsEntityId()
        {
            Log.Info("Time to go get Bob !");

            Entity bobHeroEntity = Core.Game.Entities.Values.Where(entity =>
                {
                    if (entity.CardId != null)
                    {
                        return entity.CardId == "TB_BaconShopBob";
                    }
                    else
                    {
                        return false;
                    }
                }).FirstOrDefault();

            Log.Info("Bob entity found (hopefully).");

            Entity opponentEntity = Core.Game.Entities.Values.Where(entity => entity.GetTag(HERO_ENTITY) == bobHeroEntity.Id).FirstOrDefault();

            Log.Info($"OpponentEntityId: {opponentEntity.Id}");
            Log.Info($"OpponentEntityName: {opponentEntity.Name}");
            OpponentEntityId = opponentEntity.Id;
        }

        internal async void HandleNewTurn(ActivePlayer player)
        {
            Log.Error("DLC - New Turn");
            Log.Error($"DLC - player: {player}");
            Log.Error($"DLC - turn: {Core.Game.GetTurnNumber()}");

            // TODO add a check (done only once) that the GameHistory has been initialized or wait

            if (player == ActivePlayer.Opponent)
            {
                await UpdateBattletag(OpponentEntityId);
            }
            else if (player == ActivePlayer.Player)
            {
                SaveNewTurnState();
            }
            else
            {
                Log.Error("ActivePlayer is None and I don't know why...");
            }

            DumpState();
        }

        private void SaveNewTurnState()
        {
            TurnState newTurn = new TurnState(GameHistory[Core.Game.GetTurnNumber() - 1], OpponentEntityId);

            GameHistory.Add(Core.Game.GetTurnNumber(), newTurn);

            /* Resources :
             * TODO At least one hero can tranform. It's probably troublesome and we need to deal with it
            //BattlegroundsUtils.GetOriginalHeroId("abc");
            //GameStats
            //GameV2 public bool Spectator;
            */
        }

        private async Task UpdateBattletag(int opponentEntityId)
        {
            Entity heroEntity = null;
            Entity opponentEntity = null;

            /* HDT decided not to "await" for SetHeroAsync() and there can be cases where
             * opponent is still Bob at the beginning of a fight...
             */
            do
            {
                opponentEntity = Core.Game.Entities.Values.Where(entity => entity.Id == opponentEntityId).FirstOrDefault();
                Log.Info($"Name: <{opponentEntity.Name}>");

                heroEntity = Core.Game.Entities.Values.Where(x => x.Id == opponentEntity.GetTag(HERO_ENTITY)).FirstOrDefault();
                await Task.Delay(100);
            } while (heroEntity.CardId == BOB_CARDID);


            TurnState currentTurnState = GameHistory[Core.Game.GetTurnNumber()];

            Hero currentOpponent = currentTurnState.Heroes.Where(hero => hero.CardID == heroEntity.CardId).FirstOrDefault();

            if (currentOpponent == null)
            {
                Log.Error($"Current opponent {opponentEntity.Name} could not be matched to any known ennemies.");
            }
            else
            {
                currentOpponent.Battletag = opponentEntity.Name;
            }
        }

        internal void SaveEndGameState()
        {
            Log.Error("DLC - End game");

            DumpState();
        }

        private void DumpState()
        {
            foreach (Hero hero in GameHistory[Core.Game.GetTurnNumber()].Heroes)
            {
                Log.Error($"Existing name <{hero.Battletag}> at <{hero.Position}>");
            }
        }
    }
}