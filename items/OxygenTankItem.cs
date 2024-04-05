using System;
using System.Collections;
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
        public override GameObject BaseObject
        {
            get
            {
                if (_baseObject == null)
                {
                    _baseObject = CustomItemManager.Instance.SetupBaseObject(PackageLoader.Instance.GetPrefab("OxygenTank"));
                }
                return _baseObject;
            }
        }
        private GameObject _baseObject;

        protected override void ConfigCustomItem(ItemInstanceData data, Photon.Pun.PhotonView playerView)
        {
            if (!data.TryGetEntry<StashAbleEntry>(out _stashableEntry))
            {
                _stashableEntry = new StashAbleEntry { isStashAble = true };
                data.AddDataEntry(_stashableEntry);
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
        }

        private void Update()
        {
            if (isHeldByMe && !_itemState.wasUsed && Player.localPlayer.input.clickWasPressed && !Player.localPlayer.HasLockedInput() && Player.localPlayer.data.usingOxygen)
            {
                ExtraItemsPlugin.Logger.LogInfo($"Player '{Player.localPlayer.name}' refreshed their oxygen with a tank");
                Player.localPlayer.StartCoroutine(ReOxidize());
                _itemState.wasUsed = true;
            }
        }

        private IEnumerator ReOxidize()
        {
            float maxOxygen = Player.localPlayer.data.maxOxygen;
            Player.localPlayer.data.usingOxygen = false;
            while (Player.localPlayer.data.remainingOxygen < maxOxygen)
            {
                Player.localPlayer.data.remainingOxygen = Mathf.MoveTowards(Player.localPlayer.data.remainingOxygen, maxOxygen, 50.0f * Time.deltaTime);
                yield return null;
            }
            Player.localPlayer.data.usingOxygen = true;
            ExtraItemsPlugin.Logger.LogInfo("Oxygen refilled");
        }
    }
}
