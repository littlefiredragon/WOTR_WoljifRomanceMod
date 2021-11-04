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
            obj.transform.SetParent(roottransform);
            view.UniqueId = guid;
            view.UpdateCachedRenderersAndColliders();
            var data = view.CreateEntityData(true);
            Game.Instance.LoadedAreaState.AddEntityData(data);
            data.AttachView(view);
            Kingmaker.Blueprints.EntityReference dataref = (Kingmaker.Blueprints.EntityReference)view.Data;
            dataref.FindData();

            return dataref;
        }
    }
}