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
    public static class WRM_Act3
    {
        static public void ModifyRerecruitScene()
        {
            //Get existing blueprints
            var answerlist = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("2479c49fd0cb64b45869b4b91ac3fdff");
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceActive");
            var affection = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_WoljifAffection");

            //Create new dialog.
            var A_AreYouOkay = DialogTools.CreateAnswer("WRM_1_a_AreYouOkay",true);
            var C_ImOkay = DialogTools.CreateCue("WRM_1_cw_ImOkay");
            DialogTools.AnswerAddNextCue(A_AreYouOkay, C_ImOkay);
            DialogTools.CueAddAnswersList(C_ImOkay, answerlist);
            DialogTools.ListAddAnswer(answerlist, A_AreYouOkay, 0);

            //Dialog increments affection and starts romance path.
            DialogTools.AnswerAddOnSelectAction(A_AreYouOkay, ActionTools.StartEtudeAction(romanceactive));
            DialogTools.AnswerAddOnSelectAction(A_AreYouOkay, ActionTools.IncrementFlagAction(affection,1));
        }
    }
}