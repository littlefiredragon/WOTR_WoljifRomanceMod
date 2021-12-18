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
        public static T GenericAction<T>(Action<T> init = null) where T : Kingmaker.ElementsSystem.GameAction, new()
        {
            var result = (T)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(T));
            init?.Invoke(result);
            return result;
        }

        public static Kingmaker.ElementsSystem.ActionList MakeList(Kingmaker.ElementsSystem.GameAction action)
        {
            var result = new Kingmaker.ElementsSystem.ActionList();
            Array.Resize(ref result.Actions, 1);
            result.Actions[0] = action;
            return result;
        }
        public static Kingmaker.ElementsSystem.ActionList MakeList(params Kingmaker.ElementsSystem.GameAction[] actions)
        {
            var result = new Kingmaker.ElementsSystem.ActionList();
            var len = actions.Length;
            Array.Resize(ref result.Actions, len);
            for (int i = 0; i < len; i++)
            {
                result.Actions[i] = actions[i];
            }
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.IncrementFlagValue IncrementFlagAction(Kingmaker.Blueprints.BlueprintUnlockableFlag flag, int amount = 1)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.IncrementFlagValue>(bp => 
            { 
                bp.m_Flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(flag);
                bp.Value = new Kingmaker.Designers.EventConditionActionSystem.Evaluators.IntConstant { Value = amount };
                bp.UnlockIfNot = true;
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.TeleportParty TeleportAction(string exitposition, Kingmaker.ElementsSystem.ActionList afterTeleport = null)
        {
            var result = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TeleportParty>(bp =>
            {
                bp.m_exitPositon = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintAreaEnterPointReference>(Resources.GetBlueprint<Kingmaker.Blueprints.Area.BlueprintAreaEnterPoint>(exitposition));
                if (afterTeleport == null)
                {
                    bp.AfterTeleport = DialogTools.EmptyActionList;
                }
                else
                {
                    bp.AfterTeleport = afterTeleport;
                    bp.AutoSaveMode = Kingmaker.EntitySystem.Persistence.AutoSaveMode.None;
                }
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit TranslocateAction(Companions unit, FakeLocator position)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit>(bp =>
            {
                bp.Unit = CommandTools.getCompanionEvaluator(unit);
                bp.translocatePositionEvaluator = position;
                bp.m_CopyRotation = true;
                bp.translocateOrientationEvaluator = position.GetRotation();
            });
            return result;
        }
        /*public static Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit TranslocateAction(Companions unit, Kingmaker.Blueprints.EntityReference position, bool setrotation = true)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit>(bp =>
            {
                bp.Unit = CommandTools.getCompanionEvaluator(unit);
                bp.translocatePosition = position;
                bp.m_CopyRotation = setrotation;
            });
            return result;
        }*/

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit HideUnitAction(Companions unit, bool unhide = false)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit>(bp =>
            {
                bp.Target = CommandTools.getCompanionEvaluator(unit);
                bp.Unhide = unhide;
            });
            return action;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene PlayCutsceneAction(Kingmaker.AreaLogic.Cutscenes.Cutscene cutscene, SimpleBlueprint owner = null)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene>(bp =>
            {
                bp.m_Cutscene = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.CutsceneReference>(cutscene);
                bp.Owner = owner;
                bp.Parameters = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter();
            });
            return action;
        }
        public static void CutsceneActionAddParameter(Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene action, string name, string type, Kingmaker.ElementsSystem.Evaluator eval)
        {
            Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.Float;
            switch (type)
            {
                case "Unit":
                case "unit":
                    paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.Unit;
                    break;
                case "Locator":
                case "locator":
                    paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.Locator;
                    break;
                case "MapObject":
                case "mapobject":
                    paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.MapObject;
                    break;
                case "Position":
                case "position":
                    paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.Position;
                    break;
                case "Blueprint":
                case "blueprint":
                    paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.Blueprint;
                    break;
                case "Float":
                case "float":
                    paramtype = Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType.Float;
                    break;
            }
            CutsceneActionAddParameter(action, name, paramtype, eval);
        }
            public static void CutsceneActionAddParameter(Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene action, string name, Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterType type, Kingmaker.ElementsSystem.Evaluator eval)
        {
            var parameter = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterEntry();
            parameter.Name = name;
            parameter.Type = type;
            parameter.Evaluator = eval;

            var len = 0;
            if (action.Parameters.Parameters == null)
            {
                action.Parameters.Parameters = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterEntry[1];
            }
            else
            {
                len = action.Parameters.Parameters.Length;
                Array.Resize(ref action.Parameters.Parameters, len + 1);
            }
            action.Parameters.Parameters[len] = parameter;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StopCutscene StopCutsceneAction(Kingmaker.AreaLogic.Cutscenes.Cutscene cutscene)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StopCutscene>(bp =>
            {
                bp.m_Cutscene = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.CutsceneReference>(cutscene);
            });
            return action;
        }


        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StartEtude StartEtudeAction(BlueprintEtudeReference etude)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StartEtude>(bp =>
            {
                bp.Etude = etude;
            });
            return action;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StartEtude StartEtudeAction(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude)
        {
            return StartEtudeAction(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintEtudeReference>(etude));
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.CompleteEtude CompleteEtudeAction(BlueprintEtudeReference etude)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.CompleteEtude>(bp =>
            {
                bp.Etude = etude;
            });
            return action;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.CompleteEtude CompleteEtudeAction(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude)
        {
            return CompleteEtudeAction(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintEtudeReference>(etude));
        }

        public static Kingmaker.Kingdom.Actions.KingdomActionStartEvent StartCommandRoomEventAction(Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent commandevent)
        {
            var action = GenericAction<Kingmaker.Kingdom.Actions.KingdomActionStartEvent>(bp =>
            {
                bp.m_Event = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintKingdomEventBaseReference>(commandevent);
                bp.m_Region = (BlueprintRegionReference)null;
            });
            return action;
        }
        public static Kingmaker.Kingdom.Actions.KingdomActionRemoveEvent EndCommandRoomEventAction(Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent commandevent)
        {
            var action = GenericAction<Kingmaker.Kingdom.Actions.KingdomActionRemoveEvent>(bp =>
            {
                bp.m_EventBlueprint = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintKingdomEventBaseReference>(commandevent);
                bp.CancelIfInProgress = true;
                bp.AllIfMultiple = true;
            });
            return action;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.AddCampingEncounter AddCampEventAction(Kingmaker.RandomEncounters.Settings.BlueprintCampingEncounter Encounter)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.AddCampingEncounter>(bp =>
            {
                bp.m_Encounter = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintCampingEncounterReference>(Encounter);
            });
            return action;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.RemoveCampingEncounter RemoveCampEventAction(Kingmaker.RandomEncounters.Settings.BlueprintCampingEncounter Encounter)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.RemoveCampingEncounter>(bp =>
            {
                bp.m_Encounter = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintCampingEncounterReference>(Encounter);
            });
            return action;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StartDialog StartDialogAction(Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, Companions owner)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StartDialog>(bp =>
            {
                bp.m_Dialogue = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintDialogReference>(dialog);
                bp.DialogueOwner = CommandTools.getCompanionEvaluator(owner);
            });
            return action;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StopCustomMusic StopMusic()
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StopCustomMusic>();
            return action;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCustomMusic StartMusic(string trackname)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCustomMusic>(bp =>
            {
                bp.MusicEventStart = "MUS_" + trackname + "_Play";
                bp.MusicEventStop = "MUS_" + trackname + "_Stop";
            });
            return action;
        }

        //Creating a conditional action can take either a condition directly, for ease of creating simple checkers, or it can take a pre-constructed conditionchecker tree.
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional ConditionalAction(Kingmaker.ElementsSystem.Condition condition)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional>();
            result.ConditionsChecker = new Kingmaker.ElementsSystem.ConditionsChecker();
            result.IfTrue = DialogTools.EmptyActionList;
            result.IfFalse = DialogTools.EmptyActionList;
            return result;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional ConditionalAction(Kingmaker.ElementsSystem.ConditionsChecker conditionchecker)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional>();
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