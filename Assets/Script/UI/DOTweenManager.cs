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
        try
        {
            // Si les références manquent, on exécute le callback et on libère IsAnimating pour éviter de bloquer les boutons
            if (NuagesParents == null)
            {
                Debug.LogWarning("[DOTweenManager] NuagesParents manquant, annulation de l'animation.");
                callback?.Invoke();
                yield break;
            }

            // Stocker les positions initiales
            Transform[] nuages = NuagesParents.GetComponentsInChildren<Transform>();
            
            DG.Tweening.Sequence s = DOTween.Sequence();
            s.SetUpdate(true); // Utilise le temps réel (unscaled time)

            // Fade du titre puis décalage des nuages un par un avec délai entre chaque
            float delay = 0f;
            int compteurs = 0;
            foreach (Transform nuage in nuages)
            {
                compteurs++;
                
                if (nuage == NuagesParents.transform) continue; // ignore le parent
                if (compteurs % 2 == 0) // nuage pair va à gauche
                    s.Join(nuage.DOMoveX(nuage.position.x + 50f, 0.5f).SetEase(Ease.OutBack));
                else // nuage impair va à droite
                    s.Insert(delay, nuage.DOMoveX(nuage.position.x + 50f, 0.5f).SetEase(Ease.OutBack));
                delay += 0.2f;
            }

            // 2. On attend que la SÉQUENCE entière soit finie
            yield return s.WaitForCompletion();
            callback?.Invoke();
            Debug.Log("[DOTweenManager] transitionChoixJeu: callback invoqué");
            Time.timeScale = 1f; // Keep gameplay unpaused after callback
            // Créer une NOUVELLE séquence pour le retour
            DG.Tweening.Sequence s2 = DOTween.Sequence();
            s2.SetUpdate(true); // Utilise le temps réel (unscaled time)
            delay = 0f;
            foreach (Transform nuage in nuages)
            {
                if (nuage == NuagesParents.transform) continue; // ignore le parent
                
                // Utiliser DOMove relatif (delta de -50 depuis la position actuelle)
                s2.Insert(delay, nuage.transform.DOMoveX(nuage.position.x - 50f, 0.5f).SetEase(Ease.OutBack));
                delay += 0.2f;
            }
            
            yield return s2.WaitForCompletion();
        }
        finally
        {
            Time.timeScale = 1f;
            IsAnimating = false;
        }
    }

    public IEnumerator animationCard(Transform cardTransform, Action callback)
    {
        Vector3 posInitial = cardTransform.position;
        Vector3 scaleInitial = cardTransform.localScale;
        IsAnimating = true;
        
        // Récupérer la position du centre de l'écran en coordonnées monde
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);
        worldCenter.z = cardTransform.position.z; // Garder la profondeur Z originale

        DG.Tweening.Sequence s = DOTween.Sequence();
        s.SetUpdate(true); // Utilise le temps réel (unscaled time)

        // Animation parallèle : déplacement, zoom et rotation
        float duration = 0.8f;
        
        // Déplacement vers le centre
        s.Append(cardTransform.DOMoveX(worldCenter.x, duration).SetEase(Ease.OutBack));
        
        // Zoom en parallèle (grossissement)
        s.Join(cardTransform.DOScale(1.3f, duration).SetEase(Ease.OutBack));
        
        // Rotation continue pendant le déplacement
        s.Join(cardTransform.DORotate(new Vector3(0, 0, 360), duration * 0.5f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops((int)(duration / (duration * 0.5f)), LoopType.Restart));

        yield return s.WaitForCompletion();
        callback?.Invoke();
        yield return new WaitForSeconds(0.2f);
        
        StartCoroutine(ReturnInitCard(cardTransform, posInitial, scaleInitial));
         
    }

    public IEnumerator ReturnInitCard(Transform cardTransform, Vector3 posInitial, Vector3 scaleInitial)
    {
        yield return new WaitForSeconds(1f);
        IsAnimating = false;
        // Rétablir la position et l'échelle initiales
        cardTransform.position = posInitial;
        cardTransform.localScale = scaleInitial;
        

    }


    
    #endregion
}

