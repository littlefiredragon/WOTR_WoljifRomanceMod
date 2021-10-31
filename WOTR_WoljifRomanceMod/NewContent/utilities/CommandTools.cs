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
    public enum Companions
    {
        None = -1, Arueshalae, Camellia, Daeran, Ember, Greybor, Lann, Nenio, Regill, Seelah, Sosiel, Wenduag, Woljif
    }
    public static class CommandTools
    {
        public static int numdelays = 0;
        public static int numlockcontrols = 0;
        public static int numcamfollows = 0;
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
            result.SharedText.String = new Kingmaker.Localization.LocalizedString { m_Key = key };
            result.Unit = getCompanionEvaluator(target);
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
        public static Kingmaker.ElementsSystem.UnitEvaluator getCompanionEvaluator (Companions companion)
        {
            var result = new Kingmaker.Designers.EventConditionActionSystem.Evaluators.CompanionInParty();
            result.IncludeDettached = true;
            result.IncludeRemote = true;
            result.m_Companion = GetCompanionReference(companion);
            return result;
        }
    }
}