using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class healthBarScript : MonoBehaviour
{
    public Slider slider;
    public CanvasGroup canvasGroup;
    private int oldHP;
    public float currentHP;

    public void UpdateSlider(int hp, int maxHP)
    {
        currentHP = Mathf.Lerp(currentHP, hp, 0.2f);
        if (Mathf.Abs(currentHP - hp) < 0.1)
        {
            oldHP = hp;
            currentHP = hp;
        }
        slider.value = currentHP / maxHP;
    }

    public void FadeIn()
    {
        if (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1, 0.1f);
            if (canvasGroup.alpha > 0.99)
                canvasGroup.alpha = 1;
        }
    }

    public void FadeOut()
    {
        if (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, 0.1f);
            if (canvasGroup.alpha < 0.01)
                canvasGroup.alpha = 0;
        }
    }
}
