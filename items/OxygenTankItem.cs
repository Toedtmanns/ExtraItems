using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zorro.Core;

namespace ExtraItems.items
{
    public class OxygenTankItem : CustomItem<OxygenTankItem>
    {
        private StashAbleEntry _stashableEntry;
        //private OnOffEntry _onOffEntry;
        private SingleUseItemEntry _itemState;

        public override string ItemName => "Oxygen Tank";
        public override int Price => 50;
        public override ShopItemCategory ShopCategory => ShopItemCategory.Medical;

        protected override void ConfigCustomItem(ItemInstanceData data, Photon.Pun.PhotonView playerView)
        {
            if (!data.TryGetEntry<StashAbleEntry>(out _stashableEntry))
            {
                _stashableEntry = new StashAbleEntry { isStashAble = true };
                data.AddDataEntry(_stashableEntry);
            }

            if (data.TryGetEntry<SingleUseItemEntry>(out _itemState))
            {
                Plugin.Logger.LogInfo($"Single use entry found with state: {_itemState.GetString()}");
            }
            else
            {
                _itemState = new SingleUseItemEntry { wasUsed = false };
                data.AddDataEntry(_itemState);
            }
        }

        private void Update()
        {
            if (!_itemState.wasUsed && isHeldByMe && Player.localPlayer.input.clickWasPressed && !Player.localPlayer.HasLockedInput() && !Plugin.DivingBell.onSurface)
            {
                Plugin.Logger.LogInfo($"Player '{Player.localPlayer.name}' refreshed their oxygen with a tank");
                Player.localPlayer.data.remainingOxygen = Player.localPlayer.data.maxOxygen;
                _itemState.wasUsed = true;
            }
        }
    }
}
