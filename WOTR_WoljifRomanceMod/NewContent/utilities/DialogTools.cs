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
    public class NewDialog : IUpdatableSettings
    {
        [JsonProperty]
        private readonly SortedDictionary<string, string> NewDialogStrings = new SortedDictionary<string, string>();
        public void OverrideSettings(IUpdatableSettings userSettings)
        {
            var loadedSettings = userSettings as NewDialog;
            if (loadedSettings == null) { return; }

            loadedSettings.NewDialogStrings.ForEach(entry => {
                if (!(NewDialogStrings.ContainsKey(entry.Key)))
                {
                    NewDialogStrings[entry.Key] = entry.Value;
                }
            });
        }
        public void LoadDialogIntoGame(string locale)
        {
            var locText = Kingmaker.Localization.LocalizationManager.CurrentPack.Strings;
            foreach (KeyValuePair<string, string> pair in NewDialogStrings)
            {
                locText[pair.Key] = pair.Value;
            }
        }
    }
    public static class DialogTools
    {
        public static NewDialog NewDialogs;
        // Shared empty objects that are required for dialog parts.
        public static readonly Kingmaker.ElementsSystem.ActionList EmptyActionList = new Kingmaker.ElementsSystem.ActionList();
        public static readonly Kingmaker.DialogSystem.CueSelection EmptyCueSelection = new Kingmaker.DialogSystem.CueSelection();
        public static readonly Kingmaker.UnitLogic.Alignments.AlignmentShift EmptyAlignmentShift = new Kingmaker.UnitLogic.Alignments.AlignmentShift();
        public static readonly Kingmaker.ElementsSystem.ConditionsChecker EmptyConditionChecker = new Kingmaker.ElementsSystem.ConditionsChecker();
        public static readonly Kingmaker.DialogSystem.DialogSpeaker EmptyDialogSpeaker= new Kingmaker.DialogSystem.DialogSpeaker();
        public static readonly Kingmaker.DialogSystem.Blueprints.ShowCheck EmptyShowCheck = new Kingmaker.DialogSystem.Blueprints.ShowCheck();
        public static readonly Kingmaker.DialogSystem.CharacterSelection EmptyCharSelect = new Kingmaker.DialogSystem.CharacterSelection();

        // Wrapper functions
        // Dialog
        public static Kingmaker.DialogSystem.Blueprints.BlueprintDialog CreateDialog(string name, Kingmaker.DialogSystem.Blueprints.BlueprintCue firstcue)
        {
            var result = Helpers.CreateBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintDialog>(name);
            result.FirstCue = new Kingmaker.DialogSystem.CueSelection();
            result.FirstCue.Cues.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintCueBaseReference>(firstcue));
            result.Conditions = EmptyConditionChecker;
            result.StartActions = EmptyActionList;
            result.FinishActions = EmptyActionList;
            result.ReplaceActions = EmptyActionList;
            result.TurnPlayer = true;
            result.TurnFirstSpeaker = true;
            return result;
        }
        public static void DialogInsertCue(Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, int position)
        {
            dialog.FirstCue.Cues.Insert(position, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintCueBaseReference>(cue));
        }

        public static void DialogAddStartAction(Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, Kingmaker.ElementsSystem.GameAction action)
        {
            if (dialog.StartActions == EmptyActionList)
            {//Make a brand new action list
                dialog.StartActions = new Kingmaker.ElementsSystem.ActionList();
            }
            var len = 0;
            if (dialog.StartActions.Actions == null)
            {
                dialog.StartActions.Actions = new Kingmaker.ElementsSystem.GameAction[1];
            }
            else
            {
                len = dialog.StartActions.Actions.Length;
                Array.Resize(ref dialog.StartActions.Actions, len + 1);
            }
            dialog.StartActions.Actions[len] = action;
        }
        public static void DialogAddFinishAction(Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, Kingmaker.ElementsSystem.GameAction action)
        {
            if (dialog.FinishActions == EmptyActionList)
            {//Make a brand new action list
                dialog.FinishActions = new Kingmaker.ElementsSystem.ActionList();
            }
            var len = 0;
            if (dialog.FinishActions.Actions == null)
            {
                dialog.FinishActions.Actions = new Kingmaker.ElementsSystem.GameAction[1];
            }
            else
            {
                len = dialog.FinishActions.Actions.Length;
                Array.Resize(ref dialog.FinishActions.Actions, len + 1);
            }
            dialog.FinishActions.Actions[len] = action;
        }


        // Cues
        public static Kingmaker.DialogSystem.Blueprints.BlueprintCue CreateCue(string name)
        {
            return CreateCue(name, name);
        }
        public static Kingmaker.DialogSystem.Blueprints.BlueprintCue CreateCue(string name, string key)
        {
            var result = Helpers.CreateBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>(name, bp =>
            {
                bp.Text = new Kingmaker.Localization.LocalizedString { m_Key = key };
                bp.Continue = EmptyCueSelection;
                bp.AlignmentShift = EmptyAlignmentShift;
                bp.Conditions = EmptyConditionChecker;
                bp.Speaker = EmptyDialogSpeaker;
                bp.OnShow = EmptyActionList;
                bp.OnStop = EmptyActionList;
            });
            return result;
        }
        
        public static void CueSetSpeaker(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, Companions speaker, bool setconditional = true)
        {
            if (cue.Speaker == EmptyDialogSpeaker)
            {
                cue.Speaker = new Kingmaker.DialogSystem.DialogSpeaker { m_Blueprint = CommandTools.GetCompanionReference(speaker) };
            }
            if (setconditional)
            {
                string name = cue.name + "_" + speaker.ToString() + "_inparty";
                CueAddCondition(cue, ConditionalTools.CreateCompanionInPartyCondition(name, speaker));
            }
        }

        public static void CueAddContinue(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, Kingmaker.DialogSystem.Blueprints.BlueprintCue nextcue, int position = -1)
        {
            if (cue.Continue == EmptyCueSelection)
            {//Make a brand new cue selection.
                cue.Continue = new Kingmaker.DialogSystem.CueSelection();
            }
            if (position == -1)
            {//Insert at the end of the list.
                cue.Continue.Cues.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintCueBaseReference>(nextcue));
            }
            else
            {
                cue.Continue.Cues.Insert(position, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintCueBaseReference>(nextcue));
            }
        }

        public static void CueAddAnswersList(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList answerlist, int position = -1)
        {
            if (position == -1)
            {//Insert at the end of the list.
                cue.Answers.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintAnswerBaseReference>(answerlist));
            }
            else
            {
                cue.Answers.Insert(position, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintAnswerBaseReference>(answerlist));
            }
        }
        // CueAddCondition is intended for simple conditions without nested logic ("A and B and C" is fine, but "A and (B or C)" is too complex.
        // For complex logic, it's best to build your entire logic tree with ConditionalTools and then use CueSetConditionChecker to plug it in.
        public static void CueAddCondition(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, Kingmaker.ElementsSystem.Condition condition)
        {
            if (cue.Conditions == EmptyConditionChecker)
            {//Make a brand new checker
                cue.Conditions = ConditionalTools.CreateChecker();
            }
            ConditionalTools.CheckerAddCondition(cue.Conditions, condition);
        }
        public static void CueSetConditionChecker(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, Kingmaker.ElementsSystem.ConditionsChecker checker)
        {
            cue.Conditions = checker;
        }

        public static void CueAddOnShowAction(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, Kingmaker.ElementsSystem.GameAction action)
        {
            if (cue.OnShow == EmptyActionList)
            {//Make a brand new action list
                cue.OnShow = new Kingmaker.ElementsSystem.ActionList();
            }
            var len = 0;
            if (cue.OnShow.Actions == null)
            {
                cue.OnShow.Actions = new Kingmaker.ElementsSystem.GameAction[1];
            }
            else
            {
                len = cue.OnShow.Actions.Length;
                Array.Resize(ref cue.OnShow.Actions, len + 1);
            }
            cue.OnShow.Actions[len] = action;
        }
        public static void CueAddOnStopAction(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, Kingmaker.ElementsSystem.GameAction action)
        {
            if (cue.OnStop == EmptyActionList)
            {//Make a brand new action list
                cue.OnStop = new Kingmaker.ElementsSystem.ActionList();
            }
            var len = 0;
            if (cue.OnStop.Actions == null)
            {
                cue.OnStop.Actions = new Kingmaker.ElementsSystem.GameAction[1];
            }
            else
            {
                len = cue.OnStop.Actions.Length;
                Array.Resize(ref cue.OnStop.Actions, len + 1);
            }
            cue.OnStop.Actions[len] = action;
        }

        // Answerslist
        public static Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList CreateAnswersList(string name)
        {
            var result = Helpers.CreateBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>(name, bp =>
            {
                bp.ShowOnce = false;
                bp.Conditions = EmptyConditionChecker;
            });
            return result;
        }
        public static void ListAddAnswer(Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList answerlist, Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, int position = -1)
        {
            if (position == -1)
            {//Insert at the end of the list.
                answerlist.Answers.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintAnswerBaseReference>(answer));
            }
            else
            {
                answerlist.Answers.Insert(position, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintAnswerBaseReference>(answer));
            }
        }

        // Answers
        public static Kingmaker.DialogSystem.Blueprints.BlueprintAnswer CreateAnswer(string name, bool showonce = false)
        {
            return CreateAnswer(name, name, showonce);
        }
        public static Kingmaker.DialogSystem.Blueprints.BlueprintAnswer CreateAnswer(string name, string key, bool showonce = false)
        {
            var result = Helpers.CreateBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswer>(name, bp =>
            {
                bp.Text = new Kingmaker.Localization.LocalizedString { m_Key = key };
                bp.NextCue = EmptyCueSelection;
                bp.AlignmentShift = EmptyAlignmentShift;
                bp.ShowConditions = EmptyConditionChecker;
                bp.SelectConditions = EmptyConditionChecker;
                bp.OnSelect = EmptyActionList;
                bp.CharacterSelection = EmptyCharSelect;
                bp.ShowCheck = EmptyShowCheck;
                bp.ShowOnce = showonce;
            });
            return result;
        }

        public static void AnswerAlignmentShift(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, string direction, string descriptionkey)
        {
            Kingmaker.UnitLogic.Alignments.AlignmentShiftDirection dir = Kingmaker.UnitLogic.Alignments.AlignmentShiftDirection.TrueNeutral;
            switch (direction)
            {
                case "Lawful":
                case "lawful":
                    dir = Kingmaker.UnitLogic.Alignments.AlignmentShiftDirection.Lawful;
                    break;
                case "Chaotic":
                case "chaotic":
                    dir = Kingmaker.UnitLogic.Alignments.AlignmentShiftDirection.Chaotic;
                    break;
                case "Good":
                case "good":
                    dir = Kingmaker.UnitLogic.Alignments.AlignmentShiftDirection.Good;
                    break;
                case "Evil":
                case "evil":
                    dir = Kingmaker.UnitLogic.Alignments.AlignmentShiftDirection.Evil;
                    break;
            }
            answer.AlignmentShift = new Kingmaker.UnitLogic.Alignments.AlignmentShift { Direction = dir, Value = 1, Description = new Kingmaker.Localization.LocalizedString { m_Key = descriptionkey } };
        }

        public static void AnswerAddNextCue(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, Kingmaker.DialogSystem.Blueprints.BlueprintCueBase cue, int position = -1)
        {
            if (answer.NextCue == EmptyCueSelection)
            {//Make a brand new cue selection.
                answer.NextCue = new Kingmaker.DialogSystem.CueSelection();
            }
            if (position == -1)
            {//Insert at the end of the list.
                answer.NextCue.Cues.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintCueBaseReference>(cue));
            }
            else
            {
                answer.NextCue.Cues.Insert(position, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintCueBaseReference>(cue));
            }
        }

        public static void AnswerAddOnSelectAction(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, Kingmaker.ElementsSystem.GameAction action)
        {
            if (answer.OnSelect == EmptyActionList)
            {//Make a brand new action list
                answer.OnSelect = new Kingmaker.ElementsSystem.ActionList();
            }
            var len = 0;
            if (answer.OnSelect.Actions == null)
            {
                answer.OnSelect.Actions = new Kingmaker.ElementsSystem.GameAction[1];
            }
            else
            {
                len = answer.OnSelect.Actions.Length;
                Array.Resize(ref answer.OnSelect.Actions, len + 1);
            }
            answer.OnSelect.Actions[len] = action;
        }

        // AddCondition is intended for simple conditions without nested logic ("A and B and C" is fine, but "A and (B or C)" is too complex.
        // For complex logic, it's best to build your entire logic tree with ConditionalTools and then use SetConditionChecker to plug it in.
        public static void AnswerAddShowCondition(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, Kingmaker.ElementsSystem.Condition condition)
        {
            if (answer.ShowConditions == EmptyConditionChecker)
            {//Make a brand new checker
                answer.ShowConditions = ConditionalTools.CreateChecker();
            }
            ConditionalTools.CheckerAddCondition(answer.ShowConditions, condition);
        }
        public static void AnswerSetShowConditionChecker(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, Kingmaker.ElementsSystem.ConditionsChecker checker)
        {
            answer.ShowConditions = checker;
        }
        public static void AnswerAddSelectCondition(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, Kingmaker.ElementsSystem.Condition condition)
        {
            if (answer.SelectConditions == EmptyConditionChecker)
            {//Make a brand new checker
                answer.SelectConditions = ConditionalTools.CreateChecker();
            }
            ConditionalTools.CheckerAddCondition(answer.SelectConditions, condition);
        }
        public static void AnswerSetSelectConditionChecker(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, Kingmaker.ElementsSystem.ConditionsChecker checker)
        {
            answer.SelectConditions = checker;
        }

        // Skill checks
        public static Kingmaker.DialogSystem.Blueprints.BlueprintCheck CreateCheck(string name, Kingmaker.EntitySystem.Stats.StatType type, int DC, Kingmaker.DialogSystem.Blueprints.BlueprintCue success, Kingmaker.DialogSystem.Blueprints.BlueprintCue failure, bool hidden = false)
        {
            return CreateCheck(name, type, DC, success, failure, DialogExperience.NoExperience, hidden);
        }
        public static Kingmaker.DialogSystem.Blueprints.BlueprintCheck CreateCheck(string name, Kingmaker.EntitySystem.Stats.StatType type, int DC, Kingmaker.DialogSystem.Blueprints.BlueprintCue success, Kingmaker.DialogSystem.Blueprints.BlueprintCue failure, Kingmaker.DialogSystem.DialogExperience exp, bool hidden = false)
        {
            var result = Helpers.CreateBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCheck>(name, bp =>
            {
                bp.Conditions = EmptyConditionChecker;
                bp.Type = type;
                bp.DC = DC;
                bp.m_Success = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintCueBaseReference>(success);
                bp.m_Fail = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintCueBaseReference>(failure);
                bp.Hidden = hidden;
                bp.Experience = exp;
            });
            return result;
        }
    }
}