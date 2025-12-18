# VR Tower Defense - Documentation Complète

## 📋 Vue d'ensemble du jeu

**VR Tower Defense** est un jeu de défense de tour en réalité virtuelle développé avec Unity. Le joueur doit protéger son château contre des vagues d'ennemis en les regardant pour leur infliger des dégâts. Le jeu utilise le regard (gaze-based interaction) comme mécanisme principal de combat.

### Concept de base
- 🏰 **Objectif** : Défendre votre château contre les ennemis
- 👁️ **Contrôle** : Regardez les ennemis pour leur infliger des dégâts
- 📱 **Support** : Conçu pour la VR mobile (Google Cardboard, etc.)
- 🎮 **Difficulté** : Trois niveaux + mode personnalisé

---

## 🎯 Mécaniques de jeu

### Comment jouer
1. **Regarder les ennemis** : Pointez votre regard (centre de l'écran) sur un ennemi pour lui infliger des dégâts continus
2. **Tap pour booster** : Tapez sur l'écran pour tripler les dégâts temporairement
3. **Protéger le château** : Empêchez les ennemis d'atteindre votre château
4. **Game Over** : La partie se termine quand le château n'a plus de vie

### Système de dégâts
- **Dégâts du joueur** : Inflligés par seconde en regardant l'ennemi (ajustable selon la difficulté)
- **Dégâts des ennemis** : Infligés au château quand un ennemi l'atteint
- **Multiplicateur de tap** : ×3 dégâts quand vous tapez l'écran

---

## 🗂️ Architecture du code

### Scripts principaux

#### 1. **GameManager.cs** 
Gère l'état global du jeu et coordonne les autres systèmes.

**Responsabilités** :
- Singleton pour accès global
- Gestion des spawners d'ennemis
- Gestion du Game Over
- Retour au menu principal
- Nettoyage des ennemis

**Fonctions clés** :
```csharp
- Awake() : Initialise le singleton
- Start() : Configure les spawners et le HUD
- GameOver() : Arrête le jeu et nettoie la scène
- ReturnToMenu() : Retourne au menu principal
```

**Points importants** :
- Pause le jeu avec `Time.timeScale = 0f`
- Arrête tous les spawners actifs
- Détruit tous les ennemis restants
- Désactive les raycasters pendant le menu

---

#### 2. **GameSettings.cs**
Singleton contenant tous les paramètres de difficulté du jeu.

**Paramètres configurables** :
- `castleMaxHP` : Points de vie maximum du château (5-50)
- `enemyMaxHP` : Points de vie des ennemis (20-150)
- `playerDamagePerSecond` : Dégâts infligés par seconde (50-300)
- `enemyDamageToCastle` : Dégâts des ennemis au château (1-5)
- `spawnRateMin/Max` : Intervalle entre les spawns (1-20 secondes)

**Niveaux de difficulté** :

| Difficulté | Vie Château | Vie Ennemis | Dégâts Joueur/s | Dégâts Ennemis | Spawn (s) |
|------------|-------------|-------------|-----------------|----------------|-----------|
| **Facile** | 20 | 30 | 200 | 1 | 8-18 |
| **Normal** | 10 | 50 | 150 | 1 | 6-15 |
| **Difficile** | 5 | 80 | 100 | 2 | 3-8 |
| **Personnalisé** | Variable | Variable | Variable | Variable | Variable |

**Fonction clé** :
```csharp
SetDifficulty(Difficulty diff) : Change tous les paramètres selon la difficulté choisie
```

---

#### 3. **EnemySpawner.cs**
Fait apparaître des ennemis à intervalles aléatoires.

**Fonctionnement** :
1. Vérifie qu'un prefab d'ennemi est assigné
2. Démarre une coroutine de spawn si `active = true`
3. Attend un temps aléatoire (basé sur `GameSettings`)
4. Instancie un ennemi à sa position
5. Répète le cycle

**Fonctions clés** :
```csharp
- Start() : Initialise et démarre le spawning
- StartSpawning() : Lance la coroutine de spawn
- SpawnLoop() : Coroutine qui spawn les ennemis
- StopSpawner() : Arrête le spawning
```

**Points importants** :
- Utilise `spawnRateMin` et `spawnRateMax` de GameSettings
- Peut être activé/désactivé via `active`
- Affiche des warnings si le prefab n'est pas assigné

---

#### 4. **EnemyMovement.cs**
Contrôle le déplacement des ennemis vers le château.

**Comportement** :
- **Déplacement principal** : Se dirige vers le château
- **Mouvement ondulant** : Ajoute une sinusoïde latérale pour un mouvement naturel
- **Collision** : Inflige des dégâts au château au contact et se détruit

**Paramètres** :
- `speed` : Vitesse de déplacement (défaut 1.5)
- `waveAmplitude` : Amplitude de l'ondulation (défaut 0.5)
- `waveFrequency` : Fréquence de l'ondulation (défaut 3.0)

**Algorithme de mouvement** :
```csharp
1. Direction principale → Vers le château
2. Direction perpendiculaire → Calculée en 2D (XZ)
3. Ondulation → sin(temps × fréquence) × amplitude
4. Direction finale → principale + perpendiculaire × ondulation
```

**OnTriggerEnter** :
- Détecte la collision avec le château (tag "Castle")
- Inflige les dégâts configurés dans GameSettings
- Se détruit après l'attaque

---

#### 5. **EnemyHealth.cs**
Gère les points de vie des ennemis et les dégâts reçus.

**Système de santé** :
- Points de vie initialisés depuis `GameSettings`
- Barre de vie en World Space automatiquement créée
- Réduction des HP selon les dégâts du joueur

**Fonctions clés** :
```csharp
- Start() : Initialise la vie et la barre de santé
- TakeDamage(float amount) : Réduit les HP
- Die() : Détruit l'ennemi quand HP ≤ 0
- UpdateSlider() : Met à jour la barre de vie visuelle
```

**Système auto-génération** :
- Si aucune barre de vie n'est assignée, le script `AutoHealthBar` est ajouté
- Crée automatiquement un Canvas World Space avec Slider
- Utilise `HealthBarFollower` pour orienter la barre vers la caméra

**Intégration du regard** :
- Reçoit des dégâts via `TakeDamage()` appelé par `LookRaycaster`
- Multiplicateur de dégâts si le joueur tape l'écran

---

#### 6. **CastleHealth.cs**
Gère les points de vie du château et le Game Over.

**Fonctionnement** :
1. Initialise la vie depuis `GameSettings`
2. Affiche une barre de vie UI
3. Reçoit des dégâts des ennemis
4. Déclenche le Game Over quand HP ≤ 0

**Fonctions clés** :
```csharp
- Start() : Initialise la vie et le slider
- TakeDamage(int dmg) : Réduit les HP du château
- OnDestroyed() : Déclenche le Game Over
- CreateGameOverUI() : Crée l'écran de fin automatiquement
- ApplyMaxFromSettings() : Applique les paramètres de difficulté
```

**Game Over** :
- Arrête tous les spawners via `GameManager`
- Affiche un écran "GAME OVER" en rouge
- Canvas Overlay avec ordre de tri élevé (1000)

---

#### 7. **LookRaycaster.cs**
Le cœur du système de combat - détecte où le joueur regarde et inflige des dégâts.

**Mécanisme de raycast** :
1. Lance un rayon depuis le centre de la caméra
2. Détecte les ennemis touchés (LayerMask)
3. Inflige des dégâts continus par frame
4. Multiplie les dégâts si le joueur tape l'écran

**Paramètres** :
- `maxDistance` : Distance max du raycast (défaut 50m)
- `enemyLayer` : Layer des ennemis à détecter
- `tapBoost` : Active/désactive le multiplicateur de tap
- `lookMultiplierOnTap` : Multiplicateur de dégâts au tap (×3)

**Fonctions clés** :
```csharp
- Update() : Lance le raycast à chaque frame
- HandleInput() : Détecte les taps (tactile ou souris)
- CreateSimpleReticle() : Crée le réticule au centre de l'écran
```

**Système de réticule** :
- Canvas Screen Space Overlay
- Croix blanche au centre de l'écran
- Change de couleur (rouge) quand il cible un ennemi
- Texture générée procéduralement

**Support des entrées** :
- New Input System (Touchscreen)
- Old Input System (Mouse)
- Compatible VR et PC

---

#### 8. **MainMenu.cs**
Crée et gère le menu principal avec les paramètres de jeu.

**Structure UI** :
- **Menu principal** : Boutons JOUER, PARAMÈTRES, QUITTER
- **Panel paramètres** : Boutons de difficulté + sliders personnalisés
- Design en overlay avec fond semi-transparent

**Fonctions principales** :
```csharp
- Start() : Crée GameSettings et le menu, met le jeu en pause
- CreateMainMenu() : Construit l'interface du menu
- CreateSettingsPanel() : Construit le panel des paramètres
- StartGame() : Lance la partie
- ToggleSettings() : Affiche/cache les paramètres
- SetDifficulty() : Change la difficulté
```

**Paramètres ajustables** :
- Vie du Château (5-50)
- Vie des Ennemis (20-150)
- Dégâts du Joueur (50-300/s)
- Dégâts des Ennemis (1-5)
- Vitesse de Spawn (1-20s)

**Workflow de démarrage** :
1. Crée `GameSettings` si inexistant
2. Met `Time.timeScale = 0` (pause)
3. Désactive tous les `EnemySpawner`
4. Affiche le menu
5. Au clic sur JOUER : Reprend le temps, active les spawners, cache le menu

---

#### 9. **CastleHUD.cs**
Affiche l'interface de jeu (vie du château, bouton retour).

**Éléments UI** :
- **Texte de vie** : Affiche "Château: X/Y" en bas à droite
- **Bouton Retour** : Permet de revenir au menu en haut à droite

**Fonctions** :
```csharp
- Awake() : Crée le canvas et les éléments UI
- Update() : Met à jour l'affichage de la vie
- CreateExitButton() : Crée le bouton de retour au menu
```

**Bouton de retour** :
- Appelle `GameManager.ReturnToMenu()`
- Visible en permanence pendant la partie
- Style : Texte blanc, bordure, fond semi-transparent

---

#### 10. **AutoHealthBar.cs**
Crée automatiquement une barre de vie World Space pour les ennemis.

**Processus** :
1. Crée un Canvas World Space
2. Ajoute un Slider UI
3. Configure les couleurs (rouge/vert)
4. Positionne au-dessus de l'ennemi
5. Ajoute `HealthBarFollower` pour orientation caméra

**Configuration** :
- Canvas à 0.5 unités au-dessus de l'ennemi
- Taille : 1×0.15 unités
- Scale : 0.01 pour visibilité optimale
- Mode World Space avec caméra assignée

---

#### 11. **HealthBarFollower.cs**
Oriente la barre de vie pour qu'elle face toujours la caméra.

**Fonctionnement** :
```csharp
- LateUpdate() : Fait tourner le canvas vers la caméra
- Algorithme : transform.LookAt(camera) + rotation inverse
```

**Pourquoi LateUpdate ?** :
- S'exécute après tous les Update()
- Garantit que la caméra a terminé son mouvement
- Évite les saccades visuelles

---

#### 12. **ReticleUI.cs**
Gère le changement de couleur du réticule selon la cible.

**États** :
- **Blanc** : Pas de cible
- **Rouge** : Cible un ennemi

**Fonction** :
```csharp
SetTargeting(bool isTargeting) : Change la couleur du réticule
```

---

#### 13. **GyroCamera.cs**
Active le contrôle gyroscopique pour la VR mobile.

**Fonctionnement** :
- Détecte si le gyroscope est disponible
- Active la rotation de la caméra selon l'orientation du téléphone
- Compatible avec Google Cardboard et autres VR mobiles

---

#### 14. **DiagnosticVR.cs**
Affiche des informations de debug pour le développement VR.

**Informations affichées** :
- FPS (images par seconde)
- État du gyroscope
- Orientation du téléphone
- Position de la caméra

---

## 🎮 Flow du jeu

### Séquence de démarrage
```
1. Unity charge la scène
   ↓
2. MainMenu.Start()
   - Crée GameSettings
   - Met Time.timeScale = 0 (pause)
   - Désactive tous les EnemySpawner
   - Affiche le menu
   ↓
3. Joueur clique sur JOUER
   ↓
4. MainMenu.StartGame()
   - Time.timeScale = 1 (reprend)
   - Active tous les EnemySpawner
   - Cache le menu
   - Active les LookRaycaster
   ↓
5. La partie commence
```

### Boucle de gameplay
```
[EnemySpawner]
    ↓ Spawn ennemi toutes les X secondes
[EnemyMovement]
    ↓ Se déplace vers le château
[LookRaycaster]
    ↓ Joueur regarde l'ennemi
[EnemyHealth]
    ↓ Reçoit des dégâts, HP diminue
    ↓ HP ≤ 0 ?
    └→ Détruit l'ennemi
    
OU

[EnemyMovement]
    ↓ Atteint le château
[CastleHealth]
    ↓ Château prend des dégâts
    ↓ HP ≤ 0 ?
    └→ Game Over
```

### Game Over
```
1. CastleHealth.OnDestroyed()
   ↓
2. Affiche "GAME OVER"
   ↓
3. GameManager.GameOver()
   - Arrête tous les spawners
   - Détruit tous les ennemis
   ↓
4. Joueur clique sur "RETOUR AU MENU"
   ↓
5. GameManager.ReturnToMenu()
   - Pause le jeu
   - Nettoie la scène
   - Réaffiche le menu
```

---

## 🏗️ Structure Unity requise

### Hiérarchie minimale
```
Scene
├── Main Camera
│   └── LookRaycaster.cs
├── GameManager (Empty GameObject)
│   └── GameManager.cs
├── Castle (Tag: "Castle")
│   ├── CastleHealth.cs
│   └── Collider (isTrigger = true)
├── EnemySpawner_1 (Empty GameObject)
│   └── EnemySpawner.cs
└── EnemySpawner_2 (Empty GameObject)
    └── EnemySpawner.cs
```

### Prefab Enemy
```
Enemy
├── EnemyMovement.cs
├── EnemyHealth.cs
├── Rigidbody (isKinematic = true)
├── Collider (isTrigger = true)
└── Model 3D
```

### Layers requis
- **Default** : Objets de base
- **Enemy** : Layer pour les ennemis (pour le raycast)

### Tags requis
- **Castle** : Pour le château
- **MainCamera** : Pour la caméra principale

---

## ⚙️ Configuration dans Unity

### 1. Créer le château
1. Créer un GameObject "Castle"
2. Ajouter un tag "Castle"
3. Ajouter `CastleHealth.cs`
4. Ajouter un Collider (Box, Sphere, etc.)
5. ✅ **Important** : Cocher "Is Trigger"

### 2. Créer le prefab Enemy
1. Créer un GameObject avec modèle 3D
2. Ajouter `EnemyMovement.cs`
3. Ajouter `EnemyHealth.cs`
4. Ajouter `Rigidbody` (cocher isKinematic)
5. Ajouter `Collider` (cocher isTrigger)
6. Mettre sur Layer "Enemy"
7. Sauvegarder comme Prefab

### 3. Configurer les spawners
1. Créer des GameObjects vides aux positions de spawn
2. Ajouter `EnemySpawner.cs`
3. Glisser le prefab Enemy dans le champ `enemyPrefab`
4. Cocher `active = true`

### 4. Configurer la caméra
1. Sélectionner Main Camera
2. Ajouter `LookRaycaster.cs`
3. Créer un Layer "Enemy"
4. Assigner le LayerMask dans l'inspecteur
5. Optionnel : Ajouter `GyroCamera.cs` pour VR mobile

### 5. GameManager
1. Créer un GameObject vide "GameManager"
2. Ajouter `GameManager.cs`

---

## 🎨 Customisation

### Changer la difficulté par défaut
Dans `GameSettings.cs` :
```csharp
public Difficulty currentDifficulty = Difficulty.Facile; // Au lieu de Normal
```

### Modifier les valeurs de difficulté
Dans `GameSettings.SetDifficulty()`, ajustez les valeurs :
```csharp
case Difficulty.Normal:
    castleMaxHP = 15;      // Au lieu de 10
    enemyMaxHP = 60f;      // Au lieu de 50
    // etc.
```

### Changer l'apparence du réticule
Dans `LookRaycaster.CreateSimpleReticle()`, modifiez :
```csharp
rt.sizeDelta = new Vector2(40f, 40f); // Taille
img.color = Color.cyan;                // Couleur
```

### Ajouter un nouveau niveau de difficulté
1. Ajouter dans l'enum :
```csharp
public enum Difficulty { Facile, Normal, Difficile, Extreme, Personnalisé }
```

2. Ajouter un case dans `SetDifficulty()` :
```csharp
case Difficulty.Extreme:
    castleMaxHP = 3;
    enemyMaxHP = 120f;
    // etc.
```

3. Ajouter un bouton dans `MainMenu.CreateSettingsPanel()`

---

## 🐛 Debugging

### Problèmes fréquents

**Les ennemis ne prennent pas de dégâts**
- ✅ Vérifier que les ennemis sont sur le Layer "Enemy"
- ✅ Vérifier que le LayerMask de LookRaycaster inclut "Enemy"
- ✅ Activer `debugMode = true` dans LookRaycaster

**Les ennemis traversent le château sans faire de dégâts**
- ✅ Vérifier que le château a le tag "Castle"
- ✅ Vérifier que les Colliders sont en mode `isTrigger = true`
- ✅ Vérifier la méthode `OnTriggerEnter` dans EnemyMovement

**Le réticule n'apparaît pas**
- ✅ Vérifier qu'il y a un Canvas Screen Space Overlay
- ✅ Activer `drawDebugRay = true` dans LookRaycaster
- ✅ Vérifier la console pour les logs de création

**Le jeu démarre en pause**
- ✅ C'est normal ! Cliquez sur "JOUER" dans le menu
- ✅ Vérifier que `MainMenu.StartGame()` met `Time.timeScale = 1`

**Les barres de vie n'apparaissent pas**
- ✅ Vérifier qu'AutoHealthBar.cs est sur le prefab ou ajouté automatiquement
- ✅ Vérifier que la caméra est assignée dans le Canvas

### Mode Debug
Activer les logs détaillés :
```csharp
LookRaycaster.debugMode = true;
LookRaycaster.drawDebugRay = true;
```

---

## 📝 Notes techniques

### Performance
- Les barres de vie utilisent World Space Canvas (attention aux performances)
- Le raycast s'exécute chaque frame (optimisé avec LayerMask)
- Les spawners utilisent des coroutines (non bloquant)

### Compatibilité
- ✅ Unity 2021.3+
- ✅ New Input System et Old Input System
- ✅ VR Mobile (Cardboard, Gear VR)
- ✅ PC (pour testing)
- ✅ Android / iOS

### Dépendances
- Unity UI (uGUI)
- Unity Input System (optionnel)
- TextMeshPro (optionnel pour meilleur rendu texte)

---

## 🚀 Améliorations futures

### Gameplay
- [ ] Système de power-ups
- [ ] Différents types d'ennemis
- [ ] Système de vagues (rounds)
- [ ] Boss fights
- [ ] Score et high scores

### Technique
- [ ] Object pooling pour les ennemis (performance)
- [ ] Système de sauvegarde
- [ ] Effets sonores et musique
- [ ] Effets de particules (explosions, impacts)
- [ ] Animations des ennemis

### UI/UX
- [ ] Tutoriel interactif
- [ ] Statistiques de fin de partie
- [ ] Leaderboard en ligne
- [ ] Paramètres audio
- [ ] Localisation multilingue

---

## 👨‍💻 Auteur

Projet développé pour la VR mobile avec Unity.

---

## 📄 Licence

Projet personnel - Libre d'utilisation et de modification.

---

## 🆘 Support

En cas de problème :
1. Vérifier la section **Debugging**
2. Activer les logs dans la console Unity
3. Vérifier que tous les scripts sont correctement assignés
4. Vérifier les tags et layers

---

*Dernière mise à jour : Décembre 2025*
