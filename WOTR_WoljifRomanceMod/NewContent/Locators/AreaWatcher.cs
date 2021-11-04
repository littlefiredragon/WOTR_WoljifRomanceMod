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
using Kingmaker.PubSubSystem;

namespace WOTR_WoljifRomanceMod
{
    class AreaWatcher : IAreaHandler
    {
        public void OnAreaDidLoad()
        {
            if (Game.Instance.CurrentlyLoadedArea.AssetGuidThreadSafe.Equals("7a25c101fe6f7aa46b192db13373d03b", StringComparison.OrdinalIgnoreCase))
            {
                foreach (Kingmaker.EntitySystem.EntityDataBase thing in Game.Instance.LoadedAreaState.AllEntityData)
                {
                    if (thing.UniqueId.Equals("87998f84-bbf9-43ae-8eeb-69b9dd40e736", StringComparison.OrdinalIgnoreCase))
                    {
                        var roottransform = thing.View.gameObject.transform.parent;

                        var dataref1 = CutsceneTools.MakeLocator("testlocator1", roottransform, 15.2f, 39.9f, 35.2f, 269.5f);
                        var dataref2 = CutsceneTools.MakeLocator("testlocator2", roottransform, 14.11f, 39.9f, 34.0f, 35.8f);

                        // TEST CUTSCENE CREATION
                        var debuganswerlist = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintAnswersList>("TEST_L_debugmenu");
                        var newdialog = Resources.GetModBlueprint<Kingmaker.DialogSystem.Blueprints.BlueprintDialog>("brandnewdialog");
                        var cutsceneanswer2 = DialogTools.CreateAnswer("TEST_a_cutscene2");
                        DialogTools.ListAddAnswer(debuganswerlist, cutsceneanswer2, 9);
                        var cutscenecue2 = DialogTools.CreateCue("TEST_cw_cutscene2");
                        DialogTools.AnswerAddNextCue(cutsceneanswer2, cutscenecue2);

                        //cutscene
                        //  trackA
                        //    Command: Lockcontrol
                        //    Endgate: DialogGate
                        //        DialogGateTrack
                        //          Command: start dialog
                        //          Endgate: null
                        //  trackB
                        //    Command: move1, move2
                        //    Endgate: cameragate
                        //      CameraGateTrack
                        //        Command: camerafollow
                        //        Endgate: dialoggate

                        // Create track A
                        var lockcommand = CommandTools.LockControlCommand();
                        var startdialogcommand = CommandTools.StartDialogCommand(newdialog, Companions.Woljif);
                        var dialogGateTrack = CutsceneTools.CreateTrack(null, startdialogcommand);
                        var dialogGate = CutsceneTools.CreateGate("dialoggatecomp", dialogGateTrack);
                        var TrackA = CutsceneTools.CreateTrack(dialogGate, lockcommand);
                        // Create Track B
                        var cameracommand = CommandTools.CamFollowCommand(Companions.Woljif);
                        var cameraGateTrack = CutsceneTools.CreateTrack(dialogGate, cameracommand);
                        var cameragate = CutsceneTools.CreateGate("cameragate", cameraGateTrack);
                        // Track B commands
                        var unhideWoljifAction = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.HideUnit>("unhidewoljif", bp =>
                        {
                            bp.Target = CommandTools.getCompanionEvaluator(Companions.Woljif);
                            bp.Unhide = true;
                        });
                        var moveWoljifAction = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit>("movewoljif", bp =>
                        {
                            bp.Unit = CommandTools.getCompanionEvaluator(Companions.Woljif);
                            bp.translocatePosition = dataref1;
                            bp.m_CopyRotation = true;
                        });
                        Kingmaker.ElementsSystem.GameAction[] actionlist1 = { unhideWoljifAction, moveWoljifAction };
                        var moveWoljifcommand = CommandTools.ActionCommand("movewoljifcommand", actionlist1);
                        moveWoljifAction.Owner = moveWoljifcommand;

                        var moveplayeraction = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.TranslocateUnit>("moveplayer", bp =>
                        {
                            bp.Unit = new Kingmaker.Designers.EventConditionActionSystem.Evaluators.PlayerCharacter();
                            bp.translocatePosition = dataref2;
                            bp.m_CopyRotation = true;
                        });
                        var moveplayercommand = CommandTools.ActionCommand("moveplayercommand", moveplayeraction);
                        moveplayeraction.Owner = moveplayercommand;
                        // Track B itself
                        Kingmaker.AreaLogic.Cutscenes.CommandBase[] trackbcommands = { moveWoljifcommand, moveplayercommand };
                        var TrackB = CutsceneTools.CreateTrack(cameragate, trackbcommands);

                        // make the cutscene
                        Kingmaker.AreaLogic.Cutscenes.Track[] cutscenetracks = { TrackA, TrackB };
                        var customcutscene = CutsceneTools.CreateCutscene("testcomplexcutscene", false, cutscenetracks);
                        var playcutsceneaction = ActionTools.GenericAction<Kingmaker.Designers.EventConditionActionSystem.Actions.PlayCutscene>("playcomplexcutscene", bp =>
                        {
                            bp.m_Cutscene = Kingmaker.Blueprints.BlueprintReferenceEx.ToReference<Kingmaker.Blueprints.CutsceneReference>(customcutscene);
                            bp.Owner = cutscenecue2;
                            bp.Parameters = new Kingmaker.Designers.EventConditionActionSystem.NamedParameters.ParametrizedContextSetter();
                        });
                        DialogTools.CueAddOnStopAction(cutscenecue2, playcutsceneaction);



                        break;
                    }
                }
            }
        }
        public void OnAreaBeginUnloading()
        {

        }
    }
}