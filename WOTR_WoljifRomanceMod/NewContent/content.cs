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


namespace WOTR_WoljifRomanceMod
{
    [HarmonyPatch(typeof(BlueprintsCache), "Init")]
    class helloworld
    {
        static void Postfix()
        {
            var locText = Kingmaker.Localization.LocalizationManager.CurrentPack.Strings;
            locText["answerkey"] = "\"Hey, I figured out how to modify dialogue trees!\"";
            locText["cuekey"] = "\"Good work, Chief!\" {n}Woljif gives you two thumbs up.{/n}";

            var modifiedanswers = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("e41585da330233143b34ef64d7d62d69");

            Kingmaker.DialogSystem.Blueprints.BlueprintCue newcue = Helpers.CreateBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("newtestcue", bp =>
            {
                bp.Text = new Kingmaker.Localization.LocalizedString { m_Key = "cuekey" };
                bp.Continue = new Kingmaker.DialogSystem.CueSelection();
                bp.AlignmentShift = new Kingmaker.UnitLogic.Alignments.AlignmentShift();
                bp.Conditions = new Kingmaker.ElementsSystem.ConditionsChecker();
                bp.Speaker = new Kingmaker.DialogSystem.DialogSpeaker();
                bp.OnShow = new Kingmaker.ElementsSystem.ActionList();
                bp.OnStop = new Kingmaker.ElementsSystem.ActionList();
                bp.Answers.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintAnswerBaseReference>(modifiedanswers));
                //bp.Continue.Cues.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintCueBaseReference>(Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCueBase>("5d6f93a16f7340b4eba002e3af55ef98")));
            });


            Kingmaker.DialogSystem.Blueprints.BlueprintAnswer newanswer = Helpers.CreateBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("newtestanswer", bp =>
            {
                bp.Text = new Kingmaker.Localization.LocalizedString { m_Key = "answerkey" };
                bp.NextCue = new Kingmaker.DialogSystem.CueSelection();
                //bp.NextCue.Cues.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintCueBaseReference>(Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("b4ab99d0d5ba2e643be56bc04d02c7f5")));
                bp.NextCue.Cues.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintCueBaseReference>(newcue));
                bp.ShowCheck = new Kingmaker.DialogSystem.Blueprints.ShowCheck();
                bp.CharacterSelection = new Kingmaker.DialogSystem.CharacterSelection();
                bp.ShowConditions = new Kingmaker.ElementsSystem.ConditionsChecker();
                bp.SelectConditions = new Kingmaker.ElementsSystem.ConditionsChecker();
                bp.OnSelect = new Kingmaker.ElementsSystem.ActionList();
                bp.AlignmentShift = new Kingmaker.UnitLogic.Alignments.AlignmentShift();
            });

            modifiedanswers.Answers.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintAnswerBaseReference>(newanswer));
        }
    }
}