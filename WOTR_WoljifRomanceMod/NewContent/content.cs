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

            // Simple conditional cue
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

            // Complex conditional cue
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

            // Conditional answers
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

            //Skill checks
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
    }
}