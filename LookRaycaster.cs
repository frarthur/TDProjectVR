using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class LookRaycaster : MonoBehaviour
{
    public float maxDistance = 50f;
    public LayerMask enemyLayer;
    public Camera cam;
    private GameObject currentTarget;
    private EnemyHealth currentEnemy;
    public bool tapBoost = true;
    public bool debugMode = true; // Active les logs de debug
    public bool drawDebugRay = true; // Dessine le rayon en mode Scene
    private ReticleUI reticle;

    void Start()
    {
        Debug.Log("[LookRaycaster] Start() appelé sur " + gameObject.name);
        
        if (cam == null) 
        {
            cam = Camera.main;
            Debug.Log("[LookRaycaster] Camera assignée: " + (cam != null ? cam.name : "NULL"));
        }
        
        // Créer un réticule simple directement
        CreateSimpleReticle();
    }
    
    void CreateSimpleReticle()
    {
        Debug.Log("[LookRaycaster] CreateSimpleReticle() démarré");
        
        try
        {
            // TOUJOURS créer un nouveau Canvas Screen Space Overlay pour le réticule
            // (ignore les Canvas World Space existants)
            Canvas canvas = null;
            
            // Chercher un Canvas Screen Space Overlay existant
            Canvas[] allCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in allCanvas)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvas = c;
                    Debug.Log("[LookRaycaster] Canvas Screen Space Overlay trouvé: " + canvas.name);
                    break;
                }
            }
            
            // Si aucun Canvas Overlay n'existe, en créer un
            if (canvas == null)
            {
                Debug.Log("[LookRaycaster] Création d'un nouveau Canvas Screen Space Overlay...");
                GameObject canvasGO = new GameObject("ReticleCanvas_Overlay");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100; // Au-dessus de tout
                
                UnityEngine.UI.CanvasScaler scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
                Debug.Log("[LookRaycaster] Canvas Overlay créé: " + canvasGO.name);
            }
            
            // Créer le GameObject du réticule
            GameObject reticleGO = new GameObject("Reticle_Crosshair");
            reticleGO.transform.SetParent(canvas.transform, false);
            Debug.Log("[LookRaycaster] GameObject réticule créé");
            
            // Ajouter l'Image
            UnityEngine.UI.Image img = reticleGO.AddComponent<UnityEngine.UI.Image>();
            img.color = Color.white;
            Debug.Log("[LookRaycaster] Image component ajouté");
            
            // Configurer position et taille
            RectTransform rt = img.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(50f, 50f); // Plus grand pour être sûr de le voir
            Debug.Log("[LookRaycaster] RectTransform configuré - Taille: 50x50 au centre");
            
            // Créer texture simple (cercle avec croix)
            Texture2D tex = new Texture2D(64, 64);
            Color transparent = new Color(1, 1, 1, 0);
            Color white = Color.white;
            
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    // Croix au centre
                    if ((x >= 30 && x <= 34 && y >= 10 && y <= 54) || 
                        (y >= 30 && y <= 34 && x >= 10 && x <= 54))
                    {
                        tex.SetPixel(x, y, white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }
            tex.Apply();
            
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
            img.sprite = sprite;
            Debug.Log("[LookRaycaster] Sprite créé et assigné");
            
            // Garder référence
            reticle = reticleGO.AddComponent<ReticleUI>();
            reticle.reticleImage = img;
            
            Debug.Log("✓✓✓ [LookRaycaster] RETICULE CRÉÉ ET VISIBLE! ✓✓✓");
            Debug.Log($"Hiérarchie: {canvas.name} -> {reticleGO.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[LookRaycaster] ERREUR lors de la création du réticule: " + e.Message);
            Debug.LogError(e.StackTrace);
        }
    }

    void Update()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        bool tapped = false;
        if (Touchscreen.current != null)
        {
            // primary touch began this frame
            tapped |= Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        }
        if (Mouse.current != null)
        {
            tapped |= Mouse.current.leftButton.wasPressedThisFrame;
        }

        // Debug visuel du rayon
        if (drawDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.yellow);
        }

        if (Physics.Raycast(ray, out hit, maxDistance, enemyLayer))
        {
            // Visualiser le point d'impact
            if (drawDebugRay)
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }
            
            GameObject go = hit.collider.gameObject;
            EnemyHealth eh = go.GetComponentInParent<EnemyHealth>();
            if (eh != null)
            {
                if (currentEnemy != eh && debugMode)
                {
                    Debug.Log($"[LookRaycaster] Visant ennemi: {eh.gameObject.name}, HP: {eh.hp:F1}/{eh.maxHP}");
                }
                currentEnemy = eh;
                currentEnemy.ApplyLookDamage(Time.deltaTime, tapped && tapBoost);
                
                // Réticule rouge = visant un ennemi
                if (reticle != null) reticle.SetTargeting(true);
            }
            else if (debugMode)
            {
                Debug.LogWarning($"[LookRaycaster] Raycast a touché '{go.name}' (layer {LayerMask.LayerToName(go.layer)}) mais pas de EnemyHealth trouvé!");
            }
        }
        else
        {
            currentEnemy = null;
            if (reticle != null) reticle.SetTargeting(false);
            
            // Debug: rien n'a été touché
            if (drawDebugRay && debugMode && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[LookRaycaster] Aucun ennemi visé. LayerMask: {enemyLayer.value}");
            }
        }
    }
}
