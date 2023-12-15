using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;


namespace HDTBobsLeagueTourneyDLC
{
    internal class GameState
    {
        private string Author;

        private Dictionary<int, TurnState> GameHistory;

        public GameState() { }

        internal void SaveNewTurnState(ActivePlayer player)
        {
            Log.Error("DLC - New Turn");
            Log.Error("DLC - player: " + player);

            for (int i = 0; i < 7; i++)
            {
                Entity entity = Core.Game.Entities.Values.Where(x => x.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE) == i + 1).FirstOrDefault();
                Log.Error(entity.ToString());
            }



            Log.Error("===================================================================");

            for (int i = 0; i < 7; i++)
            {
                Entity entity2 = Core.Game.Entities.Values.Where(x => x.GetTag(GameTag.NUM_TURNS_IN_PLAY) != 0).FirstOrDefault();
                Log.Error(entity2.ToString());
            }
            Log.Error("xjmire;guqpeori;gdql!er;swv*%PERSKG.FVPOITJGVBXWIOZTEJOVBJOKL%?QAEZFBNVOLKp<qafznrhjtbioùaterjhngbiko'ethnbg");

            Log.Error(Core.Game.Player.Id.ToString());
            Log.Error(Core.Game.Player.Name);
            Log.Error(Core.Game.Opponent.Id.ToString());
            Log.Error(Core.Game.Opponent.Name);
            Log.Error("xjmire;guqpeori;gdql!er;swv*%PERSKG.FVPOITJGVBXfWIOZTEJOVBJOKL%?QAEZFBNVOLKp<qafznrhjtbioùaterjhngbiko'ethnbg");
            Log.Error("xjmire;guqpeori;gdql!er;swv*%PERSKG.FVPOITJGVBXWIOfZTEJOVBJOKL%?QAEZFBNVOLKp<qafznrhjtbioùaterjhngbiko'ethnbg");
            Log.Error("xjmire;guqpeori;gdql!er;swv*%PERSKG.FVPOITJGVBXWfIOfZTEJOVBJOKL%?QAEZFBNVOLKp<qafznrhjtbioùaterjhngbiko'ethnbg");
            Log.Error("xjmire;guqpeori;gdql!er;swv*%PERSKG.FVPOITJGVBXWIOZTfEJOVBJOKL%?QAEZFBNVOLKp<qafznrhjtbioùaterjhngbiko'ethnbg");
            Log.Error("xjmire;guqpeori;gdql!er;swv*%PERSKG.FVPOITJGVBXWIOZTEfJOVBJOKL%?QAEZFBNVOLKp<qafznrhjtbioùaterjhngbiko'ethnbg");
            Log.Error("xjmire;guqpeori;gdql!er;swv*%PERSKG.FVPOITJGVBXWIOZTEJfOVBJOKL%?QAEZFBNVOLKp<qafznrhjtbioùaterjhngbiko'ethnbg");
            Log.Error("xjmire;guqpeori;gdql!er;swv*%PERSKG.FVPOITJGVBXWIOZTEJOfVBJOKL%?QAEZFBNVOLKp<qafznrhjtbioùaterjhngbiko'ethnbg");


            Log.Error("Ca va être massif");

            foreach (Entity e in Core.Game.Entities.Values.Where(x => { return true; }))
            {
                if (e != null && e.Name != "")
                {
                    Log.Error(e.Name);
                    Log.Error(e.ToString());

                    Log.Error("Hero titty: " + e.GetTag(GameTag.HERO_ENTITY).ToString());

                    if (e.GetTag(GameTag.HERO_ENTITY) != 0)
                    {
                        Entity entitties = Core.Game.Entities.Values.Where(x => x.Id == e.GetTag(GameTag.HERO_ENTITY)).FirstOrDefault();
                        Log.Error(entitties.ToString());
                    }

                }

            }

            Log.Error("Alors ?");

            //resources
            BattlegroundsUtils.GetOriginalHeroId("abc");
            Entity gameEntity = Core.Game.GameEntity;
            GameMode gameMode = Core.Game.CurrentGameMode;
            /*            Core.Game.Entities*/
            //GameStats
            //UpdateBattlegroundsOverlay
            //GameV2 private bool? _spectator;


            if (IsFirstTurn()) { }

            DumpState();
        }

        internal void SaveEndGameState()
        {
            Log.Error("DLC - End game");


            DumpState();
        }

        private void DumpState()
        {

        }

        private bool IsFirstTurn()
        {
            return false;
        }


    }
}