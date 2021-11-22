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

        static public void CreateTavernCommandRoomEvent()
        {
            var romancebase = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomance");
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceActive");
            var affection = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_WoljifAffection");

            //Create Dialog Tree
            var C_Invite = DialogTools.CreateCue("WRM_2a_c_Invite");
            var A_LoveTo = DialogTools.CreateAnswer("WRM_2a_a_IdLoveTo");
            var A_Sure = DialogTools.CreateAnswer("WRM_2a_a_Sure");
            var A_No = DialogTools.CreateAnswer("WRM_2a_a_No");
            var L_answers = DialogTools.CreateAnswersList("WRM_2a_L_answers");
            var C_SeeYouThere = DialogTools.CreateCue("WRM_2a_c_SeeYouThere");
            var C_ForgetIt = DialogTools.CreateCue("WRM_2a_c_ForgetIt");
            var InviteDialog = DialogTools.CreateDialog("WRM_2a_InvitationDialog", C_Invite);

            DialogTools.CueAddAnswersList(C_Invite, L_answers);
            DialogTools.ListAddAnswer(L_answers, A_LoveTo);
            DialogTools.ListAddAnswer(L_answers, A_Sure);
            DialogTools.ListAddAnswer(L_answers, A_No);
            DialogTools.AnswerAddNextCue(A_LoveTo, C_SeeYouThere);
            DialogTools.AnswerAddNextCue(A_Sure, C_SeeYouThere);
            DialogTools.AnswerAddNextCue(A_No, C_ForgetIt);
            DialogTools.AnswerAddOnSelectAction(A_LoveTo, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddOnSelectAction(A_No, ActionTools.CompleteEtudeAction(romanceactive));

            // Create the mechanics of the scene
            var NotificationEtude = EtudeTools.CreateEtude("WRM_TavernInvite_Notification", romancebase, false, false);
            var EventEtude = EtudeTools.CreateEtude("WRM_TavernInvite_Event", NotificationEtude, true, true);
            var Notification = CommandRoomEventTools.CreateEvent("WRM_note_2a_Name", "WRM_note_2a_Desc");
            CommandRoomEventTools.AddResolution(Notification, Kingmaker.Kingdom.Blueprints.EventResult.MarginType.Fail, "WRM_note_2a_Ignored");
            CommandRoomEventTools.AddResolution(Notification, Kingmaker.Kingdom.Blueprints.EventResult.MarginType.Success, "WRM_note_2a_Complete");

            EtudeTools.EtudeAddOnPlayTrigger(NotificationEtude, ActionTools.MakeList(ActionTools.StartCommandRoomEventAction(Notification)));
            EtudeTools.EtudeAddOnDeactivateTrigger(NotificationEtude, ActionTools.MakeList(ActionTools.EndCommandRoomEventAction(Notification)));
            var Capital_KTC_group = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>("10d01be767521a340978c8e57ab536b6");
            var Capital_WoljifCompanion_group = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>("97e2c4ec46765f143bf21bc9578b22f7");
            EtudeTools.EtudeAddConflictingGroups(EventEtude, Capital_KTC_group);
            EtudeTools.EtudeAddConflictingGroups(EventEtude, Capital_WoljifCompanion_group);
            EtudeTools.EtudeAddActivationCondition(EventEtude, ConditionalTools.CreateEtudeGroupCondition("WRM_TavernTrigger", Capital_KTC_group, true));

            // Parameterized cutscene stuff.
            var unhideaction = ActionTools.HideUnitAction(Companions.Woljif, true);
            var playscene = ActionTools.PlayCutsceneAction(Resources.GetBlueprint<Kingmaker.AreaLogic.Cutscenes.Cutscene>("e8d44f13de8b6154687a05f42f767eb5"));
            ActionTools.CutsceneActionAddParameter(playscene, "Unit", "unit", CommandTools.getCompanionEvaluator(Companions.Woljif));
            Kingmaker.ElementsSystem.Dialog dialogeval = (Kingmaker.ElementsSystem.Dialog)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(Kingmaker.ElementsSystem.Dialog));
            dialogeval.m_Value = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintDialogReference>(InviteDialog);
            ActionTools.CutsceneActionAddParameter(playscene, "Dialog", "blueprint", dialogeval);
            Kingmaker.ElementsSystem.EtudeBlueprint etudeeval = (Kingmaker.ElementsSystem.EtudeBlueprint)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(Kingmaker.ElementsSystem.EtudeBlueprint));
            etudeeval.m_Value = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintEtudeReference>(EventEtude);
            ActionTools.CutsceneActionAddParameter(playscene, "Etude", "blueprint", etudeeval);

            Kingmaker.ElementsSystem.GameAction[] actions = { unhideaction, playscene };
            EtudeTools.EtudeAddOnPlayTrigger(EventEtude, ActionTools.MakeList(actions));

            EventEtude.m_LinkedAreaPart = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintAreaPartReference>(Resources.GetBlueprint<Kingmaker.Blueprints.Area.BlueprintAreaPart>("2570015799edf594daf2f076f2f975d8"));

            //Create a timer to trigger the event, and hook it up to the rerecruit dialog.
            var TavernTimer = EtudeTools.CreateEtude("WRM_TimerBeforeTavernScene", romancebase, false, false);
            Kingmaker.ElementsSystem.GameAction[] delayedactions = { ActionTools.StartEtudeAction(EventEtude), ActionTools.CompleteEtudeAction(TavernTimer) };
            EtudeTools.EtudeAddDelayedAction(TavernTimer, 2, ActionTools.MakeList(delayedactions));
            var rerecruitCue = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("af3b9e51747fda2478b5cf6d97c078fe");
            DialogTools.CueAddOnStopAction(rerecruitCue, ActionTools.StartEtudeAction(TavernTimer));

            // Finally, we need to hook up the actual cutscene and teleportation to the OnStop of the "See you at the tavern" cue.
            // Because this function is potentially called before the cutscene exists, but the command room scene won't play out
            // until you go to Drezen and trigger the cutscene creation, hooking up the cue to the trigger is handled in the 
            // cutscene creation function.

            /*********** DEBUG STUFF ***********/
            //var debuganswerlist = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("TEST_L_debugmenu");
            //var taverntestanswer = DialogTools.CreateAnswer("TEST_a_startTavernEtude");
            //var taverntestcue = DialogTools.CreateCue("TEST_cw_startTavernEtude");
            //DialogTools.ListAddAnswer(debuganswerlist, taverntestanswer, 0);
            //DialogTools.AnswerAddNextCue(taverntestanswer, taverntestcue);
            //DialogTools.CueAddOnStopAction(taverntestcue, ActionTools.StartEtudeAction(EventEtude));
            /***********************************/
        }
        static public void CreateTavernCutscene(Kingmaker.Blueprints.EntityReference PCloc, Kingmaker.Blueprints.EntityReference WJloc, Kingmaker.Blueprints.EntityReference cameraloc, Kingmaker.Blueprints.EntityReference WJExitLoc)
        {
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceActive");
            var affection = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_WoljifAffection");

            /*********** DEBUG STUFF ***********/
            //var debuganswerlist = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("TEST_L_debugmenu");
            //var taverntestanswer = DialogTools.CreateAnswer("TEST_a_toTavern");
            //var taverntestcue = DialogTools.CreateCue("TEST_cw_toTavern");
            //DialogTools.ListAddAnswer(debuganswerlist, taverntestanswer, 0);
            //DialogTools.AnswerAddNextCue(taverntestanswer, taverntestcue);
            /***********************************/

            // Create dialog
            var placeholdercue = DialogTools.CreateCue("TEST_PLACEHOLDER");
            var placeholderdialog = DialogTools.CreateDialog("WRM_PLACEHOLDERDIALOG", placeholdercue);

            var C_CrazyStory = DialogTools.CreateCue("WRM_2b_c_CrazyStory");
            var A_NoodleIncident = DialogTools.CreateAnswer("WRM_2b_a_NoodleIncident", true);
            var A_WhenItWorksOut = DialogTools.CreateAnswer("WRM_2b_a_WhenItWorksOut");
            var A_HaveYouEverLoved = DialogTools.CreateAnswer("WRM_2b_a_HaveYouLoved");
            var A_WhatIf = DialogTools.CreateAnswer("WRM_2b_a_WhatIf");
            var A_LoveMakesPeopleCrazy = DialogTools.CreateAnswer("WRM_2b_a_LoveMakesPeopleCrazy");
            var Answerlist1 = DialogTools.CreateAnswersList("WRM_2b_L_1");
            var C_NoodleIncident = DialogTools.CreateCue("WRM_2b_c_NoodleIncident");
            var C_LoveIsTrouble = DialogTools.CreateCue("WRM_2b_c_LoveIsTrouble");
            var C_NeverLoved = DialogTools.CreateCue("WRM_2b_c_NeverLoved");
            var C_ICouldGoForThat = DialogTools.CreateCue("WRM_2b_c_ICouldGoForThat");
            var C_IfWishesWereHorses = DialogTools.CreateCue("WRM_2b_c_IfWishesWereHorses");
            var C_YourePayingRight = DialogTools.CreateCue("WRM_2b_c_YourePayingRight");
            var Answerlist2 = DialogTools.CreateAnswersList("WRM_2b_L_2");
            var A_FreeDrinks = DialogTools.CreateAnswer("WRM_2b_a_FreeDrinks", true);
            var A_PayForYourself = DialogTools.CreateAnswer("WRM_2b_a_PayForYourself");
            var A_JustThisOnce = DialogTools.CreateAnswer("WRM_2b_a_JustThisOnce");
            var A_Always = DialogTools.CreateAnswer("WRM_2b_a_Always");
            var A_RunForIt = DialogTools.CreateAnswer("WRM_2b_a_RunForIt");
            var C_OfCourseNot = DialogTools.CreateCue("WRM_2b_c_OfCourseNot");
            var C_NoSympathy = DialogTools.CreateCue("WRM_2b_c_NoSympathy");
            var C_Thanks = DialogTools.CreateCue("WRM_2b_c_Thanks");
            var C_WowThanks = DialogTools.CreateCue("WRM_2b_c_WowThanks");
            var C_OneTwoThree = DialogTools.CreateCue("WRM_2b_c_OneTwoThree");
            var TavernDialog = DialogTools.CreateDialog("WRM_2b_TavernDialog", C_CrazyStory);

            DialogTools.CueAddAnswersList(C_CrazyStory, Answerlist1);
            DialogTools.ListAddAnswer(Answerlist1, A_NoodleIncident);
            DialogTools.ListAddAnswer(Answerlist1, A_WhenItWorksOut);
            DialogTools.ListAddAnswer(Answerlist1, A_HaveYouEverLoved);
            DialogTools.ListAddAnswer(Answerlist1, A_WhatIf);
            DialogTools.ListAddAnswer(Answerlist1, A_LoveMakesPeopleCrazy);
            DialogTools.AnswerAddShowCondition(A_WhatIf, 
                ConditionalTools.CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.CueSeen>("WRM_SeenLoveIsTrouble", bp =>
                {
                    bp.m_Cue = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintCueBaseReference>(C_LoveIsTrouble);
                    bp.CurrentDialog = true;
                }));
            DialogTools.AnswerAddOnSelectAction(A_WhatIf, ActionTools.StartEtudeAction(romanceactive));
            DialogTools.AnswerAddOnSelectAction(A_WhatIf, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddNextCue(A_NoodleIncident, C_NoodleIncident);
            DialogTools.CueAddAnswersList(C_NoodleIncident, Answerlist1);
            DialogTools.CueAddAnswersList(C_LoveIsTrouble, Answerlist1);
            DialogTools.CueAddAnswersList(C_NeverLoved, Answerlist1);
            DialogTools.AnswerAddNextCue(A_WhenItWorksOut, C_LoveIsTrouble);
            DialogTools.AnswerAddNextCue(A_HaveYouEverLoved, C_NeverLoved);
            DialogTools.AnswerAddNextCue(A_WhatIf, C_ICouldGoForThat);
            DialogTools.CueAddContinue(C_ICouldGoForThat, C_IfWishesWereHorses);
            DialogTools.CueAddContinue(C_IfWishesWereHorses, C_YourePayingRight);
            DialogTools.AnswerAddNextCue(A_LoveMakesPeopleCrazy, C_YourePayingRight);
            DialogTools.CueAddAnswersList(C_YourePayingRight, Answerlist2);
            DialogTools.ListAddAnswer(Answerlist2, A_FreeDrinks);
            DialogTools.ListAddAnswer(Answerlist2, A_PayForYourself);
            DialogTools.ListAddAnswer(Answerlist2, A_JustThisOnce);
            DialogTools.ListAddAnswer(Answerlist2, A_Always);
            DialogTools.ListAddAnswer(Answerlist2, A_RunForIt);
            DialogTools.AnswerAlignmentShift(A_PayForYourself, "lawful", "WRM_shift_2b_PayForYourself");
            DialogTools.AnswerAlignmentShift(A_Always, "good", "WRM_shift_2b_Always");
            DialogTools.AnswerAlignmentShift(A_RunForIt, "chaotic", "WRM_shift_2b_RunForIt");
            DialogTools.AnswerAddOnSelectAction(A_JustThisOnce, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddOnSelectAction(A_Always, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddOnSelectAction(A_RunForIt, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddNextCue(A_FreeDrinks, C_OfCourseNot);
            DialogTools.CueAddAnswersList(C_OfCourseNot, Answerlist2);
            DialogTools.AnswerAddNextCue(A_PayForYourself, C_NoSympathy);
            DialogTools.AnswerAddNextCue(A_JustThisOnce, C_Thanks);
            DialogTools.AnswerAddNextCue(A_Always, C_WowThanks);
            DialogTools.AnswerAddNextCue(A_RunForIt, C_OneTwoThree);

            // Cutscene
            //   Track 0A
            //      Command: Lock Control
            //      EndGate: Gate1
            //   Track 0B
            //      Command: Action(Translocate Player, Translocate Woljif), Move Camera, Delay
            //      EndGate: Gate1
            //   Track 0C
            //      Command: Animate Player
            //      EndGate: None
            //   Track 0C
            //      Command: Animate Woljif
            //      EndGate: None
            // Gate1
            //   Track 1A 
            //      Command: Start Dialog
            //      EndGate: None

            // Create Track 1A
            var Track1A = CutsceneTools.CreateTrack(null, CommandTools.StartDialogCommand(TavernDialog, Companions.Woljif));
            // Create Gate 0
            var Gate1 = CutsceneTools.CreateGate("WRM_1_Gate1", Track1A);

            // Create Track 0A
            var Track0A = CutsceneTools.CreateTrack(Gate1, CommandTools.LockControlCommand());
            // Create Track 0B
            Kingmaker.ElementsSystem.GameAction[] Actions0B = { ActionTools.TranslocateAction(Companions.Player,PCloc), ActionTools.TranslocateAction(Companions.Woljif, WJloc) };
            Kingmaker.AreaLogic.Cutscenes.CommandBase[] Commands0B = 
                { CommandTools.ActionCommand("WRM_1_Move0B", Actions0B),
                  CommandTools.CamMoveCommand(cameraloc),
                  CommandTools.DelayCommand(0.5f) };
            var Track0B = CutsceneTools.CreateTrack(Gate1, Commands0B);
            // Create Track 0C and 0D
            var Track0C = CutsceneTools.CreateTrack(null, CommandTools.SitIdleCommand("WRM_1_Animate0C_player", Companions.Player));
            var Track0D = CutsceneTools.CreateTrack(null, CommandTools.SitIdleCommand("WRM_1_Animate0C_Woljif", Companions.Woljif));
            // Create Cutscene
            Kingmaker.AreaLogic.Cutscenes.Track[] Tracks0 = { Track0A, Track0B, Track0C, Track0D };
            var TavernCutscene = CutsceneTools.CreateCutscene("WRM_TavernCutscene", false, Tracks0);

            // Finishing dialog ends cutscene, moves Woljif back to his normal place, puts player outside tavern.
            DialogTools.CueAddOnStopAction(C_NoSympathy, ActionTools.StopCutsceneAction(TavernCutscene));
            DialogTools.CueAddOnStopAction(C_NoSympathy, ActionTools.TeleportAction("e4694f569a0003448b08fa522f7dc79f", ActionTools.MakeList(ActionTools.TranslocateAction(Companions.Woljif, WJExitLoc))));
            DialogTools.CueAddOnStopAction(C_Thanks, ActionTools.StopCutsceneAction(TavernCutscene));
            DialogTools.CueAddOnStopAction(C_Thanks, ActionTools.TeleportAction("e4694f569a0003448b08fa522f7dc79f", ActionTools.MakeList(ActionTools.TranslocateAction(Companions.Woljif, WJExitLoc))));
            DialogTools.CueAddOnStopAction(C_WowThanks, ActionTools.StopCutsceneAction(TavernCutscene));
            DialogTools.CueAddOnStopAction(C_WowThanks, ActionTools.TeleportAction("e4694f569a0003448b08fa522f7dc79f", ActionTools.MakeList(ActionTools.TranslocateAction(Companions.Woljif, WJExitLoc))));
            DialogTools.CueAddOnStopAction(C_OneTwoThree, ActionTools.StopCutsceneAction(TavernCutscene));
            DialogTools.CueAddOnStopAction(C_OneTwoThree, ActionTools.TeleportAction("e4694f569a0003448b08fa522f7dc79f", ActionTools.MakeList(ActionTools.TranslocateAction(Companions.Woljif, WJExitLoc))));


            var teleportparty = ActionTools.TeleportAction("320516612f496da4a8919ba4c78b0be4", ActionTools.MakeList(ActionTools.PlayCutsceneAction(TavernCutscene)));
            DialogTools.CueAddOnStopAction(Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("WRM_2a_c_SeeYouThere"), teleportparty);

            /*********** DEBUG STUFF ***********/
            //var teleportparty = ActionTools.TeleportAction("320516612f496da4a8919ba4c78b0be4", ActionTools.MakeList(ActionTools.PlayCutsceneAction(TavernCutscene)));
            //DialogTools.CueAddOnStopAction(taverntestcue, teleportparty);
            /***********************************/
        }
    }
}