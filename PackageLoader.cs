using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using UnityEngine;

namespace ExtraItems
{
    public class PackageLoader
    {
        public static PackageLoader Instance => _instance;
        private static PackageLoader _instance = new PackageLoader();

        public string PackagePath { get; private set; }
        public AssetBundle ExtraItemsBundle { get; private set; }
        

        public GameObject GetPrefab(string name)
        {
            if (!ExtraItemsBundle.Contains(name))
            {
                ExtraItemsPlugin.Logger.LogInfo($"Could not find prefab '{name}'");
                return null;
            }
            return ExtraItemsBundle.LoadAsset<GameObject>(name);
        }

        private PackageLoader()
        {
            PackagePath = BepInEx.Paths.PluginPath;
            LoadMainPackage();
        }
        private void LoadMainPackage()
        {
            ExtraItemsBundle = AssetBundle.LoadFromFile(Utility.CombinePaths(PackagePath, "extraitemsbundle"));
        }
    }
}
