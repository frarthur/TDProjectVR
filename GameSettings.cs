using UnityEngine;

// Singleton pour stocker les paramètres de jeu
public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    // Enum de difficulté
    public enum Difficulty { Facile, Normal, Difficile, Personnalisé }
    
    [Header("Paramètres de difficulté")]
    public Difficulty currentDifficulty = Difficulty.Normal;
    public int castleMaxHP = 10;
    public float enemyMaxHP = 50f;
    public float playerDamagePerSecond = 150f;
    public int enemyDamageToCastle = 1;
    public float spawnRateMin = 6f;  // Minimum secondes entre spawns
    public float spawnRateMax = 15f; // Maximum secondes entre spawns

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetDifficulty(Difficulty diff)
    {
        currentDifficulty = diff;
        
        switch (diff)
        {
            case Difficulty.Facile:
                castleMaxHP = 20;
                enemyMaxHP = 30f;
                playerDamagePerSecond = 200f;
                enemyDamageToCastle = 1;
                spawnRateMin = 8f;
                spawnRateMax = 18f;
                break;
                
            case Difficulty.Normal:
                castleMaxHP = 10;
                enemyMaxHP = 50f;
                playerDamagePerSecond = 150f;
                enemyDamageToCastle = 1;
                spawnRateMin = 6f;
                spawnRateMax = 15f;
                break;
                
            case Difficulty.Difficile:
                castleMaxHP = 5;
                enemyMaxHP = 80f;
                playerDamagePerSecond = 100f;
                enemyDamageToCastle = 2;
                spawnRateMin = 3f;
                spawnRateMax = 8f;
                break;
                
            case Difficulty.Personnalisé:
                // Garder les valeurs actuelles
                break;
        }
        
        Debug.Log($"[GameSettings] Difficulté: {diff} - Castle HP: {castleMaxHP}, Enemy HP: {enemyMaxHP}, Dégâts joueur: {playerDamagePerSecond}/s, Dégâts ennemis: {enemyDamageToCastle}, Spawn: {spawnRateMin:F1}-{spawnRateMax:F1}s");
    }
}
