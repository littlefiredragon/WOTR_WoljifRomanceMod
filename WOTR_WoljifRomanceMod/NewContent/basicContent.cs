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
        static void Postfix()
        {
            DialogTools.NewDialogs.LoadDialogIntoGame("enGB");
            createDebugMenu();
            WRM_Structure.buildEtudes();
            WRM_Act3.ModifyRerecruitScene();
            WRM_Act3.CreateTavernCommandRoomEvent();
        }

        static public void createDebugMenu()
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
        //Archived Test Functions, kept around for structural reference for now.
        /*static public void createSimpleConditionalCue()
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
        static public void createComplexConditionalCue()
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

        static public void createConditionalAnswers()
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

        static public void createSkillChecks()
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

        static public void createActionTest()
        {
            var debuganswerlist = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("TEST_L_debugmenu");

            var actionanswer = DialogTools.CreateAnswer("TEST_a_action");
            var actioncue = DialogTools.CreateCue("TEST_cw_action");
            DialogTools.AnswerAddNextCue(actionanswer, actioncue);
            DialogTools.CueAddAnswersList(actioncue, debuganswerlist);
            var testaction = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCustomMusic>(bp =>
            {
                bp.MusicEventStart = "MUS_MysteryTheme_Play";
                bp.MusicEventStop = "MUS_MysteryTheme_Stop";
            });
            var stoptestaction = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StopCustomMusic>();
            DialogTools.CueAddOnShowAction(actioncue, testaction);
            DialogTools.CueAddOnStopAction(actioncue, stoptestaction);
            DialogTools.ListAddAnswer(debuganswerlist, actionanswer, 7);
        }

        static public void createSimpleCutscene()
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
            var playcutsceneaction = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene>(bp =>
            {
                bp.m_Cutscene = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.CutsceneReference>(customcutscene);
                bp.Owner = cutscenecue;
                bp.Parameters = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter();
            });
            DialogTools.CueAddOnStopAction(cutscenecue, playcutsceneaction);
        }

        static public void createAlternateCutscene(Kingmaker.Blueprints.EntityReference locator1, Kingmaker.Blueprints.EntityReference locator2, Kingmaker.Blueprints.EntityReference locator3)
        {
            // Dialog Bits
            var debuganswerlist = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("TEST_L_debugmenu");
            var newdialog = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintDialog>("brandnewdialog");
            var cutsceneanswer2 = DialogTools.CreateAnswer("TEST_a_cutscene2");
            DialogTools.ListAddAnswer(debuganswerlist, cutsceneanswer2, 9);
            var cutscenecue2 = DialogTools.CreateCue("TEST_cw_cutscene2");
            DialogTools.AnswerAddNextCue(cutsceneanswer2, cutscenecue2);

            // STRUCTURE
            // Cutscene
            //   Track 0A
            //     Commands: [LockControl]
            //     EndGate: Gate 3
            //   Track 0B
            //     Commands: [Fadeout]
            //     Endgate: Gate 1
            //   Track 0C
            //     Commands: [Delay, Action(Transport Player, Transport Woljif), Delay]
            //     Endgate: Gate 1
            //   Track 0D
            //     Commands: [Camerafollowplayer]
            //     Endgate: Gate 1
            //     
            //   Gate 1
            //     Track 1A
            //       Commands: [Move Woljif, delay]
            //       Endgate: Gate 2
            //     Track 1B
            //       Commands: [Camera Follow]
            //       Endgate: Gate 3
            //   
            //   Gate 2
            //     Track 2A
            //       Commands: [Turn Woljif to face player, Bark]
            //       Endgate: Gate 3
            //   
            //   Gate 3
            //     Track 3A
            //       Commands: [Start Dialog]
            //       Endgate: Null
            //

            // Build 3rd section
            var startdialog_3A = CommandTools.StartDialogCommand(newdialog, Companions.Woljif);
            var Track_3A = CutsceneTools.CreateTrack(null, startdialog_3A);
            var Gate_3 = CutsceneTools.CreateGate("CutTestGate3", Track_3A);

            // Build 2nd Section
            var bark_2A = CommandTools.BarkCommand("CutTestBark2A", "TEST_bark2", Companions.Woljif);
            var turn_2A = CommandTools.LookAtCommand("CutTestTurn2A", Companions.Woljif, Companions.Player);
            Kingmaker.AreaLogic.Cutscenes.CommandBase[] commands_2A = { turn_2A, bark_2A };
            var Track_2A = CutsceneTools.CreateTrack(Gate_3, commands_2A);
            var Gate_2 = CutsceneTools.CreateGate("CutTestGate2", Track_2A);

            // Build 1st section
            var move_1A = CommandTools.WalkCommand("CutTestWalk1A", Companions.Woljif, locator3);
            var delay_1A = CommandTools.DelayCommand(0.5f);
            Kingmaker.AreaLogic.Cutscenes.CommandBase[] commands_1A = { move_1A, delay_1A };
            var track_1A = CutsceneTools.CreateTrack(Gate_2, commands_1A);
            var camera_1B = CommandTools.CamFollowCommand(Companions.Woljif);
            var track_1B = CutsceneTools.CreateTrack(Gate_3, camera_1B);
            Kingmaker.AreaLogic.Cutscenes.Track[] tracks_1 = { track_1A, track_1B };
            var Gate_1 = CutsceneTools.CreateGate("CutTestGate1", tracks_1);

            // Build 0th section
            var lock_0A = CommandTools.LockControlCommand();
            var track_0A = CutsceneTools.CreateTrack(Gate_3, lock_0A);
            var fade_0B = CommandTools.FadeoutCommand();
            var track_0B = CutsceneTools.CreateTrack(Gate_1, fade_0B);
            var delay1_0C = CommandTools.DelayCommand(0.5f);
            var movepc_0c = ActionTools.TranslocateAction(Companions.Player, locator1);
            var movewj_0c = ActionTools.TranslocateAction(Companions.Woljif, locator2);
            Kingmaker.ElementsSystem.GameAction[] actions_0c = { movepc_0c, movewj_0c };
            var move_0c = CommandTools.ActionCommand("CutTestTeleport", actions_0c);
            var delay2_0C = CommandTools.DelayCommand();
            Kingmaker.AreaLogic.Cutscenes.CommandBase[] commands_0c = { delay1_0C, move_0c, delay2_0C };
            var track_0c = CutsceneTools.CreateTrack(Gate_1, commands_0c);
            var camera_0d = CommandTools.CamFollowCommand(Companions.Player);
            var track_0d = CutsceneTools.CreateTrack(Gate_1, camera_0d);

            // Build cutscene
            Kingmaker.AreaLogic.Cutscenes.Track[] tracks0 = { track_0A, track_0B, track_0c, track_0d };
            var complexcutscene = CutsceneTools.CreateCutscene("testcomplexcutscene", false, tracks0);

            // Attach to dialog
            var playcutsceneaction = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene>(bp =>
            {
                bp.m_Cutscene = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.CutsceneReference>(complexcutscene);
                bp.Owner = cutscenecue2;
                bp.Parameters = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter();
            });
            DialogTools.CueAddOnStopAction(cutscenecue2, playcutsceneaction);
        }

        static public void createEtudeTest()
        {
            //Create a single entry to a debug menu for etudes
            var starttestcue = Resources.GetModBlueprint <Kingmaker.DialogSystem.Blueprints.BlueprintCue>("TEST_cw_starttesting");
            var debuganswerlist = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("TEST_L_debugmenu");
            var etudedebugmenuplz = DialogTools.CreateAnswer("TEST_a_etudemenu");
            var etudedebugcue = DialogTools.CreateCue("TEST_cw_etudemenu");
            DialogTools.AnswerAddNextCue(etudedebugmenuplz, etudedebugcue);
            DialogTools.ListAddAnswer(debuganswerlist, etudedebugmenuplz, 8);

            //Make the etude
            var woljifcompanionetude = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("14a80c048c8ceed4a9c856d85bbf10da");
            var testetude = EtudeTools.CreateEtude("TEST_Etude", woljifcompanionetude, false, false);
            var testflag = EtudeTools.CreateFlag("TEST_Flag");
            var actcondition = ConditionalTools.CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.FlagInRange>("TEST_flagcondition", bp =>
            {
                bp.m_Flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(testflag);
                bp.MinValue = 1;
                bp.MaxValue = 2;
            });
            EtudeTools.EtudeAddActivationCondition(testetude, actcondition);
            actcondition.Owner = testetude;
            

            //Create the etude debug menu
            var etudedebuglist = DialogTools.CreateAnswersList("EtudeDebugMenu");
            DialogTools.CueAddAnswersList(etudedebugcue, etudedebuglist);

            // What's the status?
            var statusrequest = DialogTools.CreateAnswer("TEST_a_etudestatus");
            DialogTools.ListAddAnswer(etudedebuglist, statusrequest);
            // Start the Etude.
            var startrequest = DialogTools.CreateAnswer("TEST_a_startetude");
            var startcue = DialogTools.CreateCue("TEST_cw_startetude");
            DialogTools.CueAddOnShowAction(startcue, ActionTools.StartEtudeAction(testetude));
            DialogTools.AnswerAddNextCue(startrequest, startcue);
            DialogTools.CueAddAnswersList(startcue, etudedebuglist);
            DialogTools.ListAddAnswer(etudedebuglist, startrequest);
            // Complete the Etude.
            var endrequest = DialogTools.CreateAnswer("TEST_a_completeetude");
            var endcue = DialogTools.CreateCue("TEST_cw_completeetude");
            DialogTools.CueAddOnShowAction(endcue, ActionTools.CompleteEtudeAction(testetude));
            DialogTools.AnswerAddNextCue(endrequest, endcue);
            DialogTools.CueAddAnswersList(endcue, etudedebuglist);
            DialogTools.ListAddAnswer(etudedebuglist, endrequest);


            //Etude status cues
            var statusNotStarted = DialogTools.CreateCue("TEST_cw_etudeisnotstarted");
            DialogTools.CueAddCondition(statusNotStarted, ConditionalTools.CreateEtudeCondition("etudenotstarted", testetude, EtudeTools.EtudeStatus.NotStarted));
            DialogTools.AnswerAddNextCue(statusrequest, statusNotStarted);
            DialogTools.CueAddAnswersList(statusNotStarted, etudedebuglist);
            var statusStarted = DialogTools.CreateCue("TEST_cw_etudeisstarted");
            DialogTools.CueAddCondition(statusStarted, ConditionalTools.CreateEtudeCondition("etudestarted", testetude, EtudeTools.EtudeStatus.Started));
            DialogTools.AnswerAddNextCue(statusrequest, statusStarted);
            DialogTools.CueAddAnswersList(statusStarted, etudedebuglist);
            var statusPlaying = DialogTools.CreateCue("TEST_cw_etudeisplaying");
            DialogTools.CueAddCondition(statusPlaying, ConditionalTools.CreateEtudeCondition("etudeplaying", testetude, EtudeTools.EtudeStatus.Playing));
            DialogTools.AnswerAddNextCue(statusrequest, statusPlaying);
            DialogTools.CueAddAnswersList(statusPlaying, etudedebuglist);
            var statusCompleting = DialogTools.CreateCue("TEST_cw_etudecompleting");
            DialogTools.CueAddCondition(statusCompleting, ConditionalTools.CreateEtudeCondition("etudecompleting", testetude, EtudeTools.EtudeStatus.CompletionInProgress));
            DialogTools.AnswerAddNextCue(statusrequest, statusCompleting);
            DialogTools.CueAddAnswersList(statusCompleting, etudedebuglist);
            var statusCompleted = DialogTools.CreateCue("TEST_cw_etudeiscomplete");
            DialogTools.CueAddCondition(statusCompleted, ConditionalTools.CreateEtudeCondition("etudecompleted", testetude, EtudeTools.EtudeStatus.Completed));
            DialogTools.AnswerAddNextCue(statusrequest, statusCompleted);
            DialogTools.CueAddAnswersList(statusCompleted, etudedebuglist);
            var statusYouBrokeIt = DialogTools.CreateCue("TEST_cw_statusplaceholder");
            DialogTools.AnswerAddNextCue(statusrequest, statusYouBrokeIt);
            DialogTools.CueAddAnswersList(statusYouBrokeIt, etudedebuglist);

            // individual status questions
            var statusNegative = DialogTools.CreateCue("TEST_cw_etudefallthrough");
            DialogTools.CueAddAnswersList(statusNegative, etudedebuglist);
            var asknotstarted = DialogTools.CreateAnswer("TEST_a_asknotstarted");
            DialogTools.ListAddAnswer(etudedebuglist, asknotstarted);
            DialogTools.AnswerAddNextCue(asknotstarted, statusNotStarted);
            DialogTools.AnswerAddNextCue(asknotstarted, statusNegative);
            var askstarted = DialogTools.CreateAnswer("TEST_a_askstarted");
            DialogTools.ListAddAnswer(etudedebuglist, askstarted);
            DialogTools.AnswerAddNextCue(askstarted, statusStarted);
            DialogTools.AnswerAddNextCue(askstarted, statusNegative);
            var askplaying = DialogTools.CreateAnswer("TEST_a_askplaying");
            DialogTools.ListAddAnswer(etudedebuglist, askplaying);
            DialogTools.AnswerAddNextCue(askplaying, statusPlaying);
            DialogTools.AnswerAddNextCue(askplaying, statusNegative);
            var askcompleting = DialogTools.CreateAnswer("TEST_a_askcompleting");
            DialogTools.ListAddAnswer(etudedebuglist, askcompleting);
            DialogTools.AnswerAddNextCue(askcompleting, statusCompleting);
            DialogTools.AnswerAddNextCue(askcompleting, statusNegative);
            var askcomplete = DialogTools.CreateAnswer("TEST_a_askcomplete");
            DialogTools.ListAddAnswer(etudedebuglist, askcomplete);
            DialogTools.AnswerAddNextCue(askcomplete, statusCompleted);
            DialogTools.AnswerAddNextCue(askcomplete, statusNegative);

            //update?
            var askupdate = DialogTools.CreateAnswer("TEST_a_unlock");
            DialogTools.ListAddAnswer(etudedebuglist, askupdate);
            var cueupdate = DialogTools.CreateCue("TEST_cw_unlock");

            var ieval = new Kingmaker.Designers.EventConditionActionSystem.Evaluators.IntConstant();
            ieval.Value = 1;

            DialogTools.CueAddOnShowAction(cueupdate, ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.UnlockFlag>( bp => 
            { bp.m_flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(testflag); }));
            DialogTools.CueAddOnShowAction(cueupdate, ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.IncrementFlagValue>(bp =>
            { 
                bp.m_Flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(testflag);
                bp.Value = ieval;
                bp.UnlockIfNot = true;
            }));

            DialogTools.CueAddOnShowAction(cueupdate, ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.UpdateEtudes>());
            DialogTools.AnswerAddNextCue(askupdate, cueupdate);
            DialogTools.CueAddAnswersList(cueupdate, etudedebuglist);

            //End etude debugging
            var donetesting = DialogTools.CreateAnswer("TEST_a_exitetudemenu");
            DialogTools.AnswerAddNextCue(donetesting, starttestcue);
            DialogTools.ListAddAnswer(etudedebuglist, donetesting);
        }*/
    }
}