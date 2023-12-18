using HDTBobsLeagueTourneyDLC.constants;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HearthDb.Enums.GameTag;
using static HDTBobsLeagueTourneyDLC.constants.PluginConstants;


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
            await HeroesSelection();
            TurnState newTurn = new TurnState();

            GameHistory = new Dictionary<int, TurnState>
            {
                { Core.Game.GetTurnNumber(), newTurn }
            };
            LogGameHistory();

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
            Log.Info($"Name: <{opponentEntity.Name}>");

            Entity heroEntity = Core.Game.Entities.Values.Where(x => x.Id == opponentEntity.GetTag(HERO_ENTITY)).FirstOrDefault();

            TurnState currentTurnState = GameHistory[Core.Game.GetTurnNumber()];

            Hero currentOpponent = currentTurnState.Heroes.Where(hero => hero.CardID == heroEntity.CardId).FirstOrDefault();

            if (currentOpponent == null)
            {
                Log.Error($"Current opponent {opponentEntity.Name} could not be matched to any known ennemies. Maybe SetHeroAsync() was too slow...");
            }

            currentOpponent.Battletag = opponentEntity.Name;

            DumpState();
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
                Log.Error($"Existing name <{hero.Battletag}>");
            }
        }
    }
}