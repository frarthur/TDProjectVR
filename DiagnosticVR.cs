using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DiagnosticVR : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("VR Tower Defense/Diagnostic Complete")]
    public static void RunDiagnostic()
    {
        string report = "=== DIAGNOSTIC VR TOWER DEFENSE ===\n\n";
        bool hasErrors = false;

        // 1. Vérifier les layers et tags
        report += "1. LAYERS ET TAGS:\n";
        bool hasEnemyLayer = false;
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (layerName == "Enemy")
            {
                hasEnemyLayer = true;
                report += $"   ✓ Layer 'Enemy' existe (index {i})\n";
                break;
            }
        }
        if (!hasEnemyLayer)
        {
            report += "   ✗ ERREUR: Layer 'Enemy' n'existe pas!\n";
            report += "     → Menu: VR Tower Defense → Setup Layers and Tags\n";
            hasErrors = true;
        }

        // 2. Vérifier le prefab Enemy
        report += "\n2. PREFAB ENEMY:\n";
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefabs.prefab");
        if (enemyPrefab == null)
        {
            report += "   ✗ ERREUR: Prefab 'Enemy.prefabs' introuvable!\n";
            hasErrors = true;
        }
        else
        {
            report += "   ✓ Prefab trouvé\n";
            
            // Vérifier layer
            if (enemyPrefab.layer != LayerMask.NameToLayer("Enemy"))
            {
                report += "   ✗ ERREUR: Le prefab n'est pas sur le layer 'Enemy'\n";
                report += "     → Ouvre le prefab, sélectionne la racine, Layer = Enemy\n";
                hasErrors = true;
            }
            else
            {
                report += "   ✓ Layer 'Enemy' assigné\n";
            }

            // Vérifier collider
            Collider col = enemyPrefab.GetComponent<Collider>();
            if (col == null)
            {
                report += "   ✗ ERREUR: Pas de Collider sur le prefab!\n";
                report += "     → Ajoute un CapsuleCollider ou BoxCollider\n";
                hasErrors = true;
            }
            else
            {
                report += $"   ✓ Collider présent ({col.GetType().Name})\n";
            }

            // Vérifier EnemyHealth
            EnemyHealth eh = enemyPrefab.GetComponent<EnemyHealth>();
            if (eh == null)
            {
                report += "   ✗ ERREUR: Pas de script EnemyHealth!\n";
                hasErrors = true;
            }
            else
            {
                report += "   ✓ EnemyHealth présent\n";
                
                // Vérifier le slider
                Canvas[] canvases = enemyPrefab.GetComponentsInChildren<Canvas>(true);
                if (canvases.Length == 0)
                {
                    report += "   ✗ ERREUR: Pas de Canvas enfant pour la barre de vie!\n";
                    hasErrors = true;
                }
                else
                {
                    report += $"   ✓ {canvases.Length} Canvas trouvé(s)\n";
                    foreach (Canvas c in canvases)
                    {
                        if (c.renderMode != RenderMode.WorldSpace)
                        {
                            report += $"   ⚠ WARNING: Canvas '{c.name}' n'est pas en World Space!\n";
                        }
                        Slider slider = c.GetComponentInChildren<Slider>();
                        if (slider != null)
                        {
                            report += $"   ✓ Slider trouvé dans '{c.name}'\n";
                        }
                    }
                }
            }
        }

        // 3. Vérifier LookRaycaster
        report += "\n3. LOOK RAYCASTER:\n";
        LookRaycaster[] raycasters = FindObjectsByType<LookRaycaster>(FindObjectsSortMode.None);
        if (raycasters.Length == 0)
        {
            report += "   ✗ ERREUR: Aucun LookRaycaster dans la scène!\n";
            report += "     → Ajoute le composant LookRaycaster à ta caméra\n";
            hasErrors = true;
        }
        else
        {
            report += $"   ✓ {raycasters.Length} LookRaycaster trouvé(s)\n";
            foreach (var lr in raycasters)
            {
                if (lr.cam == null)
                {
                    report += $"   ✗ ERREUR: '{lr.gameObject.name}' - Cam non assignée!\n";
                    hasErrors = true;
                }
                else
                {
                    report += $"   ✓ '{lr.gameObject.name}' - Cam assignée\n";
                }

                // Vérifier enemyLayer
                if (lr.enemyLayer == 0)
                {
                    report += $"   ✗ ERREUR: '{lr.gameObject.name}' - Enemy Layer = Nothing!\n";
                    report += "     → Dans l'inspecteur, coche le layer 'Enemy'\n";
                    hasErrors = true;
                }
                else
                {
                    report += $"   ✓ '{lr.gameObject.name}' - Enemy Layer configuré\n";
                }
            }
        }

        // 4. Vérifier château
        report += "\n4. CHATEAU:\n";
        GameObject[] castles = GameObject.FindGameObjectsWithTag("Castle");
        if (castles.Length == 0)
        {
            report += "   ⚠ WARNING: Aucun GameObject avec tag 'Castle'\n";
        }
        else
        {
            report += $"   ✓ {castles.Length} château(x) trouvé(s)\n";
            foreach (var castle in castles)
            {
                Collider col = castle.GetComponent<Collider>();
                if (col == null)
                {
                    report += $"   ✗ '{castle.name}' - Pas de Collider!\n";
                    hasErrors = true;
                }
                else if (!col.isTrigger)
                {
                    report += $"   ⚠ '{castle.name}' - Collider.isTrigger devrait être true\n";
                }
            }
        }

        // 5. Vérifier spawners
        report += "\n5. SPAWNERS:\n";
        EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>();
        if (spawners.Length == 0)
        {
            report += "   ⚠ WARNING: Aucun spawner dans la scène\n";
        }
        else
        {
            report += $"   ✓ {spawners.Length} spawner(s) trouvé(s)\n";
            foreach (var sp in spawners)
            {
                if (sp.enemyPrefab == null)
                {
                    report += $"   ✗ '{sp.gameObject.name}' - enemyPrefab non assigné!\n";
                    hasErrors = true;
                }
            }
        }

        report += "\n=================================\n";
        if (hasErrors)
        {
            report += "❌ DES ERREURS ONT ÉTÉ DÉTECTÉES - Corrige-les ci-dessus\n";
        }
        else
        {
            report += "✅ TOUT SEMBLE BON! Si ça ne marche toujours pas:\n";
            report += "   - Vérifie la Console pour d'autres erreurs\n";
            report += "   - Assure-toi d'avoir sauvegardé le prefab et la scène\n";
        }

        Debug.Log(report);
        EditorUtility.DisplayDialog("Diagnostic VR", report, "OK");
    }
#endif
}
