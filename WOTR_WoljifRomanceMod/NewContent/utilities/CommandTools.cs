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
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.EntitySystem.Entities;

//##########################################################################################################################
// COMMAND TOOLS
// Helper functions to generate Cutscene Commands
//##########################################################################################################################

namespace WOTR_WoljifRomanceMod
{
    public static class CommandTools
    {
        //Universal counters used for auto-generating command names.
        public static int numdelays = 0;
        public static int numlockcontrols = 0;
        public static int numcamfollows = 0;
        public static int numunnamedmoves = 0;
        public static int numfadeouts = 0;

        /*******************************************************************************************************************
         * GENERIC COMMAND TEMPLATE
         * Used as is to create one-off instances of unusual command types or as the basis for the other CommandTools
         * functions. If you're using a command type more than once, it should probably have its own function.
         ******************************************************************************************************************/
        public static T GenericCommand<T>(string name, Action<T> init = null) 
                        where T : Kingmaker.AreaLogic.Cutscenes.CommandBase, new()
        {
            var result = Helpers.CreateBlueprint<T>(name);
            init?.Invoke(result);
            result.EntryCondition = DialogTools.EmptyConditionChecker;
            return result;
        }

        /*******************************************************************************************************************
         * ACTION COMMAND
         * Runs an action or actions, as defined by ActionTools.
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.CommandAction ActionCommand(string name, 
                      Kingmaker.ElementsSystem.ActionList actions)
        {
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.CommandAction>(name);
            result.Action = actions;
            return result;
        }
        public static Kingmaker.AreaLogic.Cutscenes.CommandAction ActionCommand(string name, 
                      params Kingmaker.ElementsSystem.GameAction[] actions)
        {
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.CommandAction>(name);
            result.Action = new Kingmaker.ElementsSystem.ActionList();
            result.Action.Actions = new Kingmaker.ElementsSystem.GameAction[actions.Length];
            for (int i = 0; i < actions.Length; i++)
            {
                result.Action.Actions[i] = actions[i];
            }
            return result;
        }

        /*******************************************************************************************************************
         * BARK COMMAND
         * Show a "bark" or speech bubble dialog on run. Currently written only to support party members as bark
         * sources, but you can always make one with no companion and then manually change the target unit.
         *   name:      Included in the command's unique name. Also used as key if no key is provided.
         *   key:       The key of the dialog string in the localization json. Defaults to using the name.
         *   target:    Which of your companions is speaking this line. Defaults to none, and can be manually overwritten 
         *              with a different UnitEvaluator after creation if you want the line spoken by someone else.
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandBark BarkCommand(string name, 
                      Companions target = Companions.None)
        {
            return BarkCommand(name, name, target);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandBark BarkCommand(string name, string key, 
                      Companions target = Companions.None)
        {
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandBark>("bark_" + name);
            result.SharedText = new Kingmaker.Localization.SharedStringAsset();
            result.SharedText.String = new Kingmaker.Localization.LocalizedString { m_Key = key };
            result.Unit = CompanionTools.GetCompanionEvaluator(target, result);
            result.BarkDurationByText = true;
            result.AwaitFinish = true;
            result.CommandDurationShift = 0.0f;
            result.ControlsUnit = false;
            result.IsSubText = false;
            return result;
        }

        /*******************************************************************************************************************
         * CAMERA FOLLOW COMMAND
         * Has the camera follow the movement of a unit. Currently this function is set up to take a Companion, but you
         * can always assign a different UnitEvaluator after creating the command.
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandCameraFollow CamFollowCommand(
                      Companions target = Companions.None)
        {
            numcamfollows++;
            var name = "camfollow_" + numcamfollows.ToString();
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandCameraFollow>(name);
            var pos = new Kingmaker.Designers.EventConditionActionSystem.Evaluators.UnitPosition();
            pos.Unit = CompanionTools.GetCompanionEvaluator(target, result);
            result.Target = pos;
            return result;
        }

        /*******************************************************************************************************************
         * MOVE CAMERA COMMAND
         * Moves the camera to a specified location. Note that if you're using Unity Explorer to find the camera angles,
         * this command will flip the rotation 180 degrees from what UE reports. XYZ values are unaffected.
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandControlCamera CamMoveCommand(FakeLocator position)
        {
            numcamfollows++;
            var name = "camMove_" + numcamfollows.ToString();
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandControlCamera>(name);
            result.Move = true;
            result.MoveTarget = position;
            result.Rotate = true;
            result.RotateTarget = position.GetRotation();
            result.TimingMode = Kingmaker.AreaLogic.Cutscenes.Commands.CommandControlCamera.TimingModeType.Snap;
            return result;
        }

        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandControlCamera CamMoveCommand(FakeLocator position,
                                                                                                 float zoom)
        {
            numcamfollows++;
            var name = "camMove_" + numcamfollows.ToString();
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandControlCamera>(name);
            result.Move = true;
            result.MoveTarget = position;
            result.Rotate = true;
            result.RotateTarget = position.GetRotation();
            result.TimingMode = Kingmaker.AreaLogic.Cutscenes.Commands.CommandControlCamera.TimingModeType.Snap;
            result.ZoomTarget = zoom;
            result.Zoom = true;
            return result;
        }

        /*******************************************************************************************************************
         * DELAY COMMAND
         * Waits a certain amount of time before proceeding to the next gate.
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandDelay DelayCommand(Single time = 0.1f)
        {
            numdelays++;
            string name = "delay_" + numdelays.ToString();
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandDelay>(name);
            result.Time = time;
            return result;
        }

        /*******************************************************************************************************************
         * FADEOUT COMMAND
         * Fades camera to black and back again.
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandFadeout FadeoutCommand()
        {
            numfadeouts++;
            string name = "fadeout_" + numfadeouts.ToString();
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandFadeout>(name);
            result.m_Continuous = true;
            result.m_Lifetime = 1.0f;
            result.m_OnFaded = new Kingmaker.AreaLogic.Cutscenes.CommandBase.CommandSignalData();
            result.m_OnFaded.Gate = null;
            result.m_OnFaded.Name = "OnFaded";
            return result;
        }

        /*******************************************************************************************************************
         * LOCK CONTROL COMMAND
         * Hides the user interface and prevents the player from controlling anything until the command ends.
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandLockControls LockControlCommand()
        {
            numlockcontrols++;
            string name = "controllock_" + numlockcontrols.ToString();
            return GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandLockControls>(name);
        }

        /*******************************************************************************************************************
         * START DIALOG COMMAND
         * Starts a provided dialog with a specific character. Currently set up only to work with Companions, but you could
         * replace the speaker with a different UnitEvaluator after creation. Does not create the dialog. For that, use
         * DialogTools.
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandStartDialog StartDialogCommand(
                      Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, Companions speaker = Companions.None)
        {
            var autoname = "command_" + dialog.name;
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandStartDialog>(autoname);
            result.Speaker = CompanionTools.GetCompanionEvaluator(speaker, result);
            result.m_Dialog = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintDialogReference>(dialog);
            return result;
        }
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandStartDialog StartDialogCommand(
                      string dialogid, Companions speaker = Companions.None)
        {
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandStartDialog>("command_"+dialogid);
            result.Speaker = CompanionTools.GetCompanionEvaluator(speaker, result);
            result.m_Dialog = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintDialogReference>
                              (Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintDialog>(dialogid));
            return result;
        }

        /*******************************************************************************************************************
         * TRANSLOCATE COMMAND
         * Moves a character to a specific set of coordinates. Be aware that companions in Drezen and the War Camp will
         * automatically move back to their normal location unless you play an etude that conflicts with their default
         * position etude.
         *   name:      the name of the command, optionally. Will be autonamed if not provided.
         *   unit:      the companion to move. You could also overwrite this after creation with another UnitEvaluator.
         *   position:  the coordinates and rotation to move to.
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.CommandAction TranslocateCommand(
                      string name, Companions unit, FakeLocator position)
        {
            var action = ActionTools.TranslocateAction(null, unit, position);
            var command = ActionCommand(name, action);
            action.Owner = command;
            action.Unit.Owner = command;
            return command;
        }
        public static Kingmaker.AreaLogic.Cutscenes.CommandAction TranslocateCommand(Companions unit, FakeLocator position)
        {
            var name = "translocate_" + numunnamedmoves.ToString();
            numunnamedmoves++;
            var action = ActionTools.TranslocateAction(null, unit, position);
            var command = ActionCommand(name, action);
            action.Owner = command;
            action.Unit.Owner = command;
            return command;
        }

        /*******************************************************************************************************************
         * WALK COMMAND
         * Makes a character walk from their current position to the specified position.
         *   name:      name of the command
         *   unit:      which companion to have walk. You could also set a different UnitEvaluator after creation.
         *   position:  where to walk to
         *   vanish:    whether to disappear after reaching the location
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandMoveUnit WalkCommand(
                      string name, Companions unit, FakeLocator position, bool vanish = false)
        {
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandMoveUnit>(name);
            result.m_Timeout = 20.0f;
            result.Unit = CompanionTools.GetCompanionEvaluator(unit, result);
            result.DisableAvoidance = true;
            result.RunAway = vanish;
            result.Target = position;
            return result;
        }

        /*******************************************************************************************************************
         * ANIMATION COMMAND
         * Plays a specific animation as defined in a resource file. For player characters and companions, the resource is
         * humananimations.animation, located in the Wrath of the Righteous Bundles folder. You can find the command ID for 
         * the desired animation by opening this file in AssetStudio. Be aware that this command will never actually
         * finish for looping/continuous animations, such as sitting in a chair! Therefore, if you try to put commands for
         * two characters to sit in the same cutscene track, the cutscene will get stuck on the first and never proceed to
         * the second animation. You should put these in their own tracks.
         *   name:          name of the command
         *   commandid:     the asset ID number for the desired animation
         *   companion:     who to play the animation on. Could also be replaced with a UnitEvaluator for a non-companion 
         *                  after creation if desired.
         *   lockrotation:  prevents character from rotating while in this animation. Defaults to false.
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandUnitPlayCutsceneAnimation GenericAnimationCommand(
                      string name, string commandid, Companions companion, bool lockrotation = false)
        {
            var newhandle = Kingmaker.ResourceManagement.BundledResourceHandle
                            <Kingmaker.Visual.Animation.AnimationClipWrapper>.Request(commandid);
            var newwrapperlink = new Kingmaker.ResourceLinks.AnimationClipWrapperLink 
                { 
                    Handle = newhandle, 
                    AssetId = newhandle.m_AssetId 
                };

            var anim = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandUnitPlayCutsceneAnimation>(name, bp =>
            {
                bp.m_Unit = CompanionTools.GetCompanionEvaluator(companion, bp);
                bp.m_CutsceneClipWrapper = newwrapperlink;
                bp.m_WaitForCurrentAnimation = false;
                bp.AddToElementsList(bp.m_Unit);
                bp.m_LockRotation = lockrotation;
            });
            return anim;
        }

        // The looping sitting in chair animation, since it seemed common enough to warrant a shortcut function.
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandUnitPlayCutsceneAnimation SitIdleCommand(
                      string name, Companions companion)
        {
            return GenericAnimationCommand(name, "586ac35db41cb6044ba218511a7bae6e", companion, true);
        }
    }
}