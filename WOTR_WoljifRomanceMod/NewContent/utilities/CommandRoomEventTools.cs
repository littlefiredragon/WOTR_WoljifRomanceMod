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
    public static class CommandRoomEventTools
    {
        public static Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent CreateEvent(string name, string description)
        {
            var result = Helpers.CreateBlueprint<Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent>(name, bp =>
            {
                bp.m_DependsOnQuest = (Kingmaker.Blueprints.BlueprintQuestReference)null;
                bp.m_Tags = new Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent.TagList();
                bp.RequiredTags = new Kingmaker.Kingdom.EventLocationTagList();
                bp.OnTrigger = DialogTools.EmptyActionList;
                bp.StatsOnTrigger = new Kingmaker.Kingdom.KingdomStats.Changes();
                bp.UnapplyTriggerOnResolve = true;
                bp.LocalizedName = new Kingmaker.Localization.LocalizedString { m_Key = name };
                bp.LocalizedDescription = new Kingmaker.Localization.LocalizedString { m_Key = description };
                bp.TriggerCondition = DialogTools.EmptyConditionChecker;
                bp.ResolutionTime = 200;
                bp.NeedToVisitTheThroneRoom = true;
                bp.SkipRoll = true;
                bp.AutoResolveResult = Kingmaker.Kingdom.Blueprints.EventResult.MarginType.Fail;
                bp.Solutions = new Kingmaker.Kingdom.Blueprints.PossibleEventSolutions { Entries = new Kingmaker.Kingdom.Blueprints.PossibleEventSolution[4] };
                bp.Components = new BlueprintComponent[1];
                bp.Components[0] = new Kingmaker.Kingdom.Blueprints.EventFinalResults { Results = new Kingmaker.Kingdom.Blueprints.EventResult[0] };
            });
            //result.Solutions.Entries = new Kingmaker.Kingdom.Blueprints.PossibleEventSolution[4];
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

        public static void AddResolution(Kingmaker.Kingdom.Blueprints.BlueprintKingdomEvent eventcard, Kingmaker.Kingdom.Blueprints.EventResult.MarginType restype, string description)
        {
            string uniquename = eventcard.name + "_" + restype.ToString();
            Kingmaker.Kingdom.Blueprints.EventFinalResults resultlist = (Kingmaker.Kingdom.Blueprints.EventFinalResults)eventcard.Components[0];
            if (resultlist.Results == null)
            {
                resultlist.Results = new Kingmaker.Kingdom.Blueprints.EventResult[0];
            }
            var len = resultlist.Results.Length;

            Array.Resize(ref resultlist.Results, len + 1);
            resultlist.Results[len] = new Kingmaker.Kingdom.Blueprints.EventResult { 
                Margin = restype,
                LeaderAlignment = Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Any,
                Condition = DialogTools.EmptyConditionChecker,
                Actions = DialogTools.EmptyActionList,
                StatChanges = new Kingmaker.Kingdom.KingdomStats.Changes(),
                SuccessCount = 1,
                LocalizedDescription = new Kingmaker.Localization.LocalizedString { m_Key = description }
            };
        } 
    }
}