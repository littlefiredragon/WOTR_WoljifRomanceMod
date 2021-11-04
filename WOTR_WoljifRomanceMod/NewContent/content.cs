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
    class debugmenu
    {
        static void Postfix()
        {
            DialogTools.NewDialogs.LoadDialogIntoGame("enGB");

            createDebugMenu();
            createSimpleConditionalCue();
            createComplexConditionalCue();
            createConditionalAnswers();
            createSkillChecks();
            createActionTest();
            createSimpleCutscene();
            //Complex cutscene handled in areawatcher
        }



        static void createDebugMenu()
        {
            var originalanswers = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("e41585da330233143b34ef64d7d62d69");
            var starttestcue = DialogTools.CreateCue("TEST_cw_starttesting");
            var endtestcue = DialogTools.CreateCue("TEST_cw_donetesting");
            var starttestanswer = DialogTools.CreateAnswer("TEST_a_helpmetest");
            var endtestanswer = DialogTools.CreateAnswer("TEST_a_donetesting");
            var debuganswerlist = DialogTools.CreateAnswersList("TEST_L_debugmenu");
            DialogTools.ListAddAnswer(originalanswers, starttestanswer, 12);
            DialogTools.AnswerAddNextCue(starttestanswer, starttestcue);
            DialogTools.AnswerAddNextCue(endtestanswer, endtestcue);
            DialogTools.ListAddAnswer(debuganswerlist, endtestanswer);
            DialogTools.CueAddAnswersList(starttestcue, debuganswerlist);
            DialogTools.CueAddAnswersList(endtestcue, originalanswers);
        }
        static void createSimpleConditionalCue()
        {
            var debuganswerlist = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("TEST_L_debugmenu");

            var simpleconditionalanswer = DialogTools.CreateAnswer("TEST_a_conditionalcue");
            var simpleconditionalcuetrue = DialogTools.CreateCue("TEST_cw_trueconditionalcue");
            var simpleconditionalcuefalse = DialogTools.CreateCue("TEST_cw_falseconditionalcue");
            var simplecondition = ConditionalTools.CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PcRace>("isplayertiefling", cond =>
            {
                cond.Race = Kingmaker.Blueprints.Race.Tiefling;
            });
            DialogTools.CueAddCondition(simpleconditionalcuetrue, simplecondition);
            DialogTools.AnswerAddNextCue(simpleconditionalanswer, simpleconditionalcuetrue);
            DialogTools.AnswerAddNextCue(simpleconditionalanswer, simpleconditionalcuefalse);
            DialogTools.ListAddAnswer(debuganswerlist, simpleconditionalanswer, 0);
            DialogTools.CueAddAnswersList(simpleconditionalcuetrue, debuganswerlist);
            DialogTools.CueAddAnswersList(simpleconditionalcuefalse, debuganswerlist);
        }
        static void createComplexConditionalCue()
        {
            var debuganswerlist = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("TEST_L_debugmenu");

            var complexconditionalanswer = DialogTools.CreateAnswer("TEST_a_complexconditionalcue");
            var complexconditionalcuetrue = DialogTools.CreateCue("TEST_cw_truecomplexconditionalcue");
            var complexconditionalcuefalse = DialogTools.CreateCue("TEST_cw_falsecomplexconditionalcue");
            // Build logic tree
            var complexlogic = ConditionalTools.CreateChecker();
            ConditionalTools.CheckerAddCondition(complexlogic,
                ConditionalTools.CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PcFemale>("isplayerfemale"));
            ConditionalTools.CheckerAddCondition(complexlogic,
                ConditionalTools.CreateLogicCondition("aasimarortiefling", Kingmaker.ElementsSystem.Operation.Or,
                    ConditionalTools.CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PcRace>("isplayertiefling",
                        bp => { bp.Race = Kingmaker.Blueprints.Race.Tiefling; }),
                    ConditionalTools.CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PcRace>("isplayeraasimar",
                        bp => { bp.Race = Kingmaker.Blueprints.Race.Aasimar; })));
            DialogTools.AnswerAddNextCue(complexconditionalanswer, complexconditionalcuetrue);
            DialogTools.AnswerAddNextCue(complexconditionalanswer, complexconditionalcuefalse);
            DialogTools.CueAddAnswersList(complexconditionalcuefalse, debuganswerlist);
            DialogTools.CueAddAnswersList(complexconditionalcuetrue, debuganswerlist);
            DialogTools.ListAddAnswer(debuganswerlist, complexconditionalanswer, 1);
        }

        static void createConditionalAnswers()
        {
            var debuganswerlist = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("TEST_L_debugmenu");

            var genericcue = DialogTools.CreateCue("TEST_cw_generic");
            DialogTools.CueAddAnswersList(genericcue, debuganswerlist);
            var showcondanswertrue = DialogTools.CreateAnswer("TEST_a_trueconditionalanswer");
            var showcondanswerfalse = DialogTools.CreateAnswer("TEST_a_falseconditionalanswer");
            DialogTools.AnswerAddNextCue(showcondanswertrue, genericcue);
            DialogTools.AnswerAddNextCue(showcondanswerfalse, genericcue);
            DialogTools.AnswerAddShowCondition(showcondanswertrue,
                ConditionalTools.CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PcRace>("isplayertiefling",
                        bp => { bp.Race = Kingmaker.Blueprints.Race.Tiefling; }));
            DialogTools.AnswerAddShowCondition(showcondanswerfalse,
                ConditionalTools.CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PcRace>("isplayeraasimar",
                        bp => { bp.Race = Kingmaker.Blueprints.Race.Aasimar; }));
            DialogTools.ListAddAnswer(debuganswerlist, showcondanswerfalse, 2);
            DialogTools.ListAddAnswer(debuganswerlist, showcondanswertrue, 3);
            var unpickableanswer = DialogTools.CreateAnswer("TEST_a_unchoosableconditionalanswer");
            DialogTools.AnswerAddNextCue(unpickableanswer, genericcue);
            DialogTools.AnswerAddSelectCondition(unpickableanswer,
                ConditionalTools.CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PcRace>("isplayerelf",
                        bp => { bp.Race = Kingmaker.Blueprints.Race.Elf; }));
            DialogTools.ListAddAnswer(debuganswerlist, unpickableanswer, 4);
        }

        static void createSkillChecks()
        {
            var debuganswerlist = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("TEST_L_debugmenu");

            var easycheckanswer = DialogTools.CreateAnswer("TEST_a_skillcheckeasy");
            var hardcheckanswer = DialogTools.CreateAnswer("TEST_a_skillcheckhard");
            var failedcheckcue = DialogTools.CreateCue("TEST_sf_failedskillcheck");
            var passedcheckcue = DialogTools.CreateCue("TEST_sp_passedskillcheck");
            DialogTools.CueAddAnswersList(failedcheckcue, debuganswerlist);
            DialogTools.CueAddAnswersList(passedcheckcue, debuganswerlist);
            var easycheck = DialogTools.CreateCheck("easycheck", Kingmaker.EntitySystem.Stats.StatType.CheckDiplomacy, 3, passedcheckcue, failedcheckcue);
            var hardcheck = DialogTools.CreateCheck("hardcheck", Kingmaker.EntitySystem.Stats.StatType.SkillAthletics, 30, passedcheckcue, failedcheckcue);
            DialogTools.AnswerAddNextCue(easycheckanswer, easycheck);
            DialogTools.AnswerAddNextCue(hardcheckanswer, hardcheck);
            DialogTools.ListAddAnswer(debuganswerlist, easycheckanswer, 5);
            DialogTools.ListAddAnswer(debuganswerlist, hardcheckanswer, 6);
        }

        static void createActionTest()
        {
            var debuganswerlist = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("TEST_L_debugmenu");

            var actionanswer = DialogTools.CreateAnswer("TEST_a_action");
            var actioncue = DialogTools.CreateCue("TEST_cw_action");
            DialogTools.AnswerAddNextCue(actionanswer, actioncue);
            DialogTools.CueAddAnswersList(actioncue, debuganswerlist);
            var testaction = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCustomMusic>("testmusic", bp =>
            {
                bp.MusicEventStart = "MUS_MysteryTheme_Play";
                bp.MusicEventStop = "MUS_MysteryTheme_Stop";
            });
            var stoptestaction = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StopCustomMusic>("testmusicstop");
            DialogTools.CueAddOnShowAction(actioncue, testaction);
            DialogTools.CueAddOnStopAction(actioncue, stoptestaction);
            DialogTools.ListAddAnswer(debuganswerlist, actionanswer, 7);
        }

        static void createSimpleCutscene()
        {
            var debuganswerlist = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("TEST_L_debugmenu");

            var cutsceneanswer = DialogTools.CreateAnswer("TEST_a_cutscene");
            DialogTools.ListAddAnswer(debuganswerlist, cutsceneanswer, 8);
            var cutscenecue = DialogTools.CreateCue("TEST_cw_cutscene");
            DialogTools.AnswerAddNextCue(cutsceneanswer, cutscenecue);

            var newcue = DialogTools.CreateCue("TEST_cw_newdialog");
            var newdialog = DialogTools.CreateDialog("brandnewdialog", newcue);
            var DialogCommand = CommandTools.StartDialogCommand(newdialog, Companions.Woljif);
            //Cutscene
            // Track 1
            // Action: Lock Controls
            // Endgate: empty gate
            // Track 2
            // Action: Delay
            // Endgate: BarkGate
            // BarkGateTrack
            //Action: Bark
            //Endgate: DialogGate
            // DialogGateTrack
            // Action: Dialog start
            // Endgate: empty gate again
            var LockCommand = CommandTools.LockControlCommand();
            var emptyGate = CutsceneTools.CreateGate("emptygate");
            var Track1 = CutsceneTools.CreateTrack(emptyGate, LockCommand);

            var delayCommand = CommandTools.DelayCommand(1.0f);
            var barkcommand = CommandTools.BarkCommand("TEST_bark", Companions.Woljif);
            var dialogGateTrack = CutsceneTools.CreateTrack(emptyGate, DialogCommand);
            var dialoggate = CutsceneTools.CreateGate("dialoggate", dialogGateTrack);
            var BarkGateTrack = CutsceneTools.CreateTrack(dialoggate, barkcommand);
            var BarkGate = CutsceneTools.CreateGate("barkgate", BarkGateTrack);
            var Track2 = CutsceneTools.CreateTrack(BarkGate, delayCommand);

            Kingmaker.AreaLogic.Cutscenes.Track[] trackarray = { Track1, Track2 };
            var customcutscene = CutsceneTools.CreateCutscene("testcustomcutscene", false, trackarray);
            var playcutsceneaction = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene>("playtestcutscene", bp =>
            {
                bp.m_Cutscene = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.CutsceneReference>(customcutscene);
                bp.Owner = cutscenecue;
                bp.Parameters = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter();
            });
            DialogTools.CueAddOnStopAction(cutscenecue, playcutsceneaction);
        }
    }
}