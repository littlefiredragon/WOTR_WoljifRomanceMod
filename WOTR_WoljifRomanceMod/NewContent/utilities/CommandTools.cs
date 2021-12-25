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

namespace WOTR_WoljifRomanceMod
{
    public enum Companions
    {
        None = -1, Player, Arueshalae, Camellia, Daeran, Ember, Greybor, Lann, Nenio, Regill, Seelah, Sosiel, Wenduag, Woljif
    }

    public static class CommandTools
    {
        public static int numdelays = 0;
        public static int numlockcontrols = 0;
        public static int numcamfollows = 0;
        public static int numunnamedmoves = 0;
        public static int numfadeouts = 0;
        public static T GenericCommand<T>(string name, Action<T> init = null) where T : Kingmaker.AreaLogic.Cutscenes.CommandBase, new()
        {
            var result = Helpers.CreateBlueprint<T>(name);
            init?.Invoke(result);
            result.EntryCondition = DialogTools.EmptyConditionChecker;
            return result;
        }

        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandStartDialog StartDialogCommand(Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, Companions speaker = Companions.None)
        {
            var autoname = "command_" + dialog.name;
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandStartDialog>(autoname);
            result.Speaker = getCompanionEvaluator(speaker);
            result.m_Dialog = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintDialogReference>(dialog);
            return result;
        }
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandStartDialog StartDialogCommand(string dialogid, Companions speaker = Companions.None)
        {
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandStartDialog>("command_"+dialogid);
            result.Speaker = getCompanionEvaluator(speaker);
            result.m_Dialog = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintDialogReference>(Resources.GetBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintDialog>(dialogid));
            return result;
        }

        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandLockControls LockControlCommand()
        {
            numlockcontrols++;
            string name = "controllock_" + numlockcontrols.ToString();
            return GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandLockControls>(name);
        }

        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandDelay DelayCommand(Single time = 0.1f)
        {
            numdelays++;
            string name = "delay_" + numdelays.ToString();
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandDelay>(name);
            result.Time = time;
            return result;
        }

        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandBark BarkCommand(string name, Companions target = Companions.None)
        {
            return BarkCommand(name, name, target);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandBark BarkCommand(string name, string key, Companions target = Companions.None)
        {
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandBark>("bark_"+name);
            result.SharedText = new Kingmaker.Localization.SharedStringAsset();
            result.SharedText.String = new Kingmaker.Localization.LocalizedString { m_Key = key };
            result.Unit = getCompanionEvaluator(target);
            result.BarkDurationByText = true;
            result.AwaitFinish = true;
            result.CommandDurationShift = 0.0f;
            result.ControlsUnit = false;
            result.IsSubText = false;
            return result;
        }

        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandCameraFollow CamFollowCommand(Companions target = Companions.None)
        {
            numcamfollows++;
            var name = "camfollow_" + numcamfollows.ToString();
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandCameraFollow>(name);
            var pos = new Kingmaker.Designers.EventConditionActionSystem.Evaluators.UnitPosition();
            pos.Unit = getCompanionEvaluator(target);
            result.Target = pos;
            return result;
        }

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
        /*public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandControlCamera CamMoveCommand(Kingmaker.Blueprints.EntityReference position)
        {
            numcamfollows++;
            var name = "camMove_" + numcamfollows.ToString();
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandControlCamera>(name);
            result.Move = true;
            var locpos = (Kingmaker.Designers.EventConditionActionSystem.Evaluators.LocatorPosition)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(Kingmaker.Designers.EventConditionActionSystem.Evaluators.LocatorPosition));
            locpos.Locator = position;
            result.MoveTarget = locpos;
            result.Rotate = true;
            var rotpos = (Kingmaker.Designers.EventConditionActionSystem.Evaluators.LocatorOrientation)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(Kingmaker.Designers.EventConditionActionSystem.Evaluators.LocatorOrientation));
            rotpos.Locator = position;
            result.RotateTarget = rotpos;
            result.TimingMode = Kingmaker.AreaLogic.Cutscenes.Commands.CommandControlCamera.TimingModeType.Snap;
            return result;
        }*/

        public static Kingmaker.AreaLogic.Cutscenes.CommandAction ActionCommand(string name, Kingmaker.ElementsSystem.ActionList actions)
        {
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.CommandAction>(name);
            result.Action = actions;
            return result;
        }
        public static Kingmaker.AreaLogic.Cutscenes.CommandAction ActionCommand(string name, params Kingmaker.ElementsSystem.GameAction[] actions)
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

        public static Kingmaker.AreaLogic.Cutscenes.CommandAction TranslocateCommand(string name, Companions unit, FakeLocator position)
        {
            var action = ActionTools.TranslocateAction(unit, position);
            var command = ActionCommand(name, action);
            action.Owner = command;
            return command;
        }
        public static Kingmaker.AreaLogic.Cutscenes.CommandAction TranslocateCommand(Companions unit, FakeLocator position)
        {
            var name = "translocate_" + numunnamedmoves.ToString();
            numunnamedmoves++;
            var action = ActionTools.TranslocateAction(unit, position);
            var command = ActionCommand(name, action);
            action.Owner = command;
            return command;
        }
        /*public static Kingmaker.AreaLogic.Cutscenes.CommandAction TranslocateCommand(Companions unit, Kingmaker.Blueprints.EntityReference position, bool setrotation = true)
        {
            var name = "translocate_" + numunnamedmoves.ToString();
            numunnamedmoves++;
            return TranslocateCommand(name, unit, position, setrotation);
        }
        public static Kingmaker.AreaLogic.Cutscenes.CommandAction TranslocateCommand(string name, Companions unit, Kingmaker.Blueprints.EntityReference position, bool setrotation = true)
        {
            var action = ActionTools.TranslocateAction(unit, position, setrotation);
            var command = ActionCommand(name, action);
            action.Owner = command;
            return command;
        }*/

        /*public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandUnitLookAt LookAtCommand(string name, Companions unit, Companions lookedatunit)
        {
            var position = (Kingmaker.Designers.EventConditionActionSystem.Evaluators.UnitPosition)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(Kingmaker.Designers.EventConditionActionSystem.Evaluators.UnitPosition));
            return LookAtCommand(name, unit, position);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandUnitLookAt LookAtCommand(string name, Companions unit, Kingmaker.Blueprints.EntityReference location)
        {
            var position = (Kingmaker.Designers.EventConditionActionSystem.Evaluators.LocatorPosition)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(Kingmaker.Designers.EventConditionActionSystem.Evaluators.LocatorPosition));
            return LookAtCommand(name, unit, position);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandUnitLookAt LookAtCommand(string name, Companions unit, Kingmaker.ElementsSystem.PositionEvaluator position)
        {
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandUnitLookAt>(name);
            result.m_Position = position;
            result.m_FreezeAfterTurn = true;
            result.m_OnTurned = new Kingmaker.AreaLogic.Cutscenes.CommandBase.CommandSignalData();
            result.m_OnTurned.Gate = null;
            result.m_OnTurned.Name = "OnTurned";
            result.m_Unit = getCompanionEvaluator(unit);
            return result;
        }*/

        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandMoveUnit WalkCommand(string name, Companions unit, FakeLocator position, bool vanish = false)
        {
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandMoveUnit>(name);
            result.m_Timeout = 20.0f;
            result.Unit = getCompanionEvaluator(unit);
            result.DisableAvoidance = true;
            result.RunAway = vanish;
            result.Target = position;
            return result;
        }
        /*public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandMoveUnit WalkCommand(string name, Companions unit, Kingmaker.Blueprints.EntityReference position, bool vanish = false)
        {
            var result = GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandMoveUnit>(name);
            result.m_Timeout = 20.0f;
            result.Unit = getCompanionEvaluator(unit);
            result.DisableAvoidance = true;
            result.RunAway = vanish;
            var locatorpos = (Kingmaker.Designers.EventConditionActionSystem.Evaluators.LocatorPosition)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(Kingmaker.Designers.EventConditionActionSystem.Evaluators.LocatorPosition));
            locatorpos.Locator = position;
            result.Target = locatorpos;
            return result;
        }*/

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

        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandUnitPlayCutsceneAnimation GenericAnimationCommand(string name, string commandid, Companions companion, bool lockrotation = false)
        {
            var newhandle = Kingmaker.ResourceManagement.BundledResourceHandle<Kingmaker.Visual.Animation.AnimationClipWrapper>.Request(commandid);
            var newwrapperlink = new Kingmaker.ResourceLinks.AnimationClipWrapperLink { Handle = newhandle, AssetId = newhandle.m_AssetId };

            var animation = CommandTools.GenericCommand<Kingmaker.AreaLogic.Cutscenes.Commands.CommandUnitPlayCutsceneAnimation>(name, bp =>
            {
                bp.m_Unit = CommandTools.getCompanionEvaluator(companion, bp);
                bp.m_CutsceneClipWrapper = newwrapperlink;
                bp.m_WaitForCurrentAnimation = false;
                bp.AddToElementsList(bp.m_Unit);
                bp.m_LockRotation = lockrotation;
            });
            return animation;
        }
        public static Kingmaker.AreaLogic.Cutscenes.Commands.CommandUnitPlayCutsceneAnimation SitIdleCommand(string name, Companions companion)
        {
            return GenericAnimationCommand(name, "586ac35db41cb6044ba218511a7bae6e", companion, true);
        }


        public static Kingmaker.Blueprints.BlueprintUnitReference GetCompanionReference(Companions companion)
        {
            var companionid = "";
            switch (companion)
            {
                case Companions.Arueshalae:
                    companionid = "a352873d37ec6c54c9fa8f6da3a6b3e1";
                    break;
                case Companions.Camellia:
                    companionid = "397b090721c41044ea3220445300e1b8";
                    break;
                case Companions.Daeran:
                    companionid = "096fc4a96d675bb45a0396bcaa7aa993";
                    break;
                case Companions.Ember:
                    companionid = "2779754eecffd044fbd4842dba55312c";
                    break;
                case Companions.Greybor:
                    companionid = "f72bb7c48bb3e45458f866045448fb58";
                    break;
                case Companions.Lann:
                    companionid = "cb29621d99b902e4da6f5d232352fbda";
                    break;
                case Companions.Nenio:
                    companionid = "1b893f7cf2b150e4f8bc2b3c389ba71d";
                    break;
                case Companions.Regill:
                    companionid = "0d37024170b172346b3769df92a971f5";
                    break;
                case Companions.Seelah:
                    companionid = "54be53f0b35bf3c4592a97ae335fe765";
                    break;
                case Companions.Sosiel:
                    companionid = "1cbbbb892f93c3d439f8417ad7cbb6aa";
                    break;
                case Companions.Wenduag:
                    companionid = "ae766624c03058440a036de90a7f2009";
                    break;
                case Companions.Woljif:
                    companionid = "766435873b1361c4287c351de194e5f9";
                    break;
                case Companions.None:
                case Companions.Player:
                    companionid = null;
                    break;
            }
            Kingmaker.Blueprints.BlueprintUnitReference result = null;
            if (companionid != null)
            {
                result = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnitReference>(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnit>(companionid));
            }
            return result;
        }
        public static Kingmaker.ElementsSystem.UnitEvaluator getCompanionEvaluator (Companions companion, SimpleBlueprint owner = null)
        {
            Kingmaker.ElementsSystem.UnitEvaluator result = null;
            if (companion == Companions.Player)
            {
                //result = new Kingmaker.Designers.EventConditionActionSystem.Evaluators.PlayerCharacter();
                result = (Kingmaker.Designers.EventConditionActionSystem.Evaluators.PlayerCharacter)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(Kingmaker.Designers.EventConditionActionSystem.Evaluators.PlayerCharacter));
            }
            else
            {
                //var companioneval = new Kingmaker.Designers.EventConditionActionSystem.Evaluators.CompanionInParty();
                var companioneval = (Kingmaker.Designers.EventConditionActionSystem.Evaluators.CompanionInParty)Kingmaker.ElementsSystem.Element.CreateInstance(typeof(Kingmaker.Designers.EventConditionActionSystem.Evaluators.CompanionInParty));
                companioneval.IncludeDettached = true;
                companioneval.IncludeRemote = true;
                companioneval.m_Companion = GetCompanionReference(companion);
                result = companioneval;
            }
            result.Owner = owner;
            return result;
        }
    }
}