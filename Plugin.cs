using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using ExtraItems.items;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zorro.Core;

namespace ExtraItems;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Content Warning.exe")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private Harmony _harmony;

    bool _isInitialized;

    public static DivingBell DivingBell { get; private set; }

    private void Awake()
    {
        _isInitialized = false;
        Logger = base.Logger;

        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        SceneManager.sceneLoaded += OnSceneLoaded;

        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "Toedtmanns.ExtraItems");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        DivingBell = FindObjectOfType<DivingBell>();

        // Make the camera purchasable
        foreach (var item in SingletonAsset<ItemDatabase>.Instance.Objects)
        {
            if (item.displayName == "Camera")
            {
                item.purchasable = true;
                item.Category = ShopItemCategory.Gadgets;
                break;
            }
        }

        // Register custom items
        OxygenTankItem.AddItemToGame();
        EnergyDrinkItem.AddItemToGame();
    }
}

[HarmonyPatch(typeof(SurfaceNetworkHandler), "InitSurface")]
static class FillSavePatch
{
    static void Postfix()
    {
        Plugin.Logger.LogInfo("Adding many money to new run");
        SurfaceNetworkHandler.RoomStats.AddMoney(1000000);
    }
}