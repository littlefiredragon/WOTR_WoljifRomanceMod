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
using Kingmaker.EntitySystem.Entities;

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
            //var WoljifKickedOut = (Kingmaker.AreaLogic.Etudes.BlueprintEtude)Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(Kingmaker.Blueprints.BlueprintGuid.Parse("2046d985bbc28a04188efa5fb06b6004"));

            // build basic romance etude
            var RomanceEtude = EtudeTools.CreateEtude("WRM_WoljifRomance", CompanionEtude, false, false);
            //EtudeTools.EtudeAddActivationCondition(RomanceEtude, ConditionalTools.CreateEtudeCondition("WRM_WJ_NotKickedOutCondition", WoljifKickedOut, EtudeTools.EtudeStatus.Playing, true));
            
            // build romance active etude
            var RomanceActiveEtude = EtudeTools.CreateEtude("WRM_WoljifRomanceActive", RomanceEtude, true, false);
            //var romancecounter = Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432");
            //EtudeTools.EtudeAddOnPlayTrigger(RomanceActiveEtude, ActionTools.MakeList(ActionTools.IncrementFlagAction(romancecounter, 1)));
            //    Add component: on start, spark
            EtudeTools.EtudeAddOnPlayTrigger(RomanceActiveEtude, ActionTools.MakeList(ActionTools.StartEtudeAction(SparkEtude)));
            //    Add component: on complete, if not playing jealousy, decrement romances counter.
            //var cond = ActionTools.ConditionalAction(ConditionalTools.CreateLogicCondition("WRM_RomanceJealousyCondition", ConditionalTools.CreateEtudeCondition("WRM_JealousyStatus", JealousyEtude, EtudeTools.EtudeStatus.Playing, true)));
            //ActionTools.ConditionalActionOnTrue(cond, ActionTools.MakeList(ActionTools.IncrementFlagAction(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"), -1)));
            //EtudeTools.EtudeAddOnCompleteTrigger(RomanceActiveEtude, ActionTools.MakeList(cond));

            //    Add to RomanceEtude: when complete, autocomplete RomanceActive.
            EtudeTools.EtudeAddOnCompleteTrigger(RomanceEtude, ActionTools.MakeList(ActionTools.CompleteEtudeAction(RomanceActiveEtude)));
            //    And vice versa
            EtudeTools.EtudeAddOnCompleteTrigger(RomanceActiveEtude, ActionTools.MakeList(ActionTools.CompleteEtudeAction(RomanceEtude)));

            // build romance complete etude
            var RomanceCompleteEtude = EtudeTools.CreateEtude("WRM_WoljifRomanceFinished", RomanceActiveEtude, false, false);
            //    Add component: on start, flame
            EtudeTools.EtudeAddOnPlayTrigger(RomanceCompleteEtude, ActionTools.MakeList(ActionTools.StartEtudeAction(FlameEtude)));

            // build affection counter
            var AffectionCounter = EtudeTools.CreateFlag("WRM_WoljifAffection");

            // start romance base with companion etude
            //CompanionEtude.StartsWith.AddItem(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintEtudeReference>(RomanceEtude));
            EtudeTools.EtudeAddStartsWith(CompanionEtude, RomanceEtude);

            // build affecton gate etudes
            var AffectionGateSuccess = EtudeTools.CreateEtude("WRM_WoljifRomanceGatePassed", RomanceActiveEtude, false, false);
            var AffectionGateFail = EtudeTools.CreateEtude("WRM_WoljifRomanceGateFailed", RomanceActiveEtude, false, false);
            EtudeTools.EtudeAddOnCompleteTrigger(AffectionGateFail, ActionTools.MakeList(ActionTools.CompleteEtudeAction(RomanceActiveEtude)));
        }
    }
}