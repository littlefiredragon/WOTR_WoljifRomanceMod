using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem;
using System;
using TabletopTweaks;
using TabletopTweaks.Config;
using TabletopTweaks.Utilities;
using UnityModManagerNet;
using TabletopTweaks.Extensions;
using System.Collections.Generic;

/*
 * This content is the structural basis of the mod - not individual scenes, but rather the over-arching etudes and flags and whatnot,
 * which power the romance overall.
 */

namespace WOTR_WoljifRomanceMod
{
    /*public class TimeKeeper : Kingmaker.Kingdom.IKingdomDayHandler
    {
        //public SortedDictionary<string, Kingmaker.Blueprints.BlueprintUnlockableFlag> SavedDates = new SortedDictionary<string, Kingmaker.Blueprints.BlueprintUnlockableFlag>();
        //public Kingmaker.Blueprints.BlueprintUnlockableFlag saveddate;
        public Kingmaker.Blueprints.BlueprintUnlockableFlag currentdate;

        public Kingmaker.Blueprints.BlueprintUnlockableFlag saved_1;
        public Kingmaker.Blueprints.BlueprintUnlockableFlag saved_2;
        public Kingmaker.Blueprints.BlueprintUnlockableFlag saved_3;

        //public Kingmaker.Blueprints.BlueprintUnlockableFlag TavernTimerFlag;
        //public Kingmaker.Blueprints.BlueprintUnlockableFlag ReconciliationTimerFlag;
        //public Kingmaker.Blueprints.BlueprintUnlockableFlag ArgumentTimerFlag;
        //public Kingmaker.Blueprints.BlueprintUnlockableFlag NightmareTimerFlag;
        //public Kingmaker.Blueprints.BlueprintUnlockableFlag SnowTimerFlag;


        public TimeKeeper()
        {
            saved_1 = EtudeTools.CreateFlag("WRM_TEST_SavedDate1");
            saved_2 = EtudeTools.CreateFlag("WRM_TEST_SavedDate2");
            saved_3 = EtudeTools.CreateFlag("WRM_TEST_SavedDate3");
            currentdate = EtudeTools.CreateFlag("WRM_TEST_CurrentDate");
            //TavernTimerFlag = EtudeTools.CreateFlag("WRM_TavernTimerFlag");
            //ReconciliationTimerFlag = EtudeTools.CreateFlag("WRM_ReconciliationTimerFlag");
            //ArgumentTimerFlag = EtudeTools.CreateFlag("WRM_ArgumentTimerFlag");
            //NightmareTimerFlag = EtudeTools.CreateFlag("WRM_NightmareTimerFlag");
            //SnowTimerFlag = EtudeTools.CreateFlag("WRM_SnowTimerFlag");
        }
        public int GetSavedDate(string saveddatename)
        {
            //return saveddate.Value;
            //return SavedDates[saveddatename].Value;
            var val = 10000;
            switch (saveddatename)
            {
                case "WRM_TEST_SavedDate1":
                    val = saved_1.Value;
                    break;
                case "WRM_TEST_SavedDate2":
                    val = saved_2.Value;
                    break;
                case "WRM_TEST_SavedDate3":
                    val = saved_3.Value;
                    break;
            }*/
                    /*switch (saveddatename)
                    {
                        case "WRM_TavernTimerFlag":
                            val = TavernTimerFlag.Value;
                            break;
                        case "WRM_ReconciliationTimerFlag":
                            val = ReconciliationTimerFlag.Value;
                            break;
                        case "WRM_ArgumentTimerFlag":
                            val = ArgumentTimerFlag.Value;
                            break;
                        case "WRM_NightmareTimerFlag":
                            val = NightmareTimerFlag.Value;
                            break;
                        case "WRM_SnowTimerFlag":
                            val = SnowTimerFlag.Value;
                            break;
                    }*/
            /*return val;
        }
        public int GetCurrentDate()
        {
            currentdate.Value = Kingmaker.Kingdom.KingdomState.Instance.CurrentDay;
            return currentdate.Value;
        }
        public void SaveDate(string saveddatename)
        {
            currentdate.Value = Kingmaker.Kingdom.KingdomState.Instance.CurrentDay;
            //saveddate.Value = Kingmaker.Kingdom.KingdomState.Instance.CurrentDay;
            //SavedDates[saveddatename].Value = Kingmaker.Kingdom.KingdomState.Instance.CurrentDay;

            switch (saveddatename)
            {
                case "WRM_TEST_SavedDate1":
                    saved_1.Value = Kingmaker.Kingdom.KingdomState.Instance.CurrentDay;
                    break;
                case "WRM_TEST_SavedDate2":
                    saved_2.Value = Kingmaker.Kingdom.KingdomState.Instance.CurrentDay;
                    break;
                case "WRM_TEST_SavedDate3":
                    saved_3.Value = Kingmaker.Kingdom.KingdomState.Instance.CurrentDay;
                    break;
            }
            */
            /*switch (saveddatename)
            {
                case "WRM_TavernTimerFlag":
                    TavernTimerFlag.Value = Kingmaker.Kingdom.KingdomState.Instance.CurrentDay;
                    break;
                case "WRM_ReconciliationTimerFlag":
                    ReconciliationTimerFlag.Value = Kingmaker.Kingdom.KingdomState.Instance.CurrentDay;
                    break;
                case "WRM_ArgumentTimerFlag":
                    ArgumentTimerFlag.Value = Kingmaker.Kingdom.KingdomState.Instance.CurrentDay;
                    break;
                case "WRM_NightmareTimerFlag":
                    NightmareTimerFlag.Value = Kingmaker.Kingdom.KingdomState.Instance.CurrentDay;
                    break;
                case "WRM_SnowTimerFlag":
                    SnowTimerFlag.Value = Kingmaker.Kingdom.KingdomState.Instance.CurrentDay;
                    break;
            }*/
        /*}
        public void OnNewDay()
        {
            currentdate.Value = Kingmaker.Kingdom.KingdomState.Instance.CurrentDay;
        }
    }

    public class TimerConditional : Kingmaker.ElementsSystem.Condition
    {
        public int dayspassed;
        public string timername;
        public override bool CheckCondition()
        {
            var saved = WoljifRomanceMod.Timer.GetSavedDate(timername);
            var current = WoljifRomanceMod.Timer.GetCurrentDate();

            return current >= (saved + dayspassed);
        }

        public override string GetConditionCaption()
        {
            return "Checks status of a timer variable.";
        }
    }
    public class SetTimer : Kingmaker.ElementsSystem.GameAction
    {
        public string timername;
        public override string GetCaption()
        {
            return "Sets the date on a timer variable.";
        }

        public override void RunAction()
        {
            WoljifRomanceMod.Timer.SaveDate(timername);
        }
    }*/

    public static class WRM_Structure
    {
        public static void buildEtudes()
        {
            // get existing etudes
            var CompanionEtude = (Kingmaker.AreaLogic.Etudes.BlueprintEtude)Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(Kingmaker.Blueprints.BlueprintGuid.Parse("14a80c048c8ceed4a9c856d85bbf10da"));
            var SparkEtude = (Kingmaker.AreaLogic.Etudes.BlueprintEtude)Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(Kingmaker.Blueprints.BlueprintGuid.Parse("b9c6e3e0d6ac42ef8b9a2bad0c36420d"));
            var FlameEtude = (Kingmaker.AreaLogic.Etudes.BlueprintEtude)Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(Kingmaker.Blueprints.BlueprintGuid.Parse("c95ca64f070f42748f097cc3ff21d505"));
            var JealousyEtude = (Kingmaker.AreaLogic.Etudes.BlueprintEtude)Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(Kingmaker.Blueprints.BlueprintGuid.Parse("4799a25da39295b43a6eefcd2cb2b4a7"));
            var WoljifKickedOut = (Kingmaker.AreaLogic.Etudes.BlueprintEtude)Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(Kingmaker.Blueprints.BlueprintGuid.Parse("2046d985bbc28a04188efa5fb06b6004"));

            // build basic romance etude
            var RomanceEtude = EtudeTools.CreateEtude("WRM_WoljifRomance", CompanionEtude, false, false);
            EtudeTools.EtudeAddActivationCondition(RomanceEtude, ConditionalTools.CreateEtudeCondition("WRM_WJ_NotKickedOutCondition", WoljifKickedOut, EtudeTools.EtudeStatus.Playing, true));
            
            // build romance active etude
            var RomanceActiveEtude = EtudeTools.CreateEtude("WRM_WoljifRomanceActive", RomanceEtude, true, false);
            //var romancecounter = Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432");
            //EtudeTools.EtudeAddOnPlayTrigger(RomanceActiveEtude, ActionTools.MakeList(ActionTools.IncrementFlagAction(romancecounter, 1)));
            //    Add component: on start, spark
            EtudeTools.EtudeAddOnPlayTrigger(RomanceActiveEtude, ActionTools.MakeList(ActionTools.StartEtudeAction(SparkEtude)));
            //    Add component: on complete, if not playing jealousy, decrement romances counter.
            //var cond = ActionTools.ConditionalAction(ConditionalTools.CreateLogicCondition("WRM_RomanceJealousyCondition", ConditionalTools.CreateEtudeCondition("WRM_JealousyStatus", JealousyEtude, EtudeTools.EtudeStatus.Playing, true)));
            //ActionTools.ConditionalActionOnTrue(cond, ActionTools.MakeList(ActionTools.IncrementFlagAction(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"), -1)));
            //EtudeTools.EtudeAddCompleteTrigger(RomanceActiveEtude, ActionTools.MakeList(cond));

            //    Add to RomanceEtude: when complete, autocomplete RomanceActive.
            EtudeTools.EtudeAddCompleteTrigger(RomanceEtude, ActionTools.MakeList(ActionTools.CompleteEtudeAction(RomanceActiveEtude)));
            //    And vice versa
            EtudeTools.EtudeAddCompleteTrigger(RomanceActiveEtude, ActionTools.MakeList(ActionTools.CompleteEtudeAction(RomanceEtude)));

            // build romance complete etude
            var RomanceCompleteEtude = EtudeTools.CreateEtude("WRM_WoljifRomanceFinished", RomanceEtude, false, false);
            //    Add component: on start, flame
            EtudeTools.EtudeAddOnPlayTrigger(RomanceCompleteEtude, ActionTools.MakeList(ActionTools.StartEtudeAction(FlameEtude)));

            // build affection counter
            var AffectionCounter = EtudeTools.CreateFlag("WRM_WoljifAffection");

            // start romance base with companion etude
            CompanionEtude.StartsWith.AddItem(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintEtudeReference>(RomanceEtude));

            // build affecton gate etudes
            var AffectionGateSuccess = EtudeTools.CreateEtude("WRM_WoljifRomanceGatePassed", RomanceActiveEtude, false, false);
            var AffectionGateFail = EtudeTools.CreateEtude("WRM_WoljifRomanceGateFailed", RomanceActiveEtude, false, false);
            EtudeTools.EtudeAddCompleteTrigger(AffectionGateFail, ActionTools.MakeList(ActionTools.CompleteEtudeAction(RomanceActiveEtude)));
        }
    }
}