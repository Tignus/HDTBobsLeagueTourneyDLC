using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Windows.Controls;

namespace HDTBobsLeagueTourneyDLC
{
    public class BobsLeagueTourneyDLCPlugin : IPlugin
    {
        public string Name => "BobsLeagueTourneyDLC";

        public string Description => "Provides information about an ongoing Battlegrounds game";

        public string ButtonText => "NO SETTINGS";

        public string Author => "Mouchoir & Tignus";

        public Version Version => new Version(0, 0, 1);

        public MenuItem MenuItem => null;

        private GameState GameState;

        public void OnButtonPress()
        {
        }

        public void OnLoad()
        {
            GameState = new GameState();
            // Initializing in case plugin was loaded/unloaded after starting a game without restarting HDT ?
            // It is tricky to call it here because of how HDT replays every GameEvents if you restart it
            //GameState.InitializeGame();

            GameEvents.OnGameStart.Add(GameState.InitializeGame);
            GameEvents.OnTurnStart.Add(GameState.HandleNewTurn);
            GameEvents.OnGameEnd.Add(GameState.HandleEndGame);
        }

        public void OnUnload()
        {
            GameState = null;
        }

        public void OnUpdate()
        {
        }
    }
}
