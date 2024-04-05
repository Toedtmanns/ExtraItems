using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;

namespace ExtraItems.items
{
    public class EnergyDrinkItem : CustomItem<EnergyDrinkItem>
    {
        public override string ItemName => "Energy Drink";
        public override int Price => 30;
        public override ShopItemCategory ShopCategory => ShopItemCategory.Medical;
        public override GameObject BaseObject
        {
            get
            {
                if (_baseObject == null)
                {
                    GameObject prefab = PackageLoader.Instance.GetPrefab("EnergyDrink");
                    _baseObject = CustomItemManager.Instance.SetupBaseObject(prefab);
                    //_baseObject = GameObject.Instantiate(prefab);
                    //GameObject.DontDestroyOnLoad(_baseObject);
                    //_baseObject.AddComponent<ItemInstance>();
                    //_baseObject.transform.localScale = Vector3.zero;
                    //_baseObject.transform.GetChild(0).gameObject.AddComponent<HandGizmo>();
                    //_baseObject.GetComponentInChildren<MeshRenderer>().material.shader = CustomItemManager.Instance.NiceShader;
                }
                return _baseObject;
            }
        }
        private GameObject _baseObject = null;

        public const float DrinkDuration = 8.0f;
        public const float DrinkEffectiveness = 2.0f;

        private SingleUseItemEntry _itemState;
        private StashAbleEntry _stashable;

        protected override void ConfigCustomItem(ItemInstanceData data, PhotonView playerView)
        {
            if (!data.TryGetEntry<StashAbleEntry>(out _stashable))
            {
                _stashable = new StashAbleEntry { isStashAble = true };
                data.AddDataEntry(_stashable);
            }

            if (data.TryGetEntry<SingleUseItemEntry>(out _itemState))
            {
                ExtraItemsPlugin.Logger.LogInfo($"Single use entry found with state: {_itemState.GetString()}");
            }
            else
            {
                _itemState = new SingleUseItemEntry { wasUsed = false };
                data.AddDataEntry(_itemState);
            }

            itemInstance.transform.localScale = Vector3.one;
        }

        private void Update()
        {
            if (isHeldByMe && !_itemState.wasUsed && Player.localPlayer.input.clickWasPressed && !Player.localPlayer.HasLockedInput())
            {
                Player.localPlayer.StartCoroutine(ReEnergize());
                _itemState.wasUsed = true;
                ExtraItemsPlugin.Logger.LogInfo($"Player '{Player.localPlayer.name}' drank an energy drink with {Player.localPlayer.data.currentStamina} stamina");
            }
        }

        private IEnumerator ReEnergize()
        {
            Player.localPlayer.data.staminaDepleated = false;
            float maxStam = Player.localPlayer.refs.controller.maxStamina;
            for (float remainingDuration = DrinkDuration; remainingDuration >= 0.0f; remainingDuration -= Time.deltaTime)
            {
                Player.localPlayer.data.currentStamina = Mathf.MoveTowards(Player.localPlayer.data.currentStamina, maxStam, DrinkEffectiveness * Time.deltaTime);
                yield return null;
            }
        }
    }
}
