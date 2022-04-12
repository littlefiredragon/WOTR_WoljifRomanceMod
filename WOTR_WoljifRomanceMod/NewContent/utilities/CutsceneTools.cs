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
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

//##########################################################################################################################
// CUTSCENE TOOLS
// Helper functions to create Cutscenes. A cutscene consists of tracks and gates. Tracks contain commands, and converge at
// gates. Gates, in turn, can start tracks.
// Tracks run in parallel, but the actions within a track run sequentially. Once the actions in a track have finished, that
// track is also finished. Once all the tracks that converge to a gate finish, that gate triggers. If the gate has tracks
// of its own, they start when the gate triggers.
// Be aware that commands with no end mean the track can't proceed to the next command, nor can it finish, and so any gates
// that it converges to are also never reached. Indefinite commands like looping animations should have their own tracks
// that do not converge to gates.
//
// Here is an example flowchart of a cutscene. When it starts, Tracks 1, 2, and 3 start at the same time, which means that
// Commands A, C, and D all start at once, and immediately after Command A finishes, Command B starts. Once Command C and
// Command B have both finished, Gate 1 starts, which kicks off Track 4, which runs Command E. Command D runs until the
// cutscene ends, because it is an indefinite command, but this is not a problem because Track 3 has no gate, so nothing
// is waiting for Command D to end.
//    Cutscene +-> [Track 1] ---+-> [Gate 1] -> [Track4]
//             |     (CommandA) |                 (CommandE)
//             |     (CommandB) |
//             +-> [Track 2] ---+
//             |     (CommandC)
//             +-> [Track 3]-> NO GATE
//                   (CommandD, Indefinite)
//##########################################################################################################################

namespace WOTR_WoljifRomanceMod
{
    public static class CutsceneTools
    {
        //##################################################################################################################
        // CUTSCENES
        //##################################################################################################################

        /*******************************************************************************************************************
         * CREATE CUTSCENE
         * Builds a cutscene. The tracks should all be prepared beforehand, so this is actually the last step of making a
         * cutscene.
         *   name:      The cutscene's name.
         *   sleepless: The tooltip for sleeplessness says "If not set, cutscene is paused when all anchors are in fog of 
         *              war or away enough from party". If sleepless is true, it'll force the cutscene to play anyway.
         *   onstop:    Optional, an ActionList to run when the cutscene ends.
         *   tracks:    The tracks that should run immediately when the cutscene starts. Do not include tracks that trigger
         *              later via gates here; this is only the first round of tracks.
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.Cutscene CreateCutscene
                      ([NotNull] string name, bool sleepless, params Kingmaker.AreaLogic.Cutscenes.Track[] tracks)
        {
            return CreateCutscene(name, sleepless, DialogTools.EmptyActionList, tracks);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Cutscene CreateCutscene
                      ([NotNull] string name, bool sleepless, Kingmaker.ElementsSystem.ActionList onstop, 
                       params Kingmaker.AreaLogic.Cutscenes.Track[] tracks)
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
                bp.DefaultParameters = new ParametrizedContextSetter();
                bp.DefaultParameters.Parameters = new ParametrizedContextSetter.ParameterEntry[0];
            });
            return result;
        }

        //##################################################################################################################
        // GATES
        //##################################################################################################################

        /*******************************************************************************************************************
         * CREATE GATE
         * Builds a Gate. A gate is a convergence point - all tracks that point to the gate have to finish before the
         * the cutscene will proceed. The gate, once triggered by all pointing tracks finishing, will then start its own
         * tracks if it has any.
         *   name:          The name of the track
         *   skipmodetype:  Honestly, not too sure what this does. Defaults to DoNotSignalGate.
         *   op:            Legit I have no idea
         *   track/tracks:  A track or array of tracks to start once the gate triggers
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate
                      ([NotNull] string name)
        {
            Kingmaker.AreaLogic.Cutscenes.Track[] tracks = null;
            return CreateGate(name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType.DoNotSignalGate, 
                              Kingmaker.ElementsSystem.Operation.And, tracks);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate
                      ([NotNull] string name, Kingmaker.AreaLogic.Cutscenes.Track track)
        {
            return CreateGate(name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType.DoNotSignalGate, 
                              Kingmaker.ElementsSystem.Operation.And, track);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate
                      ([NotNull] string name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType skipmodetype, 
                       Kingmaker.AreaLogic.Cutscenes.Track track)
        {
            return CreateGate(name, skipmodetype, Kingmaker.ElementsSystem.Operation.And, track);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate
                      ([NotNull] string name, Kingmaker.ElementsSystem.Operation op, 
                       Kingmaker.AreaLogic.Cutscenes.Track track)
        {
            return CreateGate(name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType.DoNotSignalGate, op, track);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate
                      ([NotNull] string name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType skipmodetype, 
                       Kingmaker.ElementsSystem.Operation op, Kingmaker.AreaLogic.Cutscenes.Track track)
        {
            Kingmaker.AreaLogic.Cutscenes.Track[] tracks = { track };
            return CreateGate(name, skipmodetype, op, tracks);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate
                      ([NotNull] string name, params Kingmaker.AreaLogic.Cutscenes.Track[] tracks)
        {
            return CreateGate(name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType.DoNotSignalGate, 
                              Kingmaker.ElementsSystem.Operation.And, tracks);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate
                      ([NotNull] string name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType skipmodetype, 
                       params Kingmaker.AreaLogic.Cutscenes.Track[] tracks)
        {
            return CreateGate(name, skipmodetype, Kingmaker.ElementsSystem.Operation.And, tracks);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate
                      ([NotNull] string name, Kingmaker.ElementsSystem.Operation op, 
                       params Kingmaker.AreaLogic.Cutscenes.Track[] tracks)
        {
            return CreateGate(name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType.DoNotSignalGate, op, tracks);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Gate CreateGate
                      ([NotNull] string name, Kingmaker.AreaLogic.Cutscenes.Gate.SkipTracksModeType skipmodetype, 
                       Kingmaker.ElementsSystem.Operation op, params Kingmaker.AreaLogic.Cutscenes.Track[] tracks)
        {
            var numtracks = tracks?.Length;
            if (numtracks == null) numtracks = 0;
            var result = Helpers.CreateBlueprint<Kingmaker.AreaLogic.Cutscenes.Gate>(name, bp => 
                {
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

        //##################################################################################################################
        // TRACKS
        //##################################################################################################################

        /*******************************************************************************************************************
         * CREATE TRACK
         * Creates a chunk of a cutscene. Tracks run all their commands in sequence, but multiple tracks can run at the
         * same time. 
         *   gate:      The gate this track will converge to. The gate will only trigger after all tracks that converge at
         *              it finish. If nothing is waiting on this track to finish, you don't need a gate.
         *   commands:  The commands run by this track. Accepts single commands, arrays of commands, or arrays of command
         *              references.
         ******************************************************************************************************************/
        public static Kingmaker.AreaLogic.Cutscenes.Track CreateTrack
                      (Kingmaker.AreaLogic.Cutscenes.Gate endgate, Kingmaker.AreaLogic.Cutscenes.CommandBase command)
        {
            var refs = new Kingmaker.ElementsSystem.CommandReference[1];
            refs[0] = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                      <Kingmaker.ElementsSystem.CommandReference>(command);
            return CreateTrack(endgate, refs);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Track CreateTrack
                      (Kingmaker.AreaLogic.Cutscenes.Gate endgate, 
                       params Kingmaker.AreaLogic.Cutscenes.CommandBase[] commands)
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
                    refs[i] = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                              <Kingmaker.ElementsSystem.CommandReference>(commands[i]);
                }
            }
            return CreateTrack(endgate, refs);
        }
        public static Kingmaker.AreaLogic.Cutscenes.Track CreateTrack
                      (Kingmaker.AreaLogic.Cutscenes.Gate endgate, 
                       params Kingmaker.ElementsSystem.CommandReference[] commandrefs)
        {
            var numcommands = 0;
            if (commandrefs != null)
            {
                numcommands = commandrefs.Length;
            }
            var result = new Kingmaker.AreaLogic.Cutscenes.Track();
            result.m_EndGate = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                               <Kingmaker.Blueprints.GateReference>(endgate);
            for (int i = 0; i < numcommands; i++)
            {
                result.m_Commands.Add(commandrefs[i]);
            }
            return result;
        }

        /*******************************************************************************************************************
         * TRACK: ADD COMMAND
         * Adds additional commands to an existing track.
         *   track:     The track to add commands to.
         *   commands:  The commands to add to this track.
         ******************************************************************************************************************/
        public static void TrackAddCommands(Kingmaker.AreaLogic.Cutscenes.Track track, 
                                            params Kingmaker.AreaLogic.Cutscenes.CommandBase[] commands)
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
                    refs[i] = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference
                              <Kingmaker.ElementsSystem.CommandReference>(commands[i]);
                }
            }
            for (int i = 0; i < numcommands; i++)
            {
                track.m_Commands.Add(refs[i]);
            }
        }
    }
}