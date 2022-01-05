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

namespace WOTR_WoljifRomanceMod
{ 
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
}