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
using Kingmaker.Blueprints.Area;

//##########################################################################################################################
// EVENT TOOLS
// "Event" as used here is not meant in the sense of Kingmaker.Designers.EventConditionActionSystem.Events, but rather in
// the sense of campfire encounters or crusade event cards.
//##########################################################################################################################

namespace WOTR_WoljifRomanceMod
{
    public static class EventTools
    {
        //##################################################################################################################
        // COMMAND ROOM EVENTS
        //##################################################################################################################
        /*******************************************************************************************************************
         * CREATE COMMAND ROOM EVENT
         * Create a new crusade event card. Specifically, this function is intended for creating "So-and-so wants to talk"
         * cards to notify the player of new developments. It would require some reworking to make a wider variety of
         * events, as they would have different traits where here I just broadly applied settings for notifications.
         ******************************************************************************************************************/
        public static Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent CreateCommandRoomEvent
                      (string name, string description)
        {
            var result = Helpers.CreateBlueprint<Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent>(name, bp =>
                {
                    bp.m_DependsOnQuest = (Kingmaker.Blueprints.BlueprintQuestReference)null;
                    bp.m_Tags = new Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent.TagList();
                    bp.RequiredTags = new Kingmaker.Kingdom.EventLocationTagList();
                    bp.OnTrigger = DialogTools.EmptyActionList;
                    bp.StatsOnTrigger = new Kingmaker.Kingdom.KingdomStats.Changes();
                    bp.UnapplyTriggerOnResolve = true;
                    bp.LocalizedName = new Kingmaker.Localization.LocalizedString 
                                           { m_Key = name };
                    bp.LocalizedDescription = new Kingmaker.Localization.LocalizedString 
                                                  { m_Key = description };
                    bp.TriggerCondition = DialogTools.EmptyConditionChecker;
                    bp.ResolutionTime = 200;
                    bp.NeedToVisitTheThroneRoom = true;
                    bp.SkipRoll = true;
                    bp.AutoResolveResult = Kingmaker.Kingdom.Blueprints.EventResult.MarginType.Fail;
                    bp.Solutions = new Kingmaker.Kingdom.Blueprints.PossibleEventSolutions 
                                       { Entries = new Kingmaker.Kingdom.Blueprints.PossibleEventSolution[4] };
                    bp.Components = new BlueprintComponent[1];
                    bp.Components[0] = new Kingmaker.Kingdom.Blueprints.EventFinalResults 
                                           { Results = new Kingmaker.Kingdom.Blueprints.EventResult[0] };
                });
            for (int i = 0; i < 4; i++)
            {
                result.Solutions.Entries[i] = new Kingmaker.Kingdom.Blueprints.PossibleEventSolution();
                result.Solutions.Entries[i].Resolutions = new Kingmaker.Kingdom.Blueprints.EventResult[0];
            }
            result.Solutions.Entries[0].Leader = Kingmaker.Kingdom.LeaderType.Counselor;
            result.Solutions.Entries[1].Leader = Kingmaker.Kingdom.LeaderType.Strategist;
            result.Solutions.Entries[2].Leader = Kingmaker.Kingdom.LeaderType.Diplomat;
            result.Solutions.Entries[3].Leader = Kingmaker.Kingdom.LeaderType.General;

            return result;
        }

        /*******************************************************************************************************************
         * COMMAND ROOM EVENT: ADD RESOLUTION
         * Adds a resolution state to the card, such as success or failure.
         ******************************************************************************************************************/
        public static void AddResolution(Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent eventcard, 
                                         Kingmaker.Kingdom.Blueprints.EventResult.MarginType restype, string description)
        {
            string uniquename = eventcard.name + "_" + restype.ToString();
            var resultlist = (Kingmaker.Kingdom.Blueprints.EventFinalResults)eventcard.Components[0];
            if (resultlist.Results == null)
            {
                resultlist.Results = new Kingmaker.Kingdom.Blueprints.EventResult[0];
            }
            var len = resultlist.Results.Length;

            Array.Resize(ref resultlist.Results, len + 1);
            resultlist.Results[len] = new Kingmaker.Kingdom.Blueprints.EventResult 
                { 
                    Margin = restype,
                    LeaderAlignment = Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Any,
                    Condition = DialogTools.EmptyConditionChecker,
                    Actions = DialogTools.EmptyActionList,
                    StatChanges = new Kingmaker.Kingdom.KingdomStats.Changes(),
                    SuccessCount = 1,
                    LocalizedDescription = new Kingmaker.Localization.LocalizedString { m_Key = description }
                };
        }

        //##################################################################################################################
        // CAMPING ENCOUNTERS
        //##################################################################################################################
        /*******************************************************************************************************************
         * CREATE CAMPING ENCOUNTER
         * Create a new camping encounter. Does not immediately add it to the list of encounters! For that you will need
         * the AddCampEventAction from ActionTools. Also, the camping event is just the event itself, not what happens in
         * that event; that is all defined by the camp event's actions, which are added with CampEventAddAction.
         ******************************************************************************************************************/
        public static Kingmaker.RandomEncounters.Settings.BlueprintCampingEncounter CreateCampingEvent(string name, 
                                                                                                       int chance)
        {
            var result = Helpers.CreateBlueprint<Kingmaker.RandomEncounters.Settings.BlueprintCampingEncounter>(name, bp =>
                {
                    bp.Chance = chance;
                    bp.Conditions = DialogTools.EmptyConditionChecker;
                    bp.EncounterActions = DialogTools.EmptyActionList;
                    bp.InterruptsRest = false;
                    bp.PartyTired = false;
                    bp.MainCharacterTired = false;
                    bp.NotOnGlobalMap = true;
                });
            return result;
        }

        /*******************************************************************************************************************
         * CAMPING ENCOUNTER: ADD CONDITION
         * When camping, the chance of the encounter determines how likely it is to be selected, but once it's selected, a
         * camping encounter must actually meet any conditions it has in order to actually occur. This function allows you
         * to add those conditions. For instance, an event with a 100% chance that has a condition for a certain character
         * to be in the party will absolutely happen the first time you camp with that character in the party, but it won't
         * happen until then, no matter how many times you camp without that character in the party.
         ******************************************************************************************************************/
        public static void CampEventAddCondition(Kingmaker.RandomEncounters.Settings.BlueprintCampingEncounter campevent, 
                                                 Kingmaker.ElementsSystem.Condition condition)
        {
            if (campevent.Conditions == DialogTools.EmptyConditionChecker)
            {//Make a brand new checker if needed
                campevent.Conditions = ConditionalTools.CreateChecker();
            }
            condition.Owner = campevent;
            ConditionalTools.CheckerAddCondition(campevent.Conditions, condition);
        }

        /*******************************************************************************************************************
         * CAMPING ENCOUNTER: ADD ACTION
         * When a camping event occurs, what actually happens, such as cutscenes or dialog, is defined by the event's
         * actions list. This function allows you to add actions to that list.
         ******************************************************************************************************************/
        public static void CampEventAddAction(Kingmaker.RandomEncounters.Settings.BlueprintCampingEncounter campevent, 
                                              Kingmaker.ElementsSystem.GameAction action)
        {
            if (campevent.EncounterActions == DialogTools.EmptyActionList)
            {//Make a brand new action list if needed
                campevent.EncounterActions = new Kingmaker.ElementsSystem.ActionList();
            }
            var len = 0;
            if (campevent.EncounterActions.Actions == null)
            {
                campevent.EncounterActions.Actions = new Kingmaker.ElementsSystem.GameAction[1];
            }
            else
            {
                len = campevent.EncounterActions.Actions.Length;
                Array.Resize(ref campevent.EncounterActions.Actions, len + 1);
            }
            campevent.EncounterActions.Actions[len] = action;
        }
    }
}