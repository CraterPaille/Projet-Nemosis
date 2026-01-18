using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class VictoryScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup victoryPanel;
    public TMP_Text victoryTitle;
    public TMP_Text victorySubtitle;
    public Image[] starImages; // 3 étoiles
    public Image flashOverlay;
    public ParticleSystem confettiEffect;

    [Header("Boss")]
    public SpriteRenderer bossSprite;
    public Transform bossTransform;

    [Header("Audio")]
    public AudioSource victoryMusic;

    void Start()
    {
        // Cache tous les éléments au départ
        if (victoryPanel) victoryPanel.alpha = 0;
        if (victoryTitle) victoryTitle.alpha = 0;
        if (victorySubtitle) victorySubtitle.alpha = 0;
        if (flashOverlay) flashOverlay.color = new Color(1, 1, 1, 0);
        foreach (var star in starImages) if (star) star.transform.localScale = Vector3.zero;
    }

    public void PlayVictoryAnimation()
    {
        StartCoroutine(VictorySequence());
    }

    IEnumerator VictorySequence()
    {
        // === ÉTAPE 1: Le boss disparaît (2s) ===
        if (bossSprite && bossTransform)
        {
            Sequence bossSeq = DOTween.Sequence();
            bossSeq.Append(bossSprite.DOFade(0, 1f));
            bossSeq.Join(bossTransform.DOScale(0.5f, 1f).SetEase(Ease.InBack));
            bossSeq.Join(bossTransform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360));
            yield return bossSeq.WaitForCompletion();
        }
        yield return new WaitForSeconds(0.5f);

        // === ÉTAPE 2: Flash blanc (0.5s) ===
        if (flashOverlay)
        {
            flashOverlay.DOFade(1, 0.1f).OnComplete(() => flashOverlay.DOFade(0, 0.4f));
        }
        yield return new WaitForSeconds(0.5f);

        // === ÉTAPE 3: Panneau de victoire apparaît (1s) ===
        if (victoryPanel)
        {
            victoryPanel.DOFade(1, 1f).SetEase(Ease.OutQuad);
        }
        yield return new WaitForSeconds(0.5f);

        // === ÉTAPE 4: Titre "VICTOIRE !" (1.5s) ===
        if (victoryTitle)
        {
            victoryTitle.text = "★ VICTOIRE ! ★";
            victoryTitle.transform.localScale = Vector3.zero;

            Sequence titleSeq = DOTween.Sequence();
            titleSeq.Append(victoryTitle.transform.DOScale(1.3f, 0.5f).SetEase(Ease.OutBack));
            titleSeq.Append(victoryTitle.transform.DOScale(1f, 0.3f).SetEase(Ease.InOutQuad));
            titleSeq.Join(victoryTitle.DOFade(1, 0.5f));

            // Rotation continue du titre
            victoryTitle.transform.DORotate(new Vector3(0, 0, 5), 0.5f, RotateMode.Fast)
                .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            yield return titleSeq.WaitForCompletion();
        }

        // === ÉTAPE 5: Confettis ! ===
        if (confettiEffect) confettiEffect.Play();

        // === ÉTAPE 6: Étoiles apparaissent (1.5s) ===
        if (starImages != null && starImages.Length > 0)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;

                Sequence starSeq = DOTween.Sequence();
                starSeq.Append(starImages[i].transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack));
                starSeq.Append(starImages[i].transform.DOScale(1f, 0.2f));

                // Rotation continue
                starImages[i].transform.DORotate(new Vector3(0, 0, 360), 2f, RotateMode.FastBeyond360)
                    .SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);

                yield return new WaitForSeconds(0.3f);
            }
        }

        // === ÉTAPE 7: Sous-titre (1s) ===
        if (victorySubtitle)
        {
            victorySubtitle.text = "Tu as vaincu Chronos !";
            victorySubtitle.DOFade(1, 1f).SetEase(Ease.InOutQuad);
        }

        // === ÉTAPE 8: Musique de victoire ===
        if (victoryMusic) victoryMusic.Play();

        // === ÉTAPE 9: Effets continus ===
        StartCoroutine(ContinuousEffects());
    }

    IEnumerator ContinuousEffects()
    {
        // Effet de pulsation sur le titre
        if (victoryTitle)
        {
            victoryTitle.transform.DOScale(1.1f, 0.8f)
                .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        // Couleur arc-en-ciel sur le titre
        if (victoryTitle)
        {
            while (true)
            {
                Color[] colors = { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta };
                foreach (var col in colors)
                {
                    victoryTitle.DOColor(col, 1f);
                    yield return new WaitForSeconds(1f);
                }
            }
        }
    }
}