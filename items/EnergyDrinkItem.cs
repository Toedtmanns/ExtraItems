using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

namespace ExtraItems.items
{
    public class EnergyDrinkItem : CustomItem<EnergyDrinkItem>
    {
        public override string ItemName => "Energy Drink";
        public override int Price => 30;
        public override ShopItemCategory ShopCategory => ShopItemCategory.Medical;
        //public override GameObject BaseObject => ;
        //private GameObject _baseObject
        //{

        //}

        public const float DrinkDuration = 6.0f;
        public const float DrinkEffectiveness = 2.0f;

        private SingleUseItemEntry _itemState;
        private bool _isActive;
        private float _durationLeft;

        protected override void ConfigCustomItem(ItemInstanceData data, PhotonView playerView)
        {
            if (data.TryGetEntry<SingleUseItemEntry>(out _itemState))
            {
                Plugin.Logger.LogInfo($"Single use entry found with state: {_itemState.GetString()}");
            }
            else
            {
                _itemState = new SingleUseItemEntry { wasUsed = false };
                data.AddDataEntry(_itemState);
            }
            _isActive = false;
            _durationLeft = DrinkDuration;
        }

        private void Update()
        {
            if (_isActive)
            {
                _durationLeft -= Time.deltaTime;
                if (_durationLeft <= 0.0f)
                {
                    _isActive = false;
                    return;
                }
                Player.localPlayer.data.staminaDepleated = false;
                Player.localPlayer.data.currentStamina = Mathf.MoveTowards(Player.localPlayer.data.currentStamina, Player.localPlayer.refs.controller.maxStamina, DrinkEffectiveness * Time.deltaTime);

                return;
            }

            if (!_itemState.wasUsed && isHeldByMe && Player.localPlayer.input.clickWasPressed && !Player.localPlayer.HasLockedInput())
            {
                _isActive = true;
                _itemState.wasUsed = true;
                Plugin.Logger.LogInfo($"Player '{Player.localPlayer.name}' drank an energy drink with {Player.localPlayer.data.currentStamina} stamina");
            }
        }
    }
}
