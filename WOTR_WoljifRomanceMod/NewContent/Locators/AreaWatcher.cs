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
                        // Test locator: stolen from seelah's horse.
                        /*var horselocator = (Kingmaker.View.LocatorView)thing.View;
                        Kingmaker.Blueprints.EntityReference horseref = (Kingmaker.Blueprints.EntityReference) horselocator.Data;
                        horseref.FindData();*/
                        var horseholdingstate = thing.View.Data.HoldingState;

                        //obj.transform.position = new UnityEngine.Vector3(-22.8f, 39.9f, 1.0f);
                        var obj = new UnityEngine.GameObject("testlocator1", typeof(UnityEngine.Transform));
                        var view = obj.AddComponent<Kingmaker.View.LocatorView>();
                        //obj.transform.position = new UnityEngine.Vector3(10.9f, 41.2f, 29.7f);
                        obj.transform.position = new UnityEngine.Vector3(15.2f, 39.9f, 35.2f);
                        //obj.transform.rotation = new UnityEngine.Quaternion(0.0f, 176.0f, 0.0f, 0.0f);
                        obj.transform.rotation.Set(0.0f, 176.0f, 0.0f, 0.0f);
                        //view.transform.rotation.eulerAngles.Set(0.0f, 222.5f, 0.0f);
                        obj.transform.SetParent(roottransform);
                        view.UniqueId = Guid.NewGuid().ToString();
                        view.UpdateCachedRenderersAndColliders();
                        var data = view.CreateEntityData(true);
                        //data.HoldingState = horseholdingstate;
                        Game.Instance.LoadedAreaState.AddEntityData(data);
                        data.AttachView(view);

                        Kingmaker.Blueprints.EntityReference dataref1 = (Kingmaker.Blueprints.EntityReference) view.Data;
                        dataref1.FindData();

                        var obj2 = new UnityEngine.GameObject("testlocator2", typeof(UnityEngine.Transform));
                        var view2 = obj2.AddComponent<Kingmaker.View.LocatorView>();
                        //obj2.transform.position = new UnityEngine.Vector3(8.2f, 41.2f, 28.7f);
                        obj2.transform.position = new UnityEngine.Vector3(14.11f, 39.9f, 34.0f);
                        //obj2.transform.rotation = new UnityEngine.Quaternion(0.0f, 138.67f, 0.0f, 0.0f);
                        view2.transform.rotation.eulerAngles.Set(0.0f, 138.67f, 0.0f);
                        obj2.transform.SetParent(roottransform);
                        view2.UniqueId = Guid.NewGuid().ToString();
                        view2.UpdateCachedRenderersAndColliders();
                        var data2 = view2.CreateEntityData(true);
                        Game.Instance.LoadedAreaState.AddEntityData(data2);
                        data2.AttachView(view2);
                        Kingmaker.Blueprints.EntityReference dataref2 = (Kingmaker.Blueprints.EntityReference) view2.Data;
                        dataref2.FindData();


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