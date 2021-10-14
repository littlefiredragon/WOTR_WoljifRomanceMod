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
            DialogTools.NewDialogs.LoadDialogIntoGame("enGB");

            var modifiedanswers = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("e41585da330233143b34ef64d7d62d69");
            var newcue = DialogTools.CreateCue("testcue");
            var newanswer = DialogTools.CreateAnswer("testanswer");
            DialogTools.AnswerAddNextCue(newanswer, newcue);
            DialogTools.ListAddAnswer(modifiedanswers, newanswer, 12);
            DialogTools.CueAddAnswersList(newcue, modifiedanswers);
        }
    }
}