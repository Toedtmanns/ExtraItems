using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtraItems.items;
using HarmonyLib;
using Sirenix.Utilities;
using UnityEngine;
using Zorro.Core;
using Zorro.Core.Serizalization;

namespace ExtraItems
{
    public class SingleUseItemEntry : ItemDataEntry, IHaveUIData
    {
        public bool wasUsed
        {
            get => _wasUsed;
            set
            {
                _wasUsed = value;
                SetDirty();
            }
        }
        private bool _wasUsed;

        public override void Deserialize(BinaryDeserializer binaryDeserializer)
        {
            _wasUsed = binaryDeserializer.ReadBool();
        }

        public override void Serialize(BinarySerializer binarySerializer)
        {
            binarySerializer.WriteBool(_wasUsed);
        }

        public string GetString()
        {
            return _wasUsed ? "Already used" : "Unused";
        }
    }

    public abstract class CustomItem<T> : ItemInstanceBehaviour where T : CustomItem<T>, new()
    {
        public abstract string ItemName { get; }
        public virtual int Price => 100;
        public virtual ShopItemCategory ShopCategory => ShopItemCategory.Gadgets;
        public virtual GameObject BaseObject => null;

        public sealed override void ConfigItem(ItemInstanceData data, Photon.Pun.PhotonView playerView)
        {
            ExtraItemsPlugin.Logger.LogInfo($"Configuring custom item '{itemInstance.item.displayName}'");
            //itemInstance.item.itemObject.SetActive(true);
            itemInstance.transform.localScale = Vector3.one;
            ConfigCustomItem(data, playerView);
        }

        //public Item AddItemToGame()
        //{
        //    if (BaseObject == null)
        //        return CustomItemManager.Instance.CreateNewItem<T>(ItemName, Price, ShopCategory);
        //    return CustomItemManager.Instance.CreateNewItem<T>(ItemName, BaseObject, Price, ShopCategory);
        //}
        public static Item AddItemToGame()
        {
            T item = new T();
            if (item.BaseObject == null)
                return CustomItemManager.Instance.CreateNewItem<T>(item.ItemName, item.Price, item.ShopCategory);
            return CustomItemManager.Instance.CreateNewItem<T>(item.ItemName, item.BaseObject, item.Price, item.ShopCategory);
        }

        protected abstract void ConfigCustomItem(ItemInstanceData data, Photon.Pun.PhotonView playerView);
    }

    public class CustomItemManager
    {
        public static CustomItemManager Instance => _instance;
        private static CustomItemManager _instance = new CustomItemManager();

        private int _newItemID;

        public Shader NiceShader { get; private set; }

        private CustomItemManager()
        {
            _newItemID = 0;
            foreach (var item in SingletonAsset<ItemDatabase>.Instance.Objects)
            {
                if (item.id > _newItemID)
                    _newItemID = item.id;
            }
            _newItemID++;
            NiceShader = Resources.FindObjectsOfTypeAll<Shader>().First((shader) => shader.name == "NiceShader");
        }

        public GameObject SetupBaseObject(GameObject @object)
        {
            GameObject baseObject = GameObject.Instantiate(@object);
            GameObject.DontDestroyOnLoad(baseObject);
            baseObject.AddComponent<ItemInstance>();
            baseObject.transform.localScale = Vector3.zero;
            baseObject.transform.GetChild(0).gameObject.AddComponent<HandGizmo>();
            foreach (var mat in baseObject.GetComponentInChildren<MeshRenderer>().materials)
            {
                mat.shader = NiceShader;
            }
            return baseObject;
        }

        public Item CreateNewItem<T>(string itemName, int price = 100, ShopItemCategory shopCategory = ShopItemCategory.Gadgets) where T : ItemInstanceBehaviour
        {
            return CreateNewItem(itemName, typeof(T), price, shopCategory);
        }
        public Item CreateNewItem(string itemName, Type instanceBehaviour, int price = 100, ShopItemCategory shopCategory = ShopItemCategory.Gadgets)
        {
            Item flareItem = SingletonAsset<ItemDatabase>.Instance.Objects.First((item) => item.id == 7);

            GameObject flareGO = GameObject.Instantiate(flareItem.itemObject);
            flareGO.name = $"_TEMPLATE_{itemName}";
            if (instanceBehaviour.InheritsFrom(typeof(CustomItem<>).GetGenericTypeDefinition()))
            {
                flareGO.transform.localScale = Vector3.zero;
            }
            GameObject.DontDestroyOnLoad(flareGO);

            GameObject.Destroy(flareGO.GetComponent<Flare>());
            GameObject.Destroy(flareGO.GetComponent<SFX_PlayOneShot>());

            Item ret = CreateNewItem(itemName, instanceBehaviour, flareGO, price, shopCategory);
            ret.alternativeHoldRot = new Vector3(-10.0f, -10.0f, 5.0f);
            ret.useAlternativeHoldingRot= true;

            if (ret == null)
                GameObject.Destroy(flareGO);
            return ret;
        }
        public Item CreateNewItem<T>(string itemName, GameObject baseItem, int price = 100, ShopItemCategory shopCategory = ShopItemCategory.Gadgets) where T : ItemInstanceBehaviour
        {
            return CreateNewItem(itemName, typeof(T), baseItem, price, shopCategory);
        }
        public Item CreateNewItem(string itemName, Type instanceBehaviour, GameObject baseItem, int price = 100, ShopItemCategory shopCategory = ShopItemCategory.Gadgets)
        {
            if (_newItemID > 255)
            {
                ExtraItemsPlugin.Logger.LogError("Cannot add another custom object. Object limit (256 Objects) reached");
            }
            else if (!instanceBehaviour.InheritsFrom<ItemInstanceBehaviour>())
            {
                ExtraItemsPlugin.Logger.LogError($"Custom item '{itemName}' behaviour class '{instanceBehaviour.Name}' doesn't inherit from ItemInstanceBehaviour!");
                return null;
            }

            Item customItem = ScriptableObject.CreateInstance<Item>();
            customItem.price = price;
            customItem.displayName = itemName;
            customItem.name = itemName;
            customItem.purchasable = true;
            customItem.budgetCost = price;
            customItem.itemObject = baseItem;
            customItem.itemType = Item.ItemType.Tool;
            customItem.Category = shopCategory;
            customItem.quantity = 0;
            customItem.id = (byte) _newItemID;
            _newItemID++;

            customItem.itemObject.AddComponent(instanceBehaviour);

            RegisterItem(customItem);
            return customItem;
        }

        public static void RegisterItem(Item item)
        {
            SingletonAsset<ItemDatabase>.Instance.Objects = SingletonAsset<ItemDatabase>.Instance.Objects.AddItem(item).ToArray();
            ExtraItemsPlugin.Logger.LogInfo($"Custom item '{item.displayName}' was registered with ID {item.id}");
        }
        public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
