using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDamageAnimView : Module
{
    [SerializeField] private Vector2 damageAnchoredPos;
    [SerializeField] private RectTransform damageAnchoredRect;
    [SerializeField] private GameObject damageBackgroundPrefab;
    [SerializeField] private GameObject healAnchoredObject;
    [SerializeField] private GameObject healNumberPrefab;
    [SerializeField] private Color32 healPositiveColor;
    [SerializeField] private Color32 healNegativeColor;

    public void SetUnit(Unit lastUnit, Unit currentUnit) {
        if (currentUnit == null)
            return;

        UnitSkillSystem skillSystem = currentUnit.skillSystem;

        if (currentUnit.hudSystem.applyDamageAnim) {
            SetDamageObject(currentUnit.IsMyUnit(), currentUnit.hudSystem.damage, skillSystem.elementRelation, skillSystem.isHit, skillSystem.isCritical);
        }

        if (currentUnit.hudSystem.applyBuffDamageAnim) {
            SetDamageObject(currentUnit.IsMyUnit(), currentUnit.hudSystem.buffDamage, 1, true, false);
        }
        
        if (currentUnit.hudSystem.applyHealAnim) {
            SetHealObject(currentUnit.IsMyUnit(), currentUnit.hudSystem.heal, currentUnit.skill.type == SkillType.道具);
        }
    }

    private void SetHealObject(bool isMe, int heal, bool forceShowHeal = false) {
        if ((heal == 0) && (!forceShowHeal))
            return;

        GameObject obj = Instantiate(healNumberPrefab, healAnchoredObject.transform);
        Text num = obj.GetComponent<Text>();
        num.text = ((heal >= 0) ? "+" : string.Empty) + heal.ToString();
        num.color = heal > 0 ? healPositiveColor : healNegativeColor;
        StartCoroutine(SetHealAnim(num.rectTransform));
    }

    private IEnumerator SetHealAnim(RectTransform healRect) {
        float speed = -0.5f;
        while (healRect.anchoredPosition.y > -50) {
            healRect.anchoredPosition = new Vector2(healRect.anchoredPosition.x, healRect.anchoredPosition.y + speed);
            yield return null;
        }
        yield return new WaitForSeconds(0.8f);
        Destroy(healRect.gameObject);
    }


    private void SetDamageObject(bool isMe, int damage, float elementRelation = 1f, bool isHit = true, bool isCritical = false) {
        bool isDamage = (isHit && (damage != 0));
        string who = isMe ? "me" : "op";
        string absorb = isDamage ? string.Empty : "Absorb";

        GameObject obj = Instantiate(damageBackgroundPrefab, damageAnchoredRect);
        BattleDamageBackgroundView script = obj.GetComponent<BattleDamageBackgroundView>();
        Image img = script.Background;
        IAnimator anim = script.Anim;
        script.Critical.SetActive(isDamage && isCritical);
        script.Rect.anchoredPosition = damageAnchoredPos;
        script.Rect.SetAsLastSibling();

        if (isDamage) {
            script.InstantiateDamageNum(damage, isCritical);
        }

        img.SetDamageBackgroundSprite(elementRelation, isHit, (damage == 0));
        obj.SetActive(true);

        SetDamageAnim(anim, who + absorb);
    }

    private void SetDamageAnim(IAnimator iAnim, string trigger) {
        Action onAnimEnd = () => { OnDamageAnimEnd(iAnim); };
        iAnim.onAnimEndEvent.SetListener(onAnimEnd.Invoke);
        iAnim.anim.ResetAllTriggers();
        iAnim.anim.SetTrigger(trigger);
    }

    private void OnDamageAnimEnd(IAnimator anim) {
        Destroy(anim.gameObject);
    }

}
