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
                AreaSpecificContent.createWarcampTestCutscene();
            }
        }
        public void OnAreaBeginUnloading()
        {

        }
    }

    class AreaSpecificContent
    {
        static public void createWarcampTestCutscene()
        {
            foreach (Kingmaker.EntitySystem.EntityDataBase thing in Game.Instance.LoadedAreaState.AllEntityData)
            {
                if (thing.UniqueId.Equals("87998f84-bbf9-43ae-8eeb-69b9dd40e736", StringComparison.OrdinalIgnoreCase))
                { // Steal the root transform from the Seelah's Horse locator.
                    var roottransform = thing.View.gameObject.transform.parent;
                    // Create the Locators whenever the scene is loaded, since they go away on unload.
                    var dataref1 = CutsceneTools.MakeLocator("testlocator1", "30724c7a-18cf-41a2-a756-384a31b8dc1b", roottransform, 15.2f, 39.9f, 35.2f, 269.5f);
                    var dataref2 = CutsceneTools.MakeLocator("testlocator2", "1eae72b4-6c5d-4254-80c5-a12acc5d0d1d", roottransform, 14.11f, 39.9f, 34.0f, 35.8f);

                if (null == Resources.GetModBlueprint<Kingmaker.AreaLogic.Cutscenes.Cutscene>("testcomplexcutscene"))
                { // Only create the rest of the stuff once.
                    debugmenu.createComplexCutscene(dataref1, dataref2);
                }
                    break;
                }
            }
        }
    }
}