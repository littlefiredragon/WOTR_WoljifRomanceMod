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

namespace WOTR_WoljifRomanceMod
{
    public enum Companions
    {
        None = -1, Player, Arueshalae, Camellia, Daeran, Ember, Greybor, Lann, Nenio, Regill, Seelah, Sosiel, Wenduag, Woljif
    }

    class AreaWatcher : Kingmaker.PubSubSystem.IAreaHandler
    {
        public void OnAreaDidLoad()
        {
        }
        public void OnAreaBeginUnloading()
        {
            if (Game.Instance.CurrentlyLoadedArea.AssetGuidThreadSafe.Equals("2570015799edf594daf2f076f2f975d8", StringComparison.OrdinalIgnoreCase))
            {
                // Do stuff on leaving Drezen
                Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_BedroomBarksFlag").Lock();
                var EtudeBP = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_BedroomBarksEtude");
                var Etude = Game.Instance.Player.EtudesSystem.Etudes.GetFact((Kingmaker.Blueprints.Facts.BlueprintFact)EtudeBP);
                Etude.Deactivate();
            }
        }
    }
    class AreaPartWatcher : Kingmaker.PubSubSystem.IAreaPartHandler
    {
        public void OnAreaPartChanged(BlueprintAreaPart previous)
        {
            if (previous.AssetGuidThreadSafe.Equals("2570015799edf594daf2f076f2f975d8", StringComparison.OrdinalIgnoreCase))
            {
                // Do stuff on leaving Citadel
                Resources.GetModBlueprint<Kingmaker.Blueprints.BlueprintUnlockableFlag>("WRM_BedroomBarksFlag").Lock();
                var EtudeBP = Resources.GetModBlueprint<Kingmaker.AreaLogic.Etudes.BlueprintEtude>("WRM_BedroomBarksEtude");
                var Etude = Game.Instance.Player.EtudesSystem.Etudes.GetFact((Kingmaker.Blueprints.Facts.BlueprintFact)EtudeBP);
                Etude.Deactivate();
            }
        }
    }


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
                if (Clock.Value.isActive())
                {
                    Clock.Value.incrementTimer();
                }
            }
        }
    }


    public class ControlWeather : Kingmaker.ElementsSystem.GameAction
    {
        public Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType inclemency;
        public bool start;
        public Kingmaker.Controllers.WeatherController control;
        public override string GetCaption()
        {
            return "Overrides weather conditions.";
        }

        public override void RunAction()
        {
            control = Kingmaker.Controllers.WeatherController.Instance;
            if (start)
            {
                control.StartOverrideInclemency(inclemency);
            }
            else
            {
                control.StopOverrideInclemency();
            }
        }
    }

    // Oh my god I can't believe this actually works. Stupid problems require stupid solutions.
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

    // Here be dragons; completely untested.
    public class SpawnUnit : Kingmaker.ElementsSystem.GameAction
    {
        public Kingmaker.Blueprints.BlueprintUnit unit;
        public FakeLocator location;
        public Kingmaker.EntitySystem.Entities.UnitEntityData entity;
        public override string GetCaption()
        {
            return "UNTESTED: Spawn a unit.";
        }

        public override void RunAction()
        {
            entity = Game.Instance.EntityCreator.SpawnUnit(unit, location.GetValue(), UnityEngine.Quaternion.identity, null);
        }

        public Kingmaker.EntitySystem.Entities.UnitEntityData GetEntity()
        {
            return entity;
        }
    }



    public class GenericUnitEvaluator : Kingmaker.ElementsSystem.UnitEvaluator
    {
        public UnitEntityData entity;
        public override string GetCaption()
        {
            return "Generic Unit Evaluator";
        }

        public override UnitEntityData GetValueInternal()
        {
            return entity;
        }

        public void setEntity(UnitEntityData data)
        {
            entity = data;
        }
    }


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
                result = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.BlueprintUnitReference>(Resources.GetBlueprint<Kingmaker.Blueprints.BlueprintUnit>(companionid));
            }
            return result;
        }
        public static Kingmaker.ElementsSystem.UnitEvaluator GetCompanionEvaluator(Companions companion, Kingmaker.Blueprints.SimpleBlueprint owner)
        {
            Kingmaker.ElementsSystem.UnitEvaluator result = null;
            if (companion == Companions.None)
            {
                return result;
            }
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