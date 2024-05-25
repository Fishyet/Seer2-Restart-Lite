using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Skill
{
    public const int DATA_COL = 9;

    public int id;
    public string name;
    public int elementId;
    public Element element
    {
        get => (Element)elementId;
        set => elementId = (int)value;
    }
    public SkillType type;
    public int power;
    public int anger;
    public int accuracy;
    public string rawDescription;
    public string description => GetDescription(rawDescription);
    public List<Effect> effects = new List<Effect>();
    public Dictionary<string, string> options = new Dictionary<string, string>();

    /* Hidden status */
    public bool isSecondSuper;
    public List<string> referBuffList = new List<string>();

    public float critical;
    public int combo;
    public int priority;
    public bool ignoreShield = false;
    public bool ignorePowerup = false;


    /* Properties */
    public bool isSuper => type == SkillType.必杀;
    public bool isAction => IsAction();
    public bool isAttack => IsAttack();
    public bool isCapture => IsCapture();

    public PetAnimationType petAnimType => GetPetAnimationType();
    public PetAnimationType captureAnimType => GetCaptureAnimationType();

    public static bool IsMod(int id) {
        return id < -10000;
    }

    public Skill() {}

    public Skill(string[] _data, int startIndex = 0) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        id = int.Parse(_slicedData[0]);
        name = _slicedData[1];
        elementId = int.Parse(_slicedData[2]);
        element = (Element)elementId;
        type = (SkillType)int.Parse(_slicedData[3]);
        power = int.Parse(_slicedData[4]);
        anger = int.Parse(_slicedData[5]);
        accuracy = int.Parse(_slicedData[6]);
        options.ParseOptions(_slicedData[7]);

        combo = 1;
        InitOptionsProperty();

        rawDescription = _slicedData[8];
    }

    public Skill(int id, string name, Element element, SkillType type,
        int power, int anger, int accuracy, string options, string rawDescription) {
        this.id = id;
        this.name = name;
        this.element = element;
        this.type = type;
        this.power = power;
        this.anger = anger;
        this.accuracy = accuracy;
        this.options.ParseOptions(options);
        
        combo = 1;
        InitOptionsProperty();

        this.rawDescription = rawDescription;
    }

    private void InitOptionsProperty() {
        referBuffList = options.Get("ref_buff", null)?.Split('/').ToList();

        isSecondSuper = bool.Parse(options.Get("second_super", "false"));
        critical = float.Parse(options.Get("critical", "5"));
        priority = int.Parse(options.Get("priority", "0"));
        ignoreShield = bool.Parse(options.Get("ignore_shield", "false"));
        ignorePowerup = bool.Parse(options.Get("ignore_powerup", "false"));
    }

    public Skill(Skill rhs) {
        id = rhs.id;
        name = rhs.name;
        elementId = rhs.elementId;
        element = rhs.element;
        type = rhs.type;
        power = rhs.power;
        anger = rhs.anger;
        accuracy = rhs.accuracy;
        options = rhs.options.ToDictionary(entry => entry.Key, entry => entry.Value);
        rawDescription = rhs.rawDescription;
        SetEffects(rhs.effects.Select(x => new Effect(x)).ToList());

        isSecondSuper = rhs.isSecondSuper;
        critical = rhs.critical;
        combo = rhs.combo;
        priority = rhs.priority;
        ignoreShield = rhs.ignoreShield;
        ignorePowerup = rhs.ignorePowerup;
    }

    protected Skill(SkillType specialType) {
        id = (int)specialType;
        type = specialType;
        power = anger = 0;
        accuracy = 100;
    }

    public string[] GetRawInfoStringArray() {
        var rawOptionString = ((critical == 5) ? string.Empty : ("critical=" + critical + "&")) +
            ((priority == 0) ? string.Empty : ("priority=" + priority + "&")) + 
            (isSecondSuper ? ("second_super=true&") : string.Empty) +
            (ignoreShield ? ("ignore_shield=" + ignoreShield + "&") : string.Empty) + 
            (ignorePowerup ? ("ignore_powerup=" + ignorePowerup + "&") : string.Empty) + 
            (List.IsNullOrEmpty(referBuffList) ? string.Empty : ("ref_buff=" + referBuffList.ConcatToString("/")));

        rawOptionString = string.IsNullOrEmpty(rawOptionString) ? "none" : rawOptionString;

        return new string[] { id.ToString(), name, elementId.ToString(), ((int)type).ToString(),
            power.ToString(), anger.ToString(), accuracy.ToString(), rawOptionString.TrimEnd("&"), rawDescription  };
    }

    public static Skill GetSkill(int id, bool avoidNull = true) {
        Skill skill = Database.instance.GetSkill(id);
        return avoidNull ? (skill ?? GetNoOpSkill()) : skill;
    }

    public static Skill ParseRPCData(string[] data) {
        int id = int.Parse(data[0]);
        return id switch {
            (int)SkillType.空过 => Skill.GetNoOpSkill(),
            (int)SkillType.道具 => Skill.GetItemSkill(new Item(int.Parse(data[1]))),
            (int)SkillType.換场 => Skill.GetPetChangeSkill(int.Parse(data[1]), int.Parse(data[2]), bool.Parse(data[3])),
            (int)SkillType.逃跑 => Skill.GetEscapeSkill(),
            _ => Skill.GetSkill(id)
        };
    }

    public string[] ToRPCData() {
        return id switch {
            (int)SkillType.空过 => new string[] { id.ToString() },
            (int)SkillType.道具 => new string[] { id.ToString(), options.Get("item_id", "0") },
            (int)SkillType.換场 => new string[] { id.ToString(), options.Get("source_index", "0"), options.Get("target_index", "0"), options.Get("passive", "false") },
            (int)SkillType.逃跑 => new string[] { id.ToString() },
            _ => new string[] { id.ToString() }
        };
    }

    public static Skill GetRandomSkill() {
        var skillData = GameManager.versionData.skillData;
        int minSkillId = skillData.minSkillId;
        int maxSkillId = skillData.maxSkillId;
        while (true) {
            int skillId = Random.Range(minSkillId, maxSkillId + 1);
            Skill skill = Skill.GetSkill(skillId);
            if (skill.type != SkillType.必杀)
                return skill;
        }
    }

    public static Skill GetNoOpSkill() {
        Skill skill = new Skill(SkillType.空过);
        skill.name = "空过";
        skill.rawDescription = "跳过自己的回合";
        return skill;
    }

    public static Skill GetEscapeSkill() {
        Skill skill = new Skill(SkillType.逃跑);
        skill.name = "逃跑";
        skill.SetEffects(Effect.GetEscapeEffect());
        return skill;
    }

    public static Skill GetPetChangeSkill(int sourceIndex, int targetIndex, bool passive = false) {
        Skill skill = new Skill(SkillType.換场);
        skill.name = "換场";
        skill.options.Set("source_index", sourceIndex.ToString());
        skill.options.Set("target_index", targetIndex.ToString());
        skill.options.Set("passive", passive.ToString());
        skill.SetEffects(Effect.GetPetChangeEffect(sourceIndex, targetIndex, passive));
        return skill;
    }

    public static Skill GetItemSkill(Item item) {
        Skill skill = new Skill(SkillType.道具);
        skill.name = "道具";
        skill.options.Set("item_id", item.id.ToString());
        skill.effects = item.effects;
        return skill;
    }

    public static string GetSkillDescriptionPreview(string plainText) {
        return plainText.Replace("[ENDL]", "\n").Replace("[-]", "</color>").Replace("[", "<color=#").Replace("]", ">");
    }

    public string GetDescription(string plainText) {
        string desc;
        desc = plainText.Trim();
        if (priority != 0) {
            var priDesc = "[77e20c]【先制" + ((priority > 0) ? "+" : string.Empty) + priority + "】[-][ENDL]";
            desc = priDesc + desc;
        }
        if ((critical <= 100) && (critical != 5)) {
            desc = "[ff50d0]【暴击率 " + critical + "%】[-][ENDL]" + desc;
        }
        if ((accuracy <= 100) && (accuracy != (isAttack ? 95 : 100))) {
            desc = "[52e5f9]【命中率 " + accuracy + "%】[-][ENDL]" + desc;
        }
        if (!List.IsNullOrEmpty(referBuffList)) {
            for (int i = 0; i < referBuffList.Count; i++) {
                bool isWithValue = referBuffList[i].TryTrimParentheses(out var value);
                var buffId = int.Parse(isWithValue ? referBuffList[i].Substring(0, referBuffList[i].IndexOf('[')) : referBuffList[i]);
                var buffValue = isWithValue ? int.Parse(value) : 0;
                var buff = new Buff(buffId, -2, buffValue);
                desc += ("[ENDL][ENDL][ffbb33]【" + buff.name + "】[-]：" + buff.description);
            }
        }
        return Skill.GetSkillDescriptionPreview(desc);
    }   

    public void SetEffects(Effect _effect) {
        _effect.source = this;
        effects = new List<Effect>() { _effect };
    }

    public void SetEffects(List<Effect> _effects) {
        foreach (var e in _effects) {
            e.source = this;
        }
        effects = _effects;
    }

    public bool IsAction() {
        return (type != SkillType.属性) && (type != SkillType.物理) 
            && (type != SkillType.特殊) && (type != SkillType.必杀);
    }

    public bool IsAttack() {
        return (type == SkillType.物理) || (type == SkillType.特殊) || (type == SkillType.必杀);
    }

    public bool IsCapture() {
        return effects.Any(x => x.ability == EffectAbility.Capture);
    }

    public PetAnimationType GetPetAnimationType() {
        if (type == SkillType.物理)
            return PetAnimationType.Physic;

        if (type == SkillType.特殊)
            return PetAnimationType.Special;

        if (type == SkillType.属性)
            return PetAnimationType.Property;

        if (type == SkillType.必杀)
            return isSecondSuper ? PetAnimationType.SecondSuper : PetAnimationType.Super;

        return PetAnimationType.None;
    }

    public PetAnimationType GetCaptureAnimationType() {
        if (isCapture)
            return (options.Get("capture_result", "false") == "false") ? 
                PetAnimationType.CaptureFail : PetAnimationType.CaptureSuccess;

        return PetAnimationType.None;
    }

    public float GetSkillIdentifier(string id) {
        return id switch {
            "id" => this.id,
            "element" => elementId,
            "type" => (float)type,
            "power" => power,
            "anger" => anger,
            "accuracy" => accuracy,
            "priority" => priority,
            "critical" => critical,
            "combo" => combo,
            "ignoreShield" => ignoreShield ? 1 : 0,
            "ignorePowerup" => ignorePowerup ? 1 : 0,
            _ => float.MinValue,
        };
    }

    public bool TryGetSkillIdentifier(string id, out float num) {
        num = GetSkillIdentifier(id);
        return num != float.MinValue;
    }

    public void SetSkillIdentifier(string id, float value) {
        switch (id) {
            default:
                return;
            case "element":
                elementId = (int)value;
                element = (Element)elementId;
                return;
            case "type":
                type = (SkillType)value;
                return;
            case "power":
                power = (int)value;
                return;
            case "anger":
                anger = (int)value;
                return;
            case "accuracy":
                accuracy = (int)value;
                return;
            case "priority":
                priority = (int)value;
                return;
            case "critical":
                critical = value;
                return;
            case "combo":
                combo = (int)value;
                return;
            case "ignoreShield":
                ignoreShield = (value != 0);
                return;
        }
    }   

}

public enum SkillType {
    空过 = -1, 道具 = -2, 換场 = -3, 逃跑 = -4,
    属性 = 0, 物理 = 1, 特殊 = 2, 必杀 = 100,
}

