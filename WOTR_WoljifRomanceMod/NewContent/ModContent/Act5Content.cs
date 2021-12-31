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
    public static class WRM_Act5
    { 
        public static void AlterJealousyScene()
        {
            var romance = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomance");
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceActive");
            var affectiongatesuccess = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceGatePassed");

            var teleportcommand = Resources.GetBlueprint<Kingmaker.AreaLogic.Cutscenes.CommandAction>("2ecec1045eb9d9b41a2a30a6bb477e99");
            var jealousycutscene = Resources.GetBlueprint<Kingmaker.AreaLogic.Cutscenes.Cutscene>("c72670de688ed91478c673b9e451a84e");
            var jealousygate2 = Resources.GetBlueprint<Kingmaker.AreaLogic.Cutscenes.Gate>("ce447620870b9fc4da3d611d7f68c316");
            var deadex = Resources.GetBlueprint<Kingmaker.ElementsSystem.ActionsHolder>("f1d5126b188f47eea5f28763f27a45ca");
            var recount = Resources.GetBlueprint<Kingmaker.ElementsSystem.ActionsHolder>("2660cbace62d42f38d543c822dbb660f");
            var jealousyetude = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("4799a25da39295b43a6eefcd2cb2b4a7");
            var woljifetudegroup = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>("97e2c4ec46765f143bf21bc9578b22f7");

            Kingmaker.ElementsSystem.Condition[] RomanceIsActiveConds = {
                ConditionalTools.CreateEtudeCondition("WRM_RomanceActiveJealousy", romanceactive, "playing"),
                ConditionalTools.CreateEtudeCondition("WRM_AffectionGateJealousy", affectiongatesuccess, "playing")
                };
            var WoljifRomanceIsActiveCond = ConditionalTools.CreateLogicCondition("WRM_RomanceConds", RomanceIsActiveConds);

            // Add Woljif to the jealousy scene support logic
            // Jealousy trigger logic
            jealousyetude.m_ConflictingGroups.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintEtudeConflictingGroupReference>(woljifetudegroup));
            // DeadEx logic
            Kingmaker.ElementsSystem.Condition[] DeadExConds = {
                ConditionalTools.CreateEtudeCondition("WRM_RomanceActiveJealousyDE", romanceactive, "playing"),
                ConditionalTools.CreateEtudeCondition("WRM_AffectionGateJealousyDE", affectiongatesuccess, "playing"),
                ConditionalTools.CreateCompanionAvailableCondition("WRM_WoljifDeadExNotHere",Companions.Woljif,true)
                };
            var deadexlogiccond = ConditionalTools.CreateLogicCondition("WRM_DeadExLogic", DeadExConds);
            var Woljif_deadExLogic = ActionTools.ConditionalAction(deadexlogiccond);
            Kingmaker.ElementsSystem.GameAction[] DeadExActions = {
                ActionTools.CompleteEtudeAction(romanceactive),
                ActionTools.IncrementFlagAction(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"),-1)
                };
            ActionTools.ConditionalActionOnTrue(Woljif_deadExLogic, DeadExActions);
            var deadexlen = deadex.Actions.Actions.Length;
            Array.Resize(ref deadex.Actions.Actions, deadexlen + 1);
            deadex.Actions.Actions[deadexlen] = Woljif_deadExLogic;
            // Recount logic
            var WoljifRecountLogic = ActionTools.ConditionalAction(WoljifRomanceIsActiveCond);
            ActionTools.ConditionalActionOnTrue(WoljifRecountLogic, ActionTools.MakeList(ActionTools.IncrementFlagAction(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"), 1)));
            var recountlen = recount.Actions.Actions.Length;
            Array.Resize(ref recount.Actions.Actions, recountlen + 1);
            recount.Actions.Actions[recountlen] = WoljifRecountLogic;
            // Move Woljif into place for jealousy scene
            // The existing mechanism for moving characters into position is absolutely insane.
            // Instead, I'm just going to have Woljif lean against the wall in the corner.
            var WoljifJealousyLocation = new FakeLocator(183.41f, 78.65f, -5.2f, 227.0f);

            var WoljifTeleport = ActionTools.ConditionalAction(WoljifRomanceIsActiveCond);
            Kingmaker.ElementsSystem.GameAction[] TranslocateActions = {
                ActionTools.HideUnitAction(Companions.Woljif,true),
                ActionTools.TranslocateAction(Companions.Woljif, WoljifJealousyLocation)
                };
            ActionTools.ConditionalActionOnTrue(WoljifTeleport, TranslocateActions);

            Kingmaker.Designers.EventConditionActionSystem.Actions.TeleportParty teleportaction = (Kingmaker.Designers.EventConditionActionSystem.Actions.TeleportParty) teleportcommand.Action.Actions[0];
            var len = teleportaction.AfterTeleport.Actions.Length;
            Array.Resize(ref teleportaction.AfterTeleport.Actions, len + 1);
            teleportaction.AfterTeleport.Actions[len] = WoljifTeleport;

            var WoljifAnimationTrack = CutsceneTools.CreateTrack(jealousygate2, CommandTools.GenericAnimationCommand("WRM_WallLean", "643323b9c4819734281718e563720d0f", Companions.Woljif, true));
            var WoljifAnimationGate = CutsceneTools.CreateGate("WRM_WoljifJealousyAnimationGate",WoljifAnimationTrack);
            jealousycutscene.m_Tracks[2].m_EndGate = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.GateReference>(WoljifAnimationGate);

            var Woljif_Exit = new FakeLocator(-8.844f, 56.02f, 0.325f, 275.0469f);
            var gate3 = CutsceneTools.CreateGate("WRM_JealousyEndGate");
            var WoljifMoveBackTrack = CutsceneTools.CreateTrack(gate3, CommandTools.TranslocateCommand(Companions.Woljif, Woljif_Exit));
            jealousygate2.m_Tracks.Add(WoljifMoveBackTrack);

            // fetch dialog blueprints for altering or referencing
            var jealousyDialog = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintDialog>("9ce7c9655f7d1e848a25f7eadaa9b48b");
            var answerslist0013 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("22d6e1bd9be82ed4793e46b9257d2290");
            var cuesequence0006 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCueSequence>("0664882547b6b714a921276a32cde387");
            var cuesequence0015 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCueSequence>("33338fe2060ee2648887fb110743a166");
            var cuesequence0038 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCueSequence>("de484d4db94e48540a993008eb726d11");
            // Add new dialog
            var C_IcyStare = DialogTools.CreateCue("WRM_jealousy_c_IcyStare");
            DialogTools.CueSetSpeaker(C_IcyStare, Companions.Woljif, false);
            var C_WhatGives = DialogTools.CreateCue("WRM_jealousy_c_WhatGives");
            DialogTools.CueSetSpeaker(C_WhatGives, Companions.Woljif, false);
            var C_NoSharing = DialogTools.CreateCue("WRM_jealousy_c_NoSharing");
            DialogTools.CueSetSpeaker(C_NoSharing, Companions.Woljif, false);
            var C_Heartbroken = DialogTools.CreateCue("WRM_jealousy_c_Heartbroken");
            DialogTools.CueSetSpeaker(C_Heartbroken, Companions.Woljif, false);
            var C_WoljifChosen = DialogTools.CreateCue("WRM_jealousy_c_WoljifChosen");
            DialogTools.CueSetSpeaker(C_WoljifChosen, Companions.Woljif, false);
            var A_ChooseWoljif = DialogTools.CreateAnswer("WRM_jealousy_a_ChooseWoljif");
            var WoljifJealousyFlag = EtudeTools.CreateFlag("WRM_Jealousy_Woljif");

            DialogTools.CueAddCondition(C_IcyStare, WoljifRomanceIsActiveCond);
            DialogTools.CueAddOnShowAction(C_IcyStare, ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.UnlockFlag>(bp =>
                {
                    bp.m_flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(WoljifJealousyFlag);
                    bp.flagValue = 1;
                }));
            DialogTools.CueAddAnswersList(C_IcyStare, answerslist0013);
            DialogTools.CueAddContinue(C_IcyStare, cuesequence0006);

            DialogTools.CueAddCondition(C_WhatGives, WoljifRomanceIsActiveCond);
            DialogTools.CueAddCondition(C_WhatGives, ConditionalTools.CreateCueSeenCondition("WRM_DidNotSeeIcyStare", true, C_IcyStare));
            DialogTools.CueAddOnShowAction(C_WhatGives, ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.UnlockFlag>(bp =>
            {
                bp.m_flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(WoljifJealousyFlag);
                bp.flagValue = 1;
            }));
            DialogTools.CueSequenceInsertCue(cuesequence0006, C_WhatGives, 4);

            DialogTools.CueAddCondition(C_NoSharing, WoljifRomanceIsActiveCond);
            DialogTools.CueSequenceInsertCue(cuesequence0015, C_NoSharing, 5);

            DialogTools.AnswerAddShowCondition(A_ChooseWoljif, WoljifRomanceIsActiveCond);
            // Not gonna build the dump-everyone-else logic myself, just gonna steal from the reject-all option
            var dumpall = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("49d46039853452b46bcc17798e23c42f");
            A_ChooseWoljif.OnSelect = Helpers.CreateCopy< Kingmaker.ElementsSystem.ActionList >(dumpall.OnSelect);
            DialogTools.AnswerAddNextCue(A_ChooseWoljif, C_WoljifChosen);
            DialogTools.ListAddAnswer(answerslist0013, A_ChooseWoljif, 7);

            DialogTools.CueAddContinue(C_WoljifChosen, cuesequence0038);

            /*Kingmaker.ElementsSystem.Condition[] RejectedWoljif = {
                ConditionalTools.CreateEtudeCondition("WRM_WoljifRomanceEnded",romanceactive,"completed"),
                ConditionalTools.CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.FlagUnlocked>("WRM_WoljifJealousyUnlocked", bp =>
                    {
                        bp.m_ConditionFlag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(WoljifJealousyFlag);
                    })
                };*/
            DialogTools.CueAddCondition(C_Heartbroken, ConditionalTools.CreateLogicCondition("WRM_WoljifRejected1", ConditionalTools.CreateEtudeCondition("WRM_WoljifRomanceEnded", romanceactive, "completed")));
            DialogTools.CueAddCondition(C_Heartbroken, ConditionalTools.CreateLogicCondition("WRM_WoljifRejected2", ConditionalTools.CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.FlagUnlocked>("WRM_WoljifJealousyUnlocked", bp =>
                {
                    bp.m_ConditionFlag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(WoljifJealousyFlag);
                })));
            DialogTools.CueSequenceInsertCue(cuesequence0038, C_Heartbroken, 5);

            // Add new logic to existing dialog
            var choseSosiel = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("e2f8eca4d1413e94392d482057692c3b");
            var choseWenduag = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("a4c7d27ec3117114683fbec7c5b7b413");
            var choseLann = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("b18cf3ef54f5cff45a5553c8f158a65e");
            var choseArue = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("1953c294863805e48932c4d8a05429be");
            var choseDaeran = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("3f2350fc8f4b04f4f91e7415a2121fce");
            var choseCam = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("6ad9dce57025edc4f8ba38e9036739bc");
            var choseQueen = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("ff1b7853e56bc22439fb48fa199b5ca8");

            var RejectWoljifAction = ActionTools.ConditionalAction(WoljifRomanceIsActiveCond);
            ActionTools.ConditionalActionOnTrue(RejectWoljifAction, ActionTools.MakeList(ActionTools.CompleteEtudeAction(romanceactive)));

            DialogTools.AnswerAddOnSelectAction(choseSosiel, RejectWoljifAction);
            DialogTools.AnswerAddOnSelectAction(choseWenduag, RejectWoljifAction);
            DialogTools.AnswerAddOnSelectAction(choseLann, RejectWoljifAction);
            DialogTools.AnswerAddOnSelectAction(choseArue, RejectWoljifAction);
            DialogTools.AnswerAddOnSelectAction(choseDaeran, RejectWoljifAction);
            DialogTools.AnswerAddOnSelectAction(choseCam, RejectWoljifAction);
            DialogTools.AnswerAddOnSelectAction(choseQueen, RejectWoljifAction);
            DialogTools.AnswerAddOnSelectAction(dumpall, RejectWoljifAction);

            // Whoever starts the jealousy dialog, except Sosiel, does not get their jealousy flag set, and thus won't
            // have rejection dialog if not chosen. So let's fix that for Owlcat while we're in here.
            var WenduagBrokenCue = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("570ec5939426bc4429347eed57184fd7");
            var WenduagWorkingCue = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("3235af45abc085f45a8a37ac851ad790");
            WenduagBrokenCue.OnShow = WenduagWorkingCue.OnShow;
            var LannBrokenCue = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("7501c07ac346fbf419c61dbd6b042113");
            var LannWorkingCue = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("8d1334089913d02479714b6c5b14d65a");
            LannBrokenCue.OnShow = LannWorkingCue.OnShow;
            var ArueBrokenCue = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("f96061a260cf80b40b95bd4f88cb2f65");
            var ArueWorkingCue = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("11548b1c9893b044d979b1f4974baa70");
            ArueBrokenCue.OnShow = ArueWorkingCue.OnShow;
            var DaeranBrokenCue = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("6831849758f30c14d9acdf895cced267");
            var DaeranWorkingCue = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("21550d8e72249ad4a8bfc2597dc940ce");
            DaeranBrokenCue.OnShow = DaeranWorkingCue.OnShow;
        }

        public static void AddSnowSceneInvite()
        {
            var romancebase = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomance");
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceActive");
            var Act5 = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("faf70d5071e52d145bfb686244200452");

            // Create invitation dialog
            var C_MeetMeTonight = DialogTools.CreateCue("WRM_7a_c_MeetMeTonight");
            var C_Nod = DialogTools.CreateCue("WRM_7a_c_Nod");
            var C_Crestfallen = DialogTools.CreateCue("WRM_7a_c_Crestfallen");
            var A_No = DialogTools.CreateAnswer("WRM_7a_a_No");
            var A_OfCourse = DialogTools.CreateAnswer("WRM_7a_a_OfCourse");
            var L_1 = DialogTools.CreateAnswersList("WRM_7a_L_1");
            DialogTools.ListAddAnswer(L_1, A_OfCourse);
            DialogTools.ListAddAnswer(L_1, A_No);
            DialogTools.CueAddAnswersList(C_MeetMeTonight, L_1);
            DialogTools.AnswerAddNextCue(A_No, C_Crestfallen);
            DialogTools.AnswerAddNextCue(A_OfCourse, C_Nod);
            DialogTools.AnswerAddOnSelectAction(A_No, ActionTools.CompleteEtudeAction(romanceactive));
            var InviteDialog = DialogTools.CreateDialog("WRM_7a_InvitationDialog", C_MeetMeTonight);


            // Create the mechanics of the scene
            var NotificationEtude = EtudeTools.CreateEtude("WRM_SnowInvite_Notification", romancebase, false, false);
            var EventEtude = EtudeTools.CreateEtude("WRM_SnowInvite_Event", NotificationEtude, true, true);
            var Notification = EventTools.CreateCommandRoomEvent("WRM_note_7a_Name", "WRM_note_7a_Desc");
            var NotificationCounter = EtudeTools.CreateFlag("WRM_SnowInviteFlag");
            EventTools.AddResolution(Notification, Kingmaker.Kingdom.Blueprints.EventResult.MarginType.Fail, "WRM_note_7a_Ignored");
            EventTools.AddResolution(Notification, Kingmaker.Kingdom.Blueprints.EventResult.MarginType.Success, "WRM_note_7a_Complete");

            Kingmaker.ElementsSystem.GameAction[] StartNotification =
                {
                    ActionTools.IncrementFlagAction(NotificationCounter),
                    ActionTools.ConditionalAction(ConditionalTools.CreateFlagCheck("WRM_DontDoubleSnow",NotificationCounter, 2, 1000000, true))
                };
            ActionTools.ConditionalActionOnTrue((Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)StartNotification[1], ActionTools.StartCommandRoomEventAction(Notification));
            EtudeTools.EtudeAddOnPlayTrigger(NotificationEtude, ActionTools.MakeList(StartNotification));
            //EtudeTools.EtudeAddOnPlayTrigger(NotificationEtude, ActionTools.MakeList(ActionTools.StartCommandRoomEventAction(Notification)));
            EtudeTools.EtudeAddOnDeactivateTrigger(NotificationEtude, ActionTools.MakeList(ActionTools.EndCommandRoomEventAction(Notification)));
            var Capital_KTC_group = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>("10d01be767521a340978c8e57ab536b6");
            var Capital_WoljifCompanion_group = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>("97e2c4ec46765f143bf21bc9578b22f7");
            EtudeTools.EtudeAddConflictingGroups(EventEtude, Capital_KTC_group);
            EtudeTools.EtudeAddConflictingGroups(EventEtude, Capital_WoljifCompanion_group);
            EtudeTools.EtudeAddActivationCondition(EventEtude, ConditionalTools.CreateEtudeGroupCondition("WRM_SnowTrigger", Capital_KTC_group, true));
            EtudeTools.EtudeAddActivationCondition(EventEtude, ConditionalTools.CreateEtudeCondition("WRM_SnowMeansLove", romanceactive, "playing"));

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


            // Timer
            var SnowTimer = EtudeTools.CreateEtude("WRM_TimerBeforeSnowScene", romanceactive, false, false);
            Kingmaker.ElementsSystem.GameAction[] delayedactions = { ActionTools.StartEtudeAction(EventEtude), ActionTools.CompleteEtudeAction(SnowTimer) };
            //EtudeTools.EtudeAddDelayedAction(SnowTimer, 10, ActionTools.MakeList(delayedactions));
            // DEBUG VERSION WITH SHORTER TIMER
            EtudeTools.EtudeAddDelayedAction(SnowTimer, 2, ActionTools.MakeList(delayedactions));

            Act5.m_StartsWith.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintEtudeReference>(SnowTimer));

            //ce8c690ebb184474fbcb73e31144c8c3 iomedae cue
            //var IomedaeCue = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("ce8c690ebb184474fbcb73e31144c8c3");
            //DialogTools.CueAddOnShowAction(IomedaeCue, ActionTools.StartEtudeAction(SnowTimer));
        }

        public static void AddSnowCutscene()
        {
            // Build dialog tree
            var c_Waiting = DialogTools.CreateCue("WRM_7b_c_Waiting");
            var c_ThereYouAre = DialogTools.CreateCue("WRM_7b_c_ThereYouAre");
            var c_IGotPapers = DialogTools.CreateCue("WRM_7b_c_IGotPapers");
            var L_1 = DialogTools.CreateAnswersList("WRM_7b_L_1");
            var a_Examine = DialogTools.CreateAnswer("WRM_7b_a_Examine");
            var a_RunAway = DialogTools.CreateAnswer("WRM_7b_a_RunAway");
            var a_NotGoing = DialogTools.CreateAnswer("WRM_7b_a_NotGoing");
            var c_IDidntMakeThem = DialogTools.CreateCue("WRM_7b_c_IDidntMakeThem");
            var c_IGuessIAm = DialogTools.CreateCue("WRM_7b_c_IGuessIAm");
            var c_What = DialogTools.CreateCue("WRM_7b_c_What");
            var L_2 = DialogTools.CreateAnswersList("WRM_7b_L_2");
            var a_Lawful = DialogTools.CreateAnswer("WRM_7b_a_Lawful");
            var a_Good = DialogTools.CreateAnswer("WRM_7b_a_Good");
            var a_Chaotic = DialogTools.CreateAnswer("WRM_7b_a_Chaotic");
            var a_Evil = DialogTools.CreateAnswer("WRM_7b_a_Evil");
            var a_Neutral = DialogTools.CreateAnswer("WRM_7b_a_Neutral");
            var c_Lawful = DialogTools.CreateCue("WRM_7b_c_Lawful");
            var c_Good = DialogTools.CreateCue("WRM_7b_c_Good");
            var c_Chaotic = DialogTools.CreateCue("WRM_7b_c_Chaotic");
            var c_Evil = DialogTools.CreateCue("WRM_7b_c_Evil");
            var c_Neutral = DialogTools.CreateCue("WRM_7b_c_Neutral");
            var L_3 = DialogTools.CreateAnswersList("WRM_7b_L_3");
            var a_ThisIsntLikeYou = DialogTools.CreateAnswer("WRM_7b_a_ThisIsntLikeYou");
            var a_IDontWannaLoseYou = DialogTools.CreateAnswer("WRM_7b_a_IDontWannaLoseYou");
            var a_InTooDeep = DialogTools.CreateAnswer("WRM_7b_a_InTooDeep");
            var a_ImStaying = DialogTools.CreateAnswer("WRM_7b_a_ImStaying");
            var c_IDontWannaLoseYou = DialogTools.CreateCue("WRM_7b_c_IDontWannaLoseYou");
            var c_Speechless = DialogTools.CreateCue("WRM_7b_c_Speechless");
            var c_AlrightFine = DialogTools.CreateCue("WRM_7b_c_AlrightFine");
            var c_NoWay = DialogTools.CreateCue("WRM_7b_c_NoWay");
            var c_IShouldGo = DialogTools.CreateCue("WRM_7b_c_IShouldGo");
            var SnowDialog = DialogTools.CreateDialog("WRM_7b_SnowDialog", c_Waiting);

            DialogTools.CueAddContinue(c_Waiting, c_ThereYouAre);
            DialogTools.CueAddContinue(c_ThereYouAre, c_IGotPapers);
            DialogTools.CueAddAnswersList(c_IGotPapers, L_1);

            DialogTools.ListAddAnswer(L_1, a_Examine);
            DialogTools.ListAddAnswer(L_1, a_RunAway);
            DialogTools.ListAddAnswer(L_1, a_NotGoing);
            DialogTools.AnswerAddNextCue(a_Examine, c_IDidntMakeThem);
            DialogTools.CueAddAnswersList(c_IDidntMakeThem, L_1);
            DialogTools.AnswerAddNextCue(a_RunAway, c_IGuessIAm);
            DialogTools.CueAddAnswersList(c_IGuessIAm, L_1);
            DialogTools.AnswerAddNextCue(a_NotGoing, c_What);

            DialogTools.CueAddAnswersList(c_What, L_2);
            DialogTools.ListAddAnswer(L_2, a_Lawful);
            DialogTools.AnswerAlignmentShift(a_Lawful, "Lawful", "WRM_shift_7b_Lawful");
            DialogTools.AnswerAddNextCue(a_Lawful, c_Lawful);
            DialogTools.CueAddAnswersList(c_Lawful, L_3);
            DialogTools.ListAddAnswer(L_2, a_Good);
            DialogTools.AnswerAlignmentShift(a_Good, "Good", "WRM_shift_7b_Good");
            DialogTools.AnswerAddNextCue(a_Good, c_Good);
            DialogTools.CueAddAnswersList(c_Good, L_3);
            DialogTools.ListAddAnswer(L_2, a_Chaotic);
            DialogTools.AnswerAlignmentShift(a_Chaotic, "Chaotic", "WRM_shift_7b_Chaotic");
            DialogTools.AnswerAddNextCue(a_Chaotic, c_Chaotic);
            DialogTools.CueAddAnswersList(c_Chaotic, L_3);
            DialogTools.ListAddAnswer(L_2, a_Evil);
            DialogTools.AnswerAlignmentShift(a_Evil, "Evil", "WRM_shift_7b_Evil");
            DialogTools.AnswerAddNextCue(a_Evil, c_Evil);
            DialogTools.CueAddAnswersList(c_Evil, L_3);
            DialogTools.ListAddAnswer(L_2, a_Neutral);
            DialogTools.AnswerAddNextCue(a_Neutral, c_Neutral);
            DialogTools.CueAddAnswersList(c_Neutral, L_3);

            DialogTools.ListAddAnswer(L_3, a_ThisIsntLikeYou);
            DialogTools.ListAddAnswer(L_3, a_IDontWannaLoseYou);
            DialogTools.AnswerAddShowCondition(a_IDontWannaLoseYou, ConditionalTools.CreateCueSeenCondition("WRM_sawIDontWannaLoseYou", c_IDontWannaLoseYou, true));
            DialogTools.ListAddAnswer(L_3, a_InTooDeep);
            DialogTools.ListAddAnswer(L_3, a_ImStaying);
            DialogTools.AnswerAddNextCue(a_ThisIsntLikeYou, c_IDontWannaLoseYou);
            DialogTools.CueAddAnswersList(c_IDontWannaLoseYou, L_3);
            DialogTools.AnswerAddNextCue(a_IDontWannaLoseYou, c_Speechless);
            DialogTools.CueAddAnswersList(c_Speechless, L_3);
            DialogTools.AnswerAddNextCue(a_InTooDeep, c_AlrightFine);
            DialogTools.AnswerAddNextCue(a_ImStaying, c_NoWay);
            DialogTools.CueAddContinue(c_AlrightFine, c_IShouldGo);
            DialogTools.CueAddContinue(c_NoWay, c_IShouldGo);

            // Locators
            var Snow_CameraLoc = new FakeLocator(-1.0f, 60.23f, -70.36f, 1.34f);
            var Snow_PlayerStart = new FakeLocator(-6.8808f, 56.0123f, -68.1269f, 84.6274f);
            var Snow_WoljifStart = new FakeLocator(-0.56f, 56.12f, -66.11f, 179.66f);
            var Snow_PlayerEnd = new FakeLocator(-1.43f, 56.12f, -66.56f, 80.99f);
            var Woljif_Exit = new FakeLocator(-8.844f, 56.02f, 0.325f, 275.0469f);

            // Build cutscene stuff
            c_Waiting.TurnSpeaker = false;
            DialogTools.DialogAddStartAction(SnowDialog, ActionTools.StartMusic("RomanceTheme"));
            DialogTools.CueAddOnStopAction(c_IShouldGo, ActionTools.StopMusic());

            // Cutscene 1, to play immediately
            //   Track 0A
            //      Command: Lock Control
            //      EndGate: Gate1
            //   Track 0B
            //      Command: Action(Translocate Player, Translocate Woljif), Move Camera, Delay
            //      EndGate: Gate1
            // Gate1
            //   Track 1A 
            //      Command: Start Dialog
            //      EndGate: None

            // Create Track 1A
            var Track1A = CutsceneTools.CreateTrack(null, CommandTools.StartDialogCommand(SnowDialog, Companions.Woljif));
            // Create Gate 1
            var Gate1 = CutsceneTools.CreateGate("WRM_7_Gate1", Track1A);
            // Create Track 0B
            Kingmaker.ElementsSystem.GameAction[] Actions0B = 
                { 
                    ActionTools.TranslocateAction(Companions.Player, Snow_PlayerStart), 
                    ActionTools.TranslocateAction(Companions.Woljif, Snow_WoljifStart) 
                };
            Kingmaker.AreaLogic.Cutscenes.CommandBase[] Commands0B =
                { CommandTools.ActionCommand("WRM_7_Move0B", Actions0B),
                  CommandTools.CamMoveCommand(Snow_CameraLoc),
                  CommandTools.DelayCommand(0.5f) };
            var Track0B = CutsceneTools.CreateTrack(Gate1, Commands0B);
            // Create Track 0A
            var Track0A = CutsceneTools.CreateTrack(Gate1, CommandTools.LockControlCommand());
            // Create Cutscene 1
            Kingmaker.AreaLogic.Cutscenes.Track[] Tracks0 = { Track0A, Track0B };
            var SnowCutscene = CutsceneTools.CreateCutscene("WRM_7_SnowCutscene1", false, Tracks0);

            // Add animation to C_ThereYouAre
            c_ThereYouAre.Animation = Kingmaker.DialogSystem.Blueprints.DialogAnimation.Wave;

            // Cutscene 2, to play after C_ThereYouAre
            //   Track 2A
            //      Command: Action (Player walks over)
            //      EndGate: Gate2
            //   Track 2B
            //      Command: Lock Controls
            //      EndGate: Gate2
            //   Track 2C
            //      Command: Camera follow
            //      EndGate: Gate2
            var Gate2 = CutsceneTools.CreateGate("WRM_7_Gate2");
            var Track2A = CutsceneTools.CreateTrack(Gate2, CommandTools.WalkCommand("WRM_PlayerWalkToWoljif", Companions.Player, Snow_PlayerEnd));
            var Track2B = CutsceneTools.CreateTrack(Gate2, CommandTools.LockControlCommand());
            var Track2C = CutsceneTools.CreateTrack(Gate2, CommandTools.CamFollowCommand(Companions.Player));
            Kingmaker.AreaLogic.Cutscenes.Track[] Tracks2 = { Track2A, Track2B, Track2C };
            var WalkCutscene = CutsceneTools.CreateCutscene("WRM_7_SnowWalk", false, Tracks2);
            DialogTools.CueAddOnStopAction(c_ThereYouAre, ActionTools.PlayCutsceneAction(WalkCutscene));

            // Add headscratch to C_IDidntMakeThem
            c_IDidntMakeThem.Animation = Kingmaker.DialogSystem.Blueprints.DialogAnimation.HeadScratch;

            // Add intense reaction to c_NoWay
            c_NoWay.Animation = Kingmaker.DialogSystem.Blueprints.DialogAnimation.Angry;

            Kingmaker.ElementsSystem.GameAction[] afterteleport =
                {
                    ActionTools.SkipToTimeAction("Night"),
                    ActionTools.SetWeatherAction("moderate"),
                    ActionTools.PlayCutsceneAction(SnowCutscene)
                };
            var teleportparty = ActionTools.TeleportAction("51ec615b45183294bb9b065d9a913e99", ActionTools.MakeList(afterteleport));
            DialogTools.CueAddOnStopAction(Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("WRM_7a_c_Nod"), teleportparty);

            // Move Woljif back where he belongs when done
            // Cutscene 3
            //   Track 3A:
            //      Command: Lock control
            //      EndGate: Gate 3
            //   Track 3B:
            //      Command: Fadeout
            //      EndGate: Gate 3
            //   Track 3C:
            //      Command: Delay, Action (translocate Woljif)
            //      Endgate: Gate 3
            // Gate 3: Empty.

            var gate3 = CutsceneTools.CreateGate("WRM_7_Gate3");
            var Track3A = CutsceneTools.CreateTrack(gate3, CommandTools.LockControlCommand());
            var Track3B = CutsceneTools.CreateTrack(gate3, CommandTools.FadeoutCommand());
            Kingmaker.AreaLogic.Cutscenes.CommandBase[] Commands3C =
                {
                    CommandTools.DelayCommand(0.5f),
                    CommandTools.ActionCommand("WRM_7_MoveWJBack", ActionTools.TranslocateAction(Companions.Woljif, Woljif_Exit))
                };
            var Track3C = CutsceneTools.CreateTrack(gate3, Commands3C);
            Kingmaker.AreaLogic.Cutscenes.Track[] Tracks3 = { Track3A, Track3B, Track3C };
            var EndCutscene = CutsceneTools.CreateCutscene("WRM_7_SnowCutsceneEnd", false, Tracks3);
            DialogTools.CueAddOnStopAction(c_IShouldGo, ActionTools.PlayCutsceneAction(EndCutscene));
        }

        public static void AddConfessionInvite()
        {
            var romancebase = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomance");
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceActive");
            var Act5 = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("faf70d5071e52d145bfb686244200452");

            // Create invitation dialog
            var C_IGottaTellYou = DialogTools.CreateCue("WRM_8a_c_IGottaTellYou");
            var A_ComeBackLater = DialogTools.CreateAnswer("WRM_8a_a_ComeBackLater");
            var A_TellMeNow = DialogTools.CreateAnswer("WRM_8a_a_TellMeNow");
            var C_Right = DialogTools.CreateCue("WRM_8a_c_Right");
            var C_Rejected = DialogTools.CreateCue("WRM_8a_c_Rejected");
            var InviteDialog = DialogTools.CreateDialog("WRM_8a_InvitationDialog", C_IGottaTellYou);
            var L_1 = DialogTools.CreateAnswersList("WRM_8a_L_1");

            DialogTools.CueAddAnswersList(C_IGottaTellYou, L_1);
            DialogTools.ListAddAnswer(L_1, A_ComeBackLater);
            DialogTools.ListAddAnswer(L_1, A_TellMeNow);
            DialogTools.AnswerAddOnSelectAction(A_TellMeNow, ActionTools.CompleteEtudeAction(romanceactive));
            DialogTools.AnswerAddNextCue(A_ComeBackLater, C_Right);
            DialogTools.AnswerAddNextCue(A_TellMeNow, C_Rejected);

            // Create the mechanics of the scene
            var NotificationEtude = EtudeTools.CreateEtude("WRM_ConfessionInvite_Notification", romancebase, false, false);
            var EventEtude = EtudeTools.CreateEtude("WRM_ConfessionInvite_Event", NotificationEtude, true, true);
            var Notification = EventTools.CreateCommandRoomEvent("WRM_note_8a_Name", "WRM_note_8a_Desc");
            var NotificationCounter = EtudeTools.CreateFlag("WRM_ConfessionInviteFlag");
            EventTools.AddResolution(Notification, Kingmaker.Kingdom.Blueprints.EventResult.MarginType.Fail, "WRM_note_8a_Ignored");
            EventTools.AddResolution(Notification, Kingmaker.Kingdom.Blueprints.EventResult.MarginType.Success, "WRM_note_8a_Complete");

            //EtudeTools.EtudeAddOnPlayTrigger(NotificationEtude, ActionTools.MakeList(ActionTools.StartCommandRoomEventAction(Notification)));
            Kingmaker.ElementsSystem.GameAction[] StartNotification = 
                {
                    ActionTools.IncrementFlagAction(NotificationCounter),
                    ActionTools.ConditionalAction(ConditionalTools.CreateFlagCheck("WRM_DontDoubleConfession",NotificationCounter, 2, 1000000, true))
                };
            ActionTools.ConditionalActionOnTrue((Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)StartNotification[1], ActionTools.StartCommandRoomEventAction(Notification));
            EtudeTools.EtudeAddOnPlayTrigger(NotificationEtude, ActionTools.MakeList(StartNotification));
            EtudeTools.EtudeAddOnDeactivateTrigger(NotificationEtude, ActionTools.MakeList(ActionTools.EndCommandRoomEventAction(Notification)));
            var Capital_KTC_group = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>("10d01be767521a340978c8e57ab536b6");
            var Capital_WoljifCompanion_group = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>("97e2c4ec46765f143bf21bc9578b22f7");
            EtudeTools.EtudeAddConflictingGroups(EventEtude, Capital_KTC_group);
            EtudeTools.EtudeAddConflictingGroups(EventEtude, Capital_WoljifCompanion_group);
            EtudeTools.EtudeAddActivationCondition(EventEtude, ConditionalTools.CreateEtudeGroupCondition("WRM_ConfessionTrigger", Capital_KTC_group, true));
            EtudeTools.EtudeAddActivationCondition(EventEtude, ConditionalTools.CreateEtudeCondition("WRM_ConfessionRomance", romanceactive, "playing"));

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



            // Timer
            var SnowDialog = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("WRM_7b_c_IShouldGo");
            var ConfessionTimer = EtudeTools.CreateEtude("WRM_TimerBeforeConfession", romanceactive, false, false);
            Kingmaker.ElementsSystem.GameAction[] delayedactions = { ActionTools.StartEtudeAction(EventEtude), ActionTools.CompleteEtudeAction(ConfessionTimer) };
            //EtudeTools.EtudeAddDelayedAction(ConfessionTimer, 10, ActionTools.MakeList(delayedactions));
            // DEBUG VERSION WITH SHORTER TIMER
            EtudeTools.EtudeAddDelayedAction(ConfessionTimer, 2, ActionTools.MakeList(delayedactions));

            DialogTools.CueAddOnStopAction(SnowDialog, ActionTools.StartEtudeAction(ConfessionTimer));
        }

        public static void AddConfessionScene()
        {
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceActive");

            // Build Dialog Tree
            var C_Waiting = DialogTools.CreateCue("WRM_8b_c_Waiting");
            var C_TooLate = DialogTools.CreateCue("WRM_8b_c_TooLate");
            var C_Crush = DialogTools.CreateCue("WRM_8b_c_Crush");
            var L_InitialResponse = DialogTools.CreateAnswersList("WRM_8b_L_InitialResponse");
            var A_JustALittle = DialogTools.CreateAnswer("WRM_8b_a_JustALittle");
            var A_Crush = DialogTools.CreateAnswer("WRM_8b_a_Crush");
            var A_FeelTheSame = DialogTools.CreateAnswer("WRM_8b_a_FeelTheSame");
            var A_Friendzone = DialogTools.CreateAnswer("WRM_8b_a_Friendzone");
            var C_Balcony = DialogTools.CreateCue("WRM_8b_c_Balcony");
            var C_Really = DialogTools.CreateCue("WRM_8b_c_Really");
            var C_Friendzoned = DialogTools.CreateCue("WRM_8b_c_Friendzoned");
            var L_Romance = DialogTools.CreateAnswersList("WRM_8b_L_Romance");
            var A_ILoveYou = DialogTools.CreateAnswer("WRM_8b_a_ILoveYou");
            var A_Kiss = DialogTools.CreateAnswer("WRM_8b_a_Kiss");
            var A_StayWithMe = DialogTools.CreateAnswer("WRM_8b_a_StayWithMe");
            var C_LoveAfterKiss = DialogTools.CreateCue("WRM_8b_c_LoveAfterKiss");
            var C_LoveBeforeKiss = DialogTools.CreateCue("WRM_8b_c_LoveBeforeKiss");
            var C_TieflingKiss1 = DialogTools.CreateCue("WRM_8b_c_TieflingKiss1");
            var C_TieflingKiss2 = DialogTools.CreateCue("WRM_8b_c_TieflingKiss2");
            var C_Kiss1 = DialogTools.CreateCue("WRM_8b_c_Kiss1");
            var C_Kiss2 = DialogTools.CreateCue("WRM_8b_c_Kiss2");
            var C_Surprised = DialogTools.CreateCue("WRM_8b_c_Surprised");
            var L_Ace = DialogTools.CreateAnswersList("WRM_8b_L_Ace");
            var A_FirstTime = DialogTools.CreateAnswer("WRM_8b_a_FirstTime");
            var A_SomethingWrong = DialogTools.CreateAnswer("WRM_8b_a_SomethingWrong");
            var C_FirstTime = DialogTools.CreateCue("WRM_8b_c_FirstTime");
            var C_NothingWrong = DialogTools.CreateCue("WRM_8b_c_NothingWrong");
            var C_NeverThoughtAboutIt = DialogTools.CreateCue("WRM_8b_c_NeverThoughtAboutIt");
            var L_Sexuality = DialogTools.CreateAnswersList("WRM_8b_L_Sexuality");
            var A_NotGonnaWork = DialogTools.CreateAnswer("WRM_8b_a_NotGonnaWork", true);
            var A_LetMeShowYou = DialogTools.CreateAnswer("WRM_8b_a_LetMeShowYou");
            var A_YouSure = DialogTools.CreateAnswer("WRM_8b_a_YouSure");
            var A_MyFirstTimeToo = DialogTools.CreateAnswer("WRM_8b_a_MyFirstTimeToo");
            var A_IdLikeToTry = DialogTools.CreateAnswer("WRM_8b_a_IdLikeToTry");
            var A_NoThanks = DialogTools.CreateAnswer("WRM_8b_a_NoThanks");
            var C_WhatElseDoYouWant = DialogTools.CreateCue("WRM_8b_c_WhatElseDoYouWant");
            var L_Incompatible = DialogTools.CreateAnswersList("WRM_8b_L_Incompatible");
            var A_Misunderstood = DialogTools.CreateAnswer("WRM_8b_a_Misunderstood");
            var C_Relief = DialogTools.CreateCue("WRM_8b_c_Relief");
            var A_YouBastard = DialogTools.CreateAnswer("WRM_8b_a_YouBastard");
            var C_YouBrokeHisHeart = DialogTools.CreateCue("WRM_8b_c_YouBrokeHisHeart");
            var C_ChangeMyMind = DialogTools.CreateCue("WRM_8b_c_ChangeMyMind");
            var C_ImSure = DialogTools.CreateCue("WRM_8b_c_ImSure");
            var L_HesSure = DialogTools.CreateAnswersList("WRM_8b_L_HesSure");
            var A_IfYouSaySo = DialogTools.CreateAnswer("WRM_8b_a_IfYouSaySo");
            var A_WaitForNow = DialogTools.CreateAnswer("WRM_8b_a_WaitForNow");
            var A_ThatsOkay = DialogTools.CreateAnswer("WRM_8b_a_ThatsOkay");
            var C_JustAsk = DialogTools.CreateCue("WRM_8b_c_JustAsk");
            var C_LetsFindOut = DialogTools.CreateCue("WRM_8b_c_LetsFindOut");
            var C_ThatsFineToo = DialogTools.CreateCue("WRM_8b_c_ThatsFineToo");
            var ConfessionDialog = DialogTools.CreateDialog("WRM_8b_Confession", C_Waiting);

            DialogTools.CueAddContinue(C_Waiting, C_TooLate);
            DialogTools.CueAddContinue(C_TooLate, C_Crush);
            DialogTools.CueAddAnswersList(C_Crush, L_InitialResponse);

            DialogTools.ListAddAnswer(L_InitialResponse, A_JustALittle);
            DialogTools.ListAddAnswer(L_InitialResponse, A_Crush);
            DialogTools.ListAddAnswer(L_InitialResponse, A_FeelTheSame);
            DialogTools.ListAddAnswer(L_InitialResponse, A_Friendzone);
            DialogTools.AnswerAddOnSelectAction(A_Friendzone, ActionTools.CompleteEtudeAction(romanceactive));
            DialogTools.AnswerAddNextCue(A_Friendzone, C_Friendzoned);
            DialogTools.AnswerAddNextCue(A_JustALittle, C_Balcony);
            DialogTools.CueAddAnswersList(C_Balcony, L_InitialResponse);
            DialogTools.AnswerAddShowCondition(A_Crush, ConditionalTools.CreateCueSeenCondition("WRM_HaventAdmittedLove", true, C_Balcony));
            DialogTools.AnswerAddShowCondition(A_FeelTheSame, ConditionalTools.CreateCueSeenCondition("WRM_HaveAdmittedLove", C_Balcony));
            DialogTools.AnswerAddNextCue(A_Crush, C_Really);
            DialogTools.AnswerAddNextCue(A_FeelTheSame, C_Really);
            DialogTools.CueAddAnswersList(C_Really, L_Romance);

            DialogTools.ListAddAnswer(L_Romance, A_ILoveYou);
            DialogTools.ListAddAnswer(L_Romance, A_Kiss);
            DialogTools.ListAddAnswer(L_Romance, A_StayWithMe);
            DialogTools.AnswerAddNextCue(A_ILoveYou, C_LoveAfterKiss);
            DialogTools.AnswerAddNextCue(A_ILoveYou, C_LoveBeforeKiss);
            DialogTools.CueAddCondition(C_LoveAfterKiss, ConditionalTools.CreateAnswerSelectedCondition("WRM_AlreadyKissed", A_Kiss));
            DialogTools.CueAddAnswersList(C_LoveAfterKiss, L_Romance);
            DialogTools.CueAddAnswersList(C_LoveBeforeKiss, L_Romance);
            DialogTools.AnswerAddNextCue(A_Kiss, C_TieflingKiss1);
            DialogTools.AnswerAddNextCue(A_Kiss, C_Kiss1);
            DialogTools.CueAddCondition(C_TieflingKiss1, ConditionalTools.CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PcRace>("isplayertiefling", bp => { bp.Race = Kingmaker.Blueprints.Race.Tiefling; }));
            DialogTools.CueAddContinue(C_TieflingKiss1, C_TieflingKiss2);
            DialogTools.CueAddContinue(C_Kiss1, C_Kiss2);
            DialogTools.CueAddAnswersList(C_TieflingKiss2, L_Romance);
            DialogTools.CueAddAnswersList(C_Kiss2, L_Romance);
            DialogTools.AnswerAddNextCue(A_StayWithMe, C_Surprised);

            DialogTools.CueAddAnswersList(C_Surprised, L_Ace);
            DialogTools.ListAddAnswer(L_Ace, A_FirstTime);
            DialogTools.ListAddAnswer(L_Ace, A_SomethingWrong);
            DialogTools.AnswerAddNextCue(A_FirstTime, C_FirstTime);
            DialogTools.AnswerAddNextCue(A_SomethingWrong, C_NothingWrong);
            DialogTools.CueAddContinue(C_FirstTime, C_NeverThoughtAboutIt);
            DialogTools.CueAddContinue(C_NothingWrong, C_NeverThoughtAboutIt);
            DialogTools.CueAddAnswersList(C_NeverThoughtAboutIt, L_Sexuality);

            DialogTools.ListAddAnswer(L_Sexuality, A_NotGonnaWork);
            DialogTools.ListAddAnswer(L_Sexuality, A_LetMeShowYou);
            DialogTools.ListAddAnswer(L_Sexuality, A_YouSure);
            DialogTools.ListAddAnswer(L_Sexuality, A_MyFirstTimeToo);
            DialogTools.ListAddAnswer(L_Sexuality, A_IdLikeToTry);
            DialogTools.ListAddAnswer(L_Sexuality, A_NoThanks);
            DialogTools.AnswerAddNextCue(A_NotGonnaWork, C_WhatElseDoYouWant);
            DialogTools.AnswerAddNextCue(A_LetMeShowYou, C_ChangeMyMind);
            DialogTools.AnswerAddNextCue(A_YouSure, C_ImSure);
            DialogTools.AnswerAddNextCue(A_MyFirstTimeToo, C_LetsFindOut);
            DialogTools.AnswerAddNextCue(A_IdLikeToTry, C_LetsFindOut);
            DialogTools.AnswerAddNextCue(A_NoThanks, C_ThatsFineToo);

            DialogTools.CueAddAnswersList(C_WhatElseDoYouWant, L_Incompatible);
            DialogTools.ListAddAnswer(L_Incompatible, A_Misunderstood);
            DialogTools.AnswerAddNextCue(A_Misunderstood, C_Relief);
            DialogTools.CueAddAnswersList(C_Relief, L_Sexuality);
            DialogTools.ListAddAnswer(L_Incompatible, A_YouBastard);
            DialogTools.AnswerAddNextCue(A_YouBastard, C_YouBrokeHisHeart);
            DialogTools.AnswerAddOnSelectAction(A_YouBastard, ActionTools.CompleteEtudeAction(romanceactive)); // You heartless monster.

            DialogTools.CueAddAnswersList(C_ImSure, L_HesSure);
            DialogTools.ListAddAnswer(L_HesSure, A_IfYouSaySo);
            DialogTools.ListAddAnswer(L_HesSure, A_WaitForNow);
            DialogTools.ListAddAnswer(L_HesSure, A_ThatsOkay);
            DialogTools.AnswerAddNextCue(A_IfYouSaySo, C_ChangeMyMind);
            DialogTools.AnswerAddNextCue(A_WaitForNow, C_JustAsk);
            DialogTools.AnswerAddNextCue(A_ThatsOkay, C_JustAsk);

            // Set up the etude that will hide everyone in the command room and interrupt the etude that prevents Woljif from moving.
            var CompanionEtude = (Kingmaker.AreaLogic.Etudes.BlueprintEtude)Kingmaker.Blueprints.ResourcesLibrary.TryGetBlueprint(Kingmaker.Blueprints.BlueprintGuid.Parse("14a80c048c8ceed4a9c856d85bbf10da"));
            var ConfessionScenePlaying = EtudeTools.CreateEtude("WRM_ConfessionSceneToggle", CompanionEtude, false, false);
            EtudeTools.EtudeAddConflictingGroups(ConfessionScenePlaying, Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>("97e2c4ec46765f143bf21bc9578b22f7"));
            EtudeTools.EtudeAddConflictingGroups(ConfessionScenePlaying, Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>("10d01be767521a340978c8e57ab536b6"));
            ConfessionScenePlaying.Priority = -99;

            // Could not for the life of me figure out how to get hold of Spawner references, so I couldn't build my own Hide Unit From Spawner.
            // Instead I'm just going to steal the relevant hiders from existing blueprints. Surely this cannot possibly go wrong.
            var IrabethHiderSource = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("48967c3ec4330294ab1f5d24d9b45052");
            var AneviaHiderSource = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("4b441ac2bc063f6439795edc9d98b8a1");
            var GuardHiderSource = Resources.GetBlueprint<Kingmaker.AreaLogic.Cutscenes.CommandAction>("61b837b9998bbcf4cba89f1824fd7974");
            var IrabethHider = Helpers.CreateCopy<Kingmaker.ElementsSystem.GameAction>(((Kingmaker.Designers.EventConditionActionSystem.Events.EtudePlayTrigger)IrabethHiderSource.Components[0]).Actions.Actions[0]);
            ((Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit)IrabethHider).Unhide = false;
            var AneviaHider = Helpers.CreateCopy<Kingmaker.ElementsSystem.GameAction>(((Kingmaker.Designers.EventConditionActionSystem.Events.EtudePlayTrigger)AneviaHiderSource.Components[0]).Actions.Actions[0]);
            ((Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit)AneviaHider).Unhide = false;
            var GuardHider1 = Helpers.CreateCopy<Kingmaker.ElementsSystem.GameAction>(GuardHiderSource.Action.Actions[4]);
            var GuardHider2 = Helpers.CreateCopy<Kingmaker.ElementsSystem.GameAction>(GuardHiderSource.Action.Actions[4]);
            var IrabethUnhider = Helpers.CreateCopy<Kingmaker.ElementsSystem.GameAction>(IrabethHider);
            ((Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit)IrabethUnhider).Unhide = true;
            var AneviaUnhider = Helpers.CreateCopy<Kingmaker.ElementsSystem.GameAction>(AneviaHider);
            ((Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit)AneviaUnhider).Unhide = true;
            var GuardUnhider1 = Helpers.CreateCopy<Kingmaker.ElementsSystem.GameAction>(GuardHider1);
            ((Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit)GuardUnhider1).Unhide = true;
            var GuardUnhider2 = Helpers.CreateCopy<Kingmaker.ElementsSystem.GameAction>(GuardHider2);
            ((Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit)GuardUnhider2).Unhide = true;

            Kingmaker.ElementsSystem.GameAction[] Hiders = { IrabethHider, AneviaHider, GuardHider1, GuardHider2};
            Kingmaker.ElementsSystem.GameAction[] Unhiders = { IrabethUnhider, AneviaUnhider, GuardUnhider1, GuardUnhider2 };
            // Set the etude to use the hiders on start and unhiders on complete.
            EtudeTools.EtudeAddOnPlayTrigger(ConfessionScenePlaying, ActionTools.MakeList(Hiders));
            EtudeTools.EtudeAddCompleteTrigger(ConfessionScenePlaying, ActionTools.MakeList(Unhiders));

            // Locators
            var WoljifStartPosition = new FakeLocator(206.66f, 78.71f, 12.72f, 28.26f);
            var WoljifBalconyPosition = new FakeLocator(210.31f, 78.65f, 20.43f, 8.34f);
            var PlayerBalconyPosition = new FakeLocator(211.63f, 78.65f, 20.60f, 8.34f);
            var BalconyCameraPosition = new FakeLocator(210.28f, 79.07f, 18.5f, 113.01f);

            // Set up cutscene
            DialogTools.DialogAddStartAction(ConfessionDialog, ActionTools.StartMusic("RomanceTheme"));
            DialogTools.DialogAddFinishAction(ConfessionDialog, ActionTools.StopMusic());

            // Cutscene
            //   Track0
            //      Command: Action(Skip time, set confession scene etude)
            //      EndGate: Gate0
            // Gate0
            //   Track 0A
            //      Command: Lock Control
            //      EndGate: Gate2
            //   Track 0B
            //      Command: Action(Translocate Player, Translocate Woljif, unhide Woljif), Move Camera, Delay
            //      EndGate: Gate1
            // Gate1
            //   Track 1A
            //      Command: Woljif walks to player
            //      Endgate: Gate2
            // Gate2
            //   Track 2A 
            //      Command: Start Dialog
            //      EndGate: None

            // Create Track 2A
            var Track2A = CutsceneTools.CreateTrack(null, CommandTools.StartDialogCommand(ConfessionDialog, Companions.Woljif));
            // Create Gate 2
            var Gate2 = CutsceneTools.CreateGate("WRM_8_Gate2", Track2A);
            // Create Track 1A
            var Track1A = CutsceneTools.CreateTrack(Gate2, CommandTools.WalkCommand("WRM_8_Walk", Companions.Woljif, WoljifBalconyPosition));
            // Create Gate 1
            var Gate1 = CutsceneTools.CreateGate("WRM_8_Gate1", Track1A);
            // Create Track 0B
            Kingmaker.ElementsSystem.GameAction[] Actions0B =
                {
                    ActionTools.TranslocateAction(Companions.Player, PlayerBalconyPosition),
                    ActionTools.HideUnitAction(Companions.Woljif,true),
                    ActionTools.TranslocateAction(Companions.Woljif, WoljifStartPosition)
                };
            Kingmaker.AreaLogic.Cutscenes.CommandBase[] Commands0B =
                {
                    CommandTools.ActionCommand("WRM_8_Move0B", Actions0B),
                    CommandTools.CamMoveCommand(BalconyCameraPosition),
                    CommandTools.DelayCommand(0.5f) 
                };
            var Track0B = CutsceneTools.CreateTrack(Gate1, Commands0B);
            // Create Track 0A
            var Track0A = CutsceneTools.CreateTrack(Gate2, CommandTools.LockControlCommand());
            // Create Gate 0
            Kingmaker.AreaLogic.Cutscenes.Track[] Tracks0 = { Track0A, Track0B };
            var Gate0 = CutsceneTools.CreateGate("WRM_8_Gate0", Tracks0);
            // Create Base Track
            Kingmaker.ElementsSystem.GameAction[] BaseActions = { ActionTools.SkipToTimeAction("evening")/*, ActionTools.StartEtudeAction(ConfessionScenePlaying)*/};
            var BaseTrack = CutsceneTools.CreateTrack(Gate0, CommandTools.ActionCommand("WRM_8_SkipTime", BaseActions));
            // Create Cutscene 
            var ConfessionCutscene = CutsceneTools.CreateCutscene("WRM_8_ConfessionCutscene", false, BaseTrack);

            // Make it fade out and end the etude on dialog complete
            //End_Cutscene
            //  End_Track1
            //      Command: Lock controls
            //      EndGate: End_Gate1
            //  End_Track2
            //      Command: Fadeout
            //      EndGate: End_Gate1
            //  End_Track3
            //      Command: Delay
            //      EndGate: End_Gate1
            //End_Gate1
            //  End_Track4
            //      Command Action: End Etude
            //      EndGate: none
            var End_Track4 = CutsceneTools.CreateTrack(null, CommandTools.ActionCommand("WRM_8_EndEtude", ActionTools.CompleteEtudeAction(ConfessionScenePlaying)));
            var End_Gate1 = CutsceneTools.CreateGate("WRM_8_EndGate1",End_Track4);
            var End_Track3 = CutsceneTools.CreateTrack(End_Gate1, CommandTools.DelayCommand(0.5f));
            var End_Track2 = CutsceneTools.CreateTrack(End_Gate1, CommandTools.FadeoutCommand());
            var End_Track1 = CutsceneTools.CreateTrack(End_Gate1, CommandTools.LockControlCommand());
            Kingmaker.AreaLogic.Cutscenes.Track[] EndTracks = { End_Track1, End_Track2, End_Track3 };
            var End_Cutscene = CutsceneTools.CreateCutscene("WRM_8_EndScene", false, EndTracks);
            DialogTools.DialogAddFinishAction(ConfessionDialog, ActionTools.PlayCutsceneAction(End_Cutscene));

            // Link to invitation
            EtudeTools.EtudeAddOnPlayTrigger(ConfessionScenePlaying, ActionTools.MakeList(ActionTools.PlayCutsceneAction(ConfessionCutscene)));
            //DialogTools.CueAddOnStopAction(Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("WRM_8a_c_Right"), ActionTools.PlayCutsceneAction(ConfessionCutscene));
            DialogTools.CueAddOnStopAction(Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("WRM_8a_c_Right"), ActionTools.StartEtudeAction(ConfessionScenePlaying));
        }

        public static void AddBedroomScene()
        {
            var romancecomplete = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceFinished");
            var romancebase = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomance");
            var BarksFlag = EtudeTools.CreateFlag("WRM_BedroomBarksFlag");
            var BarksEtude = EtudeTools.CreateEtude("WRM_BedroomBarksEtude", romancebase, false, false);
            var BedroomSceneEtude = EtudeTools.CreateEtude("WRM_FirstBedroomScenePlaying", romancebase, false, false);
            EtudeTools.EtudeAddConflictingGroups(BedroomSceneEtude, Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>("97e2c4ec46765f143bf21bc9578b22f7"));
            EtudeTools.EtudeAddConflictingGroups(BedroomSceneEtude, Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>("10d01be767521a340978c8e57ab536b6"));


            // Build Dialog Tree
            var C_Cuddles = DialogTools.CreateCue("WRM_9_c_Cuddles");
            var C_GetOuttaHere = DialogTools.CreateCue("WRM_9_c_GetOuttaHereDefault");
            var C_GetOuttaHerePaladin = DialogTools.CreateCue("WRM_9_c_GetOuttaHerePaladin");
            var C_GetOuttaHereInquisitor = DialogTools.CreateCue("WRM_9_c_GetOuttaHereInquisitor");
            var C_GetOuttaHereHellknight = DialogTools.CreateCue("WRM_9_c_GetOuttaHereHellknight");
            var C_GetOuttaHereCleric = DialogTools.CreateCue("WRM_9_c_GetOuttaHereCleric");
            var L_FallingInLove = DialogTools.CreateAnswersList("WRM_9_L_FallingInLove");
            var A_Unexpected = DialogTools.CreateAnswer("WRM_9_a_Unexpected");
            var A_LoveAtFirstSight = DialogTools.CreateAnswer("WRM_9_a_LoveAtFirstSight");
            var C_BestHeist = DialogTools.CreateCue("WRM_9_c_BestHeist");
            var C_IWasntLying = DialogTools.CreateCue("WRM_9_c_IWasntLying");
            var L_NeverEnd = DialogTools.CreateAnswersList("WRM_9_L_NeverEnd");
            var A_StayAWhile = DialogTools.CreateAnswer("WRM_9_a_StayAWhile");
            var A_IPromise = DialogTools.CreateAnswer("WRM_9_a_IPromise");
            var C_Forever = DialogTools.CreateCue("WRM_9_c_Forever");
            var C_HoldYouToThat = DialogTools.CreateCue("WRM_9_c_HoldYouToThat");
            var BedroomDialog = DialogTools.CreateDialog("WRM_9_BedroomDialog", C_Cuddles);

            BedroomDialog.TurnPlayer = false;
            C_Cuddles.TurnSpeaker = false;
            C_GetOuttaHere.TurnSpeaker = false;
            C_GetOuttaHerePaladin.TurnSpeaker = false;
            C_GetOuttaHereInquisitor.TurnSpeaker = false;
            C_GetOuttaHereHellknight.TurnSpeaker = false;
            C_GetOuttaHereCleric.TurnSpeaker = false;
            C_BestHeist.TurnSpeaker = false;
            C_IWasntLying.TurnSpeaker = false;
            C_Forever.TurnSpeaker = false;
            C_HoldYouToThat.TurnSpeaker = false;

            DialogTools.CueAddCondition(C_GetOuttaHerePaladin, ConditionalTools.CreateClassCheck("WRM_9_PlayerPaladin", Resources.GetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec")));
            DialogTools.CueAddCondition(C_GetOuttaHereInquisitor, ConditionalTools.CreateClassCheck("WRM_9_PlayerInquisitor", Resources.GetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce")));
            Kingmaker.ElementsSystem.Condition[] HellknightConditions = 
                {
                    ConditionalTools.CreateClassCheck("WRM_9_PlayerHellknight", Resources.GetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("ed246f1680e667b47b7427d51e651059")),
                    ConditionalTools.CreateClassCheck("WRM_9_PlayerHellknightSignifier", Resources.GetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("ee6425d6392101843af35f756ce7fefd"))
                };
            DialogTools.CueAddCondition(C_GetOuttaHereHellknight, ConditionalTools.CreateLogicCondition("WRM_9_PlayerHellknightLogic", Kingmaker.ElementsSystem.Operation.Or, HellknightConditions));
            Kingmaker.ElementsSystem.Condition[] ClericConditions =
                {
                    ConditionalTools.CreateClassCheck("WRM_9_PlayerCleric", Resources.GetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0")),
                    ConditionalTools.CreateClassCheck("WRM_9_PlayerWarpriest", Resources.GetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("30b5e47d47a0e37438cc5a80c96cfb99"))
                };
            DialogTools.CueAddCondition(C_GetOuttaHereCleric, ConditionalTools.CreateLogicCondition("WRM_9_PlayerClericLogic", Kingmaker.ElementsSystem.Operation.Or, ClericConditions));

            DialogTools.CueAddContinue(C_Cuddles, C_GetOuttaHerePaladin);
            DialogTools.CueAddContinue(C_Cuddles, C_GetOuttaHereInquisitor);
            DialogTools.CueAddContinue(C_Cuddles, C_GetOuttaHereHellknight);
            DialogTools.CueAddContinue(C_Cuddles, C_GetOuttaHereCleric);
            DialogTools.CueAddContinue(C_Cuddles, C_GetOuttaHere);
            DialogTools.CueAddAnswersList(C_GetOuttaHerePaladin, L_FallingInLove);
            DialogTools.CueAddAnswersList(C_GetOuttaHereInquisitor, L_FallingInLove);
            DialogTools.CueAddAnswersList(C_GetOuttaHereHellknight, L_FallingInLove);
            DialogTools.CueAddAnswersList(C_GetOuttaHereCleric, L_FallingInLove);
            DialogTools.CueAddAnswersList(C_GetOuttaHere, L_FallingInLove);

            DialogTools.ListAddAnswer(L_FallingInLove, A_Unexpected);
            DialogTools.ListAddAnswer(L_FallingInLove, A_LoveAtFirstSight);
            DialogTools.AnswerAddNextCue(A_Unexpected, C_BestHeist);
            DialogTools.AnswerAddNextCue(A_LoveAtFirstSight, C_IWasntLying);
            DialogTools.CueAddAnswersList(C_BestHeist, L_NeverEnd);
            DialogTools.CueAddAnswersList(C_IWasntLying, L_NeverEnd);

            DialogTools.ListAddAnswer(L_NeverEnd, A_StayAWhile);
            DialogTools.ListAddAnswer(L_NeverEnd, A_IPromise);
            DialogTools.AnswerAddNextCue(A_StayAWhile, C_Forever);
            DialogTools.AnswerAddNextCue(A_IPromise, C_HoldYouToThat);
            DialogTools.CueAddOnShowAction(C_Forever, ActionTools.StartEtudeAction(romancecomplete));
            DialogTools.CueAddOnShowAction(C_HoldYouToThat, ActionTools.StartEtudeAction(romancecomplete));

            // Locators
            var WoljifLocation = new FakeLocator(185.46f, 79.7f, -15.36f, 184.29f);
            var PlayerLocation = new FakeLocator(186f, 79.7f, -15.55f, 184.29f);
            var BedroomCameraPosition = new FakeLocator(184.44f, 79.07f, -14.646f, 159.45f);

            // Not sure how to get hold of a reference to the Rest Icon, so I'm gonna steal the appropriate action from Lann's romance.
            /*var LannSource = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("b2fdd13c27e90f542b03ce7b4343cc50");
            var HideButton = ((Kingmaker.Designers.EventConditionActionSystem.Events.EtudePlayTrigger)LannSource.Components[0]).Actions.Actions[1];
            var UnhideButton = ((Kingmaker.Designers.EventConditionActionSystem.Events.DeactivateTrigger)LannSource.Components[1]).Actions.Actions[1];*/

            // Build cutscene
            DialogTools.DialogAddStartAction(BedroomDialog, ActionTools.StartMusic("RomanceTheme"));
            DialogTools.DialogAddFinishAction(BedroomDialog, ActionTools.StopMusic());

            // Cutscene
            //   Track 0A
            //      Command: Lock Control
            //      EndGate: Gate1
            //   Track 0B
            //      Command: Action(Translocate Player, Translocate Woljif), Move Camera, Delay
            //      EndGate: Gate1
            //   Track 0C
            //      Command: Animate Player
            //      EndGate: none
            //   Track 0D
            //      Command: Animate Woljif
            //      EndGate: none
            // Gate1
            //   Track 1A
            //      Command: Start Dialog
            //      EndGate: none
            var Track1A = CutsceneTools.CreateTrack(null, CommandTools.StartDialogCommand(BedroomDialog, Companions.Woljif));
            var Gate1 = CutsceneTools.CreateGate("WRM_9_Gate1", Track1A);
            var Track0D = CutsceneTools.CreateTrack(null, CommandTools.GenericAnimationCommand("WRM_9_AnimateWoljif", "4b58b2de5f04ddf4f9012d4bc342b968", Companions.Woljif));
            var Track0C = CutsceneTools.CreateTrack(null, CommandTools.GenericAnimationCommand("WRM_9_AnimatePlayer", "4b58b2de5f04ddf4f9012d4bc342b968", Companions.Player));
            Kingmaker.ElementsSystem.GameAction[] Actions0B =
                {
                    ActionTools.SetFlyHeightAction(Companions.Player, 0.85f),
                    ActionTools.SetFlyHeightAction(Companions.Woljif, 0.85f),
                    ActionTools.TranslocateAction(Companions.Player, PlayerLocation),
                    ActionTools.TranslocateAction(Companions.Woljif, WoljifLocation),
                    ActionTools.HideWeaponsAction(Companions.Player),
                    ActionTools.HideWeaponsAction(Companions.Woljif)
                };
            Kingmaker.AreaLogic.Cutscenes.CommandBase[] Commands0B =
                { CommandTools.ActionCommand("WRM_9_Move0B", Actions0B),
                  CommandTools.CamMoveCommand(BedroomCameraPosition),
                  CommandTools.DelayCommand(0.5f) };
            var Track0B = CutsceneTools.CreateTrack(Gate1, Commands0B);
            var Track0A = CutsceneTools.CreateTrack(Gate1, CommandTools.LockControlCommand());
            Kingmaker.AreaLogic.Cutscenes.Track[] Tracks = { Track0A, Track0B, Track0C, Track0D };
            var BedroomCutscene = CutsceneTools.CreateCutscene("WRM_9_BedroomScene", false, Tracks);

            DialogTools.DialogAddFinishAction(BedroomDialog, ActionTools.SetFlyHeightAction(Companions.Player, 0f));
            DialogTools.DialogAddFinishAction(BedroomDialog, ActionTools.SetFlyHeightAction(Companions.Woljif, 0f));
            DialogTools.DialogAddFinishAction(BedroomDialog, ActionTools.StopCutsceneAction(BedroomCutscene));
            DialogTools.DialogAddFinishAction(BedroomDialog, ActionTools.UnlockFlagAction(BarksFlag));
            DialogTools.DialogAddFinishAction(BedroomDialog, ActionTools.CompleteEtudeAction(BedroomSceneEtude));
            DialogTools.DialogAddFinishAction(BedroomDialog, ActionTools.StartEtudeAction(BarksEtude));
            // Unhide Rest Icon

            // Trigger Bedroom scene after confession
            var Cue1 = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("WRM_8b_c_ChangeMyMind");
            var Cue2 = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("WRM_8b_c_LetsFindOut");
            var Cue3 = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("WRM_8b_c_JustAsk");
            var Cue4 = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("WRM_8b_c_ThatsFineToo");
            DialogTools.CueAddOnStopAction(Cue1, ActionTools.StartEtudeAction(BedroomSceneEtude));
            DialogTools.CueAddOnStopAction(Cue2, ActionTools.StartEtudeAction(BedroomSceneEtude));
            DialogTools.CueAddOnStopAction(Cue3, ActionTools.StartEtudeAction(BedroomSceneEtude));
            DialogTools.CueAddOnStopAction(Cue4, ActionTools.StartEtudeAction(BedroomSceneEtude));
            DialogTools.CueAddOnStopAction(Cue1, ActionTools.PlayCutsceneAction(BedroomCutscene));
            DialogTools.CueAddOnStopAction(Cue2, ActionTools.PlayCutsceneAction(BedroomCutscene));
            DialogTools.CueAddOnStopAction(Cue3, ActionTools.PlayCutsceneAction(BedroomCutscene));
            DialogTools.CueAddOnStopAction(Cue4, ActionTools.PlayCutsceneAction(BedroomCutscene));
        }

        public static void AddBedroomBarks()
        {
            var romancebase = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomance");
            var WoljifDialog = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintDialog>("8a38eeddc0215a84ca441439bb96b8f4");

            var BarksFlag = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_BedroomBarksFlag");
            var BarksEtude = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_BedroomBarksEtude");
            EtudeTools.EtudeAddConflictingGroups(BarksEtude, Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup>("97e2c4ec46765f143bf21bc9578b22f7"));
            EtudeTools.EtudeAddActivationCondition(BarksEtude, ConditionalTools.CreateFlagLockCheck("WRM_TurnOnBarksCond", BarksFlag, false));

            var PlayerBedroomLocation = new FakeLocator(183.91f, 78.691f, -15.50f, 274.26f);
            var WoljifBedroomLocation = new FakeLocator(183.76f, 78.691f, -14.49f, 274.26f);
            var BedroomCameraPosition = new FakeLocator(184.44f, 79.07f, -14.646f, 159.45f);
            var Woljif_Exit = new FakeLocator(-8.844f, 56.02f, 0.325f, 275.0469f);

            //On Etude activation, teleport WJ + PC, unhide them, move camera, fadeout
            var movePlayer = ActionTools.TranslocateAction(Companions.Player, PlayerBedroomLocation);
            var moveWoljif = ActionTools.TranslocateAction(Companions.Woljif, WoljifBedroomLocation);
            var movecamtrack = CutsceneTools.CreateTrack(null, CommandTools.CamMoveCommand(BedroomCameraPosition));
            var movecamcutscene = CutsceneTools.CreateCutscene("WRM_MoveCameraToBedroom", false, movecamtrack);
            var fadecutscene = Resources.GetBlueprint<Kingmaker.AreaLogic.Cutscenes.Cutscene>("4d0821789698d264bad86ac40bf785d2");
            Kingmaker.ElementsSystem.GameAction[] OnPlay =
                {
                    ActionTools.SkipTimeAction(120),
                    movePlayer,
                    moveWoljif,
                    ActionTools.HideWeaponsAction(Companions.Player),
                    ActionTools.HideWeaponsAction(Companions.Woljif),
                    ActionTools.PlayCutsceneAction(movecamcutscene),
                    ActionTools.PlayCutsceneAction(fadecutscene)
                };
            //EtudeTools.EtudeAddOnPlayTrigger(BarksEtude, ActionTools.MakeList(ActionTools.TeleportAction("ab3b5c105893562488ae5bb6e7b0cba7", ActionTools.MakeList(OnPlay))));
            EtudeTools.EtudeAddOnPlayTrigger(BarksEtude, ActionTools.MakeList(OnPlay));


            //On Etude deactivate, move WJ back to normal location, lock the Flag.
            Kingmaker.ElementsSystem.GameAction[] OnDeactivate =
                {
                    ActionTools.HideWeaponsAction(Companions.Player, false),
                    ActionTools.HideWeaponsAction(Companions.Woljif, false),
                    ActionTools.TranslocateAction(Companions.Woljif, Woljif_Exit),
                    ActionTools.LockFlagAction(BarksFlag)
                };
            EtudeTools.EtudeAddOnDeactivateTrigger(BarksEtude, ActionTools.MakeList(OnDeactivate));
            //On rest, move WJ back to normal location, lock the Flag.
            EtudeTools.EtudeAddOnRestTrigger(BarksEtude, ActionTools.MakeList(OnDeactivate));

            // Alter Woljif's main Dialog tree when Flag is unlocked.
            var BarkConditional = ActionTools.ConditionalAction(ConditionalTools.CreateFlagLockCheck("WRM_EnableBedroomBarks", BarksFlag, false));
            Kingmaker.ElementsSystem.GameAction[] BarkActions = 
                {
                    ActionTools.BarkAction("WRM_Barks_1", Companions.Woljif),
                    ActionTools.BarkAction("WRM_Barks_2", Companions.Woljif),
                    ActionTools.BarkAction("WRM_Barks_3", Companions.Woljif),
                    ActionTools.BarkAction("WRM_Barks_4", Companions.Woljif),
                    ActionTools.BarkAction("WRM_Barks_5", Companions.Woljif)
                };
            ActionTools.ConditionalActionOnTrue(BarkConditional, ActionTools.MakeList(ActionTools.RandomAction(BarkActions)));
            DialogTools.DialogAddReplaceAction(WoljifDialog, BarkConditional);
            DialogTools.DialogAddCondition(WoljifDialog, ConditionalTools.CreateFlagLockCheck("WRM_SwitchToBarks", BarksFlag, true));
        }

        public static void ChangeDialogWhenRomanced()
        {
            var successfulromance = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceFinished");
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceActive");
            var BarksFlag = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_BedroomBarksFlag");
            var BarksEtude = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_BedroomBarksEtude");
            // Pull existing parts
            var MainDialog = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintDialog>("8a38eeddc0215a84ca441439bb96b8f4");
            var AnswerList0003 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("e41585da330233143b34ef64d7d62d69");
            var AnswerList0010 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("aa9ab2093f0209949997379208cfbf41");
            var Answer0005 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("c08e22b2fa03b934aa888d16f22c083e");
            var Answer0007 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("848846faac181d34bbaf9a26766df8b9");
            var Answer0009 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("b9c26621aff1d8243b5f43bb2efeff80");
            var Answer0011 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("0249aec9ddf1b1f4a9dacbd7b870c58b");
            var Answer0012 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("104e5d75cd7ef9c48a107cf5ecd23d94");
            var Cue0006 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("ac10d2c0d42be624dab89e4531e87e64");
            var Cue0014 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("03052f736f889cd4c9c240fdeca11e51");
            // Make new parts
            var c_Greeting = DialogTools.CreateCue("WRM_Main_c_Greeting");
            var a_Breakup = DialogTools.CreateAnswer("WRM_Main_a_Breakup");
            var a_TogetherTime = DialogTools.CreateAnswer("WRM_Main_a_TogetherTime");
            var c_Vendor = DialogTools.CreateCue("WRM_Main_c_Vendor");
            var c_Breakup = DialogTools.CreateCue("WRM_Main_c_Breakup");
            var c_TogetherTime = DialogTools.CreateCue("WRM_Main_c_TogetherTime");
            var c_Dismiss = DialogTools.CreateCue("WRM_Main_c_Dismiss");
            var c_Goodbye = DialogTools.CreateCue("WRM_Main_c_Goodbye");
            var a_BreakupCancel = DialogTools.CreateAnswer("WRM_Main_a_BreakupCancel");
            var a_BreakupConfirm = DialogTools.CreateAnswer("WRM_Main_a_BreakupConfirm");
            var c_BreakupCancel = DialogTools.CreateCue("WRM_Main_c_BreakupCancel");
            var c_BreakupConfirm = DialogTools.CreateCue("WRM_Main_c_BreakupConfirm");
            var c_DismissCancel = DialogTools.CreateCue("WRM_Main_c_DismissCancel");
            var c_DismissConfirm = DialogTools.CreateCue("WRM_Main_c_DismissConfirm");
            var L_Breakup = DialogTools.CreateAnswersList("WRM_Main_L_Breakup");
            // Splice them together
            DialogTools.CueAddCondition(c_Greeting, ConditionalTools.CreateEtudeCondition("WRM_GreetingIfRomanced", successfulromance, "playing"));
            DialogTools.DialogInsertCue(MainDialog, c_Greeting, 0);
            DialogTools.CueAddAnswersList(c_Greeting, AnswerList0003);

            DialogTools.CueAddCondition(c_Vendor, ConditionalTools.CreateEtudeCondition("WRM_VendorIfRomanced", successfulromance, "playing"));
            DialogTools.AnswerAddNextCue(Answer0005, c_Vendor, 0);
            c_Vendor.OnStop = Cue0006.OnStop;

            DialogTools.CueAddCondition(c_Dismiss, ConditionalTools.CreateEtudeCondition("WRM_DismissBaseIfRomanced", successfulromance, "playing"));
            DialogTools.AnswerAddNextCue(Answer0007, c_Dismiss, 0);
            DialogTools.CueAddAnswersList(c_Dismiss, AnswerList0010);

            DialogTools.CueAddCondition(c_DismissCancel, ConditionalTools.CreateEtudeCondition("WRM_DismissCancelIfRomanced", successfulromance, "playing"));
            DialogTools.AnswerAddNextCue(Answer0011, c_DismissCancel, 0);
            DialogTools.CueAddAnswersList(c_DismissCancel, AnswerList0003);

            DialogTools.CueAddCondition(c_DismissConfirm, ConditionalTools.CreateEtudeCondition("WRM_DismissConfirmIfRomanced", successfulromance, "playing"));
            DialogTools.AnswerAddNextCue(Answer0012, c_DismissConfirm, 0);
            c_DismissConfirm.OnStop = Cue0014.OnStop;

            DialogTools.AnswerAddShowCondition(a_Breakup, ConditionalTools.CreateEtudeCondition("WRM_ShowBreakupIfRomanced", successfulromance, "playing"));
            DialogTools.ListAddAnswer(AnswerList0003, a_Breakup, 10);
            DialogTools.AnswerAddNextCue(a_Breakup, c_Breakup);
            DialogTools.CueAddAnswersList(c_Breakup, L_Breakup);
            DialogTools.ListAddAnswer(L_Breakup, a_BreakupCancel);
            DialogTools.ListAddAnswer(L_Breakup, a_BreakupConfirm);
            DialogTools.AnswerAddNextCue(a_BreakupCancel, c_BreakupCancel);
            DialogTools.CueAddAnswersList(c_BreakupCancel, AnswerList0003);
            DialogTools.AnswerAddNextCue(a_BreakupConfirm, c_BreakupConfirm);
            DialogTools.AnswerAddOnSelectAction(a_BreakupConfirm, ActionTools.CompleteEtudeAction(romanceactive));

            DialogTools.AnswerAddShowCondition(a_TogetherTime, ConditionalTools.CreateEtudeCondition("WRM_ShowTogetherTimeIfRomanced", successfulromance, "playing"));
            DialogTools.ListAddAnswer(AnswerList0003, a_TogetherTime, 11);
            DialogTools.AnswerAddNextCue(a_TogetherTime, c_TogetherTime);
            DialogTools.CueAddOnStopAction(c_TogetherTime, ActionTools.UnlockFlagAction(BarksFlag));
            // I don't know why, but even though the translocate/teleport is handled in the etude, it doesn't work properly unless I put it here too.
            var WoljifBedroomLocation = new FakeLocator(183.76f, 78.691f, -14.49f, 274.26f);
            DialogTools.CueAddOnStopAction(c_TogetherTime, ActionTools.TeleportAction("ab3b5c105893562488ae5bb6e7b0cba7", ActionTools.MakeList(ActionTools.TranslocateAction(Companions.Woljif, WoljifBedroomLocation))));
        }

        // HERE BE DRAGONS. This part was cobbled together with the bare minimum of effort in order to not break games, without much regard for actual quality.
        // Most of the animations and special effects don't work, and I can't be bothered to fix them right now.
        // Honestly, I'm not even sure the mechanics fully work. All I tested for was that this sequence didn't immediately crash the game.
        // YOU HAVE BEEN WARNED.
        public static void AlterLichScene()
        {
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceActive");

            // Alter Cue 0048
            var cue0048 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("2ebd861e55143014c8067c6832cdf21c");
            //   Add conditional to Cue 0048
            DialogTools.CueAddCondition(cue0048, ConditionalTools.CreateEtudeCondition("WRM_lich_WoljifRomance1", romanceactive, "playing"));
            //   Add logic to OnShow
            Kingmaker.ElementsSystem.Condition[] Summons =
                {
                    ConditionalTools.CreateCompanionInPartyCondition("WRM_lich_WJnotpresent", Companions.Woljif, true),
                    ConditionalTools.CreateCompanionDeadCondition("WRM_lich_WJdead", Companions.Woljif)
                };
            Kingmaker.ElementsSystem.Condition[] LoveAndSummons =
                {
                    ConditionalTools.CreateEtudeCondition("WRM_lich_WoljifRomance2", romanceactive, "playing"),
                    ConditionalTools.CreateLogicCondition("WRM_lich_NotAvailable", Kingmaker.ElementsSystem.Operation.Or, Summons)
                };
            var needSummonWJ = ConditionalTools.CreateLogicCondition("WRM_lich_NeedSummon", LoveAndSummons);
            ConditionalTools.CheckerAddCondition(((Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)cue0048.OnShow.Actions[0]).ConditionsChecker, needSummonWJ);
            //   Add logic to cutscene
            var command = Resources.GetBlueprint<Kingmaker.AreaLogic.Cutscenes.CommandAction>("d9f06282ad7352641863159ee2c664be");
            var arutrue = ((Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)command.Action.Actions[0]).IfTrue;
            var onspawn = ((Kingmaker.Designers.EventConditionActionSystem.Actions.Spawn)arutrue.Actions[0]).ActionsOnSpawn;
            var fxprefab = ((Kingmaker.Designers.EventConditionActionSystem.Actions.SpawnFx)onspawn.Actions[0]).FxPrefab;
            
            var spawnaction = ActionTools.SpawnUnitAction(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnit>("766435873b1361c4287c351de194e5f9"), new FakeLocator(211.66f, 40.99f, -296.74f, 328f));
            var eval = (GenericUnitEvaluator)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(GenericUnitEvaluator));
            eval.setEntity(spawnaction.GetEntity());
            var transform = (Kingmaker.Designers.EventConditionActionSystem.Evaluators.UnitTransform)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(Kingmaker.Designers.EventConditionActionSystem.Evaluators.UnitTransform));
            transform.Unit = eval;
            Kingmaker.ElementsSystem.GameAction[] spawnandeffects = 
                {
                    spawnaction,
                    ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.SpawnFx>(bp =>
                    {
                       bp.Target = transform;
                       bp.FxPrefab = fxprefab;
                    })
                };
            var woljifConditionalSpawn = ActionTools.ConditionalAction(ConditionalTools.CreateEtudeCondition("WRM_lich_WoljifRomance3", romanceactive, "playing"));
            ActionTools.ConditionalActionOnTrue(woljifConditionalSpawn, ActionTools.MakeList(spawnandeffects));
            var len = command.Action.Actions.Length;
            Array.Resize(ref command.Action.Actions, len+1);
            command.Action.Actions[len] = woljifConditionalSpawn;
            // Add to Cuesequence 0047
            var WoljifCue1 = DialogTools.CreateCue("WRM_lich_c_1");
            WoljifCue1.Speaker.m_SpeakerPortrait = CommandTools.GetCompanionReference(Companions.Woljif);
            DialogTools.CueAddCondition(WoljifCue1, ConditionalTools.CreateEtudeCondition("WRM_lich_WoljifRomance4", romanceactive, "playing"));
            var cuesequence0047 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCueSequence>("0409cff795b3a04479f657f55aded190");
            DialogTools.CueSequenceInsertCue(cuesequence0047, WoljifCue1);

            //Alter Cue 0050
            var cue0050 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("81a9fa5cc4df55d45abe754bffd53563");
            //   Add logic to OnShow
            var paralysis = Resources.GetBlueprint<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>("d77b4982cc0d8eb4f9210d1350199e91");
            var buff = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.AttachBuff>(bp =>
                {
                    bp.m_Buff = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintBuffReference>(paralysis);
                    bp.Target = /*CommandTools.getCompanionEvaluator(Companions.Woljif)*/ eval;
                });
            var buffifapplicable = ActionTools.ConditionalAction(ConditionalTools.CreateEtudeCondition("WRM_lich_WoljifRomance5", romanceactive, "playing"));
            ActionTools.ConditionalActionOnTrue(buffifapplicable, ActionTools.MakeList(buff));
            DialogTools.CueAddOnShowAction(cue0050, buffifapplicable);

            //Alter Talk sequence
            //   Add conditional to Answer 0030
            var answer0030 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("b8000d4a1c30b4b47923bda916919f66");
            DialogTools.AnswerAddShowCondition(answer0030, ConditionalTools.CreateCueSeenCondition("WRM_SawWoljifDialog1", WoljifCue1));
            //   Add dialog sequence
            var WoljifCue2 = DialogTools.CreateCue("WRM_lich_c_2");
            var WoljifCue3 = DialogTools.CreateCue("WRM_lich_c_3");
            WoljifCue2.Speaker.m_SpeakerPortrait = CommandTools.GetCompanionReference(Companions.Woljif);
            WoljifCue3.Speaker.m_SpeakerPortrait = CommandTools.GetCompanionReference(Companions.Woljif);
            DialogTools.CueAddCondition(WoljifCue2, ConditionalTools.CreateEtudeCondition("WRM_lich_WoljifRomance6", romanceactive, "playing"));
            DialogTools.CueAddContinue(WoljifCue2, WoljifCue3);
            var cue0085 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("ce7ce1c176d0a7344a1cd935357c7757");
            DialogTools.CueAddContinue(WoljifCue3, cue0085);
            DialogTools.AnswerAddNextCue(answer0030, WoljifCue2);

            //Alter Cue 0041
            var cue0041 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("7f532b681d64f3741a7aa0aebba7c4db");
            //   Add logic for breakup
            var breakupAction = ActionTools.ConditionalAction(ConditionalTools.CreateEtudeCondition("WRM_lich_WoljifRomance8", romanceactive, "playing"));
            ActionTools.ConditionalActionOnTrue(breakupAction, ActionTools.CompleteEtudeAction(romanceactive));
            var breakupflag = EtudeTools.CreateFlag("WRM_WoljifRomance_LichRomanceEnd_flag");
            ActionTools.ConditionalActionOnTrue(breakupAction, ActionTools.UnlockFlagAction(breakupflag));
            DialogTools.CueAddOnShowAction(cue0041, breakupAction);

            //Alter Cuesequence 0036
            var cuesequence0036 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCueSequence>("2ef9dde3f19d49444a51f38156da8846");
            var WoljifCue4 = DialogTools.CreateCue("WRM_lich_c_4");
            WoljifCue4.Speaker.m_SpeakerPortrait = CommandTools.GetCompanionReference(Companions.Woljif);
            var cutscene = Resources.GetBlueprint<Kingmaker.AreaLogic.Cutscenes.Cutscene>("442fb54c1eb24bd4cb6b991dfdc0179a");
            var playcutscene = ActionTools.PlayCutsceneAction(cutscene);
            ActionTools.CutsceneActionAddParameter(playcutscene, "WRM_RomanceDeath_Woljif", "unit", /*CommandTools.getCompanionEvaluator(Companions.Woljif)*/ eval);
            DialogTools.CueAddOnShowAction(WoljifCue4, playcutscene);
            var WoljifDeathFlag = EtudeTools.CreateFlag("WRM_KilledRomance_Woljif");
            DialogTools.CueAddOnShowAction(WoljifCue4, ActionTools.UnlockFlagAction(WoljifDeathFlag));
            DialogTools.CueAddCondition(WoljifCue4, ConditionalTools.CreateEtudeCondition("WRM_lich_WoljifRomance7", romanceactive, "playing"));
            DialogTools.CueSequenceInsertCue(cuesequence0036, WoljifCue4);

            //Alter Cue 0039
            var cue0039 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("9e2f7f03c939fa04b9acefa3375b6b9f");
            //   Unsummoning/actionholder stuff
            var cue0039cond = (Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)cue0039.OnShow.Actions[1];

            Kingmaker.ElementsSystem.Condition[] unsummonconds = 
                {
                    ConditionalTools.CreateEtudeCondition("WRM_lich_WoljifRomance9", romanceactive, "playing"),
                    ConditionalTools.CreateCompanionInPartyCondition("WRM_lich_WJnotpresent2", Companions.Woljif, true)
                };
            var woljifcond1 = ConditionalTools.CreateLogicCondition("WRM_lich_unsummoncond", unsummonconds);
            ConditionalTools.CheckerAddCondition(cue0039cond.ConditionsChecker, woljifcond1);
            var cue0039iffalse = (Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)cue0039cond.IfFalse.Actions[0];
            ConditionalTools.CheckerAddCondition(cue0039iffalse.ConditionsChecker, ConditionalTools.CreateEtudeCondition("WRM_lich_WoljifRomance10", romanceactive, "playing"));
            //Create actionholder for Woljif
            var innerConditional = ActionTools.ConditionalAction(ConditionalTools.CreateCompanionInPartyCondition("WRM_lich_WJnotpresent3", Companions.Woljif, true));
            Kingmaker.ElementsSystem.GameAction[] detachandhide =
                {
                    ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.DetachBuff>(bp =>
                        {
                            bp.m_Buff = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintBuffReference>(paralysis);
                            bp.Target = /*CommandTools.getCompanionEvaluator(Companions.Woljif)*/ eval;
                        }),
                    ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit>(bp =>
                        {
                            bp.Target = /*CommandTools.getCompanionEvaluator(Companions.Woljif)*/ eval;
                            bp.Fade = true;
                        })
                };
            ActionTools.ConditionalActionOnTrue(innerConditional, detachandhide);
            ActionTools.ConditionalActionOnFalse(innerConditional, ActionTools.MakeList(
                ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.DetachBuff>(bp =>
                    {
                        bp.m_Buff = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintBuffReference>(paralysis);
                        bp.Target = CommandTools.getCompanionEvaluator(Companions.Woljif);
                    })));
            var outerconditional = ActionTools.ConditionalAction(ConditionalTools.CreateEtudeCondition("WRM_lich_WoljifRomance11", romanceactive, "playing"));
            ActionTools.ConditionalActionOnTrue(outerconditional, ActionTools.MakeList(innerConditional));
            var WoljifActionHolder = Helpers.CreateBlueprint<Kingmaker.ElementsSystem.ActionsHolder>("WRM_lich_WoljifActionHolder");
            WoljifActionHolder.Actions = ActionTools.MakeList(outerconditional);
            //Finish setting up unsummon
            var runholder = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.RunActionHolder>(bp => 
                {
                    bp.Holder = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.ElementsSystem.ActionsReference>(WoljifActionHolder);
                });
            ActionTools.ConditionalActionOnTrue(cue0039iffalse, ActionTools.MakeList(runholder));
        }

        public static void MiscChanges()
        {
            // Calling out Toil in the trigger for Ember's act 5 quest
            var C_LoveYourself = DialogTools.CreateCue("WRM_Ember_c_LoveYourself");
            var C_CalledOut = DialogTools.CreateCue("WRM_Ember_c_CalledOut");
            var A_DefendWoljif = DialogTools.CreateAnswer("WRM_Ember_a_DefendWoljif", true);
            var A_DefendBoth = DialogTools.CreateAnswer("WRM_Ember_a_DefendBoth", true);
            var A_DefendSelf = DialogTools.CreateAnswer("WRM_Ember_a_DefendSelf", true);
            var C_Apology1 = DialogTools.CreateCue("WRM_Ember_c_Apology1");
            var C_Apology2 = DialogTools.CreateCue("WRM_Ember_c_Apology2");
            var C_Poverty = DialogTools.CreateCue("WRM_Ember_c_Poverty");
            var C_Sputter = DialogTools.CreateCue("WRM_Ember_c_Sputter");

            var answerlist0008 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("7fa1450413d74e84f80291fce4fab14a");
            var cue0026 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("84f33fd16b9e8e64b8b3cfcd084bf2f5");
            var cue3 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("a3d3b556212a430d95f57353a3a8d500");

            var cond_didntseeLoveYourself = ConditionalTools.CreateCueSeenCondition("WRM_DidntSeeLoveYourself", C_LoveYourself, true);
            var cond_sawcue3 = ConditionalTools.CreateCueSeenCondition("WRM_sawcue3", cue3);
            var cond_sawcue0026 = ConditionalTools.CreateCueSeenCondition("WRM_sawcue0026", cue0026);
            var cond_tiefling = ConditionalTools.CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PcRace>("WRM_playertiefling", bp =>
                { bp.Race = Kingmaker.Blueprints.Race.Tiefling; });

            DialogTools.AnswerAddShowCondition(A_DefendWoljif, cond_sawcue3);
            DialogTools.AnswerAddShowCondition(A_DefendWoljif, cond_didntseeLoveYourself);
            DialogTools.AnswerAddShowCondition(A_DefendBoth, cond_sawcue3);
            DialogTools.AnswerAddShowCondition(A_DefendBoth, cond_didntseeLoveYourself);
            DialogTools.AnswerAddShowCondition(A_DefendBoth, cond_tiefling);
            DialogTools.AnswerAddShowCondition(A_DefendSelf, cond_sawcue0026);
            DialogTools.AnswerAddShowCondition(A_DefendSelf, cond_didntseeLoveYourself);
            DialogTools.AnswerAddShowCondition(A_DefendSelf, cond_tiefling);

            DialogTools.AnswerAddNextCue(A_DefendWoljif, C_Apology1);
            DialogTools.AnswerAddNextCue(A_DefendBoth, C_Apology2);
            DialogTools.AnswerAddNextCue(A_DefendSelf, C_Sputter);
            DialogTools.CueAddContinue(C_Apology1, C_Poverty);
            DialogTools.CueAddContinue(C_Apology2, C_Poverty);
            DialogTools.CueAddAnswersList(C_Poverty, answerlist0008);
            DialogTools.CueAddAnswersList(C_Sputter, answerlist0008);

            DialogTools.CueSetSpeaker(C_LoveYourself, Companions.Ember);
            DialogTools.CueAddContinue(cue3, C_LoveYourself);
            DialogTools.CueAddContinue(C_LoveYourself, C_CalledOut);
            C_CalledOut.Speaker = cue3.Speaker;
        }
    }
}