using DRFV.CoinShop.Data;
using DRFV.Global;
using DRFV.Global.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.CoinShop
{
    public class CurrentShopItemComponent : MonoBehaviour
    {
        public Image icon;

        public Text name;

        public Text price;

        public Text info;

        public void Refresh(ShopItem shopItem)
        {
            icon.sprite = shopItem.icon;
            name.text = shopItem.name[Util.localizationId];
            price.text = "▼" + shopItem.price;
            info.text = shopItem.info[Util.localizationId];
        }
    }
}