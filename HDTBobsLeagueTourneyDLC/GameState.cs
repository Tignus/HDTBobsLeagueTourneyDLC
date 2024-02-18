using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static HDTBobsLeagueTourneyDLC.constants.PluginConstants;
using static HearthDb.Enums.GameTag;

namespace HDTBobsLeagueTourneyDLC
{
    internal class GameState
    {
        /* Resources :
        // TODO (Global) Use info or warn instead of error when needed

        GameStats

        GameV2 public bool Spectator;

        UpdateBattlegroundsOverlay
        */

        private string Author; // TODO (Spectator) is this the spec name when spectating ? If not, update the reset or something
        private Dictionary<int, TurnState> GameHistory;
        private bool WasConceded = false;

        /* Necessary to deal with HDT restarting during a game and sending all turn events at once.
         * Async management in this plugin is quite a mess
        */
        private ConcurrentDictionary<int, bool> IsTurnBeingStored;

        private bool IsSpectator = false;
        private bool IsGameInitialized = false;
        private int OpponentEntityId = 0;

        public GameState()
        {
            GameHistory = new Dictionary<int, TurnState>();
            IsTurnBeingStored = new ConcurrentDictionary<int, bool>();
        }

        internal async void InitializeGame()
        {
            if (Core.Game.CurrentGameMode != GameMode.Battlegrounds || IsGameInitialized)
                return;
            ResetGamestate();

            // Saving current turn now in case it switches faster that the Dictionnary init

            Log.Error("DLC - New game");
            Log.Error($"DLC - turn before Heroes selection: {Core.Game.GetTurnNumber()}");

            await HeroesSelection();
            await EmptyTurnQueue();

            int currentTurn = Core.Game.GetTurnNumber();

            Log.Error($"DLC - turn after Heroes selection: {currentTurn}");

            TurnState TurnState = new TurnState();

            GameHistory.Add(currentTurn, TurnState);

            FetchOpponentEntityId();

            // TODO (Spectator) Update Battletag if reconnecting during a fight turn ?
            IsGameInitialized = true;
            Log.Error($"DLC - Init done for turn {currentTurn}!");
            DumpState("Init", currentTurn);
        }

        internal async void HandleNewTurn(ActivePlayer player)
        {
            if (Core.Game.CurrentGameMode != GameMode.Battlegrounds)
                return;

            int currentTurn = Core.Game.GetTurnNumber();

            Log.Error("DLC - New turn");
            Log.Error($"DLC - player: {player}");
            Log.Error($"DLC - turn: {currentTurn}");

            if (player == ActivePlayer.Player)
            {
                // In case of async new turn event "flood", preventing later turns from being stored too soon
                IsTurnBeingStored[currentTurn] = true;
                Log.Info($"Turn: {currentTurn} added to queue");
            }

            int i = 0;
            while (!IsGameInitialized)
            {
                i++;
                Log.Debug($"Turn {currentTurn} waiting 200ms for init for the {i}th time.");
                await Task.Delay(200);
            }

            i = 0;
            while (!IsTurnStorageAllowed(currentTurn))
            {
                i++;
                Log.Debug($"Turn {currentTurn} not allowed yet. Waiting 200ms for the {i}th time.");
                await Task.Delay(200);
            }
            Log.Info($"Turn {currentTurn} can be handled for phase {player}");

            bool isTurnAdded = false;
            if (player == ActivePlayer.Player)
            {
                isTurnAdded = SaveNewTurnState();
                IsTurnBeingStored[currentTurn] = false;
                Log.Info($"Turn: {currentTurn} removed from queue");
            }
            else if (player == ActivePlayer.Opponent)
            {
                await UpdateBattletag(OpponentEntityId);
            }
            else
            {
                Log.Error("ActivePlayer is None and I don't know why...");
            }

            if (isTurnAdded)
            {
                DumpState("HandleTurn", currentTurn);
            }
        }

        internal void HandleEndGame()
        {
            if (Core.Game.CurrentGameMode != GameMode.Battlegrounds)
                return;
            Log.Error("DLC - End game");
            Log.Error($"DLC - Final turn: {Core.Game.GetTurnNumber()}");

            // TODO (Spectator) Make sure this is called when speccing

            SaveEndGameState();

            DumpState("EndGame", Core.Game.GetTurnNumber());
            DumpHistory();
            ResetGamestate();
        }

        private void ResetGamestate()
        {
            GameHistory.Clear();
            WasConceded = false;
            IsTurnBeingStored.Clear();
            IsGameInitialized = false;
            OpponentEntityId = 0;
        }

        private async Task HeroesSelection()
        {
            // TODO (Custom lobby) There can be less than 8 players in custom lobbies (especially during tourneys) <_<
            const int delayBetweenAttempts = 250;

            int selectedHeroesCount = 0;

            while (selectedHeroesCount != PLAYER_COUNT)
            {
                selectedHeroesCount = Core.Game.Entities.Values.Where(x => x.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE) != 0).Count();

                if (selectedHeroesCount == PLAYER_COUNT)
                {
                    Log.Debug("All heroes have been selected.");
                    break;
                }
                await Task.Delay(delayBetweenAttempts);
            }
        }

        /**
         * Wait until all turns are dequeued before moving on with initialization.
         *
         * Explaination:
         * When HDT is launched during an ongoing game, the log parser reads all the logs since the beginning of the game.
         * This has the effect to queue up every turn into a _turnQueue (cf TurnStart in HDT code)
         * and trigger the OnTurnStart event N times.
         *
         * The thing is, since all the game events are async, the OnGameStart is executed anywhere between those turns dequeue
         * which makes our InitializeGame() method start with an incorrect turn number.
         * Ideally, the _turnQueue.Count value would be passed to the Execute() method so we could use it but it is
         * private and only HDT has this privilege.
         *
         * We mesured on a SSD gaming PC that it took around 170ms/turn to dequeue hence the 250 ms polling delay.
         * This value should be turned into a settings if users need to increase it (slow PC, ...)
         */

        private async Task EmptyTurnQueue()
        {
            const int delayBetweenAttempts = 250;

            int previousTurn = Core.Game.GetTurnNumber();
            int latestDequeuedTurn;

            do
            {
                await Task.Delay(delayBetweenAttempts);
                latestDequeuedTurn = Core.Game.GetTurnNumber();
            } while (latestDequeuedTurn != previousTurn);
        }

        /**
         * This method was initially used to fetch the opponent entity ID via the Bob's hero CardId
         * until we discovered that it was already stored in the GameV2.
         * I'm keeping it in case the method changes again
         * Thanks HDT for such a well documented code that allows for quick plugin develoment without hassle !
         * ...
         */

        private void FetchOpponentEntityId()
        {
            Log.Info("Time to go get Bob !"); // Legacy log that we like :) Do not remove
            Entity OpponentEntity = Core.Game.OpponentEntity;
            Log.Info($"Opponent entity id: {OpponentEntity.Id}");
            Log.Info($"Opponent entity batttletag: {OpponentEntity.Name}");

            OpponentEntityId = OpponentEntity.Id;
        }

        private bool IsTurnStorageAllowed(int currentTurn)
        {
            KeyValuePair<int, bool> defaultTurn = default;

            // If any turn lower than currentTurn is being stored, currentTurn can't be stored
            KeyValuePair<int, bool> lowerTurn = IsTurnBeingStored
                .Where(turn => turn.Key < currentTurn && turn.Value == true)
                .FirstOrDefault();

            return lowerTurn.Equals(defaultTurn);
        }

        /**
         * @return A boolean indicating if a new state was effectively added to the game history
         */

        private bool SaveNewTurnState()
        {
            int turnNumber = Core.Game.GetTurnNumber();

            // Turn might have already been stored by InitializeGame during a midgame plugin start
            if (!GameHistory.ContainsKey(turnNumber))
            {
                if (!GameHistory.ContainsKey(turnNumber - 1))
                {
                    Log.Error($"TurnState for turn {turnNumber - 1} does not exist. Did you start HDT midgame ? You might need to increase the EmptyTurnQueue polling rate :(");
                    return false;
                }

                Log.Error($"Saving TurnState for turn {turnNumber}");
                TurnState newTurn = new TurnState(GameHistory[turnNumber - 1]);

                GameHistory.Add(turnNumber, newTurn);
                return true;
            }
            return false;
        }

        private async Task UpdateBattletag(int opponentEntityId)
        {
            int currentTurn = Core.Game.GetTurnNumber();
            Entity heroEntity = null;
            Entity opponentEntity = null;

            /* HDT decided not to "await" for SetHeroAsync() and there can be cases where
             * opponent is still Bob at the beginning of a fight...
             */
            do
            {
                opponentEntity = Core.Game.Entities.Values.Where(entity => entity.Id == opponentEntityId).FirstOrDefault();
                Log.Debug($"Opponent name is still Bob, waiting 100 more milliseconds...");

                heroEntity = Core.Game.Entities.Values.Where(x => x.Id == opponentEntity.GetTag(HERO_ENTITY)).FirstOrDefault();
                await Task.Delay(100);
            } while (heroEntity.CardId.StartsWith(BOB_CARDID_ROOT));

            int maxTries = 50;
            for (int i = 0; i <= maxTries; i++)
            {
                // This whole wait loop is made to deal with the piled up GameEvent when HDT is started midgame
                if (GameHistory.ContainsKey(currentTurn))
                {
                    // Save Battletag of opponent
                    TurnState currentTurnState = GameHistory[Core.Game.GetTurnNumber()];

                    Hero currentOpponent = currentTurnState.Heroes.Where(hero => hero.CardId == heroEntity.CardId).FirstOrDefault();

                    if (currentOpponent == null)
                    {
                        Log.Error($"Current opponent {opponentEntity.Name} could not be matched to any known ennemy.");
                    }
                    else if (currentOpponent.Battletag.Equals(""))
                    {
                        Log.Error($"Saving Battletag {opponentEntity.Name}.");
                        currentOpponent.Battletag = opponentEntity.Name;
                    }
                    return;
                }

                await Task.Delay(10);
            }

            Log.Error($"Could not find a TurnState for turn {currentTurn}. No Battletag saved");
        }

        private void SaveEndGameState()
        {
            /*
             * If the game was conceded, we already saved a state for turn N with the author's end of fight (alive) position
             * We need to overwrite this state N with the updated positions when the author is dead
             *
             * If the game was not conceded, we never saved any turn N state and current turn is still valued at N-1.
             * Hence why we create a virtual N turn. //TODO check that position values for this virtual turn are corrects
             */
            int turnNumber = Core.Game.GetTurnNumber();

            WasConceded = Core.Game.CurrentGameStats?.WasConceded ?? false;

            if (!WasConceded)
            {
                turnNumber++;
            }

            TurnState newTurn = new TurnState(GameHistory[turnNumber - 1]);

            GameHistory[turnNumber] = newTurn;

            Log.Error($"End game at turn {turnNumber} :\n{newTurn}");
        }

        /**
         * Dump a given turn state in log file
         * Should be replaced by an API call to whatever server should process it
         *
         * "turn" should be a parameter and not retrieved from GameV2 data to avoid dumping the wrong turn
         */

        private void DumpState(string reportedFrom, int turn)
        {
            Log.Error($"{Core.Game.Player.Name} reported this state at turn {turn} from {reportedFrom}");
            foreach (Hero hero in GameHistory[turn].Heroes)
            {
                string deathStatus = hero.IsDead ? "dead" : "alive";
                Log.Info($"Opponent <{hero.Battletag}> playing '{hero.HeroName}' is at position <{hero.Position}> with <{hero.Health}> HP and is {deathStatus}.");
            }
        }

        private void DumpHistory()
        {
            int turn = Core.Game.GetTurnNumber();
            Log.Error($"Game ended. Full history by {Core.Game.Player.Name}:");

            if (WasConceded)
            {
                Log.Error($"{Core.Game.Player.Name} conceded this game !");
            }

            foreach (KeyValuePair<int, TurnState> pair in GameHistory)
            {
                Log.Error($"Turn {pair.Key}:");
                foreach (Hero hero in pair.Value.Heroes)
                {
                    string deathStatus = hero.IsDead ? "dead" : "alive";
                    Log.Info($"\tOpponent <{hero.Battletag}> playing '{hero.HeroName}' was at position <{hero.Position}> with <{hero.Health}> HP and was {deathStatus}.");
                }
            }
        }
    }
}
