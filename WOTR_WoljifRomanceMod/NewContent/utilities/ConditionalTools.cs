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