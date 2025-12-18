using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public bool active = true;

    public Coroutine spawnRoutine;  // Public pour MainMenu

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: 'enemyPrefab' n'est pas assigné. Désactivation du spawner.", this);
            active = false;
            return;
        }
        
        // Ne démarrer que si active
        if (active)
        {
            StartSpawning();
        }
    }
    
    public void StartSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }
        spawnRoutine = StartCoroutine(SpawnLoop());
        Debug.Log($"[EnemySpawner] '{gameObject.name}' démarré");
    }

    IEnumerator SpawnLoop()
    {
        while (active)
        {
            // Utiliser les spawn rates de GameSettings
            float minSpawn = GameSettings.Instance != null ? GameSettings.Instance.spawnRateMin : 6f;
            float maxSpawn = GameSettings.Instance != null ? GameSettings.Instance.spawnRateMax : 15f;
            float wait = Random.Range(minSpawn, maxSpawn);
            
            yield return new WaitForSeconds(wait);
            if (enemyPrefab == null)
            {
                Debug.LogError("EnemySpawner: 'enemyPrefab' est devenu null. Arrêt du spawner.", this);
                active = false;
                yield break;
            }
            Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        }
    }

    public void StopSpawner()
    {
        active = false;
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;  // Reset pour permettre un redémarrage
        }
    }

    void OnValidate()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: Assignez le prefab d'ennemi dans l'inspecteur.", this);
        }
    }
}
