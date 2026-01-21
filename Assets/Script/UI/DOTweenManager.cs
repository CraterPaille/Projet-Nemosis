using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
public class DOTweenManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static DOTweenManager Instance;

    [Header("Animations et effets")]
    public bool IsAnimating = false;    // Update is called once per frame
    public GameObject HorlogeUI;

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
            HorlogeUI.SetActive(false);
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
            s.SetUpdate(false); // Utilise le temps réel (unscaled time)

            // Fade du titre puis décalage des nuages un par un avec délai entre chaque
            float delay = 0f;
            int compteurs = 0;
            foreach (Transform nuage in nuages)
            {
                compteurs++;
                
                if (nuage == NuagesParents.transform) continue; // ignore le parent
                if (compteurs % 2 == 0) // nuage pair va à gauche
                    s.Join(nuage.DOMoveX(nuage.position.x + 50f, 0.4f).SetEase(Ease.OutBack));
                else // nuage impair va à droite
                    s.Insert(delay, nuage.DOMoveX(nuage.position.x + 50f, 0.3f).SetEase(Ease.OutBack));
                delay += 0.1f;
            }

            // 2. On attend que la SÉQUENCE entière soit finie
            yield return s.WaitForCompletion();
            callback?.Invoke();
            // Keep gameplay unpaused after callback
            // Créer une NOUVELLE séquence pour le retour
            DG.Tweening.Sequence s2 = DOTween.Sequence();
            s2.SetUpdate(false); // Utilise le temps réel (unscaled time)
            delay = 0f;
            foreach (Transform nuage in nuages)
            {
                if (nuage == NuagesParents.transform) continue; // ignore le parent
                
                // Utiliser DOMove relatif (delta de -50 depuis la position actuelle)
                s2.Insert(delay, nuage.transform.DOMoveX(nuage.position.x - 50f, 0.3f).SetEase(Ease.OutBack));
                delay += 0.1f;
            }
            
            yield return s2.WaitForCompletion();
        }
        finally
        {
            //Time.timeScale = 1f;
            IsAnimating = false;
        }
    }

    public IEnumerator animationCard(Transform cardTransform, Action callback)
    {
        Vector3 posInitial = cardTransform.position;
        Vector3 scaleInitial = cardTransform.localScale;
        Quaternion rotInitial = cardTransform.rotation;
        IsAnimating = true;
        
        // Positions en espace écran
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0); // Milieu de l'écran
        Vector3 screenLeftTop = new Vector3(-100, Screen.height - 150, 0); // Départ hors écran à gauche
        Vector3 screenCenterTop = new Vector3(Screen.width / 2f, Screen.height - 80, 0); // Centre un peu plus haut (courbe)
        Vector3 screenRightTop = new Vector3(Screen.width + 100, Screen.height - 150, 0); // Fin hors écran à droite
        
        // Conversion en coordonnées monde
        Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);
        Vector3 worldLeftTop = Camera.main.ScreenToWorldPoint(screenLeftTop);
        Vector3 worldCenterTop = Camera.main.ScreenToWorldPoint(screenCenterTop);
        Vector3 worldRightTop = Camera.main.ScreenToWorldPoint(screenRightTop);
        
        // Garder la profondeur Z originale
        worldCenter.z = cardTransform.position.z;
        worldLeftTop.z = cardTransform.position.z;
        worldCenterTop.z = cardTransform.position.z;
        worldRightTop.z = cardTransform.position.z;

        // === PHASE 1 : Carte va au milieu en tournant 360° ===
        DG.Tweening.Sequence s1 = DOTween.Sequence();
        s1.SetUpdate(true);
        
        float phase1Duration = 1f;
        s1.Append(cardTransform.DOMove(worldCenter, phase1Duration).SetEase(Ease.OutQuad));
        s1.Join(cardTransform.DOScale(1.3f, phase1Duration).SetEase(Ease.OutBack));
        s1.Join(cardTransform.DORotate(new Vector3(0, 360, 0), phase1Duration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
        
        yield return s1.WaitForCompletion();
        
        // === PHASE 2 : Va vers la gauche (début de la courbe) ===
        DG.Tweening.Sequence s2 = DOTween.Sequence();
        s2.SetUpdate(true);
        
        float phase2Duration = 0.5f;
        s2.Append(cardTransform.DOMove(worldLeftTop, phase2Duration).SetEase(Ease.InQuad));
        s2.Join(cardTransform.DOScale(0.4f, phase2Duration).SetEase(Ease.InQuad));
        s2.Join(cardTransform.DORotate(Vector3.zero, phase2Duration).SetEase(Ease.OutQuad));
        
        yield return s2.WaitForCompletion();
        
        // === PHASE 3 : Trajectoire courbe de gauche à droite ===
        DG.Tweening.Sequence s3 = DOTween.Sequence();
        s3.SetUpdate(true);

        float phase3Duration = 2f;
        
        // Déplacement en courbe CatmullRom : gauche -> centre (haut) -> droite
        Vector3[] path = new Vector3[] { worldLeftTop, worldCenterTop, worldRightTop };
        s3.Append(cardTransform.DOPath(path, phase3Duration, PathType.CatmullRom).SetEase(Ease.InOutSine));
        callback?.Invoke();
        // Inclinaison légère et fixe vers la droite (rotation Z de -15°)
        s3.Join(cardTransform.DORotate(new Vector3(0, 0, -15f), phase3Duration * 0.3f).SetEase(Ease.OutQuad));

        yield return s3.WaitForCompletion();
        
        yield return new WaitForSeconds(1f);
        
        StartCoroutine(ReturnInitCard(cardTransform, posInitial, scaleInitial, rotInitial));
         
    }

    public IEnumerator ReturnInitCard(Transform cardTransform, Vector3 posInitial, Vector3 scaleInitial, Quaternion rotInitial)
    {
        yield return new WaitForSeconds(0.5f);
        IsAnimating = false;
        // Rétablir la position, l'échelle et la rotation initiales
        cardTransform.position = posInitial;
        cardTransform.localScale = scaleInitial;
        cardTransform.rotation = rotInitial;
        

    }

    public IEnumerator OnActionCardAnimation(Transform cardTransform, VillageCard card)
    {
        if (card == null)
        {
            Debug.LogError("[DOTweenManager] card est null dans OnActionCardAnimation!");
            yield break;
        }
        
        if (IsAnimating == false)
        {
            
            StartCoroutine(animationCard(cardTransform, () => { card.PlayCard();}));
            yield return new WaitForSeconds(4.5f);
            StartCoroutine(transitionChoixJeu(() => CardUI.Instance.AfterCard()));
        }
    }

    public IEnumerator OnActionCardMiniJeuAnimation(Transform cardTransform, Action Callback)
    {
        
        if (IsAnimating == false)
        {
            
            StartCoroutine(animationCard(cardTransform, () => {;}));
            yield return new WaitForSeconds(2f);
            StartCoroutine(transitionChoixJeu(Callback));
        }
    }
    
    #endregion
}

