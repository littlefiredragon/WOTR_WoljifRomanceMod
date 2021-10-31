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

                        var obj = new UnityEngine.GameObject("newlocator", typeof(UnityEngine.Transform));
                        var view = obj.AddComponent<Kingmaker.View.LocatorView>();
                        obj.transform.position = new UnityEngine.Vector3(-22.8f, 39.9f, 1.0f);
                        obj.transform.SetParent(roottransform);

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