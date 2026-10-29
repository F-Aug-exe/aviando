using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Nombres de Escena")]
    [Tooltip("Nombre de la escena principal de gameplay (Build Settings).")]
    public string gameplaySceneName = "Level1";

    [Header("Paneles de UI (asignar en Inspector)")]
    public GameObject mainMenuPanel;
    public GameObject skinSelectionPanel;
    public GameObject settingsPanel;
    public GameObject loadingPanel;

    [Header("Selección de Skin")]
    [Tooltip("Índice del skin seleccionado. Persiste con PlayerPrefs.")]
    public int selectedSkinIndex;

    private const string SkinPrefKey = "SelectedSkin";

    void Awake()
    {
        // Singleton sencillo: si ya existe, destruir duplicado
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        selectedSkinIndex = PlayerPrefs.GetInt(SkinPrefKey, 0);
    }

    // --- Funciones públicas para botones del Canvas ---

    /// <summary>Botón "Iniciar Juego" → muestra selección de skin.</summary>
    public void OnStartGame()
    {
        SetPanel(skinSelectionPanel);
    }

    /// <summary>Botón de skin individual. Llamar con el índice del skin.</summary>
    public void OnSkinSelected(int index)
    {
        selectedSkinIndex = index;
        PlayerPrefs.SetInt(SkinPrefKey, index);
    }

    /// <summary>Botón "Confirmar Skin" → carga la escena de gameplay.</summary>
    public void OnConfirmSkinAndPlay()
    {
        SetPanel(loadingPanel);
        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>Botón "Configuraciones".</summary>
    public void OnOpenSettings()
    {
        SetPanel(settingsPanel);
    }

    /// <summary>Botón "Volver" desde configuraciones al menú principal.</summary>
    public void OnBackToMenu()
    {
        SetPanel(mainMenuPanel);
    }

    // --- Utilidad ---

    void SetPanel(GameObject target)
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(target == mainMenuPanel);
        if (skinSelectionPanel) skinSelectionPanel.SetActive(target == skinSelectionPanel);
        if (settingsPanel) settingsPanel.SetActive(target == settingsPanel);
        if (loadingPanel) loadingPanel.SetActive(target == loadingPanel);
    }
}
