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
            EventTools.AddResolution(Notification, Kingmaker.Kingdom.Blueprints.EventResult.MarginType.Fail, "WRM_note_7a_Ignored");
            EventTools.AddResolution(Notification, Kingmaker.Kingdom.Blueprints.EventResult.MarginType.Success, "WRM_note_7a_Complete");

            EtudeTools.EtudeAddOnPlayTrigger(NotificationEtude, ActionTools.MakeList(ActionTools.StartCommandRoomEventAction(Notification)));
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
            EtudeTools.EtudeAddDelayedAction(SnowTimer, 10, ActionTools.MakeList(delayedactions));

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
        }
    }
}