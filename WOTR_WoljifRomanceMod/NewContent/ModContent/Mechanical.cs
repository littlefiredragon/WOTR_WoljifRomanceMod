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

//##########################################################################################################################
// MECHANICAL CONTENT
// Adds mod-specific content that spans the entire story, such as the romance etude.
//##########################################################################################################################

namespace WOTR_WoljifRomanceMod
{
    public static class WRM_Structure
    {
        public static void buildEtudes()
        {
            // Fetch existing etudes
            var CompanionEtude = (Kingmaker.AreaLogic.Etudes.BlueprintEtude)
                                 Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(
                                 Kingmaker.Blueprints.BlueprintGuid.Parse("14a80c048c8ceed4a9c856d85bbf10da"));
            var SparkEtude = (Kingmaker.AreaLogic.Etudes.BlueprintEtude)
                             Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(
                             Kingmaker.Blueprints.BlueprintGuid.Parse("b9c6e3e0d6ac42ef8b9a2bad0c36420d"));
            var FlameEtude = (Kingmaker.AreaLogic.Etudes.BlueprintEtude)
                             Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(
                             Kingmaker.Blueprints.BlueprintGuid.Parse("c95ca64f070f42748f097cc3ff21d505"));
            
            // Build new etudes
            var RomanceEtude = EtudeTools.CreateEtude("WRM_WoljifRomance", CompanionEtude, false, false);
            var RomanceActiveEtude = EtudeTools.CreateEtude("WRM_WoljifRomanceActive", RomanceEtude, true, false);
            var RomanceCompleteEtude = EtudeTools.CreateEtude("WRM_WoljifRomanceFinished", 
                                                              RomanceActiveEtude, false, false);

            EtudeTools.EtudeAddOnPlayTrigger(RomanceActiveEtude, 
                                             ActionTools.MakeList(ActionTools.StartEtudeAction(SparkEtude)));
            EtudeTools.EtudeAddOnCompleteTrigger(RomanceEtude, 
                                                 ActionTools.MakeList(ActionTools.CompleteEtudeAction(RomanceActiveEtude)));
            EtudeTools.EtudeAddOnCompleteTrigger(RomanceActiveEtude, 
                                                 ActionTools.MakeList(ActionTools.CompleteEtudeAction(RomanceEtude)));

            EtudeTools.EtudeAddOnPlayTrigger(RomanceCompleteEtude, 
                                             ActionTools.MakeList(ActionTools.StartEtudeAction(FlameEtude)));
            EtudeTools.EtudeAddStartsWith(CompanionEtude, RomanceEtude);

            // build affection counter
            var AffectionCounter = EtudeTools.CreateFlag("WRM_WoljifAffection");
            var AffectionGateSuccess = EtudeTools.CreateEtude("WRM_WoljifRomanceGatePassed", 
                                                              RomanceActiveEtude, false, false);
            var AffectionGateFail = EtudeTools.CreateEtude("WRM_WoljifRomanceGateFailed", 
                                                           RomanceActiveEtude, false, false);
            EtudeTools.EtudeAddOnCompleteTrigger(AffectionGateFail, 
                                                 ActionTools.MakeList(ActionTools.CompleteEtudeAction(RomanceActiveEtude)));
        }
    }
}