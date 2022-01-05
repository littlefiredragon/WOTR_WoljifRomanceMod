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
    public static class WRM_Act4
    {
        static public void ModifyQuestTrigger()
        {
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceActive");
            var affection = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_WoljifAffection");
            // Existing bits that need to be referenced or modified
            var answerslist_0002 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("27829507934a2754e9790161a9956466");
            var cue_0008 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("02e13d52e1b2f0649a2e382242c973fc");
            var answerslist_0009 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("4d7547937d19c4f47ba2432c49699c39");
            var cue_0014 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("5b83eb2dc2652b24b9fe207dc303362e");
            var cue_0015 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("9a172da2438713b498d2d845e8574d77");
            var answerslist_0021 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("00f30157918448349a0a1b71b586dd73");
            var answer_0023 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("c395ad36691b7e545afaae4f9b51b29e");
            var answer_0039 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("7edb41dfa8afded44b861c45cefc6d2f");
            var cue_0030 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("d65784bbde534de45a3deb8caad59b08");
            var melrounescaped = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("56305509e10c6c344a467541a8e1c896");
            // Create new parts
            var a_DontWanderAlone = DialogTools.CreateAnswer("WRM_5a_a_DontWanderAlone", true);
            var c_TakeCareOfMyself = DialogTools.CreateCue("WRM_5a_c_TakeCareOfMyself");
            var a_NotALoser = DialogTools.CreateAnswer("WRM_5a_a_NotALoser", true);
            var c_UhThanks = DialogTools.CreateCue("WRM_5a_c_UhThanks");
            var a_Friends = DialogTools.CreateAnswer("WRM_5a_a_Friends");

            // Add romance enders to nasty dialog
            DialogTools.CueAddOnStopAction(cue_0008, ActionTools.CompleteEtudeAction(romanceactive));
            DialogTools.CueAddOnStopAction(cue_0014, ActionTools.CompleteEtudeAction(romanceactive));
            DialogTools.CueAddOnStopAction(cue_0014, ActionTools.IncrementFlagAction(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"), -1));
            DialogTools.CueAddOnStopAction(cue_0008, ActionTools.IncrementFlagAction(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"), -1));

            // Patch in new dialog
            DialogTools.ListAddAnswer(answerslist_0002, a_DontWanderAlone);
            DialogTools.AnswerAddNextCue(a_DontWanderAlone, c_TakeCareOfMyself);
            DialogTools.AnswerAddOnSelectAction(a_DontWanderAlone, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.CueAddContinue(c_TakeCareOfMyself, cue_0015);
            DialogTools.ListAddAnswer(answerslist_0009, a_NotALoser, 0);
            DialogTools.AnswerAddOnSelectAction(a_NotALoser, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddNextCue(a_NotALoser, c_UhThanks);
            DialogTools.CueAddAnswersList(c_UhThanks, answerslist_0009);

            // Add automatic diplomacy pass if high enough affection
            DialogTools.ListAddAnswer(answerslist_0021, a_Friends, 0);
            Kingmaker.ElementsSystem.Condition[] autopassconds = {
                ConditionalTools.CreateFlagCheck("WRM_5a_Affection",affection,7,20),
                ConditionalTools.CreateEtudeCondition("WRM_5a_Romance",romanceactive,"Playing")
                };
            var autopasscond = ConditionalTools.CreateLogicCondition("WRM_RomanceAffection7", autopassconds);
            DialogTools.AnswerAddShowCondition(a_Friends, autopasscond);
            DialogTools.AnswerAddNextCue(a_Friends, cue_0030);
            // And the complicated bit, modifying the existing logic for the skill check dialogs.
            Kingmaker.ElementsSystem.Condition[] affectionfailconds = {
                ConditionalTools.CreateFlagCheck("WRM_5a_Affection",affection,7,20, true),
                ConditionalTools.CreateEtudeCondition("WRM_5a_Romance",romanceactive,"Playing", true)
                };
            var affectionfailcond = ConditionalTools.CreateLogicCondition("WRM_RomanceAffectionFail", Kingmaker.ElementsSystem.Operation.Or, affectionfailconds);
            Kingmaker.ElementsSystem.Condition[] escapedfailconds = {
                ConditionalTools.CreateEtudeCondition("WRM_escaped", melrounescaped,"Playing"), 
                affectionfailcond };
            Kingmaker.ElementsSystem.Condition[] caughtfailconds = {
                ConditionalTools.CreateEtudeCondition("WRM_caught", melrounescaped,"Playing",true),
                affectionfailcond };
            var escapedfailcond = ConditionalTools.CreateLogicCondition("WRM_RomanceAffectionFail", escapedfailconds);
            var caughtfailcond = ConditionalTools.CreateLogicCondition("WRM_RomanceAffectionFail", caughtfailconds);

            answer_0023.ShowConditions.Conditions[0] = escapedfailcond;
            answer_0039.ShowConditions.Conditions[0] = caughtfailcond;
        }

        static public void ModifyQuestDialog()
        {
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceActive");
            var affectiongatesuccess = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceGatePassed");
            var affectiongatefail = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceGateFailed");
            var affection = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_WoljifAffection");
            // Existing bits that need to be referenced or modified
            var humanending = Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("8436804f1b1e6014f9c45e6075d7aaef");
            var humanendinggood = Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("fb00853f5e7e1be449ac3320986c8afc");
            var answer2_0010 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("98150bc3d9270b54fa8d5f1a7d6e7d90");
            var list3_0006 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("e0b41b8647282584b85e209fd4b1f98a");
            var cue3_0010 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("4263f13acd8a50e4f9eb05eed1c126fd");
            var answer3_0008 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("a9512f675e141a34b8979c4cd7398a45");
            var list4_0004 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("e3c7639fd4950cc41ae8d743a697c8d4");
            var cue4_0018 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("2ca54f72490a0db49870d98016037aba");
            var cue4_0010 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("092451bcaee0e3d4e944e8fac5521742");
            var cue4_0024 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("d35bae3b091f2e245af7614b8bedf9a3");
            var cue5_0008 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("5e364e37b0023f142bac35984c2b836e");
            var list5_0002 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("66383078723475648955aed24d5adda3");
            var answer5_0005 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("a0183fc4f8223274eb1121705383e3b3");
            var cue5_0013 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("d1d3dfb9568d8d14ca5408c36efc318c");

            // Add romance enders 
            DialogTools.AnswerAddOnSelectAction(answer2_0010, ActionTools.CompleteEtudeAction(romanceactive));
            DialogTools.AnswerAddOnSelectAction(answer3_0008, ActionTools.CompleteEtudeAction(romanceactive));
            DialogTools.AnswerAddOnSelectAction(answer2_0010, ActionTools.IncrementFlagAction(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"), -1));
            DialogTools.AnswerAddOnSelectAction(answer3_0008, ActionTools.IncrementFlagAction(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"), -1));
            DialogTools.CueAddOnStopAction(cue4_0010, ActionTools.CompleteEtudeAction(romanceactive));
            DialogTools.CueAddOnStopAction(cue4_0024, ActionTools.CompleteEtudeAction(romanceactive));
            DialogTools.CueAddOnStopAction(cue5_0008, ActionTools.CompleteEtudeAction(romanceactive));
            DialogTools.CueAddOnStopAction(cue4_0010, ActionTools.IncrementFlagAction(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"), -1));
            DialogTools.CueAddOnStopAction(cue4_0024, ActionTools.IncrementFlagAction(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"), -1));
            DialogTools.CueAddOnStopAction(cue5_0008, ActionTools.IncrementFlagAction(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"), -1));
            // FailedRomanceGate
            DialogTools.AnswerAddOnSelectAction(answer5_0005, ActionTools.StartEtudeAction(affectiongatefail));

            // New dialogs
            var A_DontListen = DialogTools.CreateAnswer("WRM_5b_a_DontListen");
            var A_HoldHands = DialogTools.CreateAnswer("WRM_5b_a_HoldHands");
            var C_HoldHands = DialogTools.CreateCue("WRM_5b_c_HoldHands");
            var C_0017copy = DialogTools.CreateCue("WRM_5b_c_0017copy");
            var A_Embrace = DialogTools.CreateAnswer("WRM_5b_a_Embrace");
            var C_Embrace = DialogTools.CreateCue("WRM_5b_c_Embrace");

            DialogTools.ListAddAnswer(list3_0006, A_DontListen, 0);
            DialogTools.AnswerAddNextCue(A_DontListen, cue3_0010);
            DialogTools.AnswerAddOnSelectAction(A_DontListen, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddOnSelectAction(A_DontListen, ActionTools.IncrementFlagAction(humanending, 1));

            DialogTools.ListAddAnswer(list4_0004, A_HoldHands, 1);
            Kingmaker.ElementsSystem.Condition[] handholdconds = {
                ConditionalTools.CreateEtudeCondition("WRM_RomanceActiveHandHold", romanceactive, "playing"),
                ConditionalTools.CreateFlagCheck("WRM_8Affection",affection,8,20)
                };
            DialogTools.AnswerAddShowCondition(A_HoldHands, ConditionalTools.CreateLogicCondition("WRM_HandHoldConditions", handholdconds));
            DialogTools.AnswerAddOnSelectAction(A_HoldHands, ActionTools.IncrementFlagAction(affection, 1));
            DialogTools.AnswerAddOnSelectAction(A_HoldHands, ActionTools.IncrementFlagAction(humanending, 1));
            DialogTools.AnswerAddNextCue(A_HoldHands, C_HoldHands);
            DialogTools.CueAddOnShowAction(C_HoldHands, ActionTools.StartEtudeAction(humanendinggood));
            DialogTools.CueAddContinue(C_HoldHands, C_0017copy);
            DialogTools.CueAddContinue(C_0017copy, cue4_0018);

            DialogTools.ListAddAnswer(list5_0002, A_Embrace, 2);
            Kingmaker.ElementsSystem.Condition[] gateconds = {
                ConditionalTools.CreateEtudeCondition("WRM_RomanceActiveGate", romanceactive, "playing"),
                ConditionalTools.CreateFlagCheck("WRM_9Affection",affection,9,20),
                ConditionalTools.CreateEtudeCondition("WRM_HumanEndingGoodGate", humanendinggood, "playing")
                };
            DialogTools.AnswerAddShowCondition(A_Embrace, ConditionalTools.CreateLogicCondition("WRM_AffectionGate", gateconds));
            DialogTools.AnswerAddOnSelectAction(A_Embrace, ActionTools.StartEtudeAction(affectiongatesuccess));
            DialogTools.AnswerAddOnSelectAction(A_Embrace, ActionTools.StopMusic());
            DialogTools.AnswerAddOnSelectAction(A_Embrace, ActionTools.StartMusic("RomanceTheme"));
            DialogTools.AnswerAddNextCue(A_Embrace, C_Embrace);
            C_Embrace.OnStop = cue5_0013.OnStop;
        }

        static public void CreateNightmareScene()
        {
            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceActive");
            var embraceAnswer = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("WRM_5b_a_Embrace");

            // Create new dialogs
            var c_Nightmare = DialogTools.CreateCue("WRM_6_c_Nightmare");
            var a_Wake = DialogTools.CreateAnswer("WRM_6_a_Wake");
            var a_Ignore1 = DialogTools.CreateAnswer("WRM_6_a_Ignore1", true);
            var a_Ignore2 = DialogTools.CreateAnswer("WRM_6_a_Ignore2");
            var List1 = DialogTools.CreateAnswersList("WRM_6_L_1");
            var c_Wake = DialogTools.CreateCue("WRM_6_c_Wake");
            var c_Ignore1 = DialogTools.CreateCue("WRM_6_c_Ignore1");
            var c_Ignore2 = DialogTools.CreateCue("WRM_6_c_Ignore2");
            var a_WannaTalk = DialogTools.CreateAnswer("WRM_6_a_WannaTalk");
            var a_YouOkay = DialogTools.CreateAnswer("WRM_6_a_YouOkay");
            var List2 = DialogTools.CreateAnswersList("WRM_6_L_2");
            var c_NoBigDeal = DialogTools.CreateCue("WRM_6_c_NoBigDeal");
            var c_YouDied = DialogTools.CreateCue("WRM_6_c_YouDied");
            var a_ShelynWorshipper = DialogTools.CreateAnswer("WRM_6_a_ShelynWorshipper", "WRM_6_a_Shelyn", true);
            var a_ShelynCheck = DialogTools.CreateAnswer("WRM_6_a_ShelynCheck", "WRM_6_a_Shelyn", true);
            var a_GoOn = DialogTools.CreateAnswer("WRM_6_a_GoOn");
            var List3 = DialogTools.CreateAnswersList("WRM_6_L_3");
            var c_YourGod = DialogTools.CreateCue("WRM_6_c_YourGod");
            var c_IDunno = DialogTools.CreateCue("WRM_6_c_IDunno");
            var c_AnsweredPrayer = DialogTools.CreateCue("WRM_6_c_AnsweredPrayer");
            var a_DidYou = DialogTools.CreateAnswer("WRM_6_a_DidYou");
            var a_NeverImagined = DialogTools.CreateAnswer("WRM_6_a_NeverImagined");
            var a_WaitWhy = DialogTools.CreateAnswer("WRM_6_a_WaitWhy");
            var a_ComeOn = DialogTools.CreateAnswer("WRM_6_a_ComeOn");
            var a_ThatSoundsAwful = DialogTools.CreateAnswer("WRM_6_a_ThatSoundsAwful");
            var a_LosingMe = DialogTools.CreateAnswer("WRM_6_a_LosingMe");
            var a_SeelahMentor = DialogTools.CreateAnswer("WRM_6_a_SeelahMentor");
            var a_PCMentor = DialogTools.CreateAnswer("WRM_6_a_PCMentor");
            var a_JustADream = DialogTools.CreateAnswer("WRM_6_a_JustADream");
            var a_Goodnight = DialogTools.CreateAnswer("WRM_6_a_Goodnight");
            var List4 = DialogTools.CreateAnswersList("WRM_6_L_4");
            var c_DontRemember = DialogTools.CreateCue("WRM_6_c_DontRemember");
            var c_WorstNightmare = DialogTools.CreateCue("WRM_6_c_WorstNightmare");
            var c_Both = DialogTools.CreateCue("WRM_6_c_Both");
            var c_Gag = DialogTools.CreateCue("WRM_6_c_Gag");
            var c_Shudder = DialogTools.CreateCue("WRM_6_c_Shudder");
            var c_DontFlatterYourself = DialogTools.CreateCue("WRM_6_c_DontFlatterYourself");
            var c_NeverGonnaHappen = DialogTools.CreateCue("WRM_6_c_NeverGonnaHappen");
            var c_NeverGonnaHappenBut = DialogTools.CreateCue("WRM_6_c_NeverGonnaHappenBut");
            var c_KeepItThatWay = DialogTools.CreateCue("WRM_6_c_KeepItThatWay");
            var c_BackToSleep = DialogTools.CreateCue("WRM_6_c_BackToSleep");
            var NightmareDialog = DialogTools.CreateDialog("WRM_6_NightmareDialog", c_Nightmare);

            // Connect new dialog tree
            DialogTools.CueAddAnswersList(c_Nightmare, List1);
            DialogTools.ListAddAnswer(List1, a_Wake);
            DialogTools.ListAddAnswer(List1, a_Ignore1);
            DialogTools.ListAddAnswer(List1, a_Ignore2);
            DialogTools.AnswerAddShowCondition(a_Ignore2, ConditionalTools.CreateCueSeenCondition("WRM_IgnoredOnce", c_Ignore1));
            DialogTools.AnswerAddOnSelectAction(a_Ignore2, ActionTools.CompleteEtudeAction(romanceactive));
            DialogTools.AnswerAddOnSelectAction(a_Ignore2, ActionTools.IncrementFlagAction(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("5db9ec615236f044083a5c6bd3292432"), -1));

            DialogTools.AnswerAddNextCue(a_Wake, c_Wake);
            DialogTools.AnswerAddNextCue(a_Ignore1, c_Ignore1);
            DialogTools.AnswerAddNextCue(a_Ignore2, c_Ignore2);
            DialogTools.CueAddAnswersList(c_Ignore1, List1);
            DialogTools.CueAddAnswersList(c_Wake, List2);

            DialogTools.ListAddAnswer(List2, a_WannaTalk);
            DialogTools.ListAddAnswer(List2, a_YouOkay);
            DialogTools.AnswerAddNextCue(a_WannaTalk, c_NoBigDeal);
            DialogTools.AnswerAddNextCue(a_YouOkay, c_NoBigDeal);
            DialogTools.CueAddContinue(c_NoBigDeal, c_YouDied);
            DialogTools.CueAddOnShowAction(c_YouDied, ActionTools.StartMusic("RomanceTheme"));
            DialogTools.CueAddAnswersList(c_YouDied, List3);

            DialogTools.ListAddAnswer(List3, a_ShelynWorshipper);
            DialogTools.ListAddAnswer(List3, a_ShelynCheck);
            DialogTools.ListAddAnswer(List3, a_GoOn);
            var shelynite = Resources.GetBlueprint<Kingmaker.Blueprints.Facts.BlueprintUnitFact>("b382afa31e4287644b77a8b30ed4aa0b");
            DialogTools.AnswerAddShowCondition(a_ShelynWorshipper, ConditionalTools.CreateFactCondition("WRM_PlayerShelynite", shelynite, Companions.Player));
            DialogTools.AnswerAddShowCondition(a_ShelynCheck, ConditionalTools.CreateFactCondition("WRM_PlayerNotShelynite", shelynite, Companions.Player, true));
            DialogTools.AnswerAddShowCheck(a_ShelynCheck, Kingmaker.EntitySystem.Stats.StatType.SkillLoreReligion, 12);
            DialogTools.AnswerAddNextCue(a_ShelynWorshipper, c_YourGod);
            DialogTools.AnswerAddNextCue(a_ShelynCheck, c_IDunno);
            DialogTools.CueAddAnswersList(c_YourGod, List3);
            DialogTools.CueAddAnswersList(c_IDunno, List3);
            DialogTools.AnswerAddNextCue(a_GoOn, c_AnsweredPrayer);
            DialogTools.CueAddAnswersList(c_AnsweredPrayer, List4);

            DialogTools.ListAddAnswer(List4, a_DidYou);
            DialogTools.ListAddAnswer(List4, a_WaitWhy);
            DialogTools.ListAddAnswer(List4, a_NeverImagined);
            DialogTools.ListAddAnswer(List4, a_ComeOn);
            DialogTools.ListAddAnswer(List4, a_ThatSoundsAwful);
            DialogTools.ListAddAnswer(List4, a_LosingMe);
            DialogTools.ListAddAnswer(List4, a_SeelahMentor);
            DialogTools.ListAddAnswer(List4, a_PCMentor);
            DialogTools.ListAddAnswer(List4, a_JustADream);
            DialogTools.ListAddAnswer(List4, a_Goodnight);
            var saw_c_Both = ConditionalTools.CreateCueSeenCondition("WRM_SawCBoth", c_Both);
            var saw_c_WorstNightmare = ConditionalTools.CreateCueSeenCondition("WRM_SawCWorstNightmare", c_WorstNightmare);
            var saw_c_DontRemember = ConditionalTools.CreateCueSeenCondition("WRM_SawCDontRemember", c_DontRemember);
            var seelahNotKicked = ConditionalTools.CreateEtudeCondition("WRM_SeelahNotKicked", Resources.GetBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("c33407f132b192643a438bd33797efa1"), "playing", true);
            var playerpaladin = ConditionalTools.CreateClassCheck("WRM_PlayerPaladin", Resources.GetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec"));
            var playernotpaladin = ConditionalTools.CreateClassCheck("WRM_PlayerPaladin", Resources.GetBlueprint<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec"), true);
            Kingmaker.ElementsSystem.Condition[] SawBothOrNightmareConds = { saw_c_Both, saw_c_WorstNightmare };
            Kingmaker.ElementsSystem.Condition[] SawBothDROrNightmareConds = { saw_c_Both, saw_c_WorstNightmare, saw_c_DontRemember };
            var SawBothOrNightmare = ConditionalTools.CreateLogicCondition("WRM_SawBothOrNightmare", Kingmaker.ElementsSystem.Operation.Or, SawBothOrNightmareConds);
            var SawBothDROrNightmare = ConditionalTools.CreateLogicCondition("WRM_SawBothDROrNightmare", Kingmaker.ElementsSystem.Operation.Or, SawBothDROrNightmareConds);
            Kingmaker.ElementsSystem.Condition[] NotThatBadConds = { playerpaladin, SawBothOrNightmare };
            Kingmaker.ElementsSystem.Condition[] AwfulConds = { playernotpaladin, SawBothOrNightmare };
            Kingmaker.ElementsSystem.Condition[] SeelahMentorConds = { playernotpaladin, SawBothDROrNightmare, seelahNotKicked };
            Kingmaker.ElementsSystem.Condition[] PlayerMentorConds = { playerpaladin, SawBothDROrNightmare };
            DialogTools.AnswerAddShowCondition(a_ComeOn, ConditionalTools.CreateLogicCondition("WRM_ComeOnCond", NotThatBadConds));
            DialogTools.AnswerAddShowCondition(a_ThatSoundsAwful, ConditionalTools.CreateLogicCondition("WRM_SoundsAwfulCond", AwfulConds));
            DialogTools.AnswerAddShowCondition(a_LosingMe, saw_c_Both);
            DialogTools.AnswerAddShowCondition(a_SeelahMentor, ConditionalTools.CreateLogicCondition("WRM_SeelahMentorCond", SeelahMentorConds));
            DialogTools.AnswerAddShowCondition(a_PCMentor, ConditionalTools.CreateLogicCondition("WRM_PCMentorCond", PlayerMentorConds));

            DialogTools.AnswerAddNextCue(a_DidYou, c_DontRemember);
            DialogTools.CueAddAnswersList(c_DontRemember, List4);
            DialogTools.AnswerAddNextCue(a_NeverImagined, c_WorstNightmare);
            DialogTools.CueAddAnswersList(c_WorstNightmare, List4);
            DialogTools.AnswerAddNextCue(a_WaitWhy, c_Both);
            DialogTools.CueAddAnswersList(c_Both, List4);
            DialogTools.AnswerAddNextCue(a_ComeOn, c_Gag);
            DialogTools.CueAddAnswersList(c_Gag, List4);
            DialogTools.AnswerAddNextCue(a_ThatSoundsAwful, c_Shudder);
            DialogTools.CueAddAnswersList(c_Shudder, List4);
            DialogTools.AnswerAddNextCue(a_LosingMe, c_DontFlatterYourself);
            DialogTools.CueAddAnswersList(c_DontFlatterYourself, List4);
            DialogTools.AnswerAddNextCue(a_SeelahMentor, c_NeverGonnaHappen);
            DialogTools.CueAddAnswersList(c_NeverGonnaHappen, List4);
            DialogTools.AnswerAddNextCue(a_PCMentor, c_NeverGonnaHappenBut);
            DialogTools.CueAddAnswersList(c_NeverGonnaHappenBut, List4);
            DialogTools.AnswerAddNextCue(a_JustADream, c_KeepItThatWay);
            DialogTools.CueAddAnswersList(c_KeepItThatWay, List4);
            DialogTools.AnswerAddNextCue(a_Goodnight, c_BackToSleep);
            var ConsoledWoljif = EtudeTools.CreateEtude("WRM_ConsoledWoljif", romanceactive, false, false);
            DialogTools.CueAddOnShowAction(c_BackToSleep, ActionTools.StartEtudeAction(ConsoledWoljif));
            DialogTools.CueAddOnStopAction(c_BackToSleep, ActionTools.StopMusic());

            // Create the Nightmare event and trigger
            var NightmareEvent = EventTools.CreateCampingEvent("WRM_NightmareEvent", 100);
            EventTools.CampEventAddCondition(NightmareEvent, ConditionalTools.CreateDialogSeenCondition("WRM_NightmareNotHappenedYet", NightmareDialog, true));
            EventTools.CampEventAddCondition(NightmareEvent, ConditionalTools.CreateEtudeCondition("WRM_IsRomanceActive", romanceactive, "Playing"));
            EventTools.CampEventAddCondition(NightmareEvent, ConditionalTools.CreateCurrentAreaIsCondition("WRM_NightmareNotInCapital", Resources.GetBlueprint<Kingmaker.Blueprints.Area.BlueprintArea>("2570015799edf594daf2f076f2f975d8"), true));
            EventTools.CampEventAddCondition(NightmareEvent, ConditionalTools.CreateCurrentAreaIsCondition("WRM_NightmareNotInNexus", Resources.GetBlueprint<Kingmaker.Blueprints.Area.BlueprintArea>("7847c3e3537104f4694167af0b9fcd0e"), true));
            EventTools.CampEventAddCondition(NightmareEvent, ConditionalTools.CreateCompanionInPartyCondition("WRM_WoljifInPartyForNightmare", Companions.Woljif));
            EventTools.CampEventAddAction(NightmareEvent, ActionTools.RemoveCampEventAction(NightmareEvent));
            EventTools.CampEventAddAction(NightmareEvent, ActionTools.StartDialogAction(NightmareDialog, Companions.Woljif));

            //var NightmareTimer = EtudeTools.CreateEtude("WRM_TimerBeforeNightmare", romanceactive, false, false);
            var CampCounter = EtudeTools.CreateFlag("WRM_NightmareCampEventFlag");
            Kingmaker.ElementsSystem.GameAction[] AddCampEvent =
                {
                    ActionTools.IncrementFlagAction(CampCounter),
                    ActionTools.ConditionalAction(ConditionalTools.CreateFlagCheck("WRM_DontDoubleNightmare", CampCounter, 2, 1000000, true))
                };
            ActionTools.ConditionalActionOnTrue((Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)AddCampEvent[1], ActionTools.AddCampEventAction(NightmareEvent));
            //EtudeTools.EtudeAddDelayedAction(NightmareTimer, 3, ActionTools.MakeList(AddCampEvent));
            //DialogTools.AnswerAddOnSelectAction(embraceAnswer, ActionTools.StartEtudeAction(NightmareTimer));

            var NightmareTimer = WoljifRomanceMod.Clock.AddTimer("WRM_Timers_Nightmare");
            var NightmareTimerEtude = EtudeTools.CreateEtude("WRM_Timers_NightmareEtude", romanceactive, false, false);
            EtudeTools.EtudeAddActivationCondition(NightmareTimerEtude, ConditionalTools.CreateFlagCheck("WRM_Timers_NightmareTrigger", NightmareTimer.time, 3, 1000000));
            Kingmaker.ElementsSystem.GameAction[] delayedactions = { AddCampEvent[0], AddCampEvent[1], ActionTools.CompleteEtudeAction(NightmareTimerEtude) };
            EtudeTools.EtudeAddOnPlayTrigger(NightmareTimerEtude, ActionTools.MakeList(delayedactions));

            DialogTools.AnswerAddOnSelectAction(embraceAnswer, ActionTools.IncrementFlagAction(NightmareTimer.active));
            DialogTools.AnswerAddOnSelectAction(embraceAnswer, ActionTools.StartEtudeAction(NightmareTimerEtude));

            //EtudeTools.EtudeAddDelayedAction(NightmareTimer, 3, ActionTools.MakeList(ActionTools.AddCampEventAction(NightmareEvent)));

            //var affectiongatesuccess = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceGatePassed");
            //EtudeTools.EtudeAddOnPlayTrigger(affectiongatesuccess, ActionTools.MakeList(ActionTools.StartEtudeAction(NightmareTimer)));
        }

        static public void MiscChanges()
        {
            // Thousand Delights scene tweaks
            var WoljifReaction = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("3876c6a0f4a910545bb3ae2b14961654");
            var WenduagComment = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("aa83d327afdd2ac47830700a844ee663");
            var AnswersList0043 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("1f6e0090838cb474b9192d84ea187705");
            var Answer1 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("5429d7c9935f8c14bb367dd9587a27ab");
            var Answer2 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("f47a58e48043af24db5fd7ac9fbdcb58");
            var Answer3 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("e3810159c2913fc419d0dbfb21c21f13");
            var Answer4 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("46a3a73af744efc48b539e68ea59b06c");
            var Answer5 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("34627632eba0a6a459b27e5609ff47de");
            var SequenceExit = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintSequenceExit>("3f8d8cb20fbedb2429f09dc60c49fe45");

            var L_Interrupt = DialogTools.CreateAnswersList("WRM_L_Interrupt");
            DialogTools.ListAddCondition(L_Interrupt, ConditionalTools.CreateCompanionInPartyCondition("WRM_Brothel_WenduagSnark", Companions.Wenduag));
            var A_Elbow1 = DialogTools.CreateAnswer("WRM_brothel_a_Elbow_1", "WRM_brothel_a_Elbow");
            var A_Nothing = DialogTools.CreateAnswer("WRM_brothel_a_DoNothing");
            DialogTools.ListAddAnswer(L_Interrupt, A_Elbow1);
            DialogTools.ListAddAnswer(L_Interrupt, A_Nothing);
            var C_SnapOut1 = DialogTools.CreateCue("WRM_brothel_c_SnapOut_1", "WRM_brothel_c_SnapOut");
            DialogTools.CueSetSpeaker(C_SnapOut1, Companions.Woljif, false);
            DialogTools.AnswerAddNextCue(A_Elbow1, C_SnapOut1);
            DialogTools.AnswerAddNextCue(A_Nothing, WenduagComment);
            DialogTools.CueAddContinue(C_SnapOut1, WenduagComment);

            WoljifReaction.Continue.Cues.RemoveAt(0);
            DialogTools.CueAddAnswersList(WoljifReaction, L_Interrupt);

            var L_Reaction = DialogTools.CreateAnswersList("WRM_L_Reaction");
            var A_Elbow2 = DialogTools.CreateAnswer("WRM_brothel_a_Elbow_2", "WRM_brothel_a_Elbow");
            var A_WaitWhat = DialogTools.CreateAnswer("WRM_brothel_a_WaitWhat");
            Kingmaker.ElementsSystem.Condition[] ElbowConditions = { ConditionalTools.CreateCueSeenCondition("Saw0078", WoljifReaction, true), ConditionalTools.CreateCueSeenCondition("DidNotSee0079", true, WenduagComment, true) };
            DialogTools.AnswerAddShowCondition(A_Elbow2, ConditionalTools.CreateLogicCondition("WRM_NoWenduag", ElbowConditions));
            DialogTools.AnswerAddShowCondition(A_WaitWhat, ConditionalTools.CreateCueSeenCondition("WRM_WenduagComment", WenduagComment, true));

            var C_SnapOut2 = DialogTools.CreateCue("WRM_brothel_c_SnapOut_2", "WRM_brothel_c_SnapOut");
            var C_HoneyTrap = DialogTools.CreateCue("WRM_brothel_c_HoneyTrap");
            DialogTools.CueSetSpeaker(C_SnapOut2, Companions.Woljif, false);
            DialogTools.CueSetSpeaker(C_HoneyTrap, Companions.Woljif, false);
            DialogTools.AnswerAddNextCue(A_Elbow2, C_SnapOut2);
            DialogTools.AnswerAddNextCue(A_WaitWhat, C_HoneyTrap);
            DialogTools.CueAddAnswersList(C_SnapOut2, AnswersList0043);
            DialogTools.CueAddAnswersList(C_HoneyTrap, AnswersList0043);

            DialogTools.ListAddAnswer(L_Reaction, A_Elbow2);
            DialogTools.ListAddAnswer(L_Reaction, A_WaitWhat);
            DialogTools.ListAddAnswer(L_Reaction, Answer1);
            DialogTools.ListAddAnswer(L_Reaction, Answer2);
            DialogTools.ListAddAnswer(L_Reaction, Answer3);
            DialogTools.ListAddAnswer(L_Reaction, Answer4);
            DialogTools.ListAddAnswer(L_Reaction, Answer5);

            SequenceExit.Answers.RemoveAt(0);
            SequenceExit.Answers.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintAnswerBaseReference>(L_Reaction));

            // Post-quest dialog tweak
            var affectiongatesuccess = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceGatePassed");
            var PostQuest = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("bbe0ab34a6bd87640b59afe736e6e1c5");
            var PostQuestAnswers = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("e41585da330233143b34ef64d7d62d69");
            var C_Belief_LG = DialogTools.CreateCue("WRM_5c_c_BeliefLG");
            var C_Belief_G = DialogTools.CreateCue("WRM_5c_c_BeliefG");
            var C_Belief_L = DialogTools.CreateCue("WRM_5c_c_BeliefL");
            var C_Belief = DialogTools.CreateCue("WRM_5c_c_Belief");
            DialogTools.CueAddAnswersList(C_Belief, PostQuestAnswers);
            DialogTools.CueAddAnswersList(C_Belief_L, PostQuestAnswers);
            DialogTools.CueAddAnswersList(C_Belief_G, PostQuestAnswers);
            DialogTools.CueAddAnswersList(C_Belief_LG, PostQuestAnswers);

            var romanceactive = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_WoljifRomanceActive");
            var romanceactivecond = ConditionalTools.CreateEtudeCondition("WRM_RomanceActive", romanceactive, "playing");
            var gatepassed = ConditionalTools.CreateEtudeCondition("WRM_AffectionPassed", affectiongatesuccess, "playing");
            var playerL = ConditionalTools.CreateAlignmentCondition("WRM_PlayerLawful", "Lawful");
            var playerG = ConditionalTools.CreateAlignmentCondition("WRM_PlayerLawful", "Good");
            Kingmaker.ElementsSystem.Condition[] LGConds = { gatepassed, romanceactivecond, playerG, playerL };
            Kingmaker.ElementsSystem.Condition[] GConds = { gatepassed, romanceactivecond, playerG };
            Kingmaker.ElementsSystem.Condition[] LConds = { gatepassed, romanceactivecond, playerL };
            Kingmaker.ElementsSystem.Condition[] OtherConds = { gatepassed, romanceactivecond };
            DialogTools.CueAddCondition(C_Belief, ConditionalTools.CreateLogicCondition("WRM_OtherBeliefsCond", OtherConds));
            DialogTools.CueAddCondition(C_Belief_L, ConditionalTools.CreateLogicCondition("WRM_LBeliefsCond", LConds));
            DialogTools.CueAddCondition(C_Belief_G, ConditionalTools.CreateLogicCondition("WRM_GBeliefsCond", GConds));
            DialogTools.CueAddCondition(C_Belief_LG, ConditionalTools.CreateLogicCondition("WRM_LGBeliefsCond", LGConds));

            DialogTools.CueAddContinue(PostQuest, C_Belief, 0);
            DialogTools.CueAddContinue(PostQuest, C_Belief_L, 0);
            DialogTools.CueAddContinue(PostQuest, C_Belief_G, 0);
            DialogTools.CueAddContinue(PostQuest, C_Belief_LG, 0);

            // Vellexia conversation commentary
            var affection = Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_WoljifAffection");

            var C_RaisedEyebrow = DialogTools.CreateCue("WRM_vel_c_RaisedEyebrow");
            var C_InsideJoke = DialogTools.CreateCue("WRM_vel_c_InsideJoke");
            var C_OddLook = DialogTools.CreateCue("WRM_vel_c_OddLook");
            var C_Duty = DialogTools.CreateCue("WRM_vel_c_Duty");
            var C_TheyDoThat = DialogTools.CreateCue("WRM_vel_c_TheyDoThat");

            var Answer0027 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("5ffd00a5135de9a4f899523c1773fd87");
            var Cue0031 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("082314f3b812e1745913f9ba4742c829");
            var Answer0028 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("67678955c64ac674d8a16b937ff5e70e");
            var Cue0032 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("b9ab93e6f6705694cad11d9fbd45d17d");
            var Answer0040 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>("7162ac08d4487814eae83ae05cf75508");
            var Cue0045 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("756c9ad94fe845e4ea5f1d37e6032c1c");
            var Cue0058 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("4788c8731a4717d48a618373423bef95");
            var Cue0060 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("9c1cd9b01cdf28e4a8af8961a6ab09e3");
            var Cue0083 = Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("bb4abb113e87e264b943c55c924603c9");

            var callback_cue = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>("WRM_2b_c_LoveIsTrouble");
            var affection7 = ConditionalTools.CreateFlagCheck("WRM_Flag_Affection7", affection, 7, 20);
            var affection9 = ConditionalTools.CreateFlagCheck("WRM_Flag_Affection7", affection, 9, 20);
            Kingmaker.ElementsSystem.Condition[] Affection7Conds = { affection7, romanceactivecond };
            Kingmaker.ElementsSystem.Condition[] Affection9Conds = { affection9, romanceactivecond };
            var affection7active = ConditionalTools.CreateLogicCondition("WRM_Affection7", Affection7Conds);
            var affection9active = ConditionalTools.CreateLogicCondition("WRM_Affection9", Affection9Conds);
            var sawInsideJokeLine = ConditionalTools.CreateCueSeenCondition("WRM_SeenTavernLine", callback_cue);

            DialogTools.CueAddCondition(C_RaisedEyebrow, affection7active);
            DialogTools.AnswerAddNextCue(Answer0027, C_RaisedEyebrow, 0);
            DialogTools.CueAddContinue(C_RaisedEyebrow, Cue0031);
            DialogTools.CueSetSpeaker(C_RaisedEyebrow, Companions.Woljif);

            DialogTools.CueAddCondition(C_InsideJoke, sawInsideJokeLine);
            DialogTools.AnswerAddNextCue(Answer0028, C_InsideJoke, 0);
            DialogTools.CueAddContinue(C_InsideJoke, Cue0032);
            DialogTools.CueSetSpeaker(C_InsideJoke, Companions.Woljif);

            DialogTools.CueAddCondition(C_OddLook, affection7active);
            DialogTools.AnswerAddNextCue(Answer0040, C_OddLook, 0);
            DialogTools.CueAddContinue(C_OddLook, Cue0045);
            DialogTools.CueSetSpeaker(C_OddLook, Companions.Woljif);

            DialogTools.CueAddCondition(C_Duty, affection9active);
            DialogTools.CueAddContinue(Cue0058, C_Duty, 0);
            DialogTools.CueAddContinue(C_Duty, Cue0083);
            DialogTools.CueSetSpeaker(C_Duty, Companions.Woljif);

            DialogTools.CueAddCondition(C_TheyDoThat, affection7active);
            DialogTools.CueAddContinue(Cue0060, C_TheyDoThat, 0);
            DialogTools.CueAddContinue(C_TheyDoThat, Cue0083);
            DialogTools.CueSetSpeaker(C_TheyDoThat, Companions.Woljif);
        }
    }
}