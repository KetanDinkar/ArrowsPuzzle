using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    public LifeSystem life;
    public Image[] hearts;
    public Sprite fullSprite, brokenSprite, emptySprite;

    int last;

    void Start()
    {
        foreach (var h in hearts) h.preserveAspect = true;
        last = life.Lives;
        life.OnLivesChanged += Refresh;
        for (int i = 0; i < hearts.Length; i++)
            hearts[i].sprite = i < life.Lives ? fullSprite : emptySprite;
    }

    void OnDestroy()
    {
        if (life != null) life.OnLivesChanged -= Refresh;
    }

    void Refresh(int lives)
    {
        // Hearts are lost from the right: heart 3 first, then 2, then 1
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < lives) hearts[i].sprite = fullSprite;
            else if (i < last) StartCoroutine(Break(hearts[i]));
        }
        last = lives;
    }

    IEnumerator Break(Image h)
    {
        var rt = h.rectTransform;
        h.sprite = brokenSprite;

        // pop and shake while broken
        float dur = 0.7f;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float k = t / dur;
            float pop = 1f + 0.35f * Mathf.Sin(k * Mathf.PI);
            float shake = Mathf.Sin(k * 40f) * (1f - k) * 12f;
            rt.localScale = Vector3.one * pop;
            rt.localRotation = Quaternion.Euler(0, 0, shake);
            yield return null;
        }

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        h.sprite = emptySprite;
    }
}