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
    public static class CutsceneTools
    {
        // CUTSCENE
        public static Kingmaker.AreaLogic.Cutscenes.Cutscene CreateCutscene([NotNull] string name, bool sleepless, params Kingmaker.AreaLogic.Cutscenes.Track[] tracks)
        {
            return CreateCutscene(name, sleepless, DialogTools.EmptyActionList, tracks);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Cutscene CreateCutscene([NotNull] string name, bool sleepless, Kingmaker.ElementsSystem.ActionList onstop, params Kingmaker.AreaLogic.Cutscenes.Track[] tracks)
        {
            var numtracks = tracks?.Length;
            if (numtracks == null) numtracks = 0;
            var result = Helpers.CreateBlueprint<Kingmaker.AreaLogic.Cutscenes.Cutscene>(name, bp => {
                bp.Color = UnityEngine.Color.white;
                bp.m_Tracks = new List<Kingmaker.AreaLogic.Cutscenes.Track>();
                for (int i = 0; i < numtracks; i++)
                {
                    bp.m_Tracks.Add(tracks[i]);
                }
                bp.m_ActivationMode = Kingmaker.AreaLogic.Cutscenes.Gate.ActivationModeType.AllTracks;
                bp.PauseForOneFrame = false;

                bp.NonSkippable = false;
                bp.ForbidDialogs = false;
                bp.ForbidRandomIdles = true;
                bp.IsBackground = false;
                bp.Sleepless = sleepless;
                if (sleepless) bp.AwakeRange = 23.4F;
                bp.OnStopped = onstop;
                bp.DefaultParameters = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter();
                bp.DefaultParameters.Parameters = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterEntry[0];
            });

            //            var result = (Kingmaker.AreaLogic.Cutscenes.Cutscene)CreateGate(name, tracks);
            /*result.NonSkippable = false;
            result.ForbidDialogs = false;
            result.ForbidRandomIdles = true;
            result.IsBackground = false;
            result.Sleepless = sleepless;
            if (sleepless) result.AwakeRange = 23.4F;
            result.OnStopped = onstop;
            result.DefaultParameters = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter();
            result.DefaultParameters.Parameters = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter.ParameterEntry[0];*/
            return result;
        }

        // GATES
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate([NotNull] string name)
        {
            Kingmaker.AreaLogic.Cutscenes.Track[] tracks = null;
            return CreateGate(name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType.DoNotSignalGate, Kingmaker.ElementsSystem.Operation.And, tracks);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate([NotNull] string name, Kingmaker.AreaLogic.Cutscenes.Track track)
        {
            return CreateGate(name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType.DoNotSignalGate, Kingmaker.ElementsSystem.Operation.And, track);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate([NotNull] string name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType skipmodetype, Kingmaker.AreaLogic.Cutscenes.Track track)
        {
            return CreateGate(name, skipmodetype, Kingmaker.ElementsSystem.Operation.And, track);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate([NotNull] string name, Kingmaker.ElementsSystem.Operation op, Kingmaker.AreaLogic.Cutscenes.Track track)
        {
            return CreateGate(name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType.DoNotSignalGate, op, track);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate([NotNull] string name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType skipmodetype, Kingmaker.ElementsSystem.Operation op, Kingmaker.AreaLogic.Cutscenes.Track track)
        {
            Kingmaker.AreaLogic.Cutscenes.Track[] tracks = { track };
            return CreateGate(name, skipmodetype, op, tracks);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate([NotNull] string name, params Kingmaker.AreaLogic.Cutscenes.Track[] tracks)
        {
            return CreateGate(name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType.DoNotSignalGate, Kingmaker.ElementsSystem.Operation.And, tracks);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate([NotNull] string name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType skipmodetype, params Kingmaker.AreaLogic.Cutscenes.Track[] tracks)
        {
            return CreateGate(name, skipmodetype, Kingmaker.ElementsSystem.Operation.And, tracks);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate([NotNull] string name, Kingmaker.ElementsSystem.Operation op, params Kingmaker.AreaLogic.Cutscenes.Track[] tracks)
        {
            return CreateGate(name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType.DoNotSignalGate, op, tracks);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate([NotNull] string name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType skipmodetype, Kingmaker.ElementsSystem.Operation op, params Kingmaker.AreaLogic.Cutscenes.Track[] tracks)
        {
            var numtracks = tracks?.Length;
            if (numtracks == null) numtracks = 0;
            var result = Helpers.CreateBlueprint<Kingmaker.AreaLogic.Cutscenes.Gate>(name, bp => {
                bp.Color = UnityEngine.Color.white;
                bp.m_Tracks = new List<Kingmaker.AreaLogic.Cutscenes.Track>();
                for (int i = 0; i<numtracks; i++)
                {
                    bp.m_Tracks.Add(tracks[i]);
                }
                bp.m_Op = op;
                bp.m_ActivationMode = Kingmaker.AreaLogic.Cutscenes.Gate.ActivationModeType.AllTracks;
                bp.WhenTrackIsSkipped = skipmodetype;
                bp.PauseForOneFrame = false;
            });
            return result;
        }

        // TRACKS
        public static Kingmaker.AreaLogic.Cutscenes.Track CreateTrack(Kingmaker.AreaLogic.Cutscenes.Gate endgate, Kingmaker.AreaLogic.Cutscenes.CommandBase command)
        {
            var refs = new Kingmaker.ElementsSystem.CommandReference[1];
            refs[0] = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.ElementsSystem.CommandReference>(command);
            return CreateTrack(endgate, refs);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Track CreateTrack(Kingmaker.AreaLogic.Cutscenes.Gate endgate, params Kingmaker.AreaLogic.Cutscenes.CommandBase[] commands)
        {
            var numcommands = 0;
            if (commands != null)
            {
                numcommands = commands.Length;
            }
            var refs = new Kingmaker.ElementsSystem.CommandReference[numcommands];
            if (numcommands > 0)
            {
                for (int i = 0; i < numcommands; i++)
                {
                    refs[i] = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.ElementsSystem.CommandReference>(commands[i]);
                }
            }
            return CreateTrack(endgate, refs);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Track CreateTrack(Kingmaker.AreaLogic.Cutscenes.Gate endgate, params Kingmaker.ElementsSystem.CommandReference[] commandrefs)
        {
            var numcommands = 0;
            if (commandrefs != null)
            {
                numcommands = commandrefs.Length;
            }
            var result = new Kingmaker.AreaLogic.Cutscenes.Track();
            result.m_EndGate = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.GateReference>(endgate);
            for (int i = 0; i < numcommands; i++)
            {
                result.m_Commands.Add(commandrefs[i]);
            }
            return result;
        }

    }
}