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
    public static class EtudeTools
    {
        public enum EtudeStatus
        {
            NotStarted, Started, Playing, CompletionInProgress, Completed
        }

        public static Kingmaker.Blueprints.BlueprintUnlockableFlag CreateFlag([NotNull] string name)
        {
            return Helpers.CreateBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>(name);
        }
        public static Kingmaker.AreaLogic.Etudes.BlueprintEtude CreateEtude([NotNull] string name, Kingmaker.AreaLogic.Etudes.BlueprintEtude parent, bool startsparent, bool completesparent)
        {
            return CreateEtude(name, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintEtudeReference>(parent), startsparent, completesparent);
        }
        public static Kingmaker.AreaLogic.Etudes.BlueprintEtude CreateEtude([NotNull] string name, BlueprintEtudeReference parent, bool startsparent, bool completesparent)
        {
            var result = Helpers.CreateBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>(name, bp =>
            {
                bp.m_StartsParent = startsparent;
                bp.m_CompletesParent = completesparent;
                bp.m_AllowActionStart = true;
                bp.m_IncludeAreaParts = false;

                bp.m_Parent = parent;

                bp.m_StartsOnComplete = new List<BlueprintEtudeReference>();
                bp.m_StartsWith = new List<BlueprintEtudeReference>();
                bp.m_Synchronized = new List<BlueprintEtudeReference>();

                bp.m_ConflictingGroups = new List<BlueprintEtudeConflictingGroupReference>();

                //bp.m_LinkedAreaPart = null;
                bp.m_LinkedAreaPart = new BlueprintAreaPartReference() { deserializedGuid = BlueprintGuid.Empty };
                bp.m_AddedAreaMechanics = new List<Kingmaker.Blueprints.Area.BlueprintAreaMechanicsReference>();

                bp.CompletionCondition = DialogTools.EmptyConditionChecker;
                bp.ActivationCondition = DialogTools.EmptyConditionChecker;
            });
            return result;
        }

        public static void EtudeAddStartsWith(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, Kingmaker.AreaLogic.Etudes.BlueprintEtude startedetude)
        {
            etude.m_StartsWith.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference <BlueprintEtudeReference>(startedetude));
        }
        public static void EtudeAddStartsWith(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, params Kingmaker.AreaLogic.Etudes.BlueprintEtude[] startedetudes)
        {
            foreach (Kingmaker.AreaLogic.Etudes.BlueprintEtude addedetude in startedetudes)
            {
                etude.m_StartsWith.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintEtudeReference>(addedetude));
            }
        }

        public static void EtudeAddStartsOnComplete(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, Kingmaker.AreaLogic.Etudes.BlueprintEtude startedetude)
        {
            etude.m_StartsOnComplete.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintEtudeReference>(startedetude));
        }
        public static void EtudeAddStartsOnComplete(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, params Kingmaker.AreaLogic.Etudes.BlueprintEtude[] startedetudes)
        {
            foreach (Kingmaker.AreaLogic.Etudes.BlueprintEtude addedetude in startedetudes)
            {
                etude.m_StartsOnComplete.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintEtudeReference>(addedetude));
            }
        }

        public static void EtudeAddComponent(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, BlueprintComponent comp)
        {
            var currentlen = 0;
            if (etude.Components == null)
            {
                etude.Components = new BlueprintComponent[1];
            }
            else
            {
                currentlen = etude.Components.Length;
                Array.Resize(ref etude.Components, currentlen + 1);
            }
            comp.OwnerBlueprint = etude;
            comp.name = Guid.NewGuid().ToString();
            etude.Components[currentlen] = comp;
        }
        public static void EtudeAddOnPlayTrigger(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, Kingmaker.ElementsSystem.ActionList actionlist)
        {
            var comp = new Kingmaker.Designers.EventConditionActionSystem.Events.EtudePlayTrigger();
            comp.Conditions = DialogTools.EmptyConditionChecker;
            comp.Actions = actionlist;
            EtudeAddComponent(etude, comp);
        }
        public static void EtudeAddOnDeactivateTrigger(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, Kingmaker.ElementsSystem.ActionList actionlist)
        {
            var comp = new Kingmaker.Designers.EventConditionActionSystem.Events.DeactivateTrigger();
            comp.Conditions = DialogTools.EmptyConditionChecker;
            comp.Actions = actionlist;
            EtudeAddComponent(etude, comp);
        }
        public static void EtudeAddOnRestTrigger(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, Kingmaker.ElementsSystem.ActionList actionlist)
        {
            var comp = new Kingmaker.Designers.EventConditionActionSystem.Events.RestTrigger();
            comp.Conditions = DialogTools.EmptyConditionChecker;
            comp.Actions = actionlist;
            EtudeAddComponent(etude, comp);
        }
        public static void EtudeAddCompleteTrigger(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, Kingmaker.ElementsSystem.ActionList actionlist)
        {
            var comp = new Kingmaker.Designers.EventConditionActionSystem.Events.EtudeCompleteTrigger();
            comp.Actions = actionlist;
            EtudeAddComponent(etude, comp);
        }

        public static void EtudeAddConflictingGroups(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup group)
        {
            etude.m_ConflictingGroups.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintEtudeConflictingGroupReference>(group));
        }

        public static void EtudeAddActivationCondition(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, Kingmaker.ElementsSystem.Condition condition)
        {
            if (etude.ActivationCondition == DialogTools.EmptyConditionChecker)
            {//Make a brand new checker
                etude.ActivationCondition = ConditionalTools.CreateChecker();
            }
            condition.Owner = etude;
            ConditionalTools.CheckerAddCondition(etude.ActivationCondition, condition);
        }

        public static void EtudeAddDelayedAction(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, int days, Kingmaker.ElementsSystem.ActionList actionlist)
        {
            var comp = new Kingmaker.Designers.EventConditionActionSystem.Events.EtudeInvokeActionsDelayed();
            comp.m_ActionList = actionlist;
            comp.m_Days = days;
            EtudeAddComponent(etude, comp);
        }
    }
}