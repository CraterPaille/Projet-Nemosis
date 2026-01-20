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
    public bool IsAnimating = false;    // Update is called once per frame

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
        IsAnimating = true;
        TMPro.TextMeshProUGUI titreVillage = villageModeUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        
        // Stocker les positions initiales
        Transform[] nuages = NuagesParents.GetComponentsInChildren<Transform>();
        
        DG.Tweening.Sequence s = DOTween.Sequence();
        s.SetUpdate(true); // Utilise le temps réel (unscaled time)

        // Fade du titre puis décalage des nuages un par un avec délai entre chaque
        float delay = 0f;
        foreach (Transform nuage in nuages)
        {
            if (nuage == NuagesParents.transform) continue; // ignore le parent
            
            s.Insert(delay, nuage.DOMoveX(nuage.position.x + 50f, 0.7f).SetEase(Ease.OutBack));
            delay += 0.2f;
        }

        // 2. On attend que la SÉQUENCE entière soit finie
        yield return s.WaitForCompletion();
        callback?.Invoke();
        Time.timeScale = 0f; // S'assure que le temps est à 1
        // Créer une NOUVELLE séquence pour le retour
        DG.Tweening.Sequence s2 = DOTween.Sequence();
        s2.SetUpdate(true); // Utilise le temps réel (unscaled time)
        delay = 0f;
        foreach (Transform nuage in nuages)
        {
            if (nuage == NuagesParents.transform) continue; // ignore le parent
            
            // Utiliser DOMove relatif (delta de -50 depuis la position actuelle)
            s2.Insert(delay, nuage.transform.DOMoveX(nuage.position.x - 50f, 1f).SetEase(Ease.OutBack));
            delay += 0.2f;
        }
        
        yield return s2.WaitForCompletion();
        Time.timeScale = 1f;
        IsAnimating = false;
    }

    
    #endregion
}

