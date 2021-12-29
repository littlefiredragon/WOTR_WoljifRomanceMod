using TabletopTweaks;
using TabletopTweaks.Config;
using TabletopTweaks.Utilities;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using Kingmaker.Utility;
using JetBrains.Annotations;
using System;

namespace WOTR_WoljifRomanceMod
{
    /* Conditions work like this: a ConditionChecker object contains an indicator of whether it should "and" or "or" the values, and an array 
       of Condition objects. A condition object is either of a type that does a true/false check, with the nature of the check being 
       determined by the type of object and the actual value checked for determined by a variable specific to that object type (for instance, 
       the PcRace condition has a Race variable that can be set to e.g. "Tiefling"), or it can be a logic condition. A logic condition contains 
       another ConditionChecker object as its special variable, so you can "nest" the conditions to achieve something like "A and (B or C)". */
    public static class ConditionalTools
    {
        public static T CreateCondition<T>([NotNull] string name, Action<T> init = null) where T : Kingmaker.ElementsSystem.Condition, new()
        {
            return CreateCondition<T>(name, false, init);
        }
        public static T CreateCondition<T>([NotNull] string name, bool Not, Action<T> init = null) where T : Kingmaker.ElementsSystem.Condition, new()
        {
            /*var result = new T();
            result.Not = Not;
            init?.Invoke(result);
            return result;*/

            var result = (T)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(T));
            init?.Invoke(result);
            result.Not = Not;
            return result;
        }

        /*public static Kingmaker.Designers.EventConditionActionSystem.Conditions.OrAndLogic CreateSafeTimerConditional(string name, string timername, Kingmaker.AreaLogic.Etudes.BlueprintEtude pairedetude, int days, bool not = false)
        {
            Kingmaker.ElementsSystem.Condition[] conditions = 
                { 
                    CreateEtudeCondition(name + "_etudeplaying", pairedetude, "playing"),
                    CreateTimerConditional(name + "_timerup", timername, days)
                };
            var result = CreateLogicCondition(name, not, conditions);
            return result;
        }

        public static TimerConditional CreateTimerConditional(string name, string timername, int days, bool not = false)
        {
            var result = CreateCondition<TimerConditional>(name, not, bp =>
            {
                bp.timername = timername;
                bp.dayspassed = days;
            });
            return result;
        }*/

        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.EtudeStatus CreateEtudeCondition(string name, Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, string status, bool not = false)
        {
            EtudeTools.EtudeStatus statevar = EtudeTools.EtudeStatus.NotStarted;
            switch (status)
            {
                case "notstarted":
                case "notStarted":
                case "NotStarted":
                    statevar = EtudeTools.EtudeStatus.NotStarted;
                    break;
                case "started":
                case "Started":
                    statevar = EtudeTools.EtudeStatus.Started;
                    break;
                case "playing":
                case "Playing":
                    statevar = EtudeTools.EtudeStatus.Playing;
                    break;
                case "CompletionInProgress":
                case "completionInProgress":
                case "completioninprogress":
                case "completing":
                case "Completing":
                    statevar = EtudeTools.EtudeStatus.CompletionInProgress;
                    break;
                case "Completed":
                case "completed":
                case "Complete":
                case "complete":
                    statevar = EtudeTools.EtudeStatus.Completed;
                    break;
            }
            return CreateEtudeCondition(name, etude, statevar, not);
        }
            public static Kingmaker.Designers.EventConditionActionSystem.Conditions.EtudeStatus CreateEtudeCondition(string name, Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, EtudeTools.EtudeStatus status, bool not=false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.EtudeStatus>(name,not);
            result.m_Etude = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintEtudeReference>(etude);
            switch (status)
            {
                case EtudeTools.EtudeStatus.NotStarted:
                    result.NotStarted = true;
                    break;
                case EtudeTools.EtudeStatus.Started:
                    result.Started = true;
                    break;
                case EtudeTools.EtudeStatus.Playing:
                    result.Playing = true;
                    break;
                case EtudeTools.EtudeStatus.CompletionInProgress:
                    result.CompletionInProgress = true;
                    break;
                case EtudeTools.EtudeStatus.Completed:
                    result.Completed = true;
                    break;
            }
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.FlagUnlocked CreateFlagLockCheck(string name, Kingmaker.Blueprints.BlueprintUnlockableFlag flag, bool locked)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.FlagUnlocked>(name, locked, bp =>
            {
                bp.ConditionFlag = flag;
            });
            return result;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.FlagInRange CreateFlagCheck(string name, Kingmaker.Blueprints.BlueprintUnlockableFlag flag, int min, int max, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.FlagInRange>(name, not, bp =>
            {
                bp.m_Flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(flag);
                bp.MinValue = min;
                bp.MaxValue = max;
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.AnotherEtudeOfGroupIsPlaying CreateEtudeGroupCondition(string name, Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup group, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.AnotherEtudeOfGroupIsPlaying>(name, not);
            result.m_Group = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintEtudeConflictingGroupReference>(group);
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.QuestStatus CreateQuestStatusCondition(string name, string status, Kingmaker.Blueprints.Quests.BlueprintQuest Quest, bool not = false)
        {
            Kingmaker.AreaLogic.QuestSystem.QuestState statevar = Kingmaker.AreaLogic.QuestSystem.QuestState.None;
            switch (status)
            {
                case "started":
                case "Started":
                    statevar = Kingmaker.AreaLogic.QuestSystem.QuestState.Started;
                    break;
                case "complete":
                case "Complete":
                case "completed":
                case "Completed":
                    statevar = Kingmaker.AreaLogic.QuestSystem.QuestState.Completed;
                    break;
                case "failed":
                case "Failed":
                    statevar = Kingmaker.AreaLogic.QuestSystem.QuestState.Failed;
                    break;
            }
            return CreateQuestStatusCondition(name, statevar, Quest, not);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.QuestStatus CreateQuestStatusCondition(string name, Kingmaker.AreaLogic.QuestSystem.QuestState status, Kingmaker.Blueprints.Quests.BlueprintQuest Quest, bool not = false)
        {
            var questref = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintQuestReference>(Quest);
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.QuestStatus>(name, not, bp =>
            {
                bp.m_Quest = questref;
                bp.State = status;
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.CueSeen CreateCueSeenCondition(string name, Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, bool currentonly = false)
        {
            return CreateCueSeenCondition(name, false, cue, currentonly);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.CueSeen CreateCueSeenCondition(string name, bool not, Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, bool currentonly = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.CueSeen>(name, not, bp =>
                {
                    bp.m_Cue = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintCueBaseReference>(cue);
                    bp.CurrentDialog = currentonly;
                });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.AnswerSelected CreateAnswerSelectedCondition(string name, Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, bool currentonly = false)
        {
            return CreateAnswerSelectedCondition(name, false, answer, currentonly);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.AnswerSelected CreateAnswerSelectedCondition(string name, bool not, Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, bool currentonly = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.AnswerSelected>(name, not, bp =>
            {
                bp.m_Answer = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintAnswerReference>(answer);
                bp.CurrentDialog = currentonly;
            });
            return result;
        }

        public static Kingmaker.Assets.Designers.EventConditionActionSystem.Conditions.DialogSeen CreateDialogSeenCondition(string name, Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Assets.Designers.EventConditionActionSystem.Conditions.DialogSeen>(name, not, bp =>
            {
                bp.m_Dialog = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintDialogReference>(dialog);
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.CompanionInParty CreateCompanionInPartyCondition(string name, Companions companion, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.CompanionInParty>(name, not, bp => 
            {
                bp.m_companion = CommandTools.GetCompanionReference(companion);
                bp.MatchWhenActive = true;
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.CompanionInParty CreateCompanionAvailableCondition(string name, Companions companion, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.CompanionInParty>(name, not, bp =>
            {
                bp.m_companion = CommandTools.GetCompanionReference(companion);
                bp.MatchWhenActive = true;
                bp.MatchWhenDetached = true;
                bp.MatchWhenRemote = true;
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.CurrentAreaIs CreateCurrentAreaIsCondition (string name, Kingmaker.Blueprints.Area.BlueprintArea area, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.CurrentAreaIs>(name, not, bp =>
            {
                bp.m_Area = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintAreaReference>(area);
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.AlignmentCheck CreateAlignmentCondition(string name, string alignment, bool not=false)
        {
            var detected = Kingmaker.Enums.AlignmentComponent.Neutral;
            switch (alignment)
            {
                case "Neutral":
                case "neutral":
                    detected = Kingmaker.Enums.AlignmentComponent.Neutral;
                    break;
                case "Good":
                case "good":
                    detected = Kingmaker.Enums.AlignmentComponent.Good;
                    break;
                case "Evil":
                case "evil":
                    detected = Kingmaker.Enums.AlignmentComponent.Evil;
                    break;
                case "Lawful":
                case "lawful":
                    detected = Kingmaker.Enums.AlignmentComponent.Lawful;
                    break;
                case "Chaotic":
                case "chaotic":
                    detected = Kingmaker.Enums.AlignmentComponent.Chaotic;
                    break;
            }
            return CreateAlignmentCondition(name, detected, not);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.AlignmentCheck CreateAlignmentCondition (string name, Kingmaker.Enums.AlignmentComponent alignment, bool not=false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.AlignmentCheck>(name, not, bp =>
            {
                bp.Alignment = alignment;
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.HasFact CreateFactCondition(string name, Kingmaker.Blueprints.Facts.BlueprintUnitFact fact, Companions companion, bool not=false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.HasFact>(name, not, bp =>
            {
                bp.m_Fact = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnitFactReference>(fact);
                bp.Unit = CommandTools.getCompanionEvaluator(companion);
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.PlayerSignificantClassIs CreateClassCheck(string name, Kingmaker.Blueprints.Classes.BlueprintCharacterClass charclass, bool not=false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PlayerSignificantClassIs>(name, not, bp =>
            {
                bp.CharacterClass = charclass;
            });
            return result;
        }

        /* Because every condition is structured differently there's really no good way to make a wrapper to edit them. Their components tend
           to be public, though, so you can just do something like 
             var myconditional = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PcRace>("isplayertiefling");
             myconditional.Race = Kingmaker.Blueprints.Race.Tiefling;
           Alternatively, you can do it this way:
             var myconditional = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PcRace>("isplayertiefling", bp =>
                 {bp.Race = Kingmaker.Blueprints.Race.Tiefling;});
        */
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.OrAndLogic CreateLogicCondition([NotNull] string name, bool Not, params Kingmaker.ElementsSystem.Condition[] conditions)
        {
            return CreateLogicCondition(name, Not, Kingmaker.ElementsSystem.Operation.And, conditions);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.OrAndLogic CreateLogicCondition([NotNull] string name, params Kingmaker.ElementsSystem.Condition[] conditions)
        {
            return CreateLogicCondition(name, false, Kingmaker.ElementsSystem.Operation.And, conditions);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.OrAndLogic CreateLogicCondition([NotNull] string name, Kingmaker.ElementsSystem.Operation op, params Kingmaker.ElementsSystem.Condition[] conditions)
        {
            return CreateLogicCondition(name, false, op, conditions);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.OrAndLogic CreateLogicCondition([NotNull] string name, bool Not, Kingmaker.ElementsSystem.Operation op, params Kingmaker.ElementsSystem.Condition[] conditions)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.OrAndLogic>(name, Not);
            result.ConditionsChecker = CreateChecker(op);
            for (int i = 0; i < conditions.Length; i++)
            {
                CheckerAddCondition(result.ConditionsChecker, conditions[i]);
            }
            return result;
        }
        public static Kingmaker.ElementsSystem.ConditionsChecker CreateChecker(Kingmaker.ElementsSystem.Operation op = Kingmaker.ElementsSystem.Operation.And, Action<Kingmaker.ElementsSystem.ConditionsChecker> init = null)
        {
            var result = new Kingmaker.ElementsSystem.ConditionsChecker();
            init?.Invoke(result);
            result.Operation = op;
            return result;
        }
        public static void LogicAddCondition(Kingmaker.Designers.EventConditionActionSystem.Conditions.OrAndLogic logic, Kingmaker.ElementsSystem.Condition condition)
        {
            CheckerAddCondition(logic.ConditionsChecker, condition);
        }
        public static void CheckerAddCondition(Kingmaker.ElementsSystem.ConditionsChecker checker, Kingmaker.ElementsSystem.Condition condition)
        {
            var len = 0;
            if (checker.Conditions == null)
            {
                checker.Conditions = new Kingmaker.ElementsSystem.Condition[1];
            }
            else 
            {
                len = checker.Conditions.Length;
                Array.Resize(ref checker.Conditions, len + 1);
            }
            checker.Conditions[len] = condition;
        }
    }
}