# 📅 SYSTÈME D'ÉVÉNEMENTS - GUIDE D'UTILISATION

## 📋 RÉCAPITULATIF DES FICHIERS CRÉÉS/MODIFIÉS

### ✅ Fichiers créés :
1. **BaseGameEvent.cs** - Classe abstraite pour les événements
2. **EventScheduler.cs** - Manager des événements (Singleton)
3. **BaseEventManager.cs** - Classe abstraite pour les managers de mini-jeux
4. **ExampleHarvestFestivalEvent.cs** - EXEMPLE d'événement concret
5. **ExampleClickerEventManager.cs** - EXEMPLE de manager de mini-jeu

### ✏️ Fichiers modifiés :
1. **GameManager.cs** - Ajout de l'integration EventScheduler
2. **UIManager.cs** - Ajout de ShowEventPanel() et HideEventPanel()

---

## 🎮 CE QUE VOUS DEVEZ FAIRE DANS UNITY

### 1️⃣ CONFIGURATION DE L'EVENTSCHEDULER

**Dans votre scène principale :**

1. Créer un GameObject vide : `EventScheduler`
2. Ajouter le component `EventScheduler`
3. Dans le GameManager :
   - Assigner la référence `EventScheduler` dans l'Inspector

### 2️⃣ CRÉER UN PANEL D'ÉVÉNEMENT DANS L'UI

**Dans votre Canvas UI :**

1. Créer un nouveau Panel : `EventPanel`
2. Ajouter les composants enfants :
   - Image : `EventImage` (pour l'image de l'événement)
   - TextMeshPro : `EventTitle` (titre de l'événement)
   - TextMeshPro : `EventDescription` (description)
   - Button : `EventStartButton` (bouton "Commencer")

3. Dans UIManager :
   - Assigner toutes ces références dans l'Inspector

### 3️⃣ CRÉER VOS PROPRES ÉVÉNEMENTS

**Créer un ScriptableObject d'événement :**

1. Dans Unity : `Clic droit > Create > Events > Game Event`
2. Renommer : `MonPremierEvenement`
3. Configurer dans l'Inspector :
   - **Scene Name** : Nom de la scène du mini-jeu (ex: "HarvestFestivalScene")
   - **Event Image** : Sprite de l'événement
   - **Event Name** : "Festival de la Moisson"
   - **Description** : "Aidez le village à récolter..."
   - **Duration Half Days** : 3 (= 1.5 jours)
   - **Thresholds & Rewards** : Configurez les seuils et récompenses

### 4️⃣ PLANIFIER LES ÉVÉNEMENTS

**Dans EventScheduler (Inspector) :**

1. Dans `Scheduled Events`, cliquer sur `+`
2. Configurer :
   - **Day** : 5 (jour où l'événement se déclenche)
   - **Game Event** : Glisser votre ScriptableObject ici

Répétez pour chaque événement du calendrier (28 jours max).

### 5️⃣ CRÉER UNE SCÈNE DE MINI-JEU

**Pour chaque mini-jeu :**

1. Créer une nouvelle scène : `HarvestFestivalScene`
2. Ajouter un GameObject vide : `EventManager`
3. Créer un nouveau script hérité de `BaseEventManager` :

```csharp
public class HarvestFestivalManager : BaseEventManager
{
    protected override int CalculateScore()
    {
        // Votre logique de calcul du score
        return monScore;
    }

    protected override void ReturnToBaseGame()
    {
        SceneManager.LoadScene("VotreScenePrincipale");
    }

    // Appellez CompleteEvent() quand le mini-jeu se termine
}
```

4. Ajouter votre UI de mini-jeu
5. À la fin du mini-jeu, appeler `CompleteEvent()`

### 6️⃣ AJOUTER LA SCÈNE AUX BUILD SETTINGS

**Important !**

1. `File > Build Settings`
2. Ajouter votre scène principale
3. Ajouter toutes vos scènes de mini-jeux
4. Remplacer `"MainScene"` dans BaseEventManager par le nom exact de votre scène

---

## 🔄 FLUX D'EXÉCUTION

```
Jour 5 Matin arrive
    ↓
GameManager.EndHalfDay()
    ↓
EventScheduler.CheckAndTriggerEvent()
    ↓
Événement trouvé au jour 5
    ↓
UIManager.ShowEventPanel() → Affiche l'image + description
    ↓
Joueur clique sur "Commencer"
    ↓
BaseGameEvent.StartEvent() → Charge la scène du mini-jeu
    ↓
EventScheduler calcule : fin = Jour 6 Aprem (durée 3)
    ↓
--- Le joueur joue au mini-jeu ---
    ↓
Mini-jeu terminé → HarvestFestivalManager.CompleteEvent()
    ↓
CalculateScore() → score = 85
    ↓
EventScheduler.SetEventScore(85)
    ↓
ReturnToBaseGame() → Retour scène principale
    ↓
--- Jour 6 Matin : événement toujours actif, bloque gameplay ---
--- Jour 6 Aprem : événement toujours actif, bloque gameplay ---
--- Jour 7 Matin : HasEventEnded() = true ---
    ↓
EventScheduler.EndCurrentEvent()
    ↓
BaseGameEvent.ApplyRewards(85) → +50 nourriture
    ↓
Gameplay normal reprend
```

---

## 📝 CHECKLIST RAPIDE

- [ ] EventScheduler configuré dans la scène
- [ ] EventPanel UI créé et assigné dans UIManager
- [ ] Événement ScriptableObject créé
- [ ] Événement ajouté au calendrier (EventScheduler)
- [ ] Scène de mini-jeu créée
- [ ] Manager de mini-jeu créé (hérite de BaseEventManager)
- [ ] Scène ajoutée aux Build Settings
- [ ] Nom de scène principale correct dans ReturnToBaseGame()

---

## 🐛 DEBUGGING

**L'événement ne se déclenche pas :**
- Vérifier que EventScheduler est assigné dans GameManager
- Vérifier que le jour est correct (1-28)
- Vérifier que le GameEvent est assigné dans Scheduled Events

**Le panel ne s'affiche pas :**
- Vérifier que EventPanel et ses enfants sont assignés dans UIManager
- Vérifier que le Sprite eventImage est bien assigné dans le SO

**Le mini-jeu ne charge pas :**
- Vérifier que sceneName est correct (sensible à la casse)
- Vérifier que la scène est dans Build Settings
- Regarder les logs de Debug.Log

**Les récompenses ne s'appliquent pas :**
- Vérifier que CompleteEvent() est bien appelé
- Vérifier que CalculateScore() retourne un score
- Vérifier que ApplyRewards() modifie bien les stats

---

## 🎯 EXEMPLE COMPLET

Voir **ExampleHarvestFestivalEvent.cs** et **ExampleClickerEventManager.cs** pour un exemple fonctionnel.

Bon développement ! 🚀
