using UnityEngine;
using UnityEngine.UI;

public class CastleHealth : MonoBehaviour
{
    public int maxHP = 10;
    public int hp;
    public Slider hpSlider;
    public GameObject gameOverUI;

    void Start()
    {
        // Utiliser les paramètres de GameSettings si disponibles
        if (GameSettings.Instance != null)
        {
            maxHP = GameSettings.Instance.castleMaxHP;
        }
        
        hp = maxHP;
        if (hpSlider) { hpSlider.maxValue = maxHP; hpSlider.value = hp; }
        
        Debug.Log($"[CastleHealth] Château initialisé avec {hp}/{maxHP} HP");
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hpSlider) hpSlider.value = hp;
        Debug.Log($"[CastleHealth] Château prend {dmg} dégâts! HP: {hp}/{maxHP}");
        if (hp <= 0) OnDestroyed();
    }

    void OnDestroyed()
    {
        hp = 0;
        if (hpSlider) hpSlider.value = 0;
        
        Debug.Log("[CastleHealth] ===== GAME OVER =====");
        
        if (gameOverUI)
        {
            gameOverUI.SetActive(true);
            Debug.Log("[CastleHealth] UI Game Over activée");
        }
        else
        {
            Debug.LogWarning("[CastleHealth] Pas de gameOverUI assigné! Créer un message automatiquement...");
            CreateGameOverUI();
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
    
    void CreateGameOverUI()
    {
        // Créer un Canvas Overlay pour le Game Over
        GameObject canvasGO = new GameObject("GameOver_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Créer le texte GAME OVER
        GameObject textGO = new GameObject("GameOverText");
        textGO.transform.SetParent(canvasGO.transform, false);
        
        UnityEngine.UI.Text text = textGO.AddComponent<UnityEngine.UI.Text>();
        text.text = "GAME OVER";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 80;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.red;
        
        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        
        Debug.Log("[CastleHealth] UI Game Over créée automatiquement");
    }

    // Applique la vie max depuis GameSettings (utile au démarrage après réglages)
    public void ApplyMaxFromSettings(bool refill = true)
    {
        if (GameSettings.Instance == null) return;
        maxHP = GameSettings.Instance.castleMaxHP;
        if (refill) hp = maxHP;
        if (hpSlider)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = hp;
        }
        Debug.Log($"[CastleHealth] Paramètres appliqués: {hp}/{maxHP}");
    }
}
