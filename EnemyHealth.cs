using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHP = 50f;
    public float hp;
    public Slider hpSlider; // assign in prefab (world-space canvas)
    public float lookDamagePerSecond = 150f;  // 10x pour ~10 dégâts par frame
    public float lookMultiplierOnTap = 3f;
    
    private bool settingsApplied = false;

    void OnValidate()
    {
        // S'assurer qu'il y a un collider pour le raycast
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"EnemyHealth sur '{gameObject.name}': Ajoute un Collider pour le raycast!", this);
        }
    }

    void Start()
    {
        // Appliquer les paramètres une seule fois
        if (!settingsApplied && GameSettings.Instance != null)
        {
            maxHP = GameSettings.Instance.enemyMaxHP;
            lookDamagePerSecond = GameSettings.Instance.playerDamagePerSecond;
            settingsApplied = true;
        }
        
        Debug.Log($"[EnemyHealth] Start() sur '{gameObject.name}' - HP: {maxHP}");
        hp = maxHP;
        
        // Auto-créer la barre si pas de slider assigné
        if (hpSlider == null)
        {
            Debug.Log($"[EnemyHealth] Pas de slider assigné, recherche...");
            // D'abord chercher un slider existant
            hpSlider = GetComponentInChildren<Slider>();
            
            // Si toujours rien, créer automatiquement
            if (hpSlider == null)
            {
                Debug.Log($"[EnemyHealth] Aucun slider trouvé, création automatique...");
                AutoHealthBar autoBar = GetComponent<AutoHealthBar>();
                if (autoBar == null)
                {
                    Debug.Log($"[EnemyHealth] Ajout du composant AutoHealthBar...");
                    autoBar = gameObject.AddComponent<AutoHealthBar>();
                }
                else
                {
                    Debug.Log($"[EnemyHealth] AutoHealthBar déjà présent");
                }
                // Attendre un frame pour que AutoHealthBar crée le slider
                StartCoroutine(DelayedSliderSetup());
            }
            else
            {
                Debug.Log($"[EnemyHealth] Slider trouvé sur '{gameObject.name}'");
                Canvas canvas = hpSlider.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    Debug.Log($"[EnemyHealth] Canvas trouvé: '{canvas.name}', RenderMode: {canvas.renderMode}, WorldPos: {canvas.transform.position}");
                }
                UpdateSlider();
            }
        }
        else
        {
            Debug.Log($"[EnemyHealth] Slider déjà assigné");
            UpdateSlider();
        }
    }
    
    System.Collections.IEnumerator DelayedSliderSetup()
    {
        yield return null; // Attendre 1 frame
        hpSlider = GetComponentInChildren<Slider>();
        if (hpSlider != null)
        {
            UpdateSlider();
            Debug.Log($"EnemyHealth: Barre auto-créée pour '{gameObject.name}'");
        }
    }
    
    void UpdateSlider()
    {
        if (hpSlider) 
        { 
            hpSlider.maxValue = maxHP; 
            hpSlider.value = hp;
            
            // Forcer le refresh visuel
            if (hpSlider.fillRect != null)
            {
                hpSlider.fillRect.GetComponent<UnityEngine.UI.Image>()?.SetNativeSize();
            }
            
            // Changer la couleur en fonction de la vie (vert -> jaune -> rouge)
            if (hpSlider.fillRect != null)
            {
                UnityEngine.UI.Image fillImg = hpSlider.fillRect.GetComponent<UnityEngine.UI.Image>();
                if (fillImg != null)
                {
                    float hpPercent = hp / maxHP;
                    if (hpPercent > 0.5f)
                        fillImg.color = Color.green;
                    else if (hpPercent > 0.25f)
                        fillImg.color = Color.yellow;
                    else
                        fillImg.color = Color.red;
                }
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        UpdateSlider();
        if (hp <= 0) Die();
    }

    void Die()
    {
        // optional: particle, sound
        Destroy(gameObject);
        // notify score etc.
    }

    // called each frame when looked at
    public void ApplyLookDamage(float deltaTime, bool tapped)
    {
        float dmg = lookDamagePerSecond * deltaTime;
        if (tapped) dmg *= lookMultiplierOnTap;
        TakeDamage(dmg);
        
        // Log occasionnel (tous les 0.5s environ)
        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"[EnemyHealth] '{gameObject.name}' prend {dmg:F1} dégâts (HP: {hp:F1}/{maxHP})");
        }
    }
}
