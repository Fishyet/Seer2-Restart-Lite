using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class BattleResourceView : BattleBaseView
{
    public ResourceManager RM => ResourceManager.instance;
    [SerializeField] private SpriteRenderer background;

    public override void Init()
    {
        LoadResources();
    }

    private void LoadResources()
    {
        LoadBackground();
        LoadDamage();
    }

    private void LoadBackground()
    {
        Sprite fightMap = (Sprite)Player.GetSceneData("fightBg");
        if (fightMap == null)
            return;

        background.sprite = fightMap;
        background.gameObject.transform.localScale = new Vector3(1920 / background.sprite.rect.width,
            1080 / background.sprite.rect.height, 1);
        ;
        background.color = Player.instance.currentMap.fightMapColor;
    }

    private void LoadDamage()
    {
        string[] damageBgType = new string[5] { "miss", "absorb", "weak", "resist", "normal" };
        for (int i = 0; i < 5; i++)
            RM.LoadSprite("Fight/damage " + damageBgType[i]);

        for (int i = 0; i < RM.numString.Length; i++)
            RM.LoadSprite("Numbers/Damage/" + RM.numString[i].ToString());
    }
}