using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 1.5f;
    public Transform target; // castle
    public float waveAmplitude = 0.5f;  // Amplitude de l'ondulation latérale
    public float waveFrequency = 3f;   // Fréquence de l'ondulation
    
    private Rigidbody rb;
    private float waveTime = 0f;  // Temps pour la sinusoïde

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (target == null && GameObject.FindWithTag("Castle") != null)
            target = GameObject.FindWithTag("Castle").transform;
        
        // Vérifier que le Collider est en isTrigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.Log($"[EnemyMovement] Collider de '{gameObject.name}' mis en isTrigger=true automatiquement");
        }
        
        // Initialiser le temps avec un décalage aléatoire pour que les ennemis ne bougent pas en sync
        waveTime = Random.Range(0f, 6.28f);
    }

    void Update()
    {
        if (target == null) return;
        
        // Direction vers le château
        Vector3 mainDir = (target.position - transform.position).normalized;
        
        // Calculer une direction perpendiculaire (pour l'ondulation latérale)
        Vector3 perp = new Vector3(-mainDir.z, 0f, mainDir.x);  // Perpendiculaire en XZ
        
        // Ondulation sinusoïdale
        float wave = Mathf.Sin(waveTime * waveFrequency) * waveAmplitude;
        waveTime += Time.deltaTime;
        
        // Combiner direction principale + ondulation latérale
        Vector3 finalDir = (mainDir + perp * wave).normalized;
        
        // Avancer
        transform.position += finalDir * speed * Time.deltaTime;
        
        // Regarder vers le château (légèrement vers le haut)
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Castle"))
        {
            CastleHealth ch = other.GetComponent<CastleHealth>();
            if (ch != null)
            {
                int damage = GameSettings.Instance != null ? GameSettings.Instance.enemyDamageToCastle : 1;
                ch.TakeDamage(damage);
                Debug.Log($"[Enemy] '{gameObject.name}' a attaqué le château pour {damage} dégât(s)!");
            }
            else
            {
                Debug.LogWarning($"[Enemy] Collision avec '{other.name}' (tag Castle) mais pas de CastleHealth!");
            }
            Destroy(gameObject);
        }
    }
}
