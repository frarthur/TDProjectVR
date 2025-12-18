using UnityEngine;

// Attache ce script à la barre de vie (Canvas) pour qu'elle suive l'ennemi et regarde toujours la caméra
public class HealthBarFollower : MonoBehaviour
{
    private Transform cam;
    
    void Start()
    {
        cam = Camera.main?.transform;
        if (cam == null)
        {
            Debug.LogWarning("HealthBarFollower: Camera.main introuvable.", this);
        }
        
        // Configuration initiale pour s'assurer que la barre est visible
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            Debug.Log("HealthBarFollower: Canvas configuré en WorldSpace");
        }
    }
    
    void LateUpdate()
    {
        if (cam != null)
        {
            // La barre regarde toujours la caméra (billboard effect)
            transform.LookAt(transform.position + cam.forward);
        }
    }
}
