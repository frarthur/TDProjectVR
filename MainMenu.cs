using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private Canvas canvas;
    private GameObject settingsPanel;
    private GameObject settingsInnerPanel;
    private GameObject mainMenuPanel;
    private bool settingsOpen = false;

    void Start()
    {
        // Créer GameSettings AVANT tout le reste
        if (GameSettings.Instance == null)
        {
            GameObject settingsGO = new GameObject("GameSettings");
            settingsGO.AddComponent<GameSettings>();
            Debug.Log("[MainMenu] GameSettings créé");
        }
        
        // Mettre le jeu en pause jusqu'à ce qu'on clique JOUER
        Time.timeScale = 0f;
        
        // Désactiver tous les spawners
        EnemySpawner[] spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        foreach (var spawner in spawners)
        {
            spawner.active = false;
        }
        
        CreateMainMenu();
        
        Debug.Log("[MainMenu] Jeu en pause, spawners désactivés. Cliquez JOUER pour commencer.");
    }

    void CreateMainMenu()
    {
        // Canvas principal
        GameObject canvasGO = new GameObject("MainMenu_Canvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();

        // Panel du menu principal
        mainMenuPanel = new GameObject("MainMenuPanel");
        mainMenuPanel.transform.SetParent(canvasGO.transform, false);
        RectTransform mainPanelRT = mainMenuPanel.AddComponent<RectTransform>();
        mainPanelRT.anchorMin = Vector2.zero;
        mainPanelRT.anchorMax = Vector2.one;
        mainPanelRT.sizeDelta = Vector2.zero;

        // Fond noir semi-transparent
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(mainMenuPanel.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.8f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Titre
        CreateText(mainMenuPanel.transform, "Title", "VR TOWER DEFENSE", new Vector2(0, 250), 80, Color.white);

        // Boutons
        CreateButton(mainMenuPanel.transform, "PlayButton", "JOUER", new Vector2(0, 50), () => StartGame());
        CreateButton(mainMenuPanel.transform, "SettingsButton", "PARAMÈTRES", new Vector2(0, -50), () => ToggleSettings());
        CreateButton(mainMenuPanel.transform, "QuitButton", "QUITTER", new Vector2(0, -150), () => QuitGame());

        // Panel des paramètres (caché par défaut)
        CreateSettingsPanel(canvasGO.transform);
        
        Debug.Log("[MainMenu] Menu principal créé");
    }

    void CreateSettingsPanel(Transform parent)
    {
        settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(parent, false);
        
        // Fond opaque pour masquer tout ce qu'il y a derrière
        RectTransform panelRect = settingsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        Image bgImg = settingsPanel.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.95f);  // Fond noir quasi-opaque
        
        // Panel des paramètres par-dessus
        settingsInnerPanel = new GameObject("InnerPanel");
        settingsInnerPanel.transform.SetParent(settingsPanel.transform, false);
        
        Image panelImg = settingsInnerPanel.AddComponent<Image>();
        panelImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        
        RectTransform innerRect = settingsInnerPanel.GetComponent<RectTransform>();
        innerRect.anchorMin = new Vector2(0.5f, 0.5f);
        innerRect.anchorMax = new Vector2(0.5f, 0.5f);
        innerRect.pivot = new Vector2(0.5f, 0.5f);
        innerRect.sizeDelta = new Vector2(800, 600);
        
        // Titre Paramètres
        CreateText(settingsInnerPanel.transform, "SettingsTitle", "PARAMÈTRES", new Vector2(0, 250), 50, Color.white);

        // Difficulté
        float yPos = 180;
        CreateText(settingsInnerPanel.transform, "DiffLabel", "Difficulté:", new Vector2(-200, yPos), 30, Color.white);
        CreateButton(settingsInnerPanel.transform, "DiffEasy", "Facile", new Vector2(-100, yPos - 50), () => SetDifficulty(GameSettings.Difficulty.Facile), new Vector2(150, 40));
        CreateButton(settingsInnerPanel.transform, "DiffNormal", "Normal", new Vector2(100, yPos - 50), () => SetDifficulty(GameSettings.Difficulty.Normal), new Vector2(150, 40));
        CreateButton(settingsInnerPanel.transform, "DiffHard", "Difficile", new Vector2(300, yPos - 50), () => SetDifficulty(GameSettings.Difficulty.Difficile), new Vector2(150, 40));

        // Sliders personnalisés (vérifier que GameSettings existe)
        if (GameSettings.Instance != null)
        {
            yPos = 50;
            CreateSlider(settingsInnerPanel.transform, "CastleHP", "Vie Château:", new Vector2(0, yPos), 5, 50, GameSettings.Instance.castleMaxHP,
                (val) => { if (GameSettings.Instance != null) { GameSettings.Instance.castleMaxHP = (int)val; GameSettings.Instance.currentDifficulty = GameSettings.Difficulty.Personnalisé; } });
            
            yPos -= 60;
            CreateSlider(settingsInnerPanel.transform, "EnemyHP", "Vie Ennemis:", new Vector2(0, yPos), 20, 150, GameSettings.Instance.enemyMaxHP,
                (val) => { if (GameSettings.Instance != null) { GameSettings.Instance.enemyMaxHP = val; GameSettings.Instance.currentDifficulty = GameSettings.Difficulty.Personnalisé; } });
            
            yPos -= 60;
            CreateSlider(settingsInnerPanel.transform, "PlayerDmg", "Dégâts Joueur:", new Vector2(0, yPos), 50, 300, GameSettings.Instance.playerDamagePerSecond,
                (val) => { if (GameSettings.Instance != null) { GameSettings.Instance.playerDamagePerSecond = val; GameSettings.Instance.currentDifficulty = GameSettings.Difficulty.Personnalisé; } });
            
            yPos -= 60;
            CreateSlider(settingsInnerPanel.transform, "EnemyDmg", "Dégâts Ennemis:", new Vector2(0, yPos), 1, 5, GameSettings.Instance.enemyDamageToCastle,
                (val) => { if (GameSettings.Instance != null) { GameSettings.Instance.enemyDamageToCastle = (int)val; GameSettings.Instance.currentDifficulty = GameSettings.Difficulty.Personnalisé; } });
            
            yPos -= 60;
            CreateSlider(settingsInnerPanel.transform, "SpawnRate", "Spawn (s):", new Vector2(0, yPos), 1, 20, GameSettings.Instance.spawnRateMin,
                (val) => { if (GameSettings.Instance != null) { GameSettings.Instance.spawnRateMin = val; GameSettings.Instance.currentDifficulty = GameSettings.Difficulty.Personnalisé; } });
        }

        // Bouton Retour
        CreateButton(settingsInnerPanel.transform, "BackButton", "RETOUR", new Vector2(0, -250), () => ToggleSettings());

        settingsPanel.SetActive(false);

        // Appliquer les valeurs visuelles actuelles
        RefreshSettingsUI();
    }

    void CreateText(Transform parent, string name, string text, Vector2 pos, int fontSize, Color color)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);
        
        Text txt = textGO.AddComponent<Text>();
        txt.text = text;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = color;
        
        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(600, fontSize + 20);
    }

    void CreateButton(Transform parent, string name, string text, Vector2 pos, System.Action onClick, Vector2? size = null)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        
        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.8f);
        
        Button btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick());
        
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.6f, 0.9f);
        colors.pressedColor = new Color(0.1f, 0.4f, 0.7f);
        btn.colors = colors;
        
        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size ?? new Vector2(300, 60);
        
        // Texte du bouton
        GameObject txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        Text txt = txtGO.AddComponent<Text>();
        txt.text = text;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 28;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        
        RectTransform txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;
    }

    void CreateSlider(Transform parent, string name, string label, Vector2 pos, float min, float max, float value, System.Action<float> onValueChanged)
    {
        // Label
        CreateText(parent, name + "_Label", label, new Vector2(-200, pos.y), 24, Color.white);
        
        // Slider
        GameObject sliderGO = new GameObject(name);
        sliderGO.transform.SetParent(parent, false);
        
        RectTransform sliderRT = sliderGO.AddComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRT.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRT.anchoredPosition = new Vector2(50, pos.y);
        sliderRT.sizeDelta = new Vector2(300, 30);
        
        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        slider.wholeNumbers = (name.Contains("HP") && name.Contains("Castle")) || name.Contains("EnemyDmg");
        slider.onValueChanged.AddListener((val) => { onValueChanged(val); UpdateValueText(name + "_Value", val); });
        
        // Background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f);
        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;
        
        // Fill Area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRT = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.sizeDelta = new Vector2(-10, -10);
        
        // Fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.7f, 0.3f);
        RectTransform fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.sizeDelta = Vector2.zero;
        
        slider.fillRect = fillRT;
        
        // Value text
        GameObject valueTxtGO = new GameObject(name + "_Value");
        valueTxtGO.transform.SetParent(parent, false);
        Text valueTxt = valueTxtGO.AddComponent<Text>();
        valueTxt.text = value.ToString("F0");
        valueTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        valueTxt.fontSize = 24;
        valueTxt.alignment = TextAnchor.MiddleLeft;
        valueTxt.color = Color.yellow;
        RectTransform valueTxtRT = valueTxtGO.GetComponent<RectTransform>();
        valueTxtRT.anchorMin = new Vector2(0.5f, 0.5f);
        valueTxtRT.anchorMax = new Vector2(0.5f, 0.5f);
        valueTxtRT.anchoredPosition = new Vector2(220, pos.y);
        valueTxtRT.sizeDelta = new Vector2(100, 30);
    }

    void UpdateValueText(string name, float value)
    {
        Text txt = GameObject.Find(name)?.GetComponent<Text>();
        if (txt != null)
        {
            txt.text = value.ToString("F0");
        }
    }

    void SetDifficulty(GameSettings.Difficulty diff)
    {
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.SetDifficulty(diff);
            Debug.Log($"[MainMenu] Difficulté changée: {diff}");
            // Mettre à jour les sliders pour refléter le preset
            RefreshSettingsUI();
        }
        else
        {
            Debug.LogError("[MainMenu] GameSettings.Instance est null!");
        }
    }

    // Met à jour les sliders et leurs textes après changement de difficulté
    void RefreshSettingsUI()
    {
        if (GameSettings.Instance == null || settingsPanel == null) return;
        UpdateSliderValue("CastleHP", GameSettings.Instance.castleMaxHP);
        UpdateSliderValue("EnemyHP", GameSettings.Instance.enemyMaxHP);
        UpdateSliderValue("PlayerDmg", GameSettings.Instance.playerDamagePerSecond);
        UpdateSliderValue("EnemyDmg", GameSettings.Instance.enemyDamageToCastle);
        UpdateSliderValue("SpawnRate", GameSettings.Instance.spawnRateMin);
    }

    void UpdateSliderValue(string name, float value)
    {
        Slider s = null;
        if (settingsInnerPanel != null)
        {
            var t = settingsInnerPanel.transform.Find(name);
            if (t != null) s = t.GetComponent<Slider>();
        }
        if (s == null)
        {
            s = GameObject.Find(name)?.GetComponent<Slider>();
        }
        if (s != null)
        {
            s.value = value;
            UpdateValueText(name + "_Value", value);
        }
    }

    void ToggleSettings()
    {
        settingsOpen = !settingsOpen;
        settingsPanel.SetActive(settingsOpen);
        mainMenuPanel.SetActive(!settingsOpen);  // Masquer le menu principal quand les paramètres sont ouverts
    }

    void StartGame()
    {
        Debug.Log("[MainMenu] Démarrage du jeu...");
        
        // Appliquer les réglages courants au château (max HP depuis GameSettings)
        var castle = FindFirstObjectByType<CastleHealth>();
        if (castle != null)
        {
            castle.ApplyMaxFromSettings(true);
        }
        
        // Réactiver les spawners
        EnemySpawner[] spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        foreach (var spawner in spawners)
        {
            spawner.active = true;
            // Redémarrer la coroutine si nécessaire
            if (spawner.spawnRoutine == null)
            {
                spawner.StartSpawning();
            }
        }
        Debug.Log($"[MainMenu] {spawners.Length} spawner(s) réactivé(s)");
        
        // Réactiver les caméras et composants désactivés
        LookRaycaster[] raycasters = FindObjectsByType<LookRaycaster>(FindObjectsSortMode.None);
        foreach (var r in raycasters)
        {
            r.enabled = true;
        }
        Debug.Log($"[MainMenu] {raycasters.Length} LookRaycaster(s) réactivé(s)");
        
        // Reprendre le temps
        Time.timeScale = 1f;
        
        // Détruire SEULEMENT le menu, pas tout le Canvas
        Destroy(mainMenuPanel);
        Destroy(settingsPanel);
        
        // Garder le Canvas pour le réticule qui est dessus
        // Mais on peut désactiver ce script
        this.enabled = false;
        
        Debug.Log("[MainMenu] Jeu démarré! Spawners activés, temps repris, réticule conservé.");
    }

    void QuitGame()
    {
        Debug.Log("[MainMenu] Quitter le jeu");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // Réaffiche le menu (utilisé par le bouton Menu/Exit)
    public void ShowMenu()
    {
        Debug.Log("[MainMenu] ShowMenu()");
        
        // Créer GameSettings AVANT tout le reste (important pour ShowMenu())
        if (GameSettings.Instance == null)
        {
            GameObject settingsGO = new GameObject("GameSettings");
            settingsGO.AddComponent<GameSettings>();
            Debug.Log("[MainMenu] GameSettings créé dans ShowMenu()");
        }
        
        Time.timeScale = 0f;

        // Si les panels ont été détruits après StartGame, on recrée tout
        bool needRecreate = canvas == null || mainMenuPanel == null || settingsPanel == null;
        if (needRecreate)
        {
            // Nettoyer l'ancien canvas si présent mais sans panels
            if (canvas != null)
            {
                Destroy(canvas.gameObject);
                canvas = null;
            }
            CreateMainMenu();
            return;
        }

        // Sinon on réactive simplement le menu principal
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        settingsOpen = false;
        this.enabled = true;
    }
}
