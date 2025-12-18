using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private List<EnemySpawner> spawners = new List<EnemySpawner>();
    public bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    void Start()
    {
        EnemySpawner[] s = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        spawners.AddRange(s);

        // Ensure HUD exists
        if (FindFirstObjectByType<CastleHUD>() == null)
        {
            var go = new GameObject("CastleHUD");
            go.AddComponent<CastleHUD>();
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        foreach (var sp in spawners) sp.StopSpawner();
        // optionally stop all enemies
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (var e in enemies) Destroy(e.gameObject);
        // show UI handled by CastleHealth
    }

    public void ReturnToMenu()
    {
        Debug.Log("[GameManager] ReturnToMenu() appelé");
        // Stop gameplay and clean up
        Time.timeScale = 0f;
        isGameOver = false;

        foreach (var sp in spawners)
        {
            sp.StopSpawner();
            sp.active = false;
        }

        // Clear existing enemies
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (var e in enemies) Destroy(e.gameObject);

        // Disable look raycasters while in menu
        LookRaycaster[] rays = FindObjectsByType<LookRaycaster>(FindObjectsSortMode.None);
        foreach (var r in rays) r.enabled = false;

        // Destroy Game Over Canvas if present
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c.gameObject.name == "GameOver_Canvas")
            {
                Destroy(c.gameObject);
                Debug.Log("[GameManager] Game Over Canvas détruit");
            }
        }

        // Recreate or show the main menu
        MainMenu menu = null;
        var allMenus = Resources.FindObjectsOfTypeAll<MainMenu>();
        if (allMenus != null && allMenus.Length > 0)
        {
            menu = allMenus[0];
        }
        if (menu == null)
        {
            var menuGO = new GameObject("MainMenu_Auto");
            menu = menuGO.AddComponent<MainMenu>();
        }
        if (menu != null)
        {
            menu.ShowMenu();
        }
    }
}
