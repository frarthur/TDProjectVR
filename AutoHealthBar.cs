using UnityEngine;
using UnityEngine.UI;

// Attache ce script au prefab Enemy pour créer automatiquement une barre de vie
public class AutoHealthBar : MonoBehaviour
{
    public Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    public Vector2 healthBarSize = new Vector2(100f, 15f);
    private Slider slider;
    private Canvas canvas;

    void Awake()
    {
        Debug.Log($"[AutoHealthBar] Awake() sur '{gameObject.name}'");
        CreateHealthBar();
    }

    void CreateHealthBar()
    {
        Debug.Log($"[AutoHealthBar] CreateHealthBar() démarré pour '{gameObject.name}'");
        
        // Créer le Canvas enfant
        GameObject canvasGO = new GameObject("HealthBar_Canvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = healthBarOffset;
        canvasGO.transform.localRotation = Quaternion.identity;
        canvasGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        canvasGO.layer = gameObject.layer;
        
        Debug.Log($"[AutoHealthBar] Canvas créé: '{canvasGO.name}', Parent: '{canvasGO.transform.parent.name}', LocalPos: {healthBarOffset}");

        // Configurer le Canvas
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        // Ajouter le script pour que ça regarde la caméra
        HealthBarFollower follower = canvasGO.AddComponent<HealthBarFollower>();

        // Créer le Background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Gris foncé
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.sizeDelta = healthBarSize;

        // Créer le Slider
        GameObject sliderGO = new GameObject("HealthSlider");
        sliderGO.transform.SetParent(canvasGO.transform, false);
        slider = sliderGO.AddComponent<Slider>();
        RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.sizeDelta = healthBarSize;

        // Fill Area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        // Fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0, 1, 0, 0.9f); // Vert
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        slider.fillRect = fillRect;
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;
        
        Debug.Log($"[AutoHealthBar] Slider configuré: min={slider.minValue}, max={slider.maxValue}, value={slider.value}");

        // Assigner à EnemyHealth s'il existe
        EnemyHealth eh = GetComponent<EnemyHealth>();
        if (eh != null)
        {
            eh.hpSlider = slider;
            slider.maxValue = eh.maxHP;
            slider.value = eh.hp;
            Debug.Log($"✓✓✓ [AutoHealthBar] BARRE CRÉÉE pour '{gameObject.name}' - HP: {eh.hp}/{eh.maxHP} ✓✓✓");
        }
        else
        {
            Debug.LogWarning($"[AutoHealthBar] Pas de EnemyHealth trouvé sur '{gameObject.name}'");
        }
        
        Debug.Log($"[AutoHealthBar] Position Canvas dans le monde: {canvasGO.transform.position}");
        Debug.Log($"[AutoHealthBar] Hiérarchie complète: {gameObject.name} -> {canvasGO.name} -> Slider");
    }

    public Slider GetSlider()
    {
        return slider;
    }
}
