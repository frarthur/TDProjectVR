using UnityEngine;
using UnityEngine.UI;

public class ReticleUI : MonoBehaviour
{
    public Image reticleImage;
    public Color normalColor = Color.white;
    public Color targetingColor = Color.red;
    
    void Start()
    {
        if (reticleImage == null)
        {
            // Créer automatiquement un réticule simple
            CreateSimpleReticle();
        }
    }
    
    public void SetTargeting(bool isTargeting)
    {
        if (reticleImage != null)
        {
            reticleImage.color = isTargeting ? targetingColor : normalColor;
        }
    }
    
    void CreateSimpleReticle()
    {
        // Créer un Canvas s'il n'existe pas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("ReticleCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Créer le réticule
        GameObject reticleGO = new GameObject("Reticle");
        reticleGO.transform.SetParent(canvas.transform, false);
        
        reticleImage = reticleGO.AddComponent<Image>();
        reticleImage.color = normalColor;
        
        // Position au centre
        RectTransform rt = reticleGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(20f, 20f);
        
        // Créer une texture simple (croix)
        Texture2D tex = new Texture2D(32, 32);
        Color transparent = new Color(1, 1, 1, 0);
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                if (x == 16 || y == 16)
                    tex.SetPixel(x, y, Color.white);
                else
                    tex.SetPixel(x, y, transparent);
            }
        }
        tex.Apply();
        
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        reticleImage.sprite = sprite;
        
        Debug.Log("Réticule créé automatiquement au centre de l'écran");
    }
}
