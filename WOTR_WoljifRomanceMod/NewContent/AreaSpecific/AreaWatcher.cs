/*using HarmonyLib;
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
            if (Game.Instance.CurrentlyLoadedArea.AssetGuidThreadSafe.Equals("2570015799edf594daf2f076f2f975d8", StringComparison.OrdinalIgnoreCase))
            { 
                AreaSpecificContent.createTavernCutscene();
            }
        }
        public void OnAreaBeginUnloading()
        {

        }
    }

    class AreaSpecificContent
    {
        static public void createTavernCutscene()
        {
            foreach (Kingmaker.EntitySystem.EntityDataBase thing in Game.Instance.LoadedAreaState.AllEntityData)
            {
                if (thing.UniqueId.Equals("0f725582-3a78-4eff-9a7c-477018aadeec", StringComparison.OrdinalIgnoreCase))
                {   // Steal the root transform from PC_Loc locator.
                    var roottransform = thing.View.gameObject.transform.parent;
                    // Create the Locators whenever the scene is loaded, since they go away on unload.
                    /*var Tavern_PlayerLoc = LocatorTools.MakeLocator("Tavern_PlayerLoc", roottransform, -46.40f, 49.005f, -150.35f, 277.30f);
                    var Tavern_WoljifLoc = LocatorTools.MakeLocator("Tavern_WoljifLoc", roottransform, -48.22f, 49.005f, -150.33f, 99.20f);
                    var Tavern_CameraLoc = LocatorTools.MakeLocator("Tavern_CameraLoc", roottransform, -46.88f, 49.19f, -150.19f, -34.06f);

                    var Woljif_Exit = LocatorTools.MakeLocator("WoljifDefault", roottransform, -8.844f, 56.02f, 0.325f, 275.0469f);*/

                    /*if (null == Resources.GetModBlueprint<Kingmaker.AreaLogic.Cutscenes.Cutscene>("WRM_TavernCutscene"))
                    { // Only create the rest of the stuff once.
                        //WRM_Act3.CreateTavernCutscene(Tavern_PlayerLoc, Tavern_WoljifLoc, Tavern_CameraLoc, Woljif_Exit);
                    }*/
                    //WRM_Act3.AddLocatorsTavernCutscene();

                    //Kingmaker.Blueprints.EntityReferenceTracker.Register(WoljifRomanceMod.LocatorReferences[0]);
                    //Kingmaker.Blueprints.EntityReferenceTracker.Register(WoljifRomanceMod.LocatorReferences[1]);
                    //Kingmaker.Blueprints.EntityReferenceTracker.Register(WoljifRomanceMod.LocatorReferences[2]);
                    //Kingmaker.Blueprints.EntityReferenceTracker.Register(WoljifRomanceMod.LocatorReferences[3]);
                    /*break;
                }
            }
        }

        /*static public void createWarcampTestCutscene()
        {
            foreach (Kingmaker.EntitySystem.EntityDataBase thing in Game.Instance.LoadedAreaState.AllEntityData)
            {
                if (thing.UniqueId.Equals("87998f84-bbf9-43ae-8eeb-69b9dd40e736", StringComparison.OrdinalIgnoreCase))
                { // Steal the root transform from the Seelah's Horse locator.
                    var roottransform = thing.View.gameObject.transform.parent;
                    // Create the Locators whenever the scene is loaded, since they go away on unload.
                    var dataref1 = LocatorTools.MakeLocator("testlocator1", roottransform, 15.2f, 39.9f, 35.2f, 269.5f);
                    var dataref2 = LocatorTools.MakeLocator("testlocator2", roottransform, 14.11f, 39.9f, 34.0f, 35.8f);
                    var dataref3 = LocatorTools.MakeLocator("testlocator3", roottransform, 11.17f, 40.0f, 37.0f, 313.4f);

                    if (null == Resources.GetModBlueprint<Kingmaker.AreaLogic.Cutscenes.Cutscene>("testcomplexcutscene"))
                    { // Only create the rest of the stuff once.
                    debugmenu.createAlternateCutscene(dataref1, dataref2, dataref3);
                    }
                    break;
                }
            }
        }*/
    //}
//}