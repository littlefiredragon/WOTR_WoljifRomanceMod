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
        public static Kingmaker.DialogSystem.Blueprints.BlueprintAnswer CreateAnswer(string name)
        {
            return CreateAnswer(name, name);
        }
        public static Kingmaker.DialogSystem.Blueprints.BlueprintAnswer CreateAnswer(string name, string key)
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
            });
            return result;
        }

        public static void AnswerAddNextCue(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, int position = -1)
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
    }
}