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

//##########################################################################################################################
// CONDITIONAL TOOLS
// Helper functions to generate Conditionals. Conditions work like this: a ConditionChecker object contains an indicator of
// whether it should "and" or "or" the values, and an array of Condition objects. A condition object is either of a type
// that does a true/false check, with the nature of the check being determined by the type of object and the actual value
// checked for determined by a variable specific to that object type (for instance, the PcRace condition has a Race
// variable that can be set to e.g. "Tiefling"), or it can be a logic condition. A logic condition contains another
// ConditionChecker object as its special variable, so you can "nest" the conditions to achieve something like
// "A and (B or C)".
//
// Because every condition is structured differently there's really no good way to make a wrapper to edit them. It's best
// to just do it using the bp=>{} method:
//      var myconditional = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PcRace>
//                          ("isplayertiefling", bp => {bp.Race = Kingmaker.Blueprints.Race.Tiefling;});
//##########################################################################################################################

namespace WOTR_WoljifRomanceMod
{
    public static class ConditionalTools
    {
        /*******************************************************************************************************************
         * GENERIC CONDITION TEMPLATE
         * Used as is to create one-off instances of unusual condition types or as the basis for the other ConditionalTools
         * functions. If you're using a conditional type more than once, it should probably have its own function.
         *   name:      Deprecated; does nothing but would be too much of a pain to remove.
         *   not:       If true, inverts the condition. E.G. If you create a Class Check condition for Paladin, it will
         *              normally be true if the player is a paladin. If you set Not to true, then the condition will be
         *              true if the player is NOT a paladin.
         ******************************************************************************************************************/
        public static T CreateCondition<T>([NotNull] string name, Action<T> init = null) 
                        where T : Kingmaker.ElementsSystem.Condition, new()
        {
            return CreateCondition<T>(name, false, init);
        }
        public static T CreateCondition<T>([NotNull] string name, bool Not, Action<T> init = null) 
                        where T : Kingmaker.ElementsSystem.Condition, new()
        {
            var result = (T)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(T));
            init?.Invoke(result);
            result.Not = Not;
            return result;
        }

        /*******************************************************************************************************************
         * CREATE ANSWER SELECTED CONDITION
         * Checks whether the player selected a particular answer.
         *   name:          Deprecated; does nothing.
         *   not:           Optional. Inverts the output of the check.
         *   answer:        the answer to check whether the player has selected.
         *   currentonly:   Whether to check only the current dialog. Defaults to false, which will check whether this
         *                  character has EVER selected this answer.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.AnswerSelected CreateAnswerSelectedCondition
                      (string name, Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, bool currentonly = false)
        {
            return CreateAnswerSelectedCondition(name, false, answer, currentonly);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.AnswerSelected CreateAnswerSelectedCondition
                      (string name, bool not, Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, 
                       bool currentonly = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.AnswerSelected>
                         (name, not, bp =>
                {
                    bp.m_Answer = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                  <Kingmaker.Blueprints.BlueprintAnswerReference>(answer);
                    bp.CurrentDialog = currentonly;
                });
            return result;
        }

        /*******************************************************************************************************************
         * CREATE CUE SEEN CONDITION
         * Checks whether the player has seen a particular dialog cue.
         *   name:          Deprecated; does nothing.
         *   not:           Optional. Inverts the output of the check.
         *   cue:           the cue to check whether the player has seen it
         *   currentonly:   Whether to check only the current dialog. Defaults to false, which will check whether this
         *                  character has EVER seen this cue.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.CueSeen CreateCueSeenCondition
                      (string name, Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, bool currentonly = false)
        {
            return CreateCueSeenCondition(name, false, cue, currentonly);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.CueSeen CreateCueSeenCondition
                      (string name, bool not, Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, bool currentonly = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.CueSeen>(name, not, bp =>
            {
                bp.m_Cue = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                           <Kingmaker.Blueprints.BlueprintCueBaseReference>(cue);
                bp.CurrentDialog = currentonly;
            });
            return result;
        }

        /*******************************************************************************************************************
         * CREATE DIALOG SEEN CONDITION
         * Checks whether the player has seen a particular dialog.
         *   name:      Deprecated; does nothing.
         *   dialog:    The dialog to check whether the player has seen.
         *   not:       Optional. Inverts the output of the check.
         ******************************************************************************************************************/
        public static Kingmaker.Assets.Designers.EventConditionActionSystem.Conditions.DialogSeen CreateDialogSeenCondition
                      (string name, Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Assets.Designers.EventConditionActionSystem.Conditions.DialogSeen>
                         (name, not, bp =>
                {
                    bp.m_Dialog = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                  <Kingmaker.Blueprints.BlueprintDialogReference>(dialog);
                });
            return result;
        }

        /*******************************************************************************************************************
         * CREATE ALIGNMENT CONDITION
         * Checks whether the player has a particular alignment component - that is, it checks whether the player is, for
         * example, Good, not whether the player is Neutral Good.
         *   name:      Deprecated; does nothing.
         *   alignment: The alignment component to check for: Neutral (on either axis), Good, Evil, Chaotic, or Lawful.
         *              Takes a string or an AlignmentComponent enum.
         *   not:       Optional. Inverts the output of the check.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.AlignmentCheck CreateAlignmentCondition
                      (string name, string alignment, bool not = false)
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
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.AlignmentCheck CreateAlignmentCondition
                      (string name, Kingmaker.Enums.AlignmentComponent alignment, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.AlignmentCheck>
                         (name, not, bp =>
                {
                    bp.Alignment = alignment;
                });
            return result;
        }

        /*******************************************************************************************************************
         * CREATE CAMP EVENT CHECK CONDITION
         * Checks whether a particular camp encounter is in the player's queued encounters list.
         *   name:      Deprecated; does nothing.
         *   encounter: the encounter to check whether it already exists.
         *   not:       Optional. Inverts the output of the check.
         ******************************************************************************************************************/
        public static CampEventExists CreateCampEventCheck(string name, 
                                      Kingmaker.RandomEncounters.Settings.BlueprintCampingEncounter Encounter, 
                                      bool not = false)
        {
            var result = CreateCondition<CampEventExists>(name, not, bp =>
            {
                bp.Encounter = Encounter;
            });
            return result;
        }

        /*******************************************************************************************************************
         * CREATE CLASS CONDITION
         * Checks whether the player has a significant number of levels in a particular class. The fact that it looks for
         * a significant number means it won't come back true if you just have a one-level dip.
         *   name:      Deprecated; does nothing.
         *   charclass: The class to check for.
         *   not:       Optional. Inverts the output of the check.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.PlayerSignificantClassIs CreateClassCheck
                      (string name, Kingmaker.Blueprints.Classes.BlueprintCharacterClass charclass, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.PlayerSignificantClassIs>
                         (name, not, bp =>
                {
                    bp.CharacterClass = charclass;
                });
            return result;
        }

        /*******************************************************************************************************************
         * CREATE COMPANION AVAILABLE CONDITION
         * Checks whether a companion is around, but not necessarily in your active party.
         *   name:      Deprecated; does nothing.
         *   companion: Who to check for.
         *   not:       Optional. Inverts the output of the check.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.CompanionInParty 
                      CreateCompanionAvailableCondition(string name, Companions companion, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.CompanionInParty>
                         (name, not, bp =>
                {
                    bp.m_companion = CompanionTools.GetCompanionReference(companion);
                    bp.MatchWhenActive = true;
                    bp.MatchWhenDetached = true;
                    bp.MatchWhenRemote = true;
                });
            return result;
        }

        /*******************************************************************************************************************
         * CREATE COMPANION DEAD CONDITION
         * Checks whether a companion is dead.
         *   name:      Deprecated; does nothing.
         *   companion: Who to check for.
         *   not:       Optional. Inverts the output of the check.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.CompanionInParty 
                      CreateCompanionDeadCondition(string name, Companions companion, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.CompanionInParty>
                         (name, not, bp =>
                {
                    bp.m_companion = CompanionTools.GetCompanionReference(companion);
                    bp.MatchWhenDead = true;
                });
            return result;
        }

        /*******************************************************************************************************************
         * CREATE COMPANION IN PARTY CONDITION
         * Checks whether a companion is in the current party.
         *   name:      Deprecated; does nothing.
         *   companion: Who to check for.
         *   not:       Optional. Inverts the output of the check.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.CompanionInParty 
                      CreateCompanionInPartyCondition(string name, Companions companion, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.CompanionInParty>
                         (name, not, bp =>
                {
                    bp.m_companion = CompanionTools.GetCompanionReference(companion);
                    bp.MatchWhenActive = true;
                });
            return result;
        }

        /*******************************************************************************************************************
         * CREATE CURRENT AREA IS CONDITION
         * Checks whether you're in a particular area.
         *   name:      Deprecated; does nothing.
         *   area:      Place to check whether you're in it.
         *   not:       Optional. Inverts the output of the check.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.CurrentAreaIs CreateCurrentAreaIsCondition
                      (string name, Kingmaker.Blueprints.Area.BlueprintArea area, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.CurrentAreaIs>
                         (name, not, bp =>
                {
                    bp.m_Area = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                <Kingmaker.Blueprints.BlueprintAreaReference>(area);
                });
            return result;
        }

        /*******************************************************************************************************************
         * CREATE ETUDE CONDITION
         * Checks the status of an etude.
         *   name:      Deprecated; does nothing.
         *   etude:     The etude to check.
         *   status:    What status to check for. Accepts a string or an EtudeStatus enum. Can be
         *                Not Started:          Has not been triggered
         *                Started:              Has been triggered but activation conditions have not been met. Will
         *                                      become Playing when the conditions are met. This is a sort of waiting state
         *                                      for something that WILL happen but is not currently happening.
         *                Playing:              Etude is triggered, active, and running.
         *                CompletionInProgress: Transitioning from Playing to Completed. This status is rarely seen.
         *                Completed:            Etude has been ended.
         *   not:       Optional. Inverts the output of the check.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.EtudeStatus CreateEtudeCondition
                      (string name, Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, string status, bool not = false)
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
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.EtudeStatus CreateEtudeCondition
                      (string name, Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, EtudeTools.EtudeStatus status, 
                       bool not=false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.EtudeStatus>(name,not);
            result.m_Etude = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                             <Kingmaker.Blueprints.BlueprintEtudeReference>(etude);
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

        /*******************************************************************************************************************
         * CREATE ETUDE GROUP CONDITION
         * Checks whether other etudes of a particular group are currently playing.
         *   name:      Deprecated; does nothing.
         *   group:     The group to check for other members of.
         *   not:       Optional. Inverts the output of the check.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.AnotherEtudeOfGroupIsPlaying 
                      CreateEtudeGroupCondition(string name, 
                                                Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup group, 
                                                bool not = false)
        {
            var result = CreateCondition
                         <Kingmaker.Designers.EventConditionActionSystem.Conditions.AnotherEtudeOfGroupIsPlaying>
                         (name, not);
            result.m_Group = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                             <BlueprintEtudeConflictingGroupReference>(group);
            return result;
        }

        /*******************************************************************************************************************
         * CREATE FLAG CHECK CONDITION
         * Checks whether a flag's value is within a particular range.
         *   name:      Deprecated; does nothing.
         *   min/max:   the range to check that the flag is within.
         *   not:       Optional. Inverts the output of the check.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.FlagInRange CreateFlagCheck
                      (string name, Kingmaker.Blueprints.BlueprintUnlockableFlag flag, int min, int max, bool not = false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.FlagInRange>
                         (name, not, bp =>
                {
                    bp.m_Flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                <Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(flag);
                    bp.MinValue = min;
                    bp.MaxValue = max;
                });
            return result;
        }

        /*******************************************************************************************************************
         * CREATE FLAG LOCK CONDITION
         * Checks the status of the lock on a flag.
         *   name:      Deprecated; does nothing.
         *   flag:      The flag to check the status of.
         *   locked:    If true, checks whether the flag is locked. If false, checks whether the flag is unlocked.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.FlagUnlocked CreateFlagLockCheck
                      (string name, Kingmaker.Blueprints.BlueprintUnlockableFlag flag, bool locked)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.FlagUnlocked>
                         (name, locked, bp =>
                {
                    bp.ConditionFlag = flag;
                });
            return result;
        }

        /*******************************************************************************************************************
         * CREATE CONDITION
         * Checks the player's status on a specific quest.
         *   name:      Deprecated; does nothing.
         *   status:    Accepts a string or QuestState enum. Can be Started, Completed, or Failed.
         *   quest:     The quest to check.
         *   not:       Optional. Inverts the output of the check.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.QuestStatus CreateQuestStatusCondition
                      (string name, string status, Kingmaker.Blueprints.Quests.BlueprintQuest Quest, bool not = false)
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
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.QuestStatus CreateQuestStatusCondition
                      (string name, Kingmaker.AreaLogic.QuestSystem.QuestState status, 
                       Kingmaker.Blueprints.Quests.BlueprintQuest Quest, bool not = false)
        {
            var questref = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintQuestReference>(Quest);
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.QuestStatus>
                         (name, not, bp =>
                {
                    bp.m_Quest = questref;
                    bp.State = status;
                });
            return result;
        }

        /*******************************************************************************************************************
         * CREATE FACT CONDITION
         * Checks whether a companion has a specific UnitFact. UnitFacts are traits like what god they worship.
         *   name:      Deprecated; does nothing.
         *   owner:     The blueprint that owns this condition and the included unit evaluator.
         *   fact:      the fact to check for.
         *   companion: the companion to check.
         *   not:       Optional. Inverts the output of the check.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.HasFact CreateFactCondition
                      (string name, Kingmaker.Blueprints.SimpleBlueprint owner, 
                       Kingmaker.Blueprints.Facts.BlueprintUnitFact fact, Companions companion, bool not=false)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.HasFact>(name, not, bp =>
            {
                bp.m_Fact = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                            <Kingmaker.Blueprints.BlueprintUnitFactReference>(fact);
                bp.Unit = CompanionTools.GetCompanionEvaluator(companion, owner);
            });
            return result;
        }


        /*******************************************************************************************************************
         * CREATE LOGIC CONDITION
         * Creates complex logic based on multiple conditions, such as "player is tiefling AND player is female" or
         * "Player worships Iomedae OR player worships Shelyn".
         *   name:          Deprecated; does nothing.
         *   not:           Optional. Inverts the output of the check.
         *   op:            Optional. Determines whether the logic condition uses AND (all conditions are true) or OR (at 
         *                  least one condition is true) logic. Defaults to AND.
         *   conditions:    An array of conditions to check.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.OrAndLogic CreateLogicCondition
                      ([NotNull] string name, bool Not, params Kingmaker.ElementsSystem.Condition[] conditions)
        {
            return CreateLogicCondition(name, Not, Kingmaker.ElementsSystem.Operation.And, conditions);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.OrAndLogic CreateLogicCondition
                      ([NotNull] string name, params Kingmaker.ElementsSystem.Condition[] conditions)
        {
            return CreateLogicCondition(name, false, Kingmaker.ElementsSystem.Operation.And, conditions);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.OrAndLogic CreateLogicCondition
                      ([NotNull] string name, Kingmaker.ElementsSystem.Operation op, 
                       params Kingmaker.ElementsSystem.Condition[] conditions)
        {
            return CreateLogicCondition(name, false, op, conditions);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Conditions.OrAndLogic CreateLogicCondition
                      ([NotNull] string name, bool Not, Kingmaker.ElementsSystem.Operation op, 
                       params Kingmaker.ElementsSystem.Condition[] conditions)
        {
            var result = CreateCondition<Kingmaker.Designers.EventConditionActionSystem.Conditions.OrAndLogic>(name, Not);
            result.ConditionsChecker = CreateChecker(op);
            for (int i = 0; i < conditions.Length; i++)
            {
                CheckerAddCondition(result.ConditionsChecker, conditions[i]);
            }
            return result;
        }
        /*******************************************************************************************************************
         * LOGIC CONDITION: ADD CONDITION
         * Adds an additional condition to an existing Logic Condition.
         ******************************************************************************************************************/
        public static void LogicAddCondition(Kingmaker.Designers.EventConditionActionSystem.Conditions.OrAndLogic logic, 
                                             Kingmaker.ElementsSystem.Condition condition)
        {
            CheckerAddCondition(logic.ConditionsChecker, condition);
        }

        /*******************************************************************************************************************
         * CREATE CONDITION CHECKER
         * Builds a ConditionsChecker object, the element used under the hood to check conditions. Generally you shouldn't
         * need to call this yourself, as it's mostly a building block for other functions, but you can if you want to
         * construct a full logic tree in a void and then insert it into something else.
         ******************************************************************************************************************/
        public static Kingmaker.ElementsSystem.ConditionsChecker CreateChecker
                      (Kingmaker.ElementsSystem.Operation op = Kingmaker.ElementsSystem.Operation.And, 
                       Action<Kingmaker.ElementsSystem.ConditionsChecker> init = null)
        {
            var result = new Kingmaker.ElementsSystem.ConditionsChecker();
            init?.Invoke(result);
            result.Operation = op;
            return result;
        }
        /*******************************************************************************************************************
         * CONDITION CHECKER: ADD CONDITION
         * Adds a new condition to an existing condition checker.
         ******************************************************************************************************************/
        public static void CheckerAddCondition(Kingmaker.ElementsSystem.ConditionsChecker checker, 
                                               Kingmaker.ElementsSystem.Condition condition)
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