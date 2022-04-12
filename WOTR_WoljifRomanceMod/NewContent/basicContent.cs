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
using System.Linq;

namespace WOTR_WoljifRomanceMod
{
    [HarmonyPatch(typeof(BlueprintsCache), "Init")]
    class WoljifRomanceMod
    {
        static public TimeKeeper Clock;
        static public AreaPartWatcher PartLoadWatcher;
        static public AreaWatcher LoadWatcher;
        static void Postfix()
        {
            var Locale = Kingmaker.Localization.LocalizationManager.CurrentLocale;
            switch (Locale)
            {
                case Kingmaker.Localization.Shared.Locale.ruRU:
                    DialogTools.NewDialogs_ruRU.LoadDialogIntoGame();
                    break;
                case Kingmaker.Localization.Shared.Locale.zhCN:
                    DialogTools.NewDialogs_zhCN.LoadDialogIntoGame();
                    break;
                default:
                    DialogTools.NewDialogs_enGB.LoadDialogIntoGame();
                    break;
            }

            Clock = new TimeKeeper();
            Kingmaker.PubSubSystem.EventBus.Subscribe(Clock);

            LoadWatcher = new AreaWatcher();
            PartLoadWatcher = new AreaPartWatcher();
            Kingmaker.PubSubSystem.EventBus.Subscribe(LoadWatcher);
            Kingmaker.PubSubSystem.EventBus.Subscribe(PartLoadWatcher);

            createDebugMenu();
            WRM_Structure.buildEtudes();
            WRM_Act3.ModifyRerecruitScene();
            WRM_Act3.CreateTavernCommandRoomEvent();
            WRM_Act3.CreateTavernCutscene();
            WRM_Act3.CreateArgumentScene();
            WRM_Act3.AlterPostQuestDialog();
            WRM_Act3.CreateReconciliation();
            WRM_Act3.MiscChanges();
            WRM_Act4.ModifyQuestTrigger();
            WRM_Act4.ModifyQuestDialog();
            WRM_Act4.CreateNightmareScene();
            WRM_Act4.MiscChanges();
            WRM_Act5.AlterJealousyScene();
            WRM_Act5.AddSnowSceneInvite();
            WRM_Act5.AddSnowCutscene();
            WRM_Act5.AddConfessionInvite();
            WRM_Act5.AddConfessionScene();
            WRM_Act5.AddBedroomScene();
            WRM_Act5.AddBedroomBarks();
            WRM_Act5.ChangeDialogWhenRomanced();
            WRM_Act5.AlterLichScene();
            WRM_Act5.AddIrabethConversation();
            WRM_Act5.AlterThresholdScene();
            WRM_Act5.AlterEpilogue();
            WRM_Act5.MiscChanges();
        }

        // Deprecated. Kept for now to avoid breaking saves during beta testing.
        static public void createDebugMenu()
        {
            var originalanswers = Resources.GetBlueprint
                                  <Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>
                                  ("e41585da330233143b34ef64d7d62d69");
            var starttestcue = DialogTools.CreateCue("TEST_cw_starttesting");
            var endtestcue = DialogTools.CreateCue("TEST_cw_donetesting");
            var starttestanswer = DialogTools.CreateAnswer("TEST_a_helpmetest");
            var endtestanswer = DialogTools.CreateAnswer("TEST_a_donetesting");
            var debuganswerlist = DialogTools.CreateAnswersList("TEST_L_debugmenu");
            //DialogTools.ListAddAnswer(originalanswers, starttestanswer, 12);
            DialogTools.AnswerAddNextCue(starttestanswer, starttestcue);
            DialogTools.AnswerAddNextCue(endtestanswer, endtestcue);
            DialogTools.ListAddAnswer(debuganswerlist, endtestanswer);
            DialogTools.CueAddAnswersList(starttestcue, debuganswerlist);
            DialogTools.CueAddAnswersList(endtestcue, originalanswers);
        }
    }
}