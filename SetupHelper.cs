using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SetupHelper : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("VR Tower Defense/Setup Layers and Tags")]
    public static void SetupLayersAndTags()
    {
        // Créer le tag "Castle" s'il n'existe pas
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        bool castleTagExists = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals("Castle")) { castleTagExists = true; break; }
        }
        if (!castleTagExists)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
            n.stringValue = "Castle";
        }

        // Créer le layer "Enemy" s'il n'existe pas (cherche premier slot libre)
        SerializedProperty layersProp = tagManager.FindProperty("layers");
        bool enemyLayerExists = false;
        int enemyLayerIndex = -1;
        
        for (int i = 8; i < layersProp.arraySize; i++) // 8-31 sont user layers
        {
            SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
            if (sp.stringValue == "Enemy")
            {
                enemyLayerExists = true;
                enemyLayerIndex = i;
                break;
            }
            if (string.IsNullOrEmpty(sp.stringValue) && enemyLayerIndex == -1)
            {
                enemyLayerIndex = i;
            }
        }
        
        if (!enemyLayerExists && enemyLayerIndex != -1)
        {
            SerializedProperty layerSP = layersProp.GetArrayElementAtIndex(enemyLayerIndex);
            layerSP.stringValue = "Enemy";
        }

        tagManager.ApplyModifiedProperties();
        
        Debug.Log($"Setup terminé: Tag 'Castle' et Layer 'Enemy' (index {enemyLayerIndex}) configurés.");
    }

    [MenuItem("VR Tower Defense/Quick Setup Instructions")]
    public static void ShowSetupInstructions()
    {
        EditorUtility.DisplayDialog("Configuration VR Tower Defense",
            "PREFAB ENNEMI:\n" +
            "1. Ouvre Assets/Prefabs/Enemy.prefabs\n" +
            "2. Assure-toi que le Canvas de la barre de vie est ENFANT du prefab\n" +
            "3. Canvas: Render Mode = World Space, Scale = 0.01, 0.01, 0.01\n" +
            "4. Positionne le Canvas au-dessus de l'ennemi (Y = 1 ou 2)\n" +
            "5. Assigne le layer 'Enemy' au GameObject racine\n\n" +
            "LOOK RAYCASTER:\n" +
            "1. Sélectionne ta Caméra principale\n" +
            "2. Composant LookRaycaster: assigne 'Cam' et règle 'Enemy Layer' sur 'Enemy'\n\n" +
            "SPAWNERS:\n" +
            "1. Assigne le prefab Enemy dans chaque EnemySpawner\n\n" +
            "CHATEAU:\n" +
            "1. Tag = 'Castle'\n" +
            "2. Ajoute un Collider (isTrigger = true)",
            "OK");
    }
#endif
}
