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
    //######################################################################################################################
    // NEW DIALOG
    // Mechanism based on the ones in TTT, which loads in the new dialog strings for the mod from a json file.
    //######################################################################################################################
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
        public void LoadDialogIntoGame()
        {
            var locText = Kingmaker.Localization.LocalizationManager.CurrentPack;

            foreach (KeyValuePair<string, string> pair in NewDialogStrings)
            {
                locText.PutString(pair.Key, pair.Value);
            }
        }
    }
    //######################################################################################################################
    // DIALOG TOOLS
    // Dialog is the meat of a story mod, so you need a good understanding of the structures involved and how to manipulate
    // them! Dialogs consist of cues, answers, answerlists, cuesequences, and checks. Cues are text displayed to the player
    // such as narration or things said by NPCs. Answers are lines the player can choose to say. Answerlists hold 
    // collections of answers, to glue cues and answers together.
    // Cuesequences and checks are rarer. A cuesequence is for side commentary by companions, and is essentially a 
    // simplified mechanism that checks whether each appropriate character is there and plays their commentary before 
    // moving on. A check is for in-dialog skill checks, which leads to two different outcomes depending on whether you
    // fail or succeed. 
    // Pretty much any of these things can be restricted with Conditions or do additional things with Actions.
    //######################################################################################################################
    public static class DialogTools
    {
        public static NewDialog NewDialogs_ruRU;
        public static NewDialog NewDialogs_zhCN;
        public static NewDialog NewDialogs_enGB;
        // Shared empty objects that are required for dialog parts.
        public static readonly Kingmaker.ElementsSystem.ActionList EmptyActionList = 
                                    new Kingmaker.ElementsSystem.ActionList();
        public static readonly Kingmaker.DialogSystem.CueSelection EmptyCueSelection = 
                                    new Kingmaker.DialogSystem.CueSelection();
        public static readonly Kingmaker.UnitLogic.Alignments.AlignmentShift EmptyAlignmentShift = 
                                    new Kingmaker.UnitLogic.Alignments.AlignmentShift();
        public static readonly Kingmaker.ElementsSystem.ConditionsChecker EmptyConditionChecker = 
                                    new Kingmaker.ElementsSystem.ConditionsChecker();
        public static readonly Kingmaker.DialogSystem.DialogSpeaker EmptyDialogSpeaker = 
                                    new Kingmaker.DialogSystem.DialogSpeaker();
        public static readonly Kingmaker.DialogSystem.Blueprints.ShowCheck EmptyShowCheck = 
                                    new Kingmaker.DialogSystem.Blueprints.ShowCheck();
        public static readonly Kingmaker.DialogSystem.CharacterSelection EmptyCharSelect = 
                                    new Kingmaker.DialogSystem.CharacterSelection();

        //##################################################################################################################
        // DIALOG
        // A dialog is the wrapper for the entire conversation. When it starts, it triggers the first cue, and when the 
        // dialog ends, the whole conversation is over. It can have multiple "first cues" but only one will be displayed:
        // the first one in the list that has all its conditions met.
        //##################################################################################################################

        /*******************************************************************************************************************
         * CREATE DIALOG
         *   name:      the name of the dialog
         *   firstcue:  the first cue to display. You can add more after creation.
         ******************************************************************************************************************/
        public static Kingmaker.DialogSystem.Blueprints.BlueprintDialog CreateDialog
                      (string name, Kingmaker.DialogSystem.Blueprints.BlueprintCue firstcue)
        {
            var result = Helpers.CreateBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintDialog>(name);
            result.FirstCue = new Kingmaker.DialogSystem.CueSelection();
            result.FirstCue.Cues.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                     <Kingmaker.Blueprints.BlueprintCueBaseReference>(firstcue));
            result.Conditions = EmptyConditionChecker;
            result.StartActions = EmptyActionList;
            result.FinishActions = EmptyActionList;
            result.ReplaceActions = EmptyActionList;
            result.TurnPlayer = true;
            result.TurnFirstSpeaker = true;
            return result;
        }

        /*******************************************************************************************************************
         * DIALOG: ADD CONDITION
         * If the dialog's conditions are not met, instead of displaying the cue, it will run the replaceactions.
         *   dialog:    the dialog to add a condition to.
         *   condition: the condition required for the dialog to play.
         ******************************************************************************************************************/
        public static void DialogAddCondition(Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, 
                                              Kingmaker.ElementsSystem.Condition condition)
        {
            if (dialog.Conditions == EmptyConditionChecker)
            {//Make a brand new checker
                dialog.Conditions = ConditionalTools.CreateChecker();
            }
            ConditionalTools.CheckerAddCondition(dialog.Conditions, condition);
        }

        /*******************************************************************************************************************
         * DIALOG: ADD START ACTION
         * Actions to perform when the dialog starts, such as changing music.
         *   dialog:    the dialog to add an action to.
         *   action:    the action to take when the dialog starts.
         ******************************************************************************************************************/
        public static void DialogAddStartAction(Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, 
                                                Kingmaker.ElementsSystem.GameAction action)
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

        /*******************************************************************************************************************
         * DIALOG: ADD FINISH ACTION
         * Actions to perform when the conversation ends, such as changing music or starting a cutscene.
         *   dialog:    the dialog to add an action to.
         *   action:    the action to take when the dialog ends.
         ******************************************************************************************************************/
        public static void DialogAddFinishAction(Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, 
                                                 Kingmaker.ElementsSystem.GameAction action)
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

        /*******************************************************************************************************************
         * DIALOG: ADD REPLACE ACTION
         * Actions to perform if the dialog's conditions are not met. For instance, you could have a dialog on a merchant
         * character, with the condition that you haven't spoken to them yet, and a replacement action for opening the
         * trade window. The first time you talk to that character, the condition is met, so you get the dialog. The second
         * time, the condition is not met, so the replacement action of opening the vendor window runs instead.
         *   dialog:    the dialog to add an action to.
         *   action:    the action to take when the dialog conditions aren't met.
         ******************************************************************************************************************/
        public static void DialogAddReplaceAction(Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, 
                                                  Kingmaker.ElementsSystem.GameAction action)
        {
            if (dialog.ReplaceActions == EmptyActionList)
            {//Make a brand new action list
                dialog.ReplaceActions = new Kingmaker.ElementsSystem.ActionList();
            }
            var len = 0;
            if (dialog.ReplaceActions.Actions == null)
            {
                dialog.ReplaceActions.Actions = new Kingmaker.ElementsSystem.GameAction[1];
            }
            else
            {
                len = dialog.ReplaceActions.Actions.Length;
                Array.Resize(ref dialog.ReplaceActions.Actions, len + 1);
            }
            dialog.ReplaceActions.Actions[len] = action;
        }

        /*******************************************************************************************************************
         * DIALOG: INSERT CUE
         * Insert additional cues into the dialog's first cue list. The first one that meets its conditions is the one that
         * will display, so it's generally good to have the very last one have no conditions.
         *   dialog:    the dialog to add a cue to
         *   cue:       the cue to insert
         *   position:  where in the list to insert the cue. 0 puts it at the very start, the first cue to be checked.
         ******************************************************************************************************************/
        public static void DialogInsertCue(Kingmaker.DialogSystem.Blueprints.BlueprintDialog dialog, 
                                           Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, int position)
        {
            dialog.FirstCue.Cues.Insert(position, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                        <Kingmaker.Blueprints.BlueprintCueBaseReference>(cue));
        }

        //##################################################################################################################
        // CUES
        // A cue is the text displayed to the player as narration or a character's speech. It can continue into another
        // cue or cuesequence, or an answerslist for the player to pick an answer from. Like a dialog, it can have multiple
        // things to continue to, and will choose the first one that has all conditions met.
        //##################################################################################################################

        /*******************************************************************************************************************
         * CREATE CUE
         *   name:      The name of the cue. If no key is provided, this name will be used to look up the text string.
         *   key:       optional, used to look up the text string to use for this cue. Name is used if key is not provided.
         *   speaker:   who's saying this line - handles the colored name tag and portrait if applicable. Defaults to none.
         ******************************************************************************************************************/
        public static Kingmaker.DialogSystem.Blueprints.BlueprintCue CreateCue
                      (string name, Kingmaker.DialogSystem.DialogSpeaker speaker = null)
        {
            return CreateCue(name, name, speaker);
        }
        public static Kingmaker.DialogSystem.Blueprints.BlueprintCue CreateCue
                      (string name, string key, Kingmaker.DialogSystem.DialogSpeaker speaker = null)
        {
            var result = Helpers.CreateBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCue>(name, bp =>
                {
                    bp.Text = new Kingmaker.Localization.LocalizedString { m_Key = key };
                    bp.Continue = EmptyCueSelection;
                    bp.AlignmentShift = EmptyAlignmentShift;
                    bp.Conditions = EmptyConditionChecker;
                    if (speaker == null)
                    { 
                        bp.Speaker = EmptyDialogSpeaker; 
                    }
                    else 
                    { 
                        bp.Speaker = speaker; 
                    }
                    bp.OnShow = EmptyActionList;
                    bp.OnStop = EmptyActionList;
                });
            return result;
        }

        /*******************************************************************************************************************
         * CUE: ADD CONDITION
         * Adds a condition for the cue to display. If the conditions are not met, the dialog/answer/whatever that pointed
         * to this cue will fall through and check the next cue in its list.
         * Alternatively, you can simply attach a pre-constructed condition checker, if so desired.
         *   cue:       the cue to add a condition to
         *   condition: the condition to be required
         ******************************************************************************************************************/
        public static void CueSetConditionChecker(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, 
                                                  Kingmaker.ElementsSystem.ConditionsChecker checker)
        {
            cue.Conditions = checker;
        }
        public static void CueAddCondition(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, 
                                           Kingmaker.ElementsSystem.Condition condition)
        {
            if (cue.Conditions == EmptyConditionChecker)
            {//Make a brand new checker
                cue.Conditions = ConditionalTools.CreateChecker();
            }
            ConditionalTools.CheckerAddCondition(cue.Conditions, condition);
        }

        /*******************************************************************************************************************
         * CUE: ADD ON SHOW ACTION
         * Actions to perform when the cue is displayed, such as changing music.
         *   cue:       the cue to add an action to.
         *   action:    the action to take.
         ******************************************************************************************************************/
        public static void CueAddOnShowAction(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, 
                                              Kingmaker.ElementsSystem.GameAction action)
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

        /*******************************************************************************************************************
         * CUE: ADD ON STOP ACTION
         * Actions to perform after this cue has been shown - that is, when ending the conversation, selecting an answer,
         * or clicking the "Continue" button.
         *   cue:       the cue to add an action to.
         *   action:    the action to take.
         ******************************************************************************************************************/
        public static void CueAddOnStopAction(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, 
                                              Kingmaker.ElementsSystem.GameAction action)
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

        /*******************************************************************************************************************
         * CUE: SET SPEAKER
         * Shortcut function to set a companion as the speaker of this cue, optionally also setting a conditional to check
         * that the companion in question is in the party.
         *   cue:               the cue to add an action to.
         *   speaker:           the companion to speak the line
         *   setconditional:    whether to automatically add a conditional to the cue that checks whether the companion in
         *                      question is in the party. Defaults to true if not specified.
         ******************************************************************************************************************/
        public static void CueSetSpeaker(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, 
                                         Companions speaker, bool setconditional = true)
        {
            if (cue.Speaker == EmptyDialogSpeaker)
            {
                cue.Speaker = new Kingmaker.DialogSystem.DialogSpeaker 
                                  { m_Blueprint = CompanionTools.GetCompanionReference(speaker) };
            }
            if (setconditional)
            {
                string name = cue.name + "_" + speaker.ToString() + "_inparty";
                CueAddCondition(cue, ConditionalTools.CreateCompanionInPartyCondition(name, speaker));
            }
        }

        /*******************************************************************************************************************
         * CUE: ADD CONTINUE
         * Adds another cue to the Continue list (that is, the next cue to be shown when clicking Continue).
         *   cue:               the cue you want to add a continue to
         *   nextcue/sequence:  the cue or cuesequence that you want to add as a continue
         *   position:          where in the list to insert this cue. If not specified, it adds to the end of the list.
         ******************************************************************************************************************/
        public static void CueAddContinue(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, 
                                          Kingmaker.DialogSystem.Blueprints.BlueprintCue nextcue, int position = -1)
        {
            if (cue.Continue == EmptyCueSelection)
            {//Make a brand new cue selection.
                cue.Continue = new Kingmaker.DialogSystem.CueSelection();
            }
            if (position == -1)
            {//Insert at the end of the list.
                cue.Continue.Cues.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                      <Kingmaker.Blueprints.BlueprintCueBaseReference>(nextcue));
            }
            else
            {
                cue.Continue.Cues.Insert(position, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                                   <Kingmaker.Blueprints.BlueprintCueBaseReference>(nextcue));
            }
        }
        public static void CueAddContinue(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, 
                                          Kingmaker.DialogSystem.Blueprints.BlueprintCueSequence sequence, 
                                          int position = -1)
        {
            if (cue.Continue == EmptyCueSelection)
            {//Make a brand new cue selection.
                cue.Continue = new Kingmaker.DialogSystem.CueSelection();
            }
            if (position == -1)
            {//Insert at the end of the list.
                cue.Continue.Cues.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                      <Kingmaker.Blueprints.BlueprintCueBaseReference>(sequence));
            }
            else
            {
                cue.Continue.Cues.Insert(position, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                                   <Kingmaker.Blueprints.BlueprintCueBaseReference>(sequence));
            }
        }

        /*******************************************************************************************************************
         * CUE: ADD ANSWERSLIST
         * Adds an answerslist to the cue, allowing the player to select one of its contained answers as a response.
         *   cue:         the cue you want to add player response options to
         *   answerlist:  the answerlist containing the responses
         *   position:    where in the list of lists to insert this. Defaults to the end of the list if not specified.
         *                You shouldn't have to worry about this, as answerslists very rarely have conditionals themselves,
         *                so you almost never see cues with more than one answerslist. It is, however, technically possible
         *                for a cue to have multiple answerslists with conditions - it will display the first one whose
         *                conditions are met. Generally, it's better to put conditions on individual answers.
         ******************************************************************************************************************/
        public static void CueAddAnswersList(Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, 
                                             Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList answerlist, 
                                             int position = -1)
        {
            if (position == -1)
            {//Insert at the end of the list.
                cue.Answers.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                <Kingmaker.Blueprints.BlueprintAnswerBaseReference>(answerlist));
            }
            else
            {
                cue.Answers.Insert(position, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                             <Kingmaker.Blueprints.BlueprintAnswerBaseReference>(answerlist));
            }
        }


        //##################################################################################################################
        // CUESEQUENCE
        // A cuesequence is the mechanism used to allow companions to comment on a dialog without derailing it. Unlike cues
        // and dialogs, it does not just display the first entry that has all conditions met. Rather, it contains a series
        // of first cues, and it goes through each one in turn. If the appropriate companions are present, it will show the
        // cue (and allow you to go down a conversation path from that first cue if applicable), then when it is done with
        // that branch, it will move on and check the next first cue, and so on. Once it's done, it goes to the exit, which
        // defines a cue that will be played after the cuesequence.
        // I had no need to create new cuesequences myself, so the functionality of this section is limited to just adding
        // things to existing sequences, but it could be expanded.
        //##################################################################################################################
        public static void CueSequenceInsertCue(Kingmaker.DialogSystem.Blueprints.BlueprintCueSequence sequence, 
                                                Kingmaker.DialogSystem.Blueprints.BlueprintCue cue, int position = -1)
        {
            if (position == -1)
            {
                sequence.Cues.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                  <Kingmaker.Blueprints.BlueprintCueBaseReference>(cue));
            }
            else 
            {
                sequence.Cues.Insert(position, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                               <Kingmaker.Blueprints.BlueprintCueBaseReference>(cue));
            }
        }

        //##################################################################################################################
        // ANSWERSLIST
        // An answerslist is just a collection of answers. Any answer whose conditions are met will display.
        //##################################################################################################################

        /*******************************************************************************************************************
         * CREATE ANSWERSLIST
         *   name:  the name of the answerslist.
         ******************************************************************************************************************/
        public static Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList CreateAnswersList(string name)
        {
            var result = Helpers.CreateBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>(name, bp =>
                {
                    bp.ShowOnce = false;
                    bp.Conditions = EmptyConditionChecker;
                });
            return result;
        }

        /*******************************************************************************************************************
         * LIST: ADD CONDITION
         * An answerslist can have conditions itself, but this is very rare, and you will likely not need it.
         *   answerlist:    the list to add a condition to
         *   condition:     the condition to add
         ******************************************************************************************************************/
        public static void ListAddCondition(Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList answerlist, 
                                            Kingmaker.ElementsSystem.Condition condition)
        {
            if (answerlist.Conditions == EmptyConditionChecker)
            {//Make a brand new checker
                answerlist.Conditions = ConditionalTools.CreateChecker();
            }
            ConditionalTools.CheckerAddCondition(answerlist.Conditions, condition);
        }

        /*******************************************************************************************************************
         * LIST: ADD ANSWER
         * Insert an answer into the list.
         *   answerlist:    the list to add an answer to
         *   answer:        the answer to insert into the list
         *   position:      where in the list to place the answer. Defaults to adding it to the end if unspecified.
         ******************************************************************************************************************/
        public static void ListAddAnswer(Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList answerlist, 
                                         Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, int position = -1)
        {
            if (position == -1)
            {//Insert at the end of the list.
                answerlist.Answers.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                       <Kingmaker.Blueprints.BlueprintAnswerBaseReference>(answer));
            }
            else
            {
                answerlist.Answers.Insert(position, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                                    <Kingmaker.Blueprints.BlueprintAnswerBaseReference>(answer));
            }
        }

        //##################################################################################################################
        // ANSWERS
        // Answers are the dialog options the player chooses in response to cues. An answer points to the next cue to be
        // displayed if you choose that answer - like dialogs and cues, it can have multiple listed cues, and will display
        // the first one that has its conditions met.
        //##################################################################################################################

        /*******************************************************************************************************************
         * CREATE ANSWER
         *   name:      the name of the answer. If no key is provided, this value is used to look up the text string.
         *   key:       used to look up the text string if provided. Otherwise the name is used as the key.
         *   showonce:  Whether to only show this option once. Defaults to false.
         ******************************************************************************************************************/
        public static Kingmaker.DialogSystem.Blueprints.BlueprintAnswer CreateAnswer(string name, bool showonce = false)
        {
            return CreateAnswer(name, name, showonce);
        }
        public static Kingmaker.DialogSystem.Blueprints.BlueprintAnswer CreateAnswer(string name, string key, 
                                                                                     bool showonce = false)
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

        /*******************************************************************************************************************
         * ANSWER: ADD SHOW CONDITION
         * Answers have two types of condition: one for showing, and one for selecting. A show condition determines whether
         * the answer is even displayed for the player on the list. A select condition determines whether the answer can
         * actually be chosen - if you've ever seen an answer that was grayed out because you didn't meet prerequisites, it
         * met the show conditions but not the select conditions.
         * A show condition is not used for skill checks - those cases where you see an option labeled with things like
         * "[Perception: Passed]" use a separate mechanism defined in the AnswerAddShowCheck function. Show condition is
         * for things like only displaying an option if the player is a particular race, for instance.
         * Alternatively, you could insert a pre-made condition checker instead of individual conditions.
         *   answer:    the answer to add a show condition to
         *   condition: the condition that must be met in order to display the answer
         ******************************************************************************************************************/
        public static void AnswerSetShowConditionChecker(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, 
                                                         Kingmaker.ElementsSystem.ConditionsChecker checker)
        {
            answer.ShowConditions = checker;
        }
        public static void AnswerAddShowCondition(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, 
                                                  Kingmaker.ElementsSystem.Condition condition)
        {
            if (answer.ShowConditions == EmptyConditionChecker)
            {//Make a brand new checker
                answer.ShowConditions = ConditionalTools.CreateChecker();
            }
            ConditionalTools.CheckerAddCondition(answer.ShowConditions, condition);
        }

        /*******************************************************************************************************************
         * ANSWER: ADD SHOW CHECK
         * Runs an on-the-spot skill check to determine whether to display an answer. If you've ever seen options labeled
         * with things like "[Perception: Passed]" those options used this mechanism. 
         *   answer:    the answer to add a show check to
         *   type:      what type of check to make
         *   dc:        the DC of the check.
         ******************************************************************************************************************/
        public static void AnswerAddShowCheck(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, 
                                              Kingmaker.EntitySystem.Stats.StatType type, int dc)
        {
            var check = new Kingmaker.DialogSystem.Blueprints.ShowCheck { Type = type, DC = dc };
            answer.ShowCheck = check;
        }

        /*******************************************************************************************************************
         * ANSWER: ADD SELECT CONDITION
         * Answers have two types of condition: one for showing, and one for selecting. A show condition determines whether
         * the answer is even displayed for the player on the list. A select condition determines whether the answer can
         * actually be chosen - if you've ever seen an answer that was grayed out because you didn't meet prerequisites, it
         * met the show conditions but not the select conditions.
         * Alternatively, you could insert a pre-made condition checker instead of individual conditions.
         *   answer:    the answer to add a select condition to
         *   condition: the condition that must be met in order to choose the answer
         ******************************************************************************************************************/
        public static void AnswerSetSelectConditionChecker(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, 
                                                           Kingmaker.ElementsSystem.ConditionsChecker checker)
        {
            answer.SelectConditions = checker;
        }
        public static void AnswerAddSelectCondition(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, 
                                                    Kingmaker.ElementsSystem.Condition condition)
        {
            if (answer.SelectConditions == EmptyConditionChecker)
            {//Make a brand new checker
                answer.SelectConditions = ConditionalTools.CreateChecker();
            }
            ConditionalTools.CheckerAddCondition(answer.SelectConditions, condition);
        }

        /*******************************************************************************************************************
         * ANSWER: ADD ON SELECT ACTION
         * Actions to be run when this answer is selected, such as starting or ending an etude. Don't use this for
         * alignment shifts, as those use a separate mechanism.
         *   answer:    the answer to add an action to
         *   action:    the action to take when this answer is selected
         ******************************************************************************************************************/
        public static void AnswerAddOnSelectAction(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, 
                                                   Kingmaker.ElementsSystem.GameAction action)
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

        /*******************************************************************************************************************
         * ANSWER: SET ALIGNMENT SHIFT
         * Mark an answer as having an alignment. It will automatically display the alignment flag and handle adjusting the
         * player's alignment if selected.
         *   answer:            the answer to set the alignment of.
         *   direction:         which alignment (Lawful, Chaotic, Good, or Evil) this answer is weighted with.
         *   descriptionkey:    The key to look up the string that will be displayed in the player's choice history.
         ******************************************************************************************************************/
        public static void AnswerSetAlignmentShift(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, 
                                                   string direction, string descriptionkey)
        {
            Kingmaker.UnitLogic.Alignments.AlignmentShiftDirection dir = 
                Kingmaker.UnitLogic.Alignments.AlignmentShiftDirection.TrueNeutral;
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
            answer.AlignmentShift = new Kingmaker.UnitLogic.Alignments.AlignmentShift 
                { 
                    Direction = dir, 
                    Value = 1, 
                    Description = new Kingmaker.Localization.LocalizedString { m_Key = descriptionkey } 
                };
        }

        /*******************************************************************************************************************
         * ANSWER: ADD NEXT CUE
         * Specify the cue that will display in response to this answer.
         *   answer:    the answer to add a response to
         *   cue:       the cue to add as a response
         *   position:  where in the list of next cues to put this cue. The first one that has all conditions met is the
         *              one that will display. Defaults to inserting at the end of the list.
         ******************************************************************************************************************/
        public static void AnswerAddNextCue(Kingmaker.DialogSystem.Blueprints.BlueprintAnswer answer, 
                                            Kingmaker.DialogSystem.Blueprints.BlueprintCueBase cue, int position = -1)
        {
            if (answer.NextCue == EmptyCueSelection)
            {//Make a brand new cue selection.
                answer.NextCue = new Kingmaker.DialogSystem.CueSelection();
            }
            if (position == -1)
            {//Insert at the end of the list.
                answer.NextCue.Cues.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                        <Kingmaker.Blueprints.BlueprintCueBaseReference>(cue));
            }
            else
            {
                answer.NextCue.Cues.Insert(position, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                                     <Kingmaker.Blueprints.BlueprintCueBaseReference>(cue));
            }
        }

        //##################################################################################################################
        // SKILL CHECKS
        // Essentially a skill check is a splitting cue. The player must make a check, and gets directed to the success or
        // failure cue depending on the result.
        //##################################################################################################################

        /*******************************************************************************************************************
         * CREATE CHECK
         * When this cue is reached, it will run a skill check on the spot to determine which outcome to display.
         *   name:      the name of this skill check
         *   type:      the type of check to run
         *   dc:        the DC of the check
         *   success:   the cue to display on success
         *   failure:   the cue to display on failure
         *   exp:       how much experience to award for success. Defaults to none.
         *   hidden:    Whether to hide that this is a check. If not hidden, answers leading to this check will display a
         *              tag that shows the type of check to make, and the result cue will show the "[Success]" or 
         *              "[Failure]" tags. Defaults to false.
         ******************************************************************************************************************/
        public static Kingmaker.DialogSystem.Blueprints.BlueprintCheck CreateCheck
                      (string name, Kingmaker.EntitySystem.Stats.StatType type, int DC, 
                       Kingmaker.DialogSystem.Blueprints.BlueprintCue success, 
                       Kingmaker.DialogSystem.Blueprints.BlueprintCue failure, bool hidden = false)
        {
            return CreateCheck(name, type, DC, success, failure, DialogExperience.NoExperience, hidden);
        }
        public static Kingmaker.DialogSystem.Blueprints.BlueprintCheck CreateCheck
                      (string name, Kingmaker.EntitySystem.Stats.StatType type, int DC, 
                       Kingmaker.DialogSystem.Blueprints.BlueprintCue success, 
                       Kingmaker.DialogSystem.Blueprints.BlueprintCue failure, 
                       Kingmaker.DialogSystem.DialogExperience exp, bool hidden = false)
        {
            var result = Helpers.CreateBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintCheck>(name, bp =>
                {
                    bp.Conditions = EmptyConditionChecker;
                    bp.Type = type;
                    bp.DC = DC;
                    bp.m_Success = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                   <Kingmaker.Blueprints.BlueprintCueBaseReference>(success);
                    bp.m_Fail = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                <Kingmaker.Blueprints.BlueprintCueBaseReference>(failure);
                    bp.Hidden = hidden;
                    bp.Experience = exp;
                });
            return result;
        }
    }
}