using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            await AwaitHeroesSelection();
            TurnState newTurn = new TurnState();
            Log.Info("Logging qfqsd...");

            GameHistory = new Dictionary<int, TurnState>
            {
                { Core.Game.GetTurnNumber(), newTurn }
            };
            LogGameHistory();

            FetchBobsEntityId();

            // TODO ? Update Battletag if reconnecting during a fight turn ?
        }

        private async Task AwaitHeroesSelection()
        {
            const int maxAttempts = 40;
            const int delayBetweenAttempts = 250;

            int selectedHeroesCount = 0;

            while (selectedHeroesCount != 8)
            {
                await Task.Delay(delayBetweenAttempts);

                if (selectedHeroesCount == 8)
                {
                    Log.Info("All heroes have been selected.");
                    break;
                }

            }
            for (var i = 0; i < maxAttempts; i++)
            {
                Log.Info($"Attempt: {i}. Elapsed: {i * delayBetweenAttempts}");

                await Task.Delay(delayBetweenAttempts);

                if (selectedHeroesCount == 8)
                {
                    Log.Info("All heroes have been selected.");
                    break;
                }
            }
        }

        private void LogGameHistory()
        {
            Log.Info("Logging GameHistory...");

            foreach (var kvp in GameHistory)
            {
                Log.Info(kvp.Key.ToString());
                Log.Info(kvp.Value.ToString());
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

        internal void HandleNewTurn(ActivePlayer player)
        {
            Log.Error("DLC - New Turn");
            Log.Error($"DLC - player: {player}");
            Log.Error($"DLC - turn: {Core.Game.GetTurnNumber()}");
            LogGameHistory();

            // TODO add a check (done only once) that the GameHistory has been initialized or wait

            if (player == ActivePlayer.Opponent)
            {
                UpdateBattletag(OpponentEntityId);
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
            //BattlegroundsUtils.GetOriginalHeroId("abc");
            //GameStats
            //GameV2 public bool Spectator;
            */
        }

        private void UpdateBattletag(int opponentEntityId)
        {
            Entity opponentEntity = Core.Game.Entities.Values.Where(entity => entity.Id == opponentEntityId).FirstOrDefault();
            Log.Error($"Name: <{opponentEntity.Name}>");

            Entity heroEntity = Core.Game.Entities.Values.Where(x => x.Id == opponentEntity.GetTag(HERO_ENTITY)).FirstOrDefault();

            TurnState currentTurnState = GameHistory[Core.Game.GetTurnNumber()];
            Hero currentOpponent = currentTurnState.Heroes.Where(hero => hero.HeroEntityId == heroEntity.Id).FirstOrDefault();

            currentOpponent.Battletag = opponentEntity.Name;
        }

        internal void SaveEndGameState()
        {
            Log.Error("DLC - End game");

            DumpState();
        }

        private void DumpState()
        {
            Log.Error("Ca dump dur ici");

            foreach (var hero in GameHistory[Core.Game.GetTurnNumber()].Heroes)
            {
                Log.Error(hero.Battletag);
            }
        }
    }
}