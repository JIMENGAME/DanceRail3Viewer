using DRFV.CoinShop.Data;
using DRFV.Global;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.CoinShop
{
    public class ShopItemComponent : MonoBehaviour
    {
        public Image icon;

        public Text name;

        public Text price;
        
        private int _id;

        private TheCoinShopManager _theCoinShopManager;

        public void Init(TheCoinShopManager theCoinShopManager, ShopItem shopItem)
        {
            _theCoinShopManager = theCoinShopManager;
            _id = shopItem.id;
            icon.sprite = shopItem.icon;
            name.text = shopItem.name[Util.localizationId];
            price.text = "▼" + shopItem.price;
        }

        public void OnPressed()
        {
            _theCoinShopManager.UpdatePressedItem(_id);
        }
    }
}
