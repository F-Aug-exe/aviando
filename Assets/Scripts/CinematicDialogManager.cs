using System.Collections;
using UnityEngine;
using TMPro;

public class CinematicDialogManager : MonoBehaviour
{
    [System.Serializable]
    public struct DialogLine
    {
        public string speakerName;
        [TextArea(2, 5)] public string message;
        public Transform cameraSpot; // Posición de la cámara en esta línea
        public float duration;
    }

    [Header("Referencias de UI")]
    public GameObject dialogPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI messageText;

    [Header("Referencias de Cámara y Jugador")]
    public Camera mainCamera;
    public BirdController playerBird;

    [Header("Secuencia Cinematográfica")]
    public DialogLine[] dialogueSequence;

    void Start()
    {
        StartCoroutine(PlayCinematicSequence());
    }

    IEnumerator PlayCinematicSequence()
    {
        // 1. Bloquear controles del jugador
        if (playerBird != null) playerBird.isCinematic = true;
        dialogPanel.SetActive(true);

        // 2. Recorrer la secuencia de líneas y posiciones de cámara
        foreach (var line in dialogueSequence)
        {
            nameText.text = line.speakerName;
            messageText.text = line.message;

            if (line.cameraSpot != null)
            {
                mainCamera.transform.position = line.cameraSpot.position;
                mainCamera.transform.rotation = line.cameraSpot.rotation;
            }

            yield return new WaitForSeconds(line.duration);
        }

        // 3. Finalizar cinemática y pasar al gameplay
        dialogPanel.SetActive(false);
        if (playerBird != null)
        {
            playerBird.isCinematic = false;
            
            // Reacoplar cámara detrás del ave (SmoothFollow simple)
            mainCamera.transform.SetParent(playerBird.transform);
            mainCamera.transform.localPosition = new Vector3(0, 1.5f, -3.5f);
            mainCamera.transform.localRotation = Quaternion.Euler(15, 0, 0);
        }
    }
}