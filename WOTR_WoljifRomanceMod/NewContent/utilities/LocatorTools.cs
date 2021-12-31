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

    /*public class Indestructable : UnityEngine.MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    public class IndestructableLocatorView : Kingmaker.View.LocatorView
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public class LocatorGUIDs : IUpdatableSettings
    {
        [JsonProperty]
        private readonly SortedDictionary<string, string> LocatorIDstrings = new SortedDictionary<string, string>();
        public void OverrideSettings(IUpdatableSettings userSettings)
        {
            var loadedSettings = userSettings as LocatorGUIDs;
            if (loadedSettings == null) { return; }

            loadedSettings.LocatorIDstrings.ForEach(entry => {
                if (!(LocatorIDstrings.ContainsKey(entry.Key)))
                {
                    LocatorIDstrings[entry.Key] = entry.Value;
                }
            });
        }
        public string get(string key)
        {
            string result = null;
            try
            {
                result = LocatorIDstrings[key];
            }
            catch (KeyNotFoundException)
            {
                result = null;
            }
            return result;
        }
    }

    public class LocatorTools
    {
        public static LocatorGUIDs LocatorIDs;
        public static UnityEngine.GameObject LocatorRoot;

        public static void CreateLocatorRoot()
        {
            LocatorRoot = new UnityEngine.GameObject("WRM_Locators");
            LocatorRoot.AddComponent(typeof(Indestructable));
        }

        public static Kingmaker.Blueprints.EntityReference MakeLocator(string name, UnityEngine.Transform roottransform, float posx, float posy, float posz, float rotate = 0)
        {
            string guid = LocatorIDs.get(name);
            if (guid == null)
            {
                guid = Guid.NewGuid().ToString();
            }
            return MakeLocator(name, guid, roottransform, posx, posy, posz, rotate);
        }
        public static Kingmaker.Blueprints.EntityReference MakeLocator(string name, string guid, UnityEngine.Transform roottransform, float posx, float posy, float posz, float rotate = 0)
        {
            var obj = new UnityEngine.GameObject(name, typeof(UnityEngine.Transform));
            var view = obj.AddComponent<Kingmaker.View.LocatorView>();
            obj.transform.position = new UnityEngine.Vector3(posx, posy, posz);
            view.transform.Rotate(0.0f, rotate, 0.0f);
            //obj.transform.SetParent(roottransform);
            view.UniqueId = guid;
            view.UpdateCachedRenderersAndColliders();
            var data = view.CreateEntityData(true);
            //Game.Instance.LoadedAreaState.AddEntityData(data);
            data.AttachView(view);
            Kingmaker.Blueprints.EntityReference dataref = (Kingmaker.Blueprints.EntityReference)view.Data;
            dataref.FindData();

            return dataref;
        }

        public static Kingmaker.Blueprints.EntityReference PlaceholderLocator(string name, float posx, float posy, float posz, float rotate = 0)
        {
            string guid = LocatorIDs.get(name);
            if (guid == null)
            {
                guid = Guid.NewGuid().ToString();
            }

            var obj = new UnityEngine.GameObject(name, typeof(UnityEngine.Transform));
            //obj.SetActive(true);
            var view = obj.AddComponent<Kingmaker.View.LocatorView>();
            obj.AddComponent(typeof(Indestructable));
            obj.transform.position = new UnityEngine.Vector3(posx, posy, posz);
            view.transform.Rotate(0.0f, rotate, 0.0f);
            obj.transform.parent = LocatorRoot.transform;
            view.UniqueId = guid;
            view.UpdateCachedRenderersAndColliders();
            var data = view.CreateEntityData(true);
            data.AttachView(view);
            Kingmaker.Blueprints.EntityReference dataref = (Kingmaker.Blueprints.EntityReference)view.Data;
            //dataref.FindData();

            //UnityEngine.Object.DontDestroyOnLoad(obj);
            //UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj, UnityEngine.SceneManagement.SceneManager.GetSceneByName("DontDestroyOnLoad"));

            //var len = WoljifRomanceMod.LocatorReferences.Length;
            //Array.Resize(ref WoljifRomanceMod.LocatorReferences, len + 1);
            //WoljifRomanceMod.LocatorReferences[len] = dataref;

            return dataref;
        }
    }*/
}