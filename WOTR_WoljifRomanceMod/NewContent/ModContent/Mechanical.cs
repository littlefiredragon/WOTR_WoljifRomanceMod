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

/*
 * This content is the structural basis of the mod - not individual scenes, but rather the over-arching etudes and flags and whatnot,
 * which power the romance overall.
 */

namespace WOTR_WoljifRomanceMod
{
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
            //    Add component: on complete, complete RomanceActive.
            EtudeTools.EtudeAddActivationCondition(RomanceEtude, ConditionalTools.CreateEtudeCondition("WRM_WJ_NotKickedOutCondition", WoljifKickedOut, EtudeTools.EtudeStatus.Playing, true));
            
            // build romance active etude
            var RomanceActiveEtude = EtudeTools.CreateEtude("WRM_WoljifRomanceActive", RomanceEtude, true, false);
            //    Add component: on start, increment romances counter.
            var incFlag = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.IncrementFlagValue>();
            incFlag.m_Flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"));
            EtudeTools.EtudeAddOnPlayTrigger(RomanceActiveEtude, ActionTools.MakeList(incFlag));
            //    Add component: on start, spark
            EtudeTools.EtudeAddOnPlayTrigger(RomanceActiveEtude, ActionTools.MakeList(ActionTools.StartEtudeAction(SparkEtude)));
            //    Add component: on complete, if not playing jealousy, decrement romances counter.
            var decFlag = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.IncrementFlagValue>();
            decFlag.m_Flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"));
            decFlag.Value = new Kingmaker.Designers.EventConditionActionSystem.Evaluators.IntConstant { Value = -1 };
            var cond = ActionTools.ConditionalAction(ConditionalTools.CreateLogicCondition("WRM_RomanceJealousyCondition", ConditionalTools.CreateEtudeCondition("WRM_JealousyStatus", JealousyEtude, EtudeTools.EtudeStatus.Playing, true)));
            ActionTools.ConditionalActionOnTrue(cond, ActionTools.MakeList(decFlag));
            //    Add to RomanceEtude: when complete, autocomplete RomanceActive.
            EtudeTools.EtudeAddCompleteTrigger(RomanceEtude, ActionTools.MakeList(ActionTools.CompleteEtudeAction(RomanceActiveEtude)));
            
            // build romance complete etude
            var RomanceCompleteEtude = EtudeTools.CreateEtude("WRM_WoljifRomanceFinished", RomanceEtude, false, false);
            //    Add component: on start, flame
            EtudeTools.EtudeAddOnPlayTrigger(RomanceActiveEtude, ActionTools.MakeList(ActionTools.StartEtudeAction(FlameEtude)));

            // build affection counter
            var AffectionCounter = EtudeTools.CreateFlag("WRM_WoljifAffection");

            // start romance base with companion etude
            CompanionEtude.StartsWith.AddItem(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintEtudeReference>(RomanceEtude));
        }
    }
}