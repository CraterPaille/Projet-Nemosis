using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using System;
using System.Collections;
using TMPro;
using UnityEngine.UIElements;
public class DOTweenManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static DOTweenManager Instance;

    [Header("Animations et effets")]
    bool isAnimating = false;    // Update is called once per frame

    [Header("Choix de mode")]
    public GameObject villageModeUI;
    public GameObject relationModeUI;
    public GameObject miniGameCardModeUI;
    public GameObject villageCardModeUI;
    public GameObject NuagesParents;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #region mode de jeu
    public IEnumerator transitionChoixJeu(Action callback)
    {
        TMPro.TextMeshProUGUI titreVillage = villageModeUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        DG.Tweening.Sequence s = DOTween.Sequence();

        // On ajoute plein d'animations qui jouent en même temps (Join) ou à la suite (Append)
        s.Append(titreVillage.DOFade(200, 2f)); // 1. On fait disparaître le texte "Village"
        foreach (Transform nuage in NuagesParents.GetComponentsInChildren<Transform>())
        {
            s.Join(nuage.DOMoveX(nuage.position.x + 32.5f, 2f).SetEase(Ease.OutBack));
        }

        // 2. On attend que la SÉQUENCE entière soit finie
        yield return s.WaitForCompletion();
        callback?.Invoke();
        foreach (Transform nuage in NuagesParents.GetComponentsInChildren<Transform>())
        {
            s.Join(nuage.DOMoveX(nuage.position.x - 32.5f, 2f).SetEase(Ease.OutBack));
        }
    }

    public IEnumerator TransitionVillage()
    {
        TMPro.TextMeshProUGUI titreVillage = villageModeUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        DG.Tweening.Sequence s = DOTween.Sequence();

        // On ajoute plein d'animations qui jouent en même temps (Join) ou à la suite (Append)
        s.Append(titreVillage.DOFade(200, 2f)).SetEase(Ease.InOutQuart); // 1. On fait disparaître le texte "Village"

        // 2. On attend que la SÉQUENCE entière soit finie
        yield return s.WaitForCompletion();

    }

    public IEnumerator TransitionVillageCard()
    {
        TMPro.TextMeshProUGUI titreVillage = villageModeUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        DG.Tweening.Sequence s = DOTween.Sequence();

        // On ajoute plein d'animations qui jouent en même temps (Join) ou à la suite (Append)
        s.Append(titreVillage.DOFade(200, 2f)).SetEase(Ease.InOutQuad); // 1. On fait disparaître le texte "Village"

        // 2. On attend que la SÉQUENCE entière soit finie
        yield return s.WaitForCompletion();

    }

    public IEnumerator TransitionRelationn()
    {
        TMPro.TextMeshProUGUI titreVillage = villageModeUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        DG.Tweening.Sequence s = DOTween.Sequence();

        // On ajoute plein d'animations qui jouent en même temps (Join) ou à la suite (Append)
        s.Append(titreVillage.DOFade(200, 2f)).SetEase(Ease.InOutQuad); // 1. On fait disparaître le texte "Village"

        // 2. On attend que la SÉQUENCE entière soit finie
        yield return s.WaitForCompletion();

    }

    public IEnumerator TransitionMiniGameCard()
    {
        TMPro.TextMeshProUGUI titreVillage = villageModeUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        DG.Tweening.Sequence s = DOTween.Sequence();

        // On ajoute plein d'animations qui jouent en même temps (Join) ou à la suite (Append)
        s.Append(titreVillage.DOFade(200, 2f)).SetEase(Ease.InOutQuad); // 1. On fait disparaître le texte "Village"

        // 2. On attend que la SÉQUENCE entière soit finie
        yield return s.WaitForCompletion();

    }
    #endregion
}

