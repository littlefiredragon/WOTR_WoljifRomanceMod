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

//##########################################################################################################################
// ETUDE AND FLAG TOOLS
// Etudes and Flags are essentially variables that tell you about the state of the game. Flags hold numbers and can act
// as booleans by checking whether they're unlocked or not. Etudes are a little fuzzier, acting as statements about what is
// currently going on in the world. For instance, if you are in a romance with Daeran, there's a DaeranRomanceActive Etude
// to tell the game that you have an active romance with him. Etudes can also act as more abstract signals, which trigger
// events when certain conditions are met.
// An Etude can be Not Started (hasn't been triggered yet), Started (triggered, but not actively running; this is a sort of
// wait state, in which the etude is not considered active, but is waiting for its activation conditions to be met),
// Playing (actively running; etudes move from Started to Playing immediately if they have no activation conditions, or if
// they do have conditions, they move to Playing as soon as those conditions are met), Completing (a transition state you
// will almost never see), or Completed (finished, over, done, no longer active).
//##########################################################################################################################

namespace WOTR_WoljifRomanceMod
{
    public static class EtudeTools
    {
        public enum EtudeStatus
        {
            NotStarted, Started, Playing, CompletionInProgress, Completed
        }

        //##################################################################################################################
        // FLAGS
        //##################################################################################################################
        /*******************************************************************************************************************
         * CREATE FLAG
         * Create a new unlockable flag with the provided name. Default value is 0, defaults to locked.
         ******************************************************************************************************************/
        public static Kingmaker.Blueprints.BlueprintUnlockableFlag CreateFlag([NotNull] string name)
        {
            return Helpers.CreateBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>(name);
        }

        //##################################################################################################################
        // ETUDES
        //##################################################################################################################

        /*******************************************************************************************************************
         * CREATE ETUDE
         * Creates a new etude.
         *   name:              Name of the etude
         *   parent:            The etude this one should be filed under. This is partially for structural reference, and
         *                      partially because some etudes will, for instance, complete all their children when they are
         *                      completed.
         *   startsparent:      When this etude starts, should it automatically start its parent, too?
         *   completesparent:   When this etude completes, should the parent automatically be completed, too?
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Etudes.BlueprintEtude CreateEtude
                      ([NotNull] string name, Kingmaker.AreaLogic.Etudes.BlueprintEtude parent, bool startsparent, 
                       bool completesparent)
        {
            return CreateEtude(name, Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<BlueprintEtudeReference>(parent),
                               startsparent, completesparent);
        }
        public static Kingmaker.AreaLogic.Etudes.BlueprintEtude CreateEtude
                      ([NotNull] string name, BlueprintEtudeReference parent, bool startsparent, bool completesparent)
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

                    bp.m_LinkedAreaPart = new BlueprintAreaPartReference() { deserializedGuid = BlueprintGuid.Empty };
                    bp.m_AddedAreaMechanics = new List<Kingmaker.Blueprints.Area.BlueprintAreaMechanicsReference>();

                    bp.CompletionCondition = DialogTools.EmptyConditionChecker;
                    bp.ActivationCondition = DialogTools.EmptyConditionChecker;
                });
            return result;
        }

        /*******************************************************************************************************************
         * ETUDE: ADD STARTS WITH
         * When an etude starts, it also starts any other etudes in its StartsWith list. This function adds to that list.
         *   etude:         the etude whose list you want to add to
         *   startedetudes: The etudes that should be started along with it.
         ******************************************************************************************************************/
        public static void EtudeAddStartsWith(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, 
                                              Kingmaker.AreaLogic.Etudes.BlueprintEtude startedetude)
        {
            etude.m_StartsWith.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                   <BlueprintEtudeReference>(startedetude));
        }
        public static void EtudeAddStartsWith(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, 
                                              params Kingmaker.AreaLogic.Etudes.BlueprintEtude[] startedetudes)
        {
            foreach (Kingmaker.AreaLogic.Etudes.BlueprintEtude addedetude in startedetudes)
            {
                etude.m_StartsWith.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                       <BlueprintEtudeReference>(addedetude));
            }
        }

        /*******************************************************************************************************************
         * ETUDE: ADD STARTS ON COMPLETE
         * When an etude completes, it can automatically trigger a different etude to start.
         *   etude:         the etude whose completion will trigger other etudes.
         *   startedetudes: The etudes that should be started when that happens.
         ******************************************************************************************************************/
        public static void EtudeAddStartsOnComplete(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, 
                                                    Kingmaker.AreaLogic.Etudes.BlueprintEtude startedetude)
        {
            etude.m_StartsOnComplete.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                         <BlueprintEtudeReference>(startedetude));
        }
        public static void EtudeAddStartsOnComplete(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, 
                                                    params Kingmaker.AreaLogic.Etudes.BlueprintEtude[] startedetudes)
        {
            foreach (Kingmaker.AreaLogic.Etudes.BlueprintEtude addedetude in startedetudes)
            {
                etude.m_StartsOnComplete.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                             <BlueprintEtudeReference>(addedetude));
            }
        }

        /*******************************************************************************************************************
         * ETUDE: ADD ON PLAY TRIGGER
         * When an etude begins playing (becomes active), it can trigger actions. This is how you do things like start a
         * cutscene after a certain amount of time has passed, for instance.
         *   etude:         The etude that will trigger the actions
         *   actionlist:    An ActionList constructed via ActionTools, containing the actions to run.
         ******************************************************************************************************************/
        public static void EtudeAddOnPlayTrigger(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, 
                                                 Kingmaker.ElementsSystem.ActionList actionlist)
        {
            var comp = new Kingmaker.Designers.EventConditionActionSystem.Events.EtudePlayTrigger();
            comp.Conditions = DialogTools.EmptyConditionChecker;
            comp.Actions = actionlist;
            EtudeAddComponent(etude, comp);
        }

        /*******************************************************************************************************************
         * ETUDE: ADD ON COMPLETE TRIGGER
         * When an etude completes, it can trigger actions. Completion is the permanent ending of an etude, not to be
         * confused with deactivation, which is simply moving a playing etude back into the started state to wait for its
         * activation conditions to be met again.
         *   etude:         The etude that will trigger the actions
         *   actionlist:    An ActionList constructed via ActionTools, containing the actions to run.
         ******************************************************************************************************************/
        public static void EtudeAddOnCompleteTrigger(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude,
                                                     Kingmaker.ElementsSystem.ActionList actionlist)
        {
            var comp = new Kingmaker.Designers.EventConditionActionSystem.Events.EtudeCompleteTrigger();
            comp.Actions = actionlist;
            EtudeAddComponent(etude, comp);
        }

        /*******************************************************************************************************************
         * ETUDE: ADD ON DEACTIVATE TRIGGER
         * When an etude deactivates, it can trigger actions. Deactivation is NOT the same as completion! Deactivation is
         * essentially moving a playing etude back into the started state. For most etudes, this isn't something that
         * normally happens.
         *   etude:         The etude that will trigger the actions
         *   actionlist:    An ActionList constructed via ActionTools, containing the actions to run.
         ******************************************************************************************************************/
        public static void EtudeAddOnDeactivateTrigger(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, 
                                                       Kingmaker.ElementsSystem.ActionList actionlist)
        {
            var comp = new Kingmaker.Designers.EventConditionActionSystem.Events.DeactivateTrigger();
            comp.Conditions = DialogTools.EmptyConditionChecker;
            comp.Actions = actionlist;
            EtudeAddComponent(etude, comp);
        }

        /*******************************************************************************************************************
         * ETUDE: ADD ON REST TRIGGER
         * If you rest/camp while this etude is playing, it will trigger these actions.
         *   etude:         The etude that will trigger the actions
         *   actionlist:    An ActionList constructed via ActionTools, containing the actions to run.
         ******************************************************************************************************************/
        public static void EtudeAddOnRestTrigger(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, 
                                                 Kingmaker.ElementsSystem.ActionList actionlist)
        {
            var comp = new Kingmaker.Designers.EventConditionActionSystem.Events.RestTrigger();
            comp.Conditions = DialogTools.EmptyConditionChecker;
            comp.Actions = actionlist;
            EtudeAddComponent(etude, comp);
        }

        /*******************************************************************************************************************
         * ETUDE: ADD CONFLICTING GROUPS
         * Conflicting groups are sets of etudes that can't be playing at the same time. If multiple etudes in a conflicting
         * group try to play at once, the one with the highest priority wins, and the others temporarily deactivate until
         * the higher priority etude is done.
         * This is particularly important for moving companions around! Under normal circumstances, companions have an etude
         * that places them at a default location in your base. It has a very low priority, so when you want to move them,
         * you just start an etude that conflicts with the group the default location etude is in. The default location
         * etude will temporarily deactivate, allowing you to translocate the companion.
         *   etude: The etude to make conflict with the group.
         *   group: The conflicting group to add. An etude can have multiple conflicting groups.
         ******************************************************************************************************************/
        public static void EtudeAddConflictingGroups(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, 
                                                     Kingmaker.AreaLogic.Etudes.BlueprintEtudeConflictingGroup group)
        {
            etude.m_ConflictingGroups.Add(Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                                          <BlueprintEtudeConflictingGroupReference>(group));
        }

        /*******************************************************************************************************************
         * ETUDE: ADD ACTIVATION CONDITION
         * Adds activation conditions to an etude. An etude with no activation conditions will play immediately when
         * started, but an etude that does have conditions will only move from started to playing when the conditons are
         * met. If the etude is deactivated, it moves back into started, and will only play again once the conditions are
         * met again.
         *   etude:     The etude to add a condition to.
         *   condition: the condition to be added to the requirements.
         ******************************************************************************************************************/
        public static void EtudeAddActivationCondition(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, 
                                                       Kingmaker.ElementsSystem.Condition condition)
        {
            if (etude.ActivationCondition == DialogTools.EmptyConditionChecker)
            {//Make a brand new checker
                etude.ActivationCondition = ConditionalTools.CreateChecker();
            }
            condition.Owner = etude;
            ConditionalTools.CheckerAddCondition(etude.ActivationCondition, condition);
        }

        /*******************************************************************************************************************
         * ETUDE: ADD COMPONENT
         * Some properties of etudes are held in the components array. The most common forms of components are already
         * handled by other functions in the EtudeTools, so you shouldn't have to call this directly in most cases.
         *   etude: The etude to add a component to
         *   comp:  the component to add
         ******************************************************************************************************************/
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


        /*******************************************************************************************************************
         * ETUDE: ADD DELAYED ACTION - DO NOT USE!
         * I have left this commented out function here so I can give an explanation. In vanilla WOTR, "x days later"
         * events are handled by delayed actions on etudes. However, if you try to do this with a mod-injected etude, the
         * timer will reset every time you load the game, because the mod is re-injecting its content.
         * Instead, I have created a timer mechanism you can find in MiscTools that acts as a reasonable substitute for
         * etudes with delayed actions.
         ******************************************************************************************************************/
        /*public static void EtudeAddDelayedAction(Kingmaker.AreaLogic.Etudes.BlueprintEtude etude, int days, 
                                                 Kingmaker.ElementsSystem.ActionList actionlist)
        {
            var comp = new Kingmaker.Designers.EventConditionActionSystem.Events.EtudeInvokeActionsDelayed();
            comp.m_ActionList = actionlist;
            comp.m_Days = days;
            EtudeAddComponent(etude, comp);
        }*/
    }
}