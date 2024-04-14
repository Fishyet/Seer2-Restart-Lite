using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BuffInfo
{
    public const int DATA_COL = 7;

    /* Basic Data */
    public int id { get; private set; }
    public int resId { get; private set; }
    public string name { get; private set; }
    public BuffType type { get; private set; }
    public CopyHandleType copyHandleType { get; private set; }
    public int turn { get; private set; }
    public bool keep { get; private set; }  // 換場保留
    public bool inherit { get; private set; }   // 換場繼承
    public bool hide { get; private set; }  // 不予顯示
    public bool autoRemove { get; private set; }    // value為0以下時自動移除
    public string description { get; private set; }
    public Dictionary<string, string> options { get; private set; } = new Dictionary<string, string>();
    
    public int minValue = int.MinValue, maxValue = int.MaxValue;
    public int sortPriority { get => GetSortPriority(); }
    public List<Effect> effects { get; private set; }
    public Sprite icon { get => GetIcon(); }

    public BuffInfo(string[] _data, int startIndex = 0) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);
        id = int.Parse(_slicedData[0]);
        name = _slicedData[1];
        type = _slicedData[2].ToBuffType();
        copyHandleType = _slicedData[3].ToCopyHandleType();
        turn = int.Parse(_slicedData[4]);
        options.ParseOptions(_slicedData[5]);
        description = _slicedData[6].Trim();

        resId = int.Parse(options.Get("res", id.ToString()));
        keep = (type == BuffType.Unhealthy || type == BuffType.Abnormal) ? false : bool.Parse(options.Get("keep", "true"));
        inherit = bool.Parse(options.Get("inherit", "false"));
        hide = bool.Parse(options.Get("hide", "false"));
        autoRemove = bool.Parse(options.Get("auto_remove", "false"));
        minValue = int.Parse(options.Get("min_val", int.MinValue.ToString()));
        maxValue = int.Parse(options.Get("max_val", int.MaxValue.ToString()));
    }

    public void SetEffects(List<Effect> _effects) {
        effects = _effects;
    }


    public int GetSortPriority() {
        int mod = 10_0000;
        int _type = id / mod;
        int _pet = id % mod;
        if (type == BuffType.Feature) {
            return (-2 * mod) + (_pet - mod);
        }
        if (type == BuffType.Emblem) {
            return (-1 * mod) + (_pet - mod);
        }
        return id;
    }

    public Sprite GetIcon() {
        int _type = id / 10_0000;
        int _pet = id % 10_0000;

        if (type == BuffType.Feature)
            return   PetUISystem.GetPetIcon(_pet * (_type < 5 ? 1 : -1));
        if (type == BuffType.Emblem)
            return   PetUISystem.GetEmblemIcon(_pet * (_type < 5 ? 1 : -1));

        return ResourceManager.instance.GetLocalAddressables<Sprite>("Buffs/" + resId);
    }

}

