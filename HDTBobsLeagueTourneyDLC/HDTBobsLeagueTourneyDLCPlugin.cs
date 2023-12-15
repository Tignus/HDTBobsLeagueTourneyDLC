using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Plugins;
using System;
using System.Windows.Controls;
using static Hearthstone_Deck_Tracker.Windows.MessageDialogs;

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
            /*            GameEvents.OnGameStart.Add(anomalyDisplay.HandleGameStart);
                        GameEvents.OnGameEnd.Add(anomalyDisplay.ClearCard);*/
            GameEvents.OnTurnStart.Add(GameState.SaveNewTurnState);
            GameEvents.OnGameEnd.Add(GameState.SaveEndGameState);

            // Processing GameStart logic in case plugin was loaded/unloaded after starting a game without restarting HDT
            /* anomalyDisplay.HandleGameStart();*/
        }

        public void OnUnload()
        {
        }

        public void OnUpdate()
        {
        }
    }
}
