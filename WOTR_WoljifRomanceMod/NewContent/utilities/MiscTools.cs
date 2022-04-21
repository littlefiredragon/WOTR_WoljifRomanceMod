using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem;
using System;
using TabletopTweaks;
using TabletopTweaks.Config;
using TabletopTweaks.Utilities;
using UnityModManagerNet;
using TabletopTweaks.Extensions;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Blueprints.Area;

//##########################################################################################################################
// MISC TOOLS
// While most of the tools for this mod fit into neat categories, there are a few things that needed to be defined that
// simply didn't fit in anywhere, so I've consolidated them into this catch-all section.
//##########################################################################################################################

namespace WOTR_WoljifRomanceMod
{
    //######################################################################################################################
    // EVENT HANDLERS
    // Classes that implement existing interfaces, and are meant to subscribe to the pubsub system. Once subscribed, they
    // will do things on certain events, such as loading a new area or when the calendar day changes.
    //######################################################################################################################
    class AreaWatcher : Kingmaker.PubSubSystem.IAreaHandler
    {
        public void OnAreaDidLoad()
        {
        }
        public void OnAreaBeginUnloading()
        {
            // When leaving Drezen, which includes using the war table
            if (Game.Instance.CurrentlyLoadedArea.AssetGuidThreadSafe.Equals("2570015799edf594daf2f076f2f975d8", 
                                                                             StringComparison.OrdinalIgnoreCase))
            {
                // Turn off bedroom barks mode
                Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_BedroomBarksFlag").Lock();
                var EtudeBP = Resources.GetModBlueprint
                              <Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_BedroomBarksEtude");
                var Etude = Game.Instance.Player.EtudesSystem.Etudes.GetFact(
                            (Kingmaker.Blueprints.Facts.BlueprintFact)EtudeBP);
                Etude.Deactivate();
            }
        }
    }
    class AreaPartWatcher : Kingmaker.PubSubSystem.IAreaPartHandler
    {
        public void OnAreaPartChanged(BlueprintAreaPart previous)
        {
            // When leaving the Citadel to go to another part of Drezen
            if (previous.AssetGuidThreadSafe.Equals("2570015799edf594daf2f076f2f975d8", StringComparison.OrdinalIgnoreCase))
            {
                // Turn off bedroom barks mode
                Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_BedroomBarksFlag").Lock();
                var EtudeBP = Resources.GetModBlueprint
                              <Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_BedroomBarksEtude");
                var Etude = Game.Instance.Player.EtudesSystem.Etudes.GetFact(
                            (Kingmaker.Blueprints.Facts.BlueprintFact)EtudeBP);
                Etude.Deactivate();
            }
        }
    }

    // As explained in EtudeTools, in vanilla WOTR, "x days later" events are handled by delayed actions on etudes. 
    // However, if you try to do this with a mod-injected etude, the timer will reset every time you load the game, because
    // the mod is re-injecting its content. Instead, I have created a timer mechanism that acts as a reasonable substitute 
    // for etudes with delayed actions. The TimeKeeper is responsible for actually making the Timers work by updating them
    // every day.
    public class TimeKeeper : Kingmaker.Kingdom.IKingdomDayHandler
    {
        public SortedDictionary<string, Timer> Timers;

        public TimeKeeper()
        {
            Timers = new SortedDictionary<string, Timer>();
        }
        public Timer AddTimer(string name)
        {
            Timers.Add(name, new Timer(name));
            return Timers[name];
        }
        public Timer getTimer(string name)
        {
            return Timers[name];
        }
        public void OnNewDay()
        {
            foreach (KeyValuePair<string, Timer> Clock in Timers)
            {
                // Essentially, every time the day ticks over, go through the list of timers and if a timer is active,
                // increment its days passed counter.
                if (Clock.Value.isActive())
                {
                    Clock.Value.incrementTimer();
                }
            }
        }
    }

    //######################################################################################################################
    // CUSTOM OBJECTS
    //######################################################################################################################

    // A Timer is just a pair of unlockable flags, one to mark whether the timer is active, and one to actually act as a
    // counter keeping the amount of time passed since the timer became active.
    public class Timer
    {
        public Kingmaker.Blueprints.BlueprintUnlockableFlag active;
        public Kingmaker.Blueprints.BlueprintUnlockableFlag time;
        public Timer(string name)
        {
            active = EtudeTools.CreateFlag(name + "_active");
            time = EtudeTools.CreateFlag(name + "_time");
        }
        public bool isActive()
        {
            return active.Value != 0;
        }
        public void incrementTimer()
        {
            time.Value++;
        }
    }

    //######################################################################################################################
    // CUSTOM EVALUATORS
    //######################################################################################################################
    
    /* The in-game cutscenes use an object called a Locator to help position them, but you can usually make do with
     * just using a positionevaluator of some sort. I originally tried to implement tools for the usage of actual
     * Locators, but it caused a snarl of problems. Essentially, trying to make custom Locators through code alone
     * is really freaking hard, and led to a variety of strange hoops I had to jump through, like making them on the
     * fly with an AreaWatcher that waited for areas to load. The problem with that was that it gated the creation of
     * any blueprint that needed the locator, which in turn meant that the blueprints weren't guaranteed to exist at
     * all times, which caused save file dependencies and all kinds of nastiness.
     * Trust me, just use the position evaluators. I've made this FakeLocator class to act as similarly to a real Locator
     * for the purpose of cutscenes as possible.
     */
    public class FakeLocator : Kingmaker.ElementsSystem.PositionEvaluator
    {
        public float x;
        public float y;
        public float z;
        public float r;

        public FakeLocator(float x, float y, float z, float r)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.r = r;
        }
        public override string GetCaption()
        {
            return "FakeLocator Caption";
        }

        public override UnityEngine.Vector3 GetValueInternal()
        {
            return new UnityEngine.Vector3(x, y, z);
        }

        public Kingmaker.Designers.EventConditionActionSystem.Evaluators.FloatConstant GetRotation()
        {
            return new Kingmaker.Designers.EventConditionActionSystem.Evaluators.FloatConstant { Value = r };
        }
    }

    // Meant to just pass a unit straight through to something that wants a unit evaluator, without actually doing anything
    public class ActionSpawnedUnitEvaluator : Kingmaker.ElementsSystem.UnitEvaluator
    {
        public SpawnUnit action;
        public override string GetCaption()
        {
            return "Generic Unit Evaluator";
        }

        public override UnitEntityData GetValueInternal()
        {
            return action.entity;
        }

        public void setAction(SpawnUnit inputaction)
        {
            action = inputaction;
        }
    }

    //######################################################################################################################
    // CUSTOM CONDITIONS
    //######################################################################################################################
    public class CampEventExists : Kingmaker.ElementsSystem.Condition
    {
        public Kingmaker.RandomEncounters.Settings.BlueprintCampingEncounter Encounter;
        public override bool CheckCondition()
        {
            return Game.Instance.Player.Camping.ExtraEncounters.IndexOf(Encounter) != -1;
        }
        public override string GetConditionCaption()
        {
            return "Checks if camp encounter is in encounter list.";
        }
    }

    //######################################################################################################################
    // CUSTOM ACTIONS
    // When you want to do something in a cutscene or dialog that Owlcat doesn't have an Action for, you can just make your
    // own action and write whatever code you want in the RunAction function.
    //######################################################################################################################
    public class ControlWeather : Kingmaker.ElementsSystem.GameAction
    {
        public Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType inclemency;
        public bool start;
        public Kingmaker.Controllers.WeatherController control;
        public Kingmaker.AreaLogic.Cutscenes.Cutscene owner;
        public override string GetCaption()
        {
            return "Overrides weather conditions.";
        }
        public override void RunAction()
        {
            control = Kingmaker.Controllers.WeatherController.Instance;
            if (start)
            {
                control.StartOverrideInclemency(owner, inclemency, true);
            }
            else
            {
                control.StopOverrideInclemency(owner, true);
            }
        }
    }

    // Because I could find no way to turn off the feature that keeps units tethered to the ground, placing them on top of
    // scenery such as the commander's bed was not possible. The fly height feature is intended to make hovering units like
    // birds stay a certain distance above the ground, but in a pinch, it can be used to move units up and down without
    // struggling with the clip-to-ground feature. Stupid problems require stupid solutions.
    public class SetFlyHeight : Kingmaker.ElementsSystem.GameAction
    {
        public Kingmaker.ElementsSystem.UnitEvaluator Unit;
        public float height;
        public override string GetCaption()
        {
            return "Sets FlyHeight to make units hover above the ground.";
        }
        public override void RunAction()
        {
            Unit.GetValue().FlyHeight = height;
        }
    }

    // HERE BE DRAGONS. I was unable to determine how to get my hands on proper unit spawners, so instead I made an action
    // that spawns units directly. It's relatively untested, so beware!
    public class SpawnUnit : Kingmaker.ElementsSystem.GameAction
    {
        public Kingmaker.Blueprints.BlueprintUnit unit;
        public FakeLocator location;
        public Kingmaker.EntitySystem.Entities.UnitEntityData entity;
        public UnityEngine.GameObject effect;
        public bool spawneffect = false;
        public override string GetCaption()
        {
            return "Spawn a unit.";
        }
        public override void RunAction()
        {
            entity = Game.Instance.EntityCreator.SpawnUnit(unit, location.GetValue(), UnityEngine.Quaternion.identity, 
                                                           null);
            
            if (spawneffect)
            {
                Kingmaker.Visual.Particles.FxHelper.SpawnFxOnUnit(effect, entity.View);
            }
        }
        public Kingmaker.EntitySystem.Entities.UnitEntityData GetEntity()
        {
            return entity;
        }
    }

    //######################################################################################################################
    // ENUMERATORS
    //######################################################################################################################
    public enum Companions
    {
        None = -1, Player, Arueshalae, Camellia, Daeran, Ember, Greybor, Lann, Nenio, Regill, Seelah, Sosiel, Wenduag,
        Woljif
    }
    //######################################################################################################################
    // COMPANION TOOLS
    // Shortcuts for getting unit references and evaluators for companions or the player.
    //######################################################################################################################
    public static class CompanionTools
    {
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
                result = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                         <Kingmaker.Blueprints.BlueprintUnitReference>
                         (Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnit>(companionid));
            }
            return result;
        }
        public static Kingmaker.ElementsSystem.UnitEvaluator GetCompanionEvaluator
                      (Companions companion, Kingmaker.Blueprints.SimpleBlueprint owner)
        {
            Kingmaker.ElementsSystem.UnitEvaluator result = null;
            if (companion == Companions.None)
            {
                return result;
            }
            if (companion == Companions.Player)
            {
                result = (Kingmaker.Designers.EventConditionActionSystem.Evaluators.PlayerCharacter)
                          Kingmaker.ElementsSystem.Element.CreateInstance(typeof
                            (Kingmaker.Designers.EventConditionActionSystem.Evaluators.PlayerCharacter));
            }
            else
            {
                var companioneval = (Kingmaker.Designers.EventConditionActionSystem.Evaluators.CompanionInParty)
                                     Kingmaker.ElementsSystem.Element.CreateInstance(typeof
                                        (Kingmaker.Designers.EventConditionActionSystem.Evaluators.CompanionInParty));
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