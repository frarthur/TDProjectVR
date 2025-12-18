using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class GyroCamera : MonoBehaviour
{
    private AttitudeSensor attitude; // Input System sensor for device orientation
    private GameObject cameraContainer;
    private bool gyroSupported = false;
    public float editorLookSensitivity = 2f; // fallback for testing in Editor/PC
    public bool autoCalibrateOnStart = true;
    public KeyCode recenterKey = KeyCode.Space; // Editor recenter
    private Quaternion baseRotation = Quaternion.identity; // calibration offset

    void Start()
    {
        // Créer un pivot pour corriger les axes
        cameraContainer = new GameObject("CameraContainer");
        cameraContainer.transform.position = transform.position;
        transform.SetParent(cameraContainer.transform);

        // Conserver l'orientation initiale de la caméra
        transform.localRotation = Quaternion.identity;

        // Verrouiller orientation en paysage (recommandé pour stéréoscopie)
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Activer les capteurs via le nouveau Input System
        gyroSupported = AttitudeSensor.current != null;
        if (gyroSupported)
        {
            attitude = AttitudeSensor.current;
            if (!attitude.enabled)
            {
                try { InputSystem.EnableDevice(attitude); } catch {}
            }

            // Correction d'orientation : l'espace du capteur diffère de Unity
            cameraContainer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            if (autoCalibrateOnStart)
            {
                Calibrate();
            }
        }
        else
        {
            Debug.Log("AttitudeSensor indisponible. Fallback souris activé.");
        }
    }

    void Update()
    {
        if (gyroSupported && attitude != null)
        {
            // Convertit la quaternion du gyroscope (droite) en coordonnées Unity (gauche)
            Quaternion q = attitude.attitude.ReadValue();
            Quaternion deviceRotation = GyroToUnity(q);
            transform.localRotation = baseRotation * deviceRotation;

            // Recentrer sur appui multi-touch (2 doigts) ou touche Espace en Éditeur
            bool recenter = false;
            if (Touchscreen.current != null)
            {
                var touches = Touchscreen.current.touches;
                if (touches.Count >= 2 && touches[1].press.wasPressedThisFrame) recenter = true;
            }
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) recenter = true;
            if (recenter)
            {
                Calibrate();
            }
        }
        else
        {
            // Fallback simple pour tester dans l'éditeur (souris pour look)
            if (Mouse.current != null)
            {
                Vector2 d = Mouse.current.delta.ReadValue() * editorLookSensitivity * Time.deltaTime;
                cameraContainer.transform.Rotate(0f, d.x, 0f);
                transform.Rotate(-d.y, 0f, 0f);
            }
        }
    }

    // Conversion standard du quaternion du gyroscope vers Unity
    // Voir docs/expériences : inversion de Z et W pour passer en left-handed
    private Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    // Calibrer l'orientation actuelle comme référence "regarder droit devant"
    private void Calibrate()
    {
        if (attitude == null) return;
        baseRotation = Quaternion.Inverse(GyroToUnity(attitude.attitude.ReadValue()));
    }
}
