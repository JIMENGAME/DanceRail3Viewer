using System;
using System.Collections;
using System.Diagnostics;
using DRFV.CoinShop;
using DRFV.CoinShop.Data;
using DRFV.Global;
using DRFV.Global.Components;
using DRFV.Global.Managers;
using DRFV.inokana;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TheCoinShopManager : MonoBehaviour
{
    public GameObject ItemPrefab;

    public Transform ItemParent;

    public ShopItem currentShopItem;

    public CurrentShopItemComponent currentShopItemComp;

    public PlayerDataComponent PlayerDataComponent;

    // Start is called before the first frame update
    void Start()
    {
        Refresh();
    }

    void Refresh()
    {
        Transform[] children = ItemParent.GetComponentsInChildren<Transform>();
        foreach (var child in children)
        {
            if (child != ItemParent.transform) Destroy(child.gameObject);
        }
        foreach (ShopItem shopItem in StaticResources.Instance.shopItems)
        {
            if (shopItem.enabled)
            {
                GameObject itemInst = Instantiate(ItemPrefab, ItemParent);
                ShopItemComponent itemComp = itemInst.GetComponent<ShopItemComponent>();
                itemComp.Init(this, shopItem);
            }
        }

        int i = 0;
        if (currentShopItem is { enabled: true })
        {
            i = currentShopItem.id;
        }
        UpdatePressedItem(i);
        PlayerDataComponent.Refresh();
    }

    public void UpdatePressedItem(int id)
    {
        if (currentShopItem != null && id == currentShopItem.id) return;
        currentShopItem = StaticResources.Instance.shopItems[id];
        currentShopItemComp.Refresh(currentShopItem);
        isFirstPress = true;
    }
    
    private bool isFirstPress = true;
    private Coroutine _coroutine;

    private IEnumerator SecondPressListener()
    {
        yield return new WaitForSecondsRealtime(3f);
        NotificationBarManager.Instance.Show("已取消购买");
        isFirstPress = true;
    }

    public void Buy()
    {
                
        if (currentShopItem.price > PlayerData.Instance.coin)
        {
            NotificationBarManager.Instance.Show("你钱不够");
            return;
        }
        
        if (isFirstPress)
        {
            isFirstPress = false;
            NotificationBarManager.Instance.Show("确认购买？（3秒内再次按下购买以确认）");
            _coroutine = StartCoroutine(SecondPressListener());
            return;
        }

        if (_coroutine != null) StopCoroutine(_coroutine);
        isFirstPress = true;

        PlayerData.Instance.coin -= currentShopItem.price;
        if (currentShopItem.type == ShopItem.ShopItemType.SONG)
        {
            PlayerPrefs.SetInt("unlock_" + currentShopItem.keyword, 1);
            PlayerPrefs.Save();
        }
        else
        {
            switch (currentShopItem.keyword)
            {
                case "prime_card":
                    if (currentShopItem.itemValue != null) PlayerData.Instance.BuyPrime((int)currentShopItem.itemValue);
                    else
                    {
                        NotificationBarManager.Instance.Show("会员卡数值为null");
                        throw new NullReferenceException();
                    }

                    break;
                case "purple_card":
                    if (currentShopItem.itemValue != null) PlayerData.Instance.purpleCard += (int)currentShopItem.itemValue;
                    else
                    {
                        NotificationBarManager.Instance.Show("紫薯片数值为null");
                        throw new NullReferenceException();
                    }

                    break;
                case "change_nickname_card":
                    if (currentShopItem.itemValue != null) PlayerData.Instance.changeNickNameCard += (int)currentShopItem.itemValue;
                    else
                    {
                        NotificationBarManager.Instance.Show("紫薯片数值为null");
                        throw new NullReferenceException();
                    }
                    break;
                case "exp_card":
                    if (currentShopItem.itemValue != null) StartCoroutine(BuyExpUp((int)currentShopItem.itemValue));
                    else
                    {
                        NotificationBarManager.Instance.Show("经验卡数值为null");
                        throw new NullReferenceException();
                    }

                    break;
                default:
                    NotificationBarManager.Instance.Show("未知卡片：" + currentShopItem.keyword);
                    throw new ArgumentOutOfRangeException();
            }
        }
        NotificationBarManager.Instance.Show("购买成功！");
        PlayerData.Instance.Save();
        Refresh();
    }

    private IEnumerator BuyExpUp(int exp)
    {
        PlayerData.Instance.ExpToLevel();
        int oldLevel = PlayerData.Instance.level;
        PlayerData.Instance.exp += exp;
        PlayerData.Instance.RefreshLevel();
        bool levelup = PlayerData.Instance.level - oldLevel >= 1;
        yield return new WaitForSeconds(1f);
        if (levelup)
        {
            NotificationBarManager.Instance.Show("你升级了");
            // for (int index = oldLevel; index < PlayerData.Instance.level; ++index)
            // {
            //     Sprite s = this.SongUnlockSprite(index + 1);
            //     string str = this.SongUnlockTitle(index + 1);
            //     if ((UnityEngine.Object) s != (UnityEngine.Object) null)
            //     {
            //         if (this.global.PlayerLanguage == 0)
            //             this.global.ShowMessage("NEW SONG UNLOCKED", "[" + str + "] have been unlocked.\nLet's play it now!", s);
            //         if (this.global.PlayerLanguage == 1)
            //             this.global.ShowMessage("新曲アンロック！", "新曲[" + str + "]が\nアンロックされました。\n早速やってみよう！", s);
            //         if (this.global.PlayerLanguage == 2)
            //             this.global.ShowMessage("解鎖新曲！", "[" + str + "]解鎖了哦！\n趕快去試試看吧！", s);
            //     }
            // }
        }
    }

    public void Back()
    {
        PlayerData.Instance.Save();
        FadeManager.Instance.LoadScene("main");
    }
}