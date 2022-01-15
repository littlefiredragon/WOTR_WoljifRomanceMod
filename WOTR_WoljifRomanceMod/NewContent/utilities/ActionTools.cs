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
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

//##########################################################################################################################
// ACTION TOOLS
// Helper functions to generate GameActions and ActionLists
//##########################################################################################################################

namespace WOTR_WoljifRomanceMod
{
    public static class ActionTools
    {
        /*******************************************************************************************************************
         * GENERIC ACTION TEMPLATE
         * Used as is to create one-off instances of unusual action types or as the basis for the other ActionTools
         * functions. If you're using an action type more than once, it should probably have its own function.
         ******************************************************************************************************************/
        public static T GenericAction<T>(Action<T> init = null) where T : Kingmaker.ElementsSystem.GameAction, new()
        {
            var result = (T)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(T));
            init?.Invoke(result);
            return result;
        }

        /*******************************************************************************************************************
         * MAKE LIST
         * Sometimes you need an ActionList object instead of an array of GameActions. This just takes actions and handles
         * the construction of an ActionList for you.
         ******************************************************************************************************************/
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

        /*******************************************************************************************************************
         * ADD/REMOVE CAMP EVENT ACTION
         * Adds or removes the provided encounter from the table of resting encounters when this action is run.
         * This function only creates the action; it does not construct the encounter. For that you want EventTools.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.AddCampingEncounter AddCampEventAction(
                      Kingmaker.RandomEncounters.Settings.BlueprintCampingEncounter Encounter)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.AddCampingEncounter>(bp =>
            {
                bp.m_Encounter = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                 <BlueprintCampingEncounterReference>(Encounter);
            });
            return action;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.RemoveCampingEncounter RemoveCampEventAction(
                      Kingmaker.RandomEncounters.Settings.BlueprintCampingEncounter Encounter)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.RemoveCampingEncounter>(bp =>
            {
                bp.m_Encounter = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                 <BlueprintCampingEncounterReference>(Encounter);
            });
            return action;
        }

        /*******************************************************************************************************************
         * BARK ACTION
         * Action to show a "bark" or speech bubble dialog on run. Currently written only to support party members as bark
         * sources, but you can always make one with no companion and then manually change the target unit.
         *   name:      The key/name of the dialog string in the localization json
         *   target:    Which of your companions is speaking this line. Defaults to none, and can be manually overwritten 
         *              with a different UnitEvaluator after creation if you want the line spoken by someone else.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.ShowBark BarkAction(
                      string name, Kingmaker.Blueprints.SimpleBlueprint owner, Companions target = Companions.None)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.ShowBark>(bp =>
            {
                bp.TargetUnit = CompanionTools.GetCompanionEvaluator(target, owner);
                bp.BarkDurationByText = true;
                bp.WhatToBarkShared = new Kingmaker.Localization.SharedStringAsset();
                bp.WhatToBarkShared.String = new Kingmaker.Localization.LocalizedString { m_Key = name };
                bp.WhatToBark = bp.WhatToBarkShared.String;
            });
            return result;
        }

        /*******************************************************************************************************************
         * HIDE OR UNHIDE UNIT ACTION
         * Despite the name, HideUnit can also un-hide a unit by setting the unhide boolean to true. By default, it will 
         * hide the unit. Currently written only to support party members as units to hide, but you can always make one 
         * with no companion and then manually change the target unit. 
         *   unit:      Which of your companions to hide/unhide. Can be manually overwritten with a different UnitEvaluator 
         *              after creation if you want to hide a non-companion.
         *   unhide:    defaults to false, but if true will reveal rather than hide the unit.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit HideUnitAction(
                      Kingmaker.Blueprints.SimpleBlueprint owner, Companions unit, bool unhide = false)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit>(bp =>
            {
                bp.Target = CompanionTools.GetCompanionEvaluator(unit, owner);
                bp.Unhide = unhide;
            });
            return action;
        }

        /*******************************************************************************************************************
         * HIDE OR UNHIDE WEAPONS ACTION
         * Hides or unhides the weapon models on a character without changing what's equipped. Currently written only to 
         * support party members as units to hide, but you can always make one with no companion and then manually change 
         * the target unit. 
         *   unit:      Which of your companions to affect. Can be manually overwritten with a different UnitEvaluator 
         *              after creation if you want to target a non-companion.
         *   hide:      defaults to true, but if false will reveal rather than hide the weapons.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.HideWeapons HideWeaponsAction(
                      Kingmaker.Blueprints.SimpleBlueprint owner, Companions unit, bool hide = true)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.HideWeapons>(bp =>
            {
                bp.Target = CompanionTools.GetCompanionEvaluator(unit, owner);
                bp.Hide = hide;
            });
            return result;
        }

        /*******************************************************************************************************************
         * INCREMENT/LOCK/UNLOCK FLAG ACTION
         * Flags are essentially combined integer-and-boolean variables that the game can reference. Sometimes they are
         * used as booleans by checking whether they're unlocked or not, and sometimes as integers by checking their value.
         * Incrementing the flag will unlock it if it's not already unlocked.
         *   flag:      The flag to manipulate
         *   amount:    By default, incrementing increases the value by 1, but you can specify a different amount.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.IncrementFlagValue IncrementFlagAction(
                      Kingmaker.Blueprints.BlueprintUnlockableFlag flag, int amount = 1)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.IncrementFlagValue>(bp =>
            {
                bp.m_Flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                            <Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(flag);
                bp.Value = new Kingmaker.Designers.EventConditionActionSystem.Evaluators.IntConstant { Value = amount };
                bp.UnlockIfNot = true;
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.LockFlag LockFlagAction(
                      Kingmaker.Blueprints.BlueprintUnlockableFlag flag)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.LockFlag>(bp =>
            {
                bp.m_Flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                            <Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(flag);
            });
            return result;
        }

        public static Kingmaker.Designers.EventConditionActionSystem.Actions.UnlockFlag UnlockFlagAction(
                      Kingmaker.Blueprints.BlueprintUnlockableFlag flag)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.UnlockFlag>(bp =>
            {
                bp.m_flag = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                            <Kingmaker.Blueprints.BlueprintUnlockableFlagReference>(flag);
            });
            return result;
        }

        /*******************************************************************************************************************
         * PLAY/STOP CUTSCENE ACTION
         * Create an Action to trigger or abort a provided cutscene. Does not create the cutscene; for that use 
         * CutsceneTools. You can also optionally set the action's owner blueprint, but as far as I can tell this usually
         * isn't necessary.
         *   cutscene:  Cutscene to play or stop
         *   owner:     the blueprint that "owns" the action, such as a dialog cue. Defaults to null.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene PlayCutsceneAction(
                      Kingmaker.AreaLogic.Cutscenes.Cutscene cutscene, SimpleBlueprint owner = null)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene>(bp =>
            {
                bp.m_Cutscene = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                <Kingmaker.Blueprints.CutsceneReference>(cutscene);
                bp.Owner = owner;
                bp.Parameters = new ParametrizedContextSetter();
            });
            return action;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StopCutscene StopCutsceneAction(
                      Kingmaker.AreaLogic.Cutscenes.Cutscene cutscene)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StopCutscene>(bp =>
            {
                bp.m_Cutscene = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                <Kingmaker.Blueprints.CutsceneReference>(cutscene);
            });
            return action;
        }
        /*******************************************************************************************************************
         * PLAY CUTSCENE ACTION - ADD PARAMETER
         * Some cutscenes are "parameterized" such as the one that handles a person walking into the command room. These
         * have components that define the parameters - for instance, in the case of "a person walks into the command room"
         * it will expect a Unit parameter telling it who's walking in.
         *   action:    The PlayCutscene action you want to define a parameter for
         *   name:      The name of the parameter, for human readability purposes
         *   type:      The type of parameter, specified in an enum or as a string that is automatically converted.
         *              Valid values are Unit, Locator, MapObject, Position, Blueprint, and Float.
         *   eval:      The actual parameter. For instance, in the "person walks into the command room" example, this would
         *              be the UnitEvaluator of the person in question.
         ******************************************************************************************************************/
        public static void CutsceneActionAddParameter(
                      Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene action, string name, string type,
                      Kingmaker.ElementsSystem.Evaluator eval)
        {
            ParametrizedContextSetter.ParameterType paramtype = ParametrizedContextSetter.ParameterType.Float;
            switch (type)
            {
                case "Unit":
                case "unit":
                    paramtype = ParametrizedContextSetter.ParameterType.Unit;
                    break;
                case "Locator":
                case "locator":
                    paramtype = ParametrizedContextSetter.ParameterType.Locator;
                    break;
                case "MapObject":
                case "mapobject":
                    paramtype = ParametrizedContextSetter.ParameterType.MapObject;
                    break;
                case "Position":
                case "position":
                    paramtype = ParametrizedContextSetter.ParameterType.Position;
                    break;
                case "Blueprint":
                case "blueprint":
                    paramtype = ParametrizedContextSetter.ParameterType.Blueprint;
                    break;
                case "Float":
                case "float":
                    paramtype = ParametrizedContextSetter.ParameterType.Float;
                    break;
            }
            CutsceneActionAddParameter(action, name, paramtype, eval);
        }
        public static void CutsceneActionAddParameter(
                      Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene action, string name, 
                      ParametrizedContextSetter.ParameterType type, Kingmaker.ElementsSystem.Evaluator eval)
        {
            var parameter = new ParametrizedContextSetter.ParameterEntry();
            parameter.Name = name;
            parameter.Type = type;
            parameter.Evaluator = eval;

            var len = 0;
            if (action.Parameters.Parameters == null)
            {
                action.Parameters.Parameters = new ParametrizedContextSetter.ParameterEntry[1];
            }
            else
            {
                len = action.Parameters.Parameters.Length;
                Array.Resize(ref action.Parameters.Parameters, len + 1);
            }
            action.Parameters.Parameters[len] = parameter;
        }

        /*******************************************************************************************************************
         * RANDOM ACTION
         * Randomly performs one of several weighted actions.
         *   actions:   The actions to randomly pick from. If provided unweighted actions, will automatically weight them
         *              all with a weight value of 1.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.RandomAction RandomAction(
                      params Kingmaker.ElementsSystem.GameAction[] actions)
        {
            var len = actions.Length;
            Kingmaker.Designers.EventConditionActionSystem.Actions.ActionAndWeight[] WeightedActions = 
                new Kingmaker.Designers.EventConditionActionSystem.Actions.ActionAndWeight[len];
            for (int i = 0; i < len; i++)
            {
                WeightedActions[i] = WeightedAction(actions[i], 1);
            }
            return RandomAction(WeightedActions);
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.RandomAction RandomAction(
                      params Kingmaker.Designers.EventConditionActionSystem.Actions.ActionAndWeight[] WeightedActions)
        {
            var len = WeightedActions.Length;
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.RandomAction>(bp =>
            {
                bp.Actions = new Kingmaker.Designers.EventConditionActionSystem.Actions.ActionAndWeight[len];
                for (int i = 0; i < len; i++)
                {
                    bp.Actions[i] = WeightedActions[i];
                }
            });
            return result;
        }

        /*******************************************************************************************************************
         * SET FLY HEIGHT
         * Causes a unit to hover a specified distance above the ground. This is important because there's no way to turn
         * off the functionality that clips units to the floor, so to place them on top of things that do not have ground
         * colliders, such as the commander's bed, you need to actually set the unit to "float" at the appropriate height
         * so they appear to be on top the object (in the case of the bed, 0.85). Remember to set it back to 0 afterwards,
         * or the unit will hover above the ground.
         *   unit:      Which of your companions to affect. Can be manually overwritten with a different UnitEvaluator 
         *              after creation if you want to target a non-companion.
         *   height:    The distance at which to hover. Possibly in meters?
         ******************************************************************************************************************/
        public static SetFlyHeight SetFlyHeightAction(
                      Kingmaker.Blueprints.SimpleBlueprint owner, Companions unit, float height)
        {
            var result = GenericAction<SetFlyHeight>(bp =>
            {
                bp.Unit = CompanionTools.GetCompanionEvaluator(unit, owner);
                bp.height = height;
            });
            return result;
        }

        /*******************************************************************************************************************
         * SET/END WEATHER ACTION
         * Sets the inclemency level of the weather. Whether that is snow or rain or something else seems to depend on the
         * area itself, but this will allow you to override the random weather controller to force a certain level of
         * weather effect, or end the override and return control to the weather controller.
         *   intensity: accepts either an integer or a string with the following valid values:
         *              0/Clear, 1/Light, 2/Moderate, 3/Heavy, 4/Storm. Heavy and Storm have mechanical effects.
         ******************************************************************************************************************/
        public static ControlWeather SetWeatherAction(string intensity)
        {
            Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType level = 
                Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Clear;
            switch (intensity)
            {
                case "Clear":
                case "clear":
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Clear;
                    break;
                case "Light":
                case "light":
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Light;
                    break;
                case "Moderate":
                case "moderate":
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Moderate;
                    break;
                case "Heavy":
                case "heavy":
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Heavy;
                    break;
                case "Storm":
                case "storm":
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Storm;
                    break;
            }

            var action = GenericAction<ControlWeather>(bp =>
            {
                bp.start = true;
                bp.inclemency = level;
            });
            return action;
        }
        public static ControlWeather SetWeatherAction(int intensity)
        {
            Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType level = 
                Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Clear;
            switch (intensity)
            {
                case 0:
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Clear;
                    break;
                case 1:
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Light;
                    break;
                case 2:
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Moderate;
                    break;
                case 3:
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Heavy;
                    break;
                case 4:
                    level = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Storm;
                    break;
            }

            var action = GenericAction<ControlWeather>(bp =>
            {
                bp.start = true;
                bp.inclemency = level;
            });
            return action;
        }
        public static ControlWeather EndWeatherAction()
        {
            var action = GenericAction<ControlWeather>(bp =>
            {
                bp.start = false;
                bp.inclemency = Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType.Clear;
            });
            return action;
        }

        /*******************************************************************************************************************
         * SKIP (TO) TIME ACTION
         * Skips either a set amount of minutes or to a particular time of day when run.
         *   minutes:   how many minutes to skip
         *   timeofday: time of day to skip to, with the following valid values: 
         *              Morning, Day/Afternoon/Daytime, Evening, Night/Nighttime
         *   nofatigue: whether the passage of time should not affect characters' fatigue, defaulting to true.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.TimeSkip SkipTimeAction(
                      int minutes, bool nofatigue = true)
        {
            var skip = new Kingmaker.Designers.EventConditionActionSystem.Evaluators.IntConstant { Value = minutes };
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TimeSkip>(bp =>
            {
                bp.NoFatigue = nofatigue;
                bp.m_Type = Kingmaker.Designers.EventConditionActionSystem.Actions.TimeSkip.SkipType.Minutes;
                bp.MinutesToSkip = skip;
            });
            return result;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.TimeSkip SkipToTimeAction(
                      string timeofday, bool nofatigue = true)
        {
            var time = Kingmaker.AreaLogic.TimeOfDay.Morning;
            switch (timeofday)
            {
                case "Morning":
                case "morning":
                    time = Kingmaker.AreaLogic.TimeOfDay.Morning;
                    break;
                case "Day":
                case "day":
                case "daytime":
                case "Daytime":
                case "afternoon":
                case "Afternoon":
                    time = Kingmaker.AreaLogic.TimeOfDay.Day;
                    break;
                case "Evening":
                case "evening":
                    time = Kingmaker.AreaLogic.TimeOfDay.Evening;
                    break;
                case "Night":
                case "night":
                case "Nighttime":
                case "nighttime":
                    time = Kingmaker.AreaLogic.TimeOfDay.Night;
                    break;
            }

            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TimeSkip>(bp =>
            {
                bp.NoFatigue = nofatigue;
                bp.m_Type = Kingmaker.Designers.EventConditionActionSystem.Actions.TimeSkip.SkipType.TimeOfDay;
                bp.MatchTimeOfDay = true;
                bp.TimeOfDay = time;
            });
            return result;
        }

        /*******************************************************************************************************************
         * SPAWN UNIT       !!! MOSTLY UNTESTED
         * Spawns a unit at a given location. In general I recommend translocating existing units.
         ******************************************************************************************************************/
        public static SpawnUnit SpawnUnitAction(Kingmaker.Blueprints.BlueprintUnit unit, FakeLocator location)
        {
            var action = GenericAction<SpawnUnit>(bp =>
            {
                bp.unit = unit;
                bp.location = location;
            });
            return action;
        }

        /*******************************************************************************************************************
         * START/END COMMAND ROOM EVENT ACTION
         * Brings up a notification card at the war table or marks the notification as complete. Does not create the actual
         * notification card; for that use EventTools.
         *   commandevent:  The event card to trigger/end.
         ******************************************************************************************************************/
        public static Kingmaker.Kingdom.Actions.KingdomActionStartEvent StartCommandRoomEventAction(
                      Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent commandevent)
        {
            var action = GenericAction<Kingmaker.Kingdom.Actions.KingdomActionStartEvent>(bp =>
            {
                bp.m_Event = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                             <BlueprintKingdomEventBaseReference>(commandevent);
                bp.m_Region = (BlueprintRegionReference)null;
            });
            return action;
        }
        public static Kingmaker.Kingdom.Actions.KingdomActionRemoveEvent EndCommandRoomEventAction(
                      Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent commandevent)
        {
            var action = GenericAction<Kingmaker.Kingdom.Actions.KingdomActionRemoveEvent>(bp =>
            {
                bp.m_EventBlueprint = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                      <BlueprintKingdomEventBaseReference>(commandevent);
                bp.CancelIfInProgress = true;
                bp.AllIfMultiple = true;
            });
            return action;
        }

        /*******************************************************************************************************************
         * START DIALOG ACTION
         * When run, triggers a conversation. Does not create the conversation; for that use DialogTools.
         *   dialog:    the dialog to start
         *   owner:     The conversation owner. Can be manually changed after creation to a non-companion UnitEvaluator.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StartDialog StartDialogAction(
                      Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, Companions speaker)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StartDialog>(bp =>
            {
                bp.m_Dialogue = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintDialogReference>(dialog);
                bp.DialogueOwner = CompanionTools.GetCompanionEvaluator(speaker, dialog);
            });
            return action;
        }

        /*******************************************************************************************************************
         * START/COMPLETE ETUDE ACTION
         * Starts or completes an etude. Does not create the etude; for that use EtudeTools. Starting an etude does not
         * necessarily mean it will start playing; if its activation conditions are not met, it will be started but not
         * playing until those conditions are met.
         *   etude: The etude (or a reference to it) to start or complete
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StartEtude StartEtudeAction(
                      BlueprintEtudeReference etude)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StartEtude>(bp =>
            {
                bp.Etude = etude;
            });
            return action;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StartEtude StartEtudeAction(
                      Kingmaker.AreaLogic.Etudes.BlueprintEtude etude)
        {
            return StartEtudeAction(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintEtudeReference>(etude));
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.CompleteEtude CompleteEtudeAction(
                      BlueprintEtudeReference etude)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.CompleteEtude>(bp =>
            {
                bp.Etude = etude;
            });
            return action;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.CompleteEtude CompleteEtudeAction(
                      Kingmaker.AreaLogic.Etudes.BlueprintEtude etude)
        {
            return CompleteEtudeAction(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                       <BlueprintEtudeReference>(etude));
        }

        /*******************************************************************************************************************
         * START/STOP MUSIC
         * Begins or ends a special music track by name. Unfortunately you just have to know the track's internal name,
         * which you can usually find by looking at the blueprint of a cue/dialog/cutscene that invokes the song you want.
         *   trackname - the name of the track, such as "RomanceTheme".
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCustomMusic StartMusic(string trackname)
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCustomMusic>(bp =>
            {
                bp.MusicEventStart = "MUS_" + trackname + "_Play";
                bp.MusicEventStop = "MUS_" + trackname + "_Stop";
            });
            return action;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.StopCustomMusic StopMusic()
        {
            var action = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.StopCustomMusic>();
            return action;
        }

        /*******************************************************************************************************************
         * TELEPORT ACTION
         * Teleports the party to a particular entrypoint on a map. This is not to be used to move characters into specific
         * positions! For that, you want to Translocate. For instance, you'd use this to move your character into the tavern
         * and then use a translocate to position them at a particular table.
         *   exitposition:  the GUID of the area entry point, e.g. "320516612f496da4a8919ba4c78b0be4" for the spot by the
         *                  door inside the tavern in Drezen.
         *   afterTeleport: An optional ActionList that will be executed after the teleport finishes - for instance, you
         *                  may want to put Translocate actions here so the characters will move to specific locations
         *                  after loading into the correct map.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.TeleportParty TeleportAction(
                      string exitposition, Kingmaker.ElementsSystem.ActionList afterTeleport = null)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TeleportParty>(bp =>
            {
                bp.m_exitPositon = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                   <Kingmaker.Blueprints.BlueprintAreaEnterPointReference>(
                                   Resources.GetBlueprint<Kingmaker.Blueprints.Area.BlueprintAreaEnterPoint>(exitposition));
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

        /*******************************************************************************************************************
         * TRANSLOCATE ACTION
         * Moves a character to a specific set of coordinates. Be aware that companions in Drezen and the War Camp will
         * automatically move back to their normal location unless you play an etude that conflicts with their default
         * position etude.
         *   unit:      the companion to move. You could also overwrite this after creation with another UnitEvaluator.
         *   position:  the coordinates and rotation to move to.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit TranslocateAction(
                      Kingmaker.Blueprints.SimpleBlueprint owner, Companions unit, FakeLocator position)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit>(bp =>
            {
                bp.Unit = CompanionTools.GetCompanionEvaluator(unit, owner);
                bp.translocatePositionEvaluator = position;
                bp.m_CopyRotation = true;
                bp.translocateOrientationEvaluator = position.GetRotation();
            });
            return result;
        }

        /*******************************************************************************************************************
         * WEIGHTED ACTION
         * Converts an action into a weighted action. Generally not needed outside of RandomActions. You should only need
         * to use this function outside of its existing use in RandomActions if you want to weight different actions with
         * different values, to make one more likely than another.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.ActionAndWeight WeightedAction(
                      Kingmaker.ElementsSystem.GameAction action, int weight, 
                      Kingmaker.ElementsSystem.Condition condition = null)
        {
            Kingmaker.Designers.EventConditionActionSystem.Actions.ActionAndWeight result;
            result.Conditions = new Kingmaker.ElementsSystem.ConditionsChecker();
            if (condition != null)
            {
                ConditionalTools.CheckerAddCondition(result.Conditions, condition);
            }
            result.Action = MakeList(action);
            result.Weight = weight;
            return result;
        }

        /*******************************************************************************************************************
         * CONDITIONAL ACTION
         * A conditional action will, on run, evaluate the conditions, and then execute the OnTrue or OnFalse actions as
         * appropriate. Because of the potential complexity of conditionals, the ConditionalAction functions are relatively
         * sparse, assuming you'll either provide a very simple conditional or build the conditional with ConditionalTools
         * outside of these functions.
         * 
         * This function creates a conditional action from either a simple condition or a pre-build conditionchecker. 
         * OnTrue and OnFalse actionlists must be provided separately, and will be empty by default.
         ******************************************************************************************************************/
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional ConditionalAction(
                      Kingmaker.ElementsSystem.Condition condition)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional>();
            result.ConditionsChecker = new Kingmaker.ElementsSystem.ConditionsChecker();
            ConditionalTools.CheckerAddCondition(result.ConditionsChecker, condition);
            result.IfTrue = DialogTools.EmptyActionList;
            result.IfFalse = DialogTools.EmptyActionList;
            return result;
        }
        public static Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional ConditionalAction(
                      Kingmaker.ElementsSystem.ConditionsChecker conditionchecker)
        {
            var result = GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional>();
            result.ConditionsChecker = conditionchecker;
            result.IfTrue = DialogTools.EmptyActionList;
            result.IfFalse = DialogTools.EmptyActionList;
            return result;
        }
        /*******************************************************************************************************************
         * CONDITIONAL ACTION - ON TRUE/FALSE
         * Adds the actions provided to the OnTrue or OnFalse branch of the conditional action.
         ******************************************************************************************************************/
        public static void ConditionalActionOnTrue(
                      Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional logicaction, 
                      Kingmaker.ElementsSystem.ActionList actionlist)
        {
            logicaction.IfTrue = actionlist;
        }
        public static void ConditionalActionOnTrue(
                      Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional logicaction, 
                      params Kingmaker.ElementsSystem.GameAction[] actions)
        {
            if (logicaction.IfTrue == DialogTools.EmptyActionList)
            {
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
        public static void ConditionalActionOnFalse(
                      Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional logicaction, 
                      Kingmaker.ElementsSystem.ActionList actionlist)
        {
            logicaction.IfFalse = actionlist;
        }
        public static void ConditionalActionOnFalse(
                      Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional logicaction, 
                      params Kingmaker.ElementsSystem.GameAction[] actions)
        {
            if (logicaction.IfFalse == DialogTools.EmptyActionList)
            {
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