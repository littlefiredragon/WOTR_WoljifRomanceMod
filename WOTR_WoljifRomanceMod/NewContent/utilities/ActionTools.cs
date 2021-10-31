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
    public static class ActionTools
    {
        public static T GenericAction<T>([NotNull] string name, Action<T> init = null) where T : Kingmaker.ElementsSystem.GameAction, new()
        {
            var result = (T) Kingmaker.ElementsSystem.Element.CreateInstance(typeof(T));
            result.name = name;
            init?.Invoke(result);
            return result;
        }

        //Creating a conditional action can take either a condition directly, for ease of creating simple checkers, or it can take a pre-constructed conditionchecker tree.
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional ConditionalAction([NotNull] string name, Kingmaker.ElementsSystem.Condition condition)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional>(name);
            result.ConditionsChecker = new Kingmaker.ElementsSystem.ConditionsChecker();
            result.IfTrue = DialogTools.EmptyActionList;
            result.IfFalse = DialogTools.EmptyActionList;
            return result;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional ConditionalAction([NotNull] string name, Kingmaker.ElementsSystem.ConditionsChecker conditionchecker)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional>(name);
            result.ConditionsChecker = conditionchecker;
            result.IfTrue = DialogTools.EmptyActionList;
            result.IfFalse = DialogTools.EmptyActionList;
            return result;
        }
        // Because of the multitude of scenarios for actions in the true and false list, I thought it easier to not try to include them in the constructor.
        // Instead, you just have to call a couple additional functions.
        public static void ConditionalActionOnTrue(Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional logicaction, Kingmaker.ElementsSystem.ActionList actionlist)
        {
            logicaction.IfTrue = actionlist;
        }
        public static void ConditionalActionOnTrue(Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional logicaction, params Kingmaker.ElementsSystem.GameAction[] actions)
        {
            if (logicaction.IfTrue == DialogTools.EmptyActionList)
            {//Make a brand new action list
                logicaction.IfTrue = new Kingmaker.ElementsSystem.ActionList();
            }
            var currentlen = 0;
            var paramlen = actions.Length;
            if (logicaction.IfTrue.Actions == null)
            {
                logicaction.IfTrue.Actions = new Kingmaker.ElementsSystem.GameAction[paramlen];
            }
            else
            {
                currentlen = logicaction.IfTrue.Actions.Length;
                Array.Resize(ref logicaction.IfTrue.Actions, currentlen + paramlen);
            }
            for (int i = 0; i<paramlen; i++)
            {
                logicaction.IfTrue.Actions[currentlen + i] = actions[i];
            }
        }
        public static void ConditionalActionOnFalse(Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional logicaction, Kingmaker.ElementsSystem.ActionList actionlist)
        {
            logicaction.IfFalse = actionlist;
        }
        public static void ConditionalActionOnFalse(Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional logicaction, params Kingmaker.ElementsSystem.GameAction[] actions)
        {
            if (logicaction.IfFalse == DialogTools.EmptyActionList)
            {//Make a brand new action list
                logicaction.IfFalse = new Kingmaker.ElementsSystem.ActionList();
            }
            var currentlen = 0;
            var paramlen = actions.Length;
            if (logicaction.IfFalse.Actions == null)
            {
                logicaction.IfFalse.Actions = new Kingmaker.ElementsSystem.GameAction[paramlen];
            }
            else
            {
                currentlen = logicaction.IfFalse.Actions.Length;
                Array.Resize(ref logicaction.IfFalse.Actions, currentlen + paramlen);
            }
            for (int i = 0; i < paramlen; i++)
            {
                logicaction.IfFalse.Actions[currentlen + i] = actions[i];
            }
        }
    }
}