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

//##########################################################################################################################
// ACT 3 CONTENT
//  Modify Re-recruitment scene
//  Add Tavern scene and corresponding Command Room event
//  Add Argument scene and corresponding Camping event
//  Add Reconciliation dialog
//  Make minor modifications to scenes in act 3.
//
// TOTAL AFFECTION: 9
//##########################################################################################################################

namespace WOTR_WoljifRomanceMod
{
    public static class WRM_Act3
    {
        /*******************************************************************************************************************
         * MODIFY RE-RECRUITMENT SCENE
         * Adds dialog to the re-recruitment scene in act 3 to allow the player to express worry over Woljif, which starts
         * the romance early. The romance can also be started later, in the tavern scene.
         * Affection points: 1
         ******************************************************************************************************************/
        static public void ModifyRerecruitScene()
        {
            //Get existing blueprints
            var answerlist = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>
                             ("2479c49fd0cb64b45869b4b91ac3fdff");
            var romancecount = Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>
                               ("5db9ec615236f044083a5c6bd3292432");
            var romancebase = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomance");
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>
                                ("WRM_WoljifRomanceActive");
            var affection = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_WoljifAffection");

            //Create new dialog.
            var A_AreYouOkay = DialogTools.CreateAnswer("WRM_1_a_AreYouOkay",true);
            var C_ImOkay = DialogTools.CreateCue("WRM_1_cw_ImOkay");
            DialogTools.AnswerAddNextCue(A_AreYouOkay, C_ImOkay);
            DialogTools.CueAddAnswersList(C_ImOkay, answerlist);
            DialogTools.ListAddAnswer(answerlist, A_AreYouOkay, 0);

            //Dialog increments affection and starts romance path.
            DialogTools.AnswerAddOnSelectAction(A_AreYouOkay, ActionTools.StartEtudeAction(romancebase));
            DialogTools.AnswerAddOnSelectAction(A_AreYouOkay, ActionTools.StartEtudeAction(romanceactive));
            DialogTools.AnswerAddOnSelectAction(A_AreYouOkay, ActionTools.IncrementFlagAction(affection,1));
            DialogTools.AnswerAddOnSelectAction(A_AreYouOkay, ActionTools.IncrementFlagAction(romancecount, 1));
        }

        /*******************************************************************************************************************
         * CREATE TAVERN INVITE (COMMAND ROOM EVENT)
         * Adds the crusade notification card and command room cutscene for Woljif's invitation to the tavern.
         * Affection points: 1
         ******************************************************************************************************************/
        static public void CreateTavernCommandRoomEvent()
        {
            // Get existing etudes
            var CompanionEtude = (Kingmaker.AreaLogic.Etudes.BlueprintEtude)
                                 Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(
                                 Kingmaker.Blueprints.BlueprintGuid.Parse("14a80c048c8ceed4a9c856d85bbf10da"));
            var Capital_KTC_group = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>
                                    ("10d01be767521a340978c8e57ab536b6");
            var Capital_WoljifCompanion_group = Resources.GetBlueprint
                                                <Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>
                                                ("97e2c4ec46765f143bf21bc9578b22f7");
            var cutscenetemplate = Resources.GetBlueprint<Kingmaker.AreaLogic.Cutscenes.Cutscene>
                                   ("e8d44f13de8b6154687a05f42f767eb5");
            var romancecounter = Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>
                                 ("5db9ec615236f044083a5c6bd3292432");
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>
                                ("WRM_WoljifRomanceActive");
            var affection = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_WoljifAffection");

            // Create Dialog Tree
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
            DialogTools.AnswerAddOnSelectAction(A_No, ActionTools.IncrementFlagAction(romancecounter, -1));

            // Create notification
            var NotificationEtude = EtudeTools.CreateEtude("WRM_TavernInvite_Notification", CompanionEtude, false, false);
            var Notification = EventTools.CreateCommandRoomEvent("WRM_note_2a_Name", "WRM_note_2a_Desc");
            var NotificationCounter = EtudeTools.CreateFlag("WRM_TavernInviteFlag");
            EventTools.AddResolution(Notification, Kingmaker.Kingdom.Blueprints.EventResult.MarginType.Fail, 
                                     "WRM_note_2a_Ignored");
            EventTools.AddResolution(Notification, Kingmaker.Kingdom.Blueprints.EventResult.MarginType.Success, 
                                     "WRM_note_2a_Complete");
            Kingmaker.ElementsSystem.GameAction[] StartNotification =
                {
                    ActionTools.IncrementFlagAction(NotificationCounter),
                    ActionTools.ConditionalAction(ConditionalTools.CreateFlagCheck("WRM_DontDoubleTavern",
                                                                                   NotificationCounter, 2, 1000000, true))
                };
            ActionTools.ConditionalActionOnTrue((Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)
                                                StartNotification[1], 
                                                ActionTools.StartCommandRoomEventAction(Notification));
            EtudeTools.EtudeAddOnPlayTrigger(NotificationEtude, ActionTools.MakeList(StartNotification));
            EtudeTools.EtudeAddOnCompleteTrigger(NotificationEtude, 
                                                 ActionTools.MakeList(ActionTools.EndCommandRoomEventAction(Notification)));
            
            // create cutscene event etude
            var EventEtude = EtudeTools.CreateEtude("WRM_TavernInvite_Event", NotificationEtude, true, true);
            EtudeTools.EtudeAddConflictingGroups(EventEtude, Capital_KTC_group);
            EtudeTools.EtudeAddConflictingGroups(EventEtude, Capital_WoljifCompanion_group);
            EtudeTools.EtudeAddActivationCondition(EventEtude, ConditionalTools.CreateEtudeGroupCondition(
                                                   "WRM_TavernTrigger", Capital_KTC_group, true));
            EtudeTools.EtudeAddOnCompleteTrigger(EventEtude, ActionTools.MakeList(
                                                 ActionTools.CompleteEtudeAction(NotificationEtude)));

            // Parameterized cutscene stuff.
            var unhideaction = ActionTools.HideUnitAction(EventEtude, Companions.Woljif, true);
            var playscene = ActionTools.PlayCutsceneAction(cutscenetemplate);
            ActionTools.CutsceneActionAddParameter(playscene, "Unit", "unit", 
                                                   CompanionTools.GetCompanionEvaluator(Companions.Woljif, EventEtude));
            Kingmaker.ElementsSystem.Dialog dialogeval = (Kingmaker.ElementsSystem.Dialog)
                                                         Kingmaker.ElementsSystem.Element.CreateInstance(
                                                         typeof(Kingmaker.ElementsSystem.Dialog));
            dialogeval.m_Value = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                 <Kingmaker.Blueprints.BlueprintDialogReference>(InviteDialog);
            ActionTools.CutsceneActionAddParameter(playscene, "Dialog", "blueprint", dialogeval);
            Kingmaker.ElementsSystem.EtudeBlueprint etudeeval = (Kingmaker.ElementsSystem.EtudeBlueprint)
                                                                Kingmaker.ElementsSystem.Element.CreateInstance(
                                                                typeof(Kingmaker.ElementsSystem.EtudeBlueprint));
            etudeeval.m_Value = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                <Kingmaker.Blueprints.BlueprintEtudeReference>(EventEtude);
            ActionTools.CutsceneActionAddParameter(playscene, "Etude", "blueprint", etudeeval);

            Kingmaker.ElementsSystem.GameAction[] actions = { unhideaction, playscene };
            EtudeTools.EtudeAddOnPlayTrigger(EventEtude, ActionTools.MakeList(actions));

            // not sure if needed?
            EventEtude.m_LinkedAreaPart = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                          <Kingmaker.Blueprints.BlueprintAreaPartReference>
                                          (Resources.GetBlueprint<Kingmaker.Blueprints.Area.BlueprintAreaPart>
                                          ("2570015799edf594daf2f076f2f975d8"));

            // Create timer and hook it up to the re-recruitment scene
            var rerecruitCue = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>
                               ("af3b9e51747fda2478b5cf6d97c078fe");

            var TavernTimer = WoljifRomanceMod.Clock.AddTimer("WRM_Timers_Tavern");
            var TavernTimerEtude = EtudeTools.CreateEtude("WRM_Timers_TavernEtude", CompanionEtude, false, false);

            DialogTools.CueAddOnStopAction(rerecruitCue, ActionTools.IncrementFlagAction(TavernTimer.active));
            EtudeTools.EtudeAddActivationCondition(TavernTimerEtude, ConditionalTools.CreateFlagCheck
                                                   ("WRM_Timers_TavernTrigger", TavernTimer.time, 2, 1000000));
            Kingmaker.ElementsSystem.GameAction[] delayedactions = 
                { 
                    ActionTools.StartEtudeAction(EventEtude), 
                    ActionTools.CompleteEtudeAction(TavernTimerEtude) 
                };
            EtudeTools.EtudeAddOnPlayTrigger(TavernTimerEtude, ActionTools.MakeList(delayedactions));
            DialogTools.CueAddOnStopAction(rerecruitCue, ActionTools.StartEtudeAction(TavernTimerEtude));
        }

        /*******************************************************************************************************************
         * CREATE TAVERN SCENE
         * Adds the tavern scene, which plays regardless of whether you romance Woljif; it is the second place you can
         * start the romance at, the first being his re-recruitment.
         * Affection points: 2
         ******************************************************************************************************************/
        static public void CreateTavernCutscene()
        {
            // Get existing blueprints
            var romancecounter = Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>
                                 ("5db9ec615236f044083a5c6bd3292432");
            var romancebase = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomance");
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>
                                ("WRM_WoljifRomanceActive");
            var affection = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_WoljifAffection");

            // Create dialog
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
            DialogTools.AnswerAddShowCondition(A_WhatIf, ConditionalTools.CreateCueSeenCondition
                                                         ("WRM_SeenLoveIsTrouble", C_LoveIsTrouble, true));
            DialogTools.AnswerAddOnSelectAction(A_WhatIf, ActionTools.StartEtudeAction(romanceactive));
            DialogTools.AnswerAddOnSelectAction(A_WhatIf, ActionTools.StartEtudeAction(romancebase));
            DialogTools.AnswerAddOnSelectAction(A_WhatIf, ActionTools.IncrementFlagAction(romancecounter, 1));
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
            DialogTools.AnswerSetAlignmentShift(A_PayForYourself, "lawful", "WRM_shift_2b_PayForYourself");
            DialogTools.AnswerSetAlignmentShift(A_Always, "good", "WRM_shift_2b_Always");
            DialogTools.AnswerSetAlignmentShift(A_RunForIt, "chaotic", "WRM_shift_2b_RunForIt");
            DialogTools.AnswerAddOnSelectAction(A_JustThisOnce, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddOnSelectAction(A_Always, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddOnSelectAction(A_RunForIt, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddNextCue(A_FreeDrinks, C_OfCourseNot);
            DialogTools.CueAddAnswersList(C_OfCourseNot, Answerlist2);
            DialogTools.AnswerAddNextCue(A_PayForYourself, C_NoSympathy);
            DialogTools.AnswerAddNextCue(A_JustThisOnce, C_Thanks);
            DialogTools.AnswerAddNextCue(A_Always, C_WowThanks);
            DialogTools.AnswerAddNextCue(A_RunForIt, C_OneTwoThree);

            //---------------------------------------------------------------------------------------------------------
            // CUTSCENE
            //   Track 0A
            //      Command: Lock Control
            //      EndGate: Gate1
            //   Track 0B
            //      Command: Action(Translocate Player, Translocate Woljif), Move Camera, Delay
            //      EndGate: Gate1
            //   Track 0C
            //      Command: Animate Player
            //      EndGate: None
            //   Track 0D
            //      Command: Animate Woljif
            //      EndGate: None
            // Gate1
            //   Track 1A 
            //      Command: Start Dialog
            //      EndGate: None
            //---------------------------------------------------------------------------------------------------------

            // Locators
            var Tavern_PlayerLoc = new FakeLocator(-46.40f, 49.005f, -150.35f, 277.30f);
            var Tavern_WoljifLoc = new FakeLocator(-48.22f, 49.005f, -150.33f, 99.20f);
            var Tavern_CameraLoc = new FakeLocator(-46.88f, 49.19f, -150.19f, -34.06f);
            var Woljif_Exit = new FakeLocator(-8.844f, 56.02f, 0.325f, 275.0469f);

            // Gate1
            //   Track 1A 
            //      Command: Start Dialog
            //      EndGate: None
            var Track1A = CutsceneTools.CreateTrack(null, CommandTools.StartDialogCommand(TavernDialog, Companions.Woljif));
            var Gate1 = CutsceneTools.CreateGate("WRM_1_Gate1", Track1A);

            //   Track 0A
            //      Command: Lock Control
            //      EndGate: Gate1
            var Track0A = CutsceneTools.CreateTrack(Gate1, CommandTools.LockControlCommand());

            //   Track 0B
            //      Command: Action(Translocate Player, Translocate Woljif), Move Camera, Delay
            //      EndGate: Gate1
            Kingmaker.ElementsSystem.GameAction[] Actions0B = 
                { 
                    ActionTools.TranslocateAction(null, Companions.Player, Tavern_PlayerLoc), 
                    ActionTools.TranslocateAction(null, Companions.Woljif, Tavern_WoljifLoc) 
                };
            Kingmaker.AreaLogic.Cutscenes.CommandBase[] Commands0B = 
                { 
                    CommandTools.ActionCommand("WRM_1_Move0B", Actions0B),
                    CommandTools.CamMoveCommand(Tavern_CameraLoc),
                    CommandTools.DelayCommand(0.5f) 
                };
            ((Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit)
                Actions0B[0]).Unit.Owner = Commands0B[0];
            ((Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit)
                Actions0B[1]).Unit.Owner = Commands0B[0];
            var Track0B = CutsceneTools.CreateTrack(Gate1, Commands0B);

            //   Track 0C
            //      Command: Animate Player
            //      EndGate: None
            var Track0C = CutsceneTools.CreateTrack(null, CommandTools.SitIdleCommand("WRM_1_Animate0C_player", 
                                                                                      Companions.Player));

            //   Track 0D
            //      Command: Animate Woljif
            //      EndGate: None
            var Track0D = CutsceneTools.CreateTrack(null, CommandTools.SitIdleCommand("WRM_1_Animate0C_Woljif", 
                                                                                      Companions.Woljif));
            // Create Cutscene
            Kingmaker.AreaLogic.Cutscenes.Track[] Tracks0 = { Track0A, Track0B, Track0C, Track0D };
            var TavernCutscene = CutsceneTools.CreateCutscene("WRM_TavernCutscene", false, Tracks0);



            // Finishing dialog ends cutscene, moves Woljif back to his normal place, puts player outside tavern.
            DialogTools.CueAddOnStopAction(C_NoSympathy, ActionTools.StopCutsceneAction(TavernCutscene));
            DialogTools.CueAddOnStopAction(C_NoSympathy, ActionTools.TeleportAction("e4694f569a0003448b08fa522f7dc79f", 
                                           ActionTools.MakeList(ActionTools.TranslocateAction(C_NoSympathy, 
                                           Companions.Woljif, Woljif_Exit))));
            DialogTools.CueAddOnStopAction(C_Thanks, ActionTools.StopCutsceneAction(TavernCutscene));
            DialogTools.CueAddOnStopAction(C_Thanks, ActionTools.TeleportAction("e4694f569a0003448b08fa522f7dc79f", 
                                           ActionTools.MakeList(ActionTools.TranslocateAction(C_Thanks, 
                                           Companions.Woljif, Woljif_Exit))));
            DialogTools.CueAddOnStopAction(C_WowThanks, ActionTools.StopCutsceneAction(TavernCutscene));
            DialogTools.CueAddOnStopAction(C_WowThanks, ActionTools.TeleportAction("e4694f569a0003448b08fa522f7dc79f", 
                                           ActionTools.MakeList(ActionTools.TranslocateAction(C_WowThanks, 
                                           Companions.Woljif, Woljif_Exit))));
            DialogTools.CueAddOnStopAction(C_OneTwoThree, ActionTools.StopCutsceneAction(TavernCutscene));
            DialogTools.CueAddOnStopAction(C_OneTwoThree, ActionTools.TeleportAction("e4694f569a0003448b08fa522f7dc79f", 
                                           ActionTools.MakeList(ActionTools.TranslocateAction(C_OneTwoThree, 
                                           Companions.Woljif, Woljif_Exit))));

            // Connect tavern cutscene to invitation scene
            var teleportparty = ActionTools.TeleportAction("320516612f496da4a8919ba4c78b0be4", 
                                ActionTools.MakeList(ActionTools.PlayCutsceneAction(TavernCutscene)));
            DialogTools.CueAddOnStopAction(Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>
                                           ("WRM_2a_c_SeeYouThere"), teleportparty);
        }

        /*******************************************************************************************************************
         * CREATE ARGUMENT SCENE (CAMPING ENCOUNTER)
         * Adds the scene where Woljif argues with the player. He gets mad no matter what you do.
         * Affection points: 1
         ******************************************************************************************************************/
        static public void CreateArgumentScene()
        {
            // Get existing
            var romancecount = Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>
                               ("5db9ec615236f044083a5c6bd3292432");
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>
                                ("WRM_WoljifRomanceActive");
            var Ch3Quest = Resources.GetBlueprint<Kingmaker.Blueprints.Quests.BlueprintQuest>
                           ("fbef28077988e8049a2e9d3d9dd3fd79");
            var capital = Resources.GetBlueprint<Kingmaker.Blueprints.Area.BlueprintArea>
                          ("2570015799edf594daf2f076f2f975d8");
            var woljifmaindialog = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintDialog>
                                   ("8a38eeddc0215a84ca441439bb96b8f4");
            var affection = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_WoljifAffection");
            var CompanionEtude = (Kingmaker.AreaLogic.Etudes.BlueprintEtude)
                                 Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint
                                 (Kingmaker.Blueprints.BlueprintGuid.Parse("14a80c048c8ceed4a9c856d85bbf10da"));
            var taverndialog = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintDialog>
                               ("WRM_2b_TavernDialog");

            // Create etudes and timers for getting angry, staying angry, and reconciling.
            var HadFight = EtudeTools.CreateEtude("WRM_WoljifArguedWithPlayer", CompanionEtude, false, false);
            var AngryEtude = EtudeTools.CreateEtude("WRM_WoljifAngry", CompanionEtude, false, false);
            var ReadyToReconcile = EtudeTools.CreateEtude("WRM_WoljifReadyToReconcile", CompanionEtude, false, false);
            var Reconciled = EtudeTools.CreateEtude("WRM_WoljifReconciled", CompanionEtude, false, false);
            var AngryTimer = WoljifRomanceMod.Clock.AddTimer("WRM_Timers_Angry");
            var AngryTimerEtude = EtudeTools.CreateEtude("WRM_Timers_AngryEtude", HadFight, false, false);

            EtudeTools.EtudeAddOnCompleteTrigger(AngryEtude, ActionTools.MakeList(
                                                 ActionTools.StartEtudeAction(ReadyToReconcile)));
            EtudeTools.EtudeAddStartsWith(HadFight, AngryEtude);
            EtudeTools.EtudeAddOnCompleteTrigger(ReadyToReconcile,
                                                 ActionTools.MakeList(ActionTools.StartEtudeAction(Reconciled)));

            EtudeTools.EtudeAddActivationCondition(AngryTimerEtude, ConditionalTools.CreateFlagCheck(
                                                   "WRM_Timers_AngryTrigger", AngryTimer.time, 1, 1000000));
            Kingmaker.ElementsSystem.GameAction[] finishanger = 
                { 
                    ActionTools.CompleteEtudeAction(AngryEtude), 
                    ActionTools.CompleteEtudeAction(AngryTimerEtude) 
                };
            EtudeTools.EtudeAddOnPlayTrigger(AngryTimerEtude, ActionTools.MakeList(finishanger));
            Kingmaker.ElementsSystem.GameAction[] onfight =
                {
                    ActionTools.IncrementFlagAction(AngryTimer.active),
                    ActionTools.StartEtudeAction(AngryTimerEtude)
                };
            EtudeTools.EtudeAddOnPlayTrigger(HadFight, ActionTools.MakeList(onfight));

            // Create Dialog
            var C_WhatsYourGame = DialogTools.CreateCue("WRM_3_c_WhatsYourGame");
            var A_CardGames = DialogTools.CreateAnswer("WRM_3_a_CardGames");
            var A_What1 = DialogTools.CreateAnswer("WRM_3_a_What1");
            var L_Answers1 = DialogTools.CreateAnswersList("WRM_3_L_Answers1");
            var C_YouWantSomething = DialogTools.CreateCue("WRM_3_c_YouWantSomething");
            var A_What2 = DialogTools.CreateAnswer("WRM_3_a_What2");
            var A_CalmDown = DialogTools.CreateAnswer("WRM_3_a_CalmDown");
            var A_IWantToBeNice = DialogTools.CreateAnswer("WRM_3_a_IWantToBeNice");
            var A_GrowUp = DialogTools.CreateAnswer("WRM_3_a_GrowUp");
            var A_YouWannaBeMad = DialogTools.CreateAnswer("WRM_3_a_YouWannaBeMad");
            var C_StopTheIdiotAct = DialogTools.CreateCue("WRM_3_c_StopTheIdiotAct");
            var C_CalmDown = DialogTools.CreateCue("WRM_3_c_CalmDown");
            var C_NobodysThisNice = DialogTools.CreateCue("WRM_3_c_NobodysThisNice");
            var C_GuiltTrips = DialogTools.CreateCue("WRM_3_c_GuiltTrips");
            var C_DontPityMe = DialogTools.CreateCue("WRM_3_c_DontPityMe");
            var C_IDontNeedYou = DialogTools.CreateCue("WRM_3_c_IDontNeedYou");
            var L_Answers2 = DialogTools.CreateAnswersList("WRM_3_L_Answers2");
            var C_NobodysThisNice2 = DialogTools.CreateCue("WRM_3_c_NobodysThisNice2");
            var C_GuiltTrips2 = DialogTools.CreateCue("WRM_3_c_GuiltTrips2");
            var C_DontPityMe2 = DialogTools.CreateCue("WRM_3_c_DontPityMe2");
            var A_IWantToBeNice2 = DialogTools.CreateAnswer("WRM_3_a_IWantToBeNice2", "WRM_3_a_IWantToBeNice");
            var A_YouWannaBeMad2 = DialogTools.CreateAnswer("WRM_3_a_YouWannaBeMad2", "WRM_3_a_YouWannaBeMad");
            var A_GrowUp2 = DialogTools.CreateAnswer("WRM_3_a_GrowUp2", "WRM_3_a_GrowUp");
            var L_Answers3 = DialogTools.CreateAnswersList("WRM_3_L_Answers3");
            var ArgumentDialog = DialogTools.CreateDialog("WRM_ArgumentDialog", C_WhatsYourGame);

            DialogTools.CueAddAnswersList(C_WhatsYourGame, L_Answers1);
            DialogTools.ListAddAnswer(L_Answers1, A_CardGames);
            DialogTools.ListAddAnswer(L_Answers1, A_What1);
            DialogTools.AnswerAddNextCue(A_CardGames, C_YouWantSomething);
            DialogTools.AnswerAddNextCue(A_What1, C_YouWantSomething);
            DialogTools.CueAddAnswersList(C_YouWantSomething, L_Answers2);
            DialogTools.ListAddAnswer(L_Answers2, A_What2);
            DialogTools.ListAddAnswer(L_Answers2, A_CalmDown);
            DialogTools.ListAddAnswer(L_Answers2, A_IWantToBeNice);
            DialogTools.ListAddAnswer(L_Answers2, A_YouWannaBeMad);
            DialogTools.ListAddAnswer(L_Answers2, A_GrowUp);
            DialogTools.AnswerAddOnSelectAction(A_IWantToBeNice, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddOnSelectAction(A_YouWannaBeMad, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddOnSelectAction(A_GrowUp, ActionTools.CompleteEtudeAction(romanceactive));
            DialogTools.AnswerAddOnSelectAction(A_GrowUp, ActionTools.IncrementFlagAction(romancecount, -1));
            DialogTools.AnswerAddNextCue(A_What2, C_StopTheIdiotAct);
            DialogTools.AnswerAddNextCue(A_CalmDown, C_CalmDown);
            DialogTools.AnswerAddNextCue(A_IWantToBeNice, C_NobodysThisNice);
            DialogTools.AnswerAddNextCue(A_YouWannaBeMad, C_DontPityMe);
            DialogTools.AnswerAddNextCue(A_GrowUp, C_GuiltTrips);
            DialogTools.CueAddContinue(C_GuiltTrips, C_IDontNeedYou);
            DialogTools.CueAddContinue(C_NobodysThisNice, C_IDontNeedYou);
            DialogTools.CueAddAnswersList(C_CalmDown, L_Answers3);
            DialogTools.CueAddAnswersList(C_StopTheIdiotAct, L_Answers3);
            DialogTools.ListAddAnswer(L_Answers3, A_IWantToBeNice2);
            DialogTools.ListAddAnswer(L_Answers3, A_YouWannaBeMad2);
            DialogTools.ListAddAnswer(L_Answers3, A_GrowUp2);
            DialogTools.AnswerAddNextCue(A_IWantToBeNice2, C_NobodysThisNice2);
            DialogTools.AnswerAddNextCue(A_YouWannaBeMad2, C_DontPityMe2);
            DialogTools.AnswerAddNextCue(A_GrowUp2, C_GuiltTrips2);
            DialogTools.AnswerAddOnSelectAction(A_GrowUp2, ActionTools.CompleteEtudeAction(romanceactive));
            DialogTools.AnswerAddOnSelectAction(A_GrowUp2, ActionTools.IncrementFlagAction(romancecount, -1));
            DialogTools.CueAddContinue(C_GuiltTrips2, C_IDontNeedYou);
            DialogTools.CueAddContinue(C_NobodysThisNice2, C_IDontNeedYou);

            DialogTools.DialogAddFinishAction(ArgumentDialog, ActionTools.StartEtudeAction(HadFight));

            // Create Camp Event
            var ArgumentEvent = EventTools.CreateCampingEvent("WRM_ArgumentEvent", 100);
            EventTools.CampEventAddCondition(ArgumentEvent, ConditionalTools.CreateEtudeCondition
                                             ("WRM_HaveNotFoughtYet", HadFight, "Playing", true));
            EventTools.CampEventAddCondition(ArgumentEvent, ConditionalTools.CreateEtudeCondition
                                             ("WRM_IsRomanceActive", romanceactive, "Playing"));
            EventTools.CampEventAddCondition(ArgumentEvent, ConditionalTools.CreateQuestStatusCondition
                                             ("WRM_Ch3Qfinished", "Completed", Ch3Quest));
            EventTools.CampEventAddCondition(ArgumentEvent, ConditionalTools.CreateDialogSeenCondition
                                             ("WRM_SeenTavernScene", taverndialog));
            EventTools.CampEventAddCondition(ArgumentEvent, ConditionalTools.CreateCurrentAreaIsCondition
                                             ("WRM_argumentNotInCapital", capital, true));
            EventTools.CampEventAddCondition(ArgumentEvent, ConditionalTools.CreateCompanionInPartyCondition
                                             ("WRM_WoljifInPartyForFight", Companions.Woljif));
            EventTools.CampEventAddAction(ArgumentEvent, ActionTools.RemoveCampEventAction(ArgumentEvent));
            EventTools.CampEventAddAction(ArgumentEvent, ActionTools.StartDialogAction(ArgumentDialog, Companions.Woljif));

            // Timer after completing Crescent of the Abyss.
            var CampCounter = EtudeTools.CreateFlag("WRM_ArgumentCampEventFlag");
            Kingmaker.ElementsSystem.GameAction[] AddCampEvent = 
                {
                    ActionTools.IncrementFlagAction(CampCounter),
                    ActionTools.ConditionalAction(ConditionalTools.CreateCampEventCheck("WRM_CheckArgumentExistence", 
                                                                                        ArgumentEvent, true))
                };
            ActionTools.ConditionalActionOnTrue((Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)
                                                AddCampEvent[1], ActionTools.AddCampEventAction(ArgumentEvent));


            var Ch3_FinalizedReRecruit1 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>
                                          ("326e5a8ef1f92914ab122ebac703b4c8");
            var Ch3_FinalizedReRecruit2 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>
                                          ("941797cc93bac7f4e824022ec3ee08b0");
            var Ch3_FinalizedReRecruit3 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>
                                          ("0b926925ab25d794a9dce6629a9a98de");

            // New timer
            var ArgumentTimer = WoljifRomanceMod.Clock.AddTimer("WRM_Timers_Argument");
            var ArgumentTimerEtude = EtudeTools.CreateEtude("WRM_Timers_ArgumentEtude", romanceactive, false, false);
            EtudeTools.EtudeAddActivationCondition(ArgumentTimerEtude, ConditionalTools.CreateFlagCheck
                                                   ("WRM_Timers_ArgumentTrigger", ArgumentTimer.time, 3, 1000000));
            Kingmaker.ElementsSystem.GameAction[] delayedactions = 
                { 
                    AddCampEvent[0], 
                    AddCampEvent[1], 
                    ActionTools.CompleteEtudeAction(ArgumentTimerEtude) 
                };
            EtudeTools.EtudeAddOnPlayTrigger(ArgumentTimerEtude, ActionTools.MakeList(delayedactions));

            DialogTools.CueAddOnStopAction(Ch3_FinalizedReRecruit1, ActionTools.IncrementFlagAction(ArgumentTimer.active));
            DialogTools.CueAddOnStopAction(Ch3_FinalizedReRecruit1, ActionTools.StartEtudeAction(ArgumentTimerEtude));
            DialogTools.CueAddOnStopAction(Ch3_FinalizedReRecruit2, ActionTools.IncrementFlagAction(ArgumentTimer.active));
            DialogTools.CueAddOnStopAction(Ch3_FinalizedReRecruit2, ActionTools.StartEtudeAction(ArgumentTimerEtude));
            DialogTools.CueAddOnStopAction(Ch3_FinalizedReRecruit3, ActionTools.IncrementFlagAction(ArgumentTimer.active));
            DialogTools.CueAddOnStopAction(Ch3_FinalizedReRecruit3, ActionTools.StartEtudeAction(ArgumentTimerEtude));

            
            // Alter dialog while Woljif is mad at you
            var C_DontTalkToMe = DialogTools.CreateCue("WRM_3a_c_DontTalkToMe");
            DialogTools.CueAddCondition(C_DontTalkToMe, ConditionalTools.CreateEtudeCondition
                                        ("WRM_WoljifMadCheck", AngryEtude, EtudeTools.EtudeStatus.Playing));
            woljifmaindialog.FirstCue.Cues.Insert(0, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                                  <Kingmaker.Blueprints.BlueprintCueBaseReference>(C_DontTalkToMe));
        }

        /*******************************************************************************************************************
         * ALTER POST-QUEST DIALOG
         * Alters dialog when finishing Crescent of the Abyss, allowing you to welcome Woljif warmly back to the party.
         * Affection points: 1
         ******************************************************************************************************************/
        static public void AlterPostQuestDialog()
        {
            var affection = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_WoljifAffection");
            var alteredanswerslist = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>
                                     ("42e88596d802aec47af9da5b8179b8ef");
            var devalkilled = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>
                              ("326e5a8ef1f92914ab122ebac703b4c8");
            var devalnotkilled = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>
                                 ("2a4e76637816fbc428c22879a2d29652");

            var C_WarmSmile = DialogTools.CreateCue("WRM_3b_c_WarmSmile");
            var A_OfCourseYouCan = DialogTools.CreateAnswer("WRM_3b_a_OfCourseYouCan");
        
            DialogTools.AnswerAddOnSelectAction(A_OfCourseYouCan, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddNextCue(A_OfCourseYouCan, C_WarmSmile);
            DialogTools.ListAddAnswer(alteredanswerslist, A_OfCourseYouCan, 2);
            DialogTools.CueAddContinue(C_WarmSmile, devalkilled);
            DialogTools.CueAddContinue(C_WarmSmile, devalnotkilled);
            var giveobjective = ActionTools.GenericAction
                                <Kingmaker.Designers.EventConditionActionSystem.Actions.GiveObjective>(bp =>
                {
                    bp.m_Objective = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                     <Kingmaker.Blueprints.BlueprintQuestObjectiveReference>
                                     (Resources.GetBlueprint<Kingmaker.Blueprints.Quests.BlueprintQuestObjective>
                                     ("9b97232f7c5d6774c822e91ea6d075b8"));
                });
            DialogTools.CueAddOnStopAction(C_WarmSmile, giveobjective);
            var setobjective = ActionTools.GenericAction
                               <Kingmaker.Designers.EventConditionActionSystem.Actions.SetObjectiveStatus>(bp =>
                {
                    bp.m_Objective = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                     <Kingmaker.Blueprints.BlueprintQuestObjectiveReference>
                                     (Resources.GetBlueprint<Kingmaker.Blueprints.Quests.BlueprintQuestObjective>
                                     ("54b9562225773f640a271b2c0686fbbc"));
                });
            DialogTools.CueAddOnStopAction(C_WarmSmile, setobjective);
        }

        /*******************************************************************************************************************
         * CREATE RECONCILIATION
         * Adds new dialog when you talk to Woljif after arguing with him, allowing you to reconcile.
         * Affection points: 2
         ******************************************************************************************************************/
        static public void CreateReconciliation()
        {
            var romancecounter = Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>
                                 ("5db9ec615236f044083a5c6bd3292432");
            var affection = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_WoljifAffection");
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>
                                ("WRM_WoljifRomanceActive");
            var readytoreconcile = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>
                                   ("WRM_WoljifReadyToReconcile");
            var woljifnormalanswerlist = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>
                                         ("e41585da330233143b34ef64d7d62d69");
            var woljifmaindialog = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintDialog>
                                   ("8a38eeddc0215a84ca441439bb96b8f4");
            var Ch4QuestEtude = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>
                                ("b969089c8ec6ba04abb3aa51d9c54876");
            var hadfight = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>
                           ("WRM_WoljifArguedWithPlayer");
            var reconciled = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifReconciled");

            // Add non-romantic reconciliation dialog. This is minimal, just to smooth the transition from 
            // him being mad to asking for help in act 4, in the event that you've ended the romance.
            var C_ForgiveAndForget = DialogTools.CreateCue("WRM_4justfriends_c_Sorry");
            var C_Anyway = DialogTools.CreateCue("WRM_4justfriends_c_Anyway");

            var readytoreconcilecond = ConditionalTools.CreateEtudeCondition("WRM_WoljifReconciliation", readytoreconcile, 
                                                                             EtudeTools.EtudeStatus.Playing);
            DialogTools.CueAddCondition(C_ForgiveAndForget, readytoreconcilecond);
            DialogTools.CueAddCondition(C_ForgiveAndForget, ConditionalTools.CreateEtudeCondition
                                                            ("WRM_WoljifPlatonicReconciliation", romanceactive, 
                                                            EtudeTools.EtudeStatus.Playing, true));
            DialogTools.CueAddContinue(C_ForgiveAndForget, C_Anyway);
            DialogTools.CueAddAnswersList(C_Anyway, woljifnormalanswerlist);
            DialogTools.CueAddOnShowAction(C_ForgiveAndForget, ActionTools.CompleteEtudeAction(readytoreconcile));
            
            woljifmaindialog.FirstCue.Cues.Insert(0, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                                  <Kingmaker.Blueprints.BlueprintCueBaseReference>(C_ForgiveAndForget));

            // Add romantic reconciliation dialog.
            var C_Sorry = DialogTools.CreateCue("WRM_4_c_Sorry");
            var C_Sorry2 = DialogTools.CreateCue("WRM_4_c_Sorry2");
            var L_AnswersList1 = DialogTools.CreateAnswersList("WRM_4_L_ApologyResponse");
            var A_ApologyAccepted = DialogTools.CreateAnswer("WRM_4_a_ApologyAccepted");
            var A_WhatDoYouMean = DialogTools.CreateAnswer("WRM_4_a_WhatDoYouMean");
            var A_YoureStillLearning = DialogTools.CreateAnswer("WRM_4_a_YoureStillLearning");
            var A_ApologyRejected = DialogTools.CreateAnswer("WRM_4_a_ApologyRejected");
            var C_Thanks = DialogTools.CreateCue("WRM_4_c_Thanks");
            var C_WellYoure = DialogTools.CreateCue("WRM_4_c_WellYoure");
            var C_LargerThanLife = DialogTools.CreateCue("WRM_4_c_LargerThanLife");
            var C_ThatsJustIt = DialogTools.CreateCue("WRM_4_c_ThatsJustIt");
            var C_NothingICanDo = DialogTools.CreateCue("WRM_4_c_NothingICanDo");
            var C_ActLikeNothingHappened = DialogTools.CreateCue("WRM_4_c_ActLikeNothingHappened");
            var C_EvenIWannaFollowYou = DialogTools.CreateCue("WRM_4_c_EvenIWannaFollowYou");
            var L_AnswersList2 = DialogTools.CreateAnswersList("WRM_4_L_ScareResponse");
            var A_IDidntMeanTo = DialogTools.CreateAnswer("WRM_4_a_IDidntMeanTo");
            var A_YouCanTrustMe = DialogTools.CreateAnswer("WRM_4_a_YouCanTrustMe");
            var A_IAmScary = DialogTools.CreateAnswer("WRM_4_a_IAmScary");
            var C_IBelieveYou = DialogTools.CreateCue("WRM_4_c_IBelieveYou");
            var C_DifferentKindOfScary = DialogTools.CreateCue("WRM_4_c_DifferentKindOfScary");
            var L_AnswersList3 = DialogTools.CreateAnswersList("WRM_4_L_MovingOn");
            var A_UsOrYou = DialogTools.CreateAnswer("WRM_4_a_UsOrYou", true);
            var A_Understandable = DialogTools.CreateAnswer("WRM_4_a_Understandable");
            var C_WhatsThatMean = DialogTools.CreateCue("WRM_4_c_WhatsThatMean");
            var C_ChangeTopic = DialogTools.CreateCue("WRM_4_c_ChangeTopic");

            DialogTools.CueAddContinue(C_Sorry, C_Sorry2);
            DialogTools.CueAddAnswersList(C_Sorry2, L_AnswersList1);
            DialogTools.ListAddAnswer(L_AnswersList1, A_ApologyAccepted);
            DialogTools.ListAddAnswer(L_AnswersList1, A_WhatDoYouMean);
            DialogTools.ListAddAnswer(L_AnswersList1, A_YoureStillLearning);
            DialogTools.ListAddAnswer(L_AnswersList1, A_ApologyRejected);
            DialogTools.AnswerAddOnSelectAction(A_ApologyRejected, ActionTools.CompleteEtudeAction(romanceactive));
            DialogTools.AnswerAddOnSelectAction(A_ApologyRejected, ActionTools.IncrementFlagAction(romancecounter, -1));
            DialogTools.AnswerAddNextCue(A_ApologyAccepted, C_Thanks);
            DialogTools.AnswerAddNextCue(A_WhatDoYouMean, C_WellYoure);
            DialogTools.AnswerAddNextCue(A_YoureStillLearning, C_ThatsJustIt);
            DialogTools.AnswerAddNextCue(A_ApologyRejected, C_NothingICanDo);
            DialogTools.CueAddContinue(C_NothingICanDo, C_ActLikeNothingHappened);
            DialogTools.CueAddAnswersList(C_ActLikeNothingHappened, woljifnormalanswerlist);
            DialogTools.CueAddContinue(C_Thanks, C_LargerThanLife);
            DialogTools.CueAddContinue(C_WellYoure, C_LargerThanLife);
            DialogTools.CueAddContinue(C_ThatsJustIt, C_EvenIWannaFollowYou);
            DialogTools.CueAddContinue(C_LargerThanLife, C_EvenIWannaFollowYou);
            DialogTools.CueAddAnswersList(C_EvenIWannaFollowYou, L_AnswersList2);
            DialogTools.ListAddAnswer(L_AnswersList2, A_IDidntMeanTo);
            DialogTools.ListAddAnswer(L_AnswersList2, A_YouCanTrustMe);
            DialogTools.ListAddAnswer(L_AnswersList2, A_IAmScary);
            DialogTools.AnswerAddOnSelectAction(A_IDidntMeanTo, ActionTools.IncrementFlagAction(affection));
            DialogTools.AnswerAddOnSelectAction(A_YouCanTrustMe, ActionTools.IncrementFlagAction(affection));
            DialogTools.AnswerAddNextCue(A_IDidntMeanTo, C_IBelieveYou);
            DialogTools.AnswerAddNextCue(A_YouCanTrustMe, C_IBelieveYou);
            DialogTools.AnswerAddNextCue(A_IAmScary, C_DifferentKindOfScary);
            DialogTools.CueAddAnswersList(C_IBelieveYou, L_AnswersList3);
            DialogTools.CueAddAnswersList(C_DifferentKindOfScary, L_AnswersList3);
            DialogTools.ListAddAnswer(L_AnswersList3, A_UsOrYou);
            DialogTools.ListAddAnswer(L_AnswersList3, A_Understandable);
            DialogTools.AnswerAddOnSelectAction(A_UsOrYou, ActionTools.IncrementFlagAction(affection));
            DialogTools.AnswerAddNextCue(A_UsOrYou, C_WhatsThatMean);
            DialogTools.CueAddAnswersList(C_WhatsThatMean, L_AnswersList3);
            DialogTools.AnswerAddNextCue(A_Understandable, C_ChangeTopic);
            DialogTools.CueAddAnswersList(C_ChangeTopic, woljifnormalanswerlist);

            DialogTools.CueAddOnShowAction(C_Sorry2, ActionTools.CompleteEtudeAction(readytoreconcile));
            DialogTools.CueAddCondition(C_Sorry, readytoreconcilecond);
            DialogTools.CueAddCondition(C_Sorry, ConditionalTools.CreateEtudeCondition("WRM_WoljifRomanticReconciliation", 
                                                 romanceactive, EtudeTools.EtudeStatus.Playing, false));
            woljifmaindialog.FirstCue.Cues.Insert(0, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                                     <Kingmaker.Blueprints.BlueprintCueBaseReference>(C_Sorry));

            // Prevent Ch4 quest from triggering if you fought with Woljif and haven't made up yet.
            Kingmaker.ElementsSystem.Condition[] conditiongroup =
                {
                    ConditionalTools.CreateEtudeCondition("WRM_DidNotFight",hadfight,"Playing",true),
                    ConditionalTools.CreateEtudeCondition("WRM_MadeUp",reconciled,"Playing",false)
                };
            var newcondition = ConditionalTools.CreateLogicCondition("WRM_DidntFightOrMadeUp", 
                                                                     Kingmaker.ElementsSystem.Operation.Or, conditiongroup);
            EtudeTools.EtudeAddActivationCondition(Ch4QuestEtude, newcondition);
        }

        /*******************************************************************************************************************
         * MINOR CHANGES
         * Small changes at Areelu's lab and Elan's wedding.
         * Affection points: 1
         ******************************************************************************************************************/
        static public void MiscChanges()
        {
            var affection = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_WoljifAffection");
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>
                                ("WRM_WoljifRomanceActive");
            var reconciled = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifReconciled");

            // Slight alteration of dialog in Woljif's vision in Areelu's lab if romanced and above 4 affection.
            var wish_dialog = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintDialog>
                              ("70d8abb125a1a0b46a7207b3181c48aa");
            var wish_answerlist = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>
                                  ("3f350eb01cead394d80ca211291da035");
            var wish_altered = DialogTools.CreateCue("WRM_wish_c_MissedYou");
            Kingmaker.ElementsSystem.Condition[] conds = 
                {
                    ConditionalTools.CreateEtudeCondition("WRM_wish_romance", romanceactive, "Playing"),
                    ConditionalTools.CreateEtudeCondition("WRM_wish_reconciled", reconciled, "Playing"),
                    ConditionalTools.CreateFlagCheck("WRM_wish_affection", affection, 4, 30)
                };
            var cond = ConditionalTools.CreateLogicCondition("WRM_wish_romancecondition", conds);
            DialogTools.CueAddCondition(wish_altered, cond);
            DialogTools.CueAddAnswersList(wish_altered, wish_answerlist);
            DialogTools.DialogInsertCue(wish_dialog, wish_altered, 0);

            // Quick side dialog at Elan's wedding.
            var callback_cue = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>
                               ("WRM_2b_c_LoveIsTrouble");
            var wedding_cue = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>
                              ("129096ab9053bb94d9af259f0086e91c");
            var wedding_answerlist = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>
                                     ("06219022c60b7a5498522b8ae19cf86f");
            var wedding_answer1 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>
                                  ("f9c0231349cfa0941886896ceebe22a0");
            var wedding_answer2 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>
                                  ("a66eb55f054a25d4499ccb20e256c6d5");
            var wedding_answer3 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>
                                  ("06376a7020b39114b9a1f1998863b9da");
            var wedding_answer4 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>
                                  ("a14e18a0dd93e7a48b7645a97a1d1f61");

            var c_CrazyThings = DialogTools.CreateCue("WRM_wedding_c_CrazyThings");
            DialogTools.CueSetSpeaker(c_CrazyThings, Companions.Woljif);
            DialogTools.CueAddCondition(c_CrazyThings, ConditionalTools.CreateCueSeenCondition
                                                       ("WRM_SeenTavernLine", callback_cue));
            DialogTools.CueAddContinue(wedding_cue, c_CrazyThings);
            var a_LikePaying = DialogTools.CreateAnswer("WRM_wedding_a_LikePaying");
            var c_InsideJoke = DialogTools.CreateCue("WRM_wedding_c_InsideJoke");
            DialogTools.CueSetSpeaker(c_InsideJoke, Companions.Woljif, false);
            DialogTools.AnswerAddNextCue(a_LikePaying, c_InsideJoke);
            DialogTools.AnswerAddOnSelectAction(a_LikePaying, ActionTools.IncrementFlagAction(affection, 1));
            var L_jokeanswers = DialogTools.CreateAnswersList("WRM_L_weddingjoke");
            DialogTools.ListAddAnswer(L_jokeanswers, a_LikePaying);
            DialogTools.ListAddAnswer(L_jokeanswers, wedding_answer1);
            DialogTools.ListAddAnswer(L_jokeanswers, wedding_answer2);
            DialogTools.ListAddAnswer(L_jokeanswers, wedding_answer3);
            DialogTools.ListAddAnswer(L_jokeanswers, wedding_answer4);
            DialogTools.CueAddAnswersList(c_CrazyThings, L_jokeanswers);
            DialogTools.CueAddAnswersList(c_InsideJoke, wedding_answerlist);
        }
    }
}