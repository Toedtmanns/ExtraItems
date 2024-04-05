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
public class ExtraItemsPlugin : BaseUnityPlugin
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

        PackageLoader _ = PackageLoader.Instance;
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

#if DEBUG
[HarmonyPatch(typeof(SurfaceNetworkHandler), "InitSurface")]
static class FillSavePatch
{
    static void Postfix()
    {
        ExtraItemsPlugin.Logger.LogInfo("Adding money to new run");
        SurfaceNetworkHandler.RoomStats.AddMoney(1000000);
    }
}
#endif

[HarmonyPatch(typeof(ItemInstanceData), "GetEntryIdentifier")]
static class EntryIdentifierPatch
{
    static Exception Finalizer(Exception __exception, ref byte __result, object[] __args)
    {
        Type arg = (Type) __args[0];
        if (arg == typeof(SingleUseItemEntry))
        {
            __result = 9;
            return null;
        }
        return __exception;
    }
}

[HarmonyPatch(typeof(ItemInstanceData), "GetEntryType")]
static class EntryTypePatch
{
    static Exception Finalizer(Exception __exception, ref ItemDataEntry __result, object[] __args)
    {
        byte arg = (byte) __args[0];
        if (arg == 9)
        {
            __result = new SingleUseItemEntry();
            return null;
        }
        return __exception;
    }
}