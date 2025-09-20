using UnityEngine;
using System.Collections;
using Project.Settings;

namespace Project.Core
{
    /// <summary>
    /// Central game manager responsible for game state and core system coordination
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        #endregion

        [Header("Game State")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        public System.Action<GameState> OnGameStateChanged;
        public GameState CurrentGameState => currentGameState;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            StartCoroutine(InitializeGameSystems());
        }

        private void InitializeGame()
        {
            // Set application settings from ProjectSettings
            var settings = ProjectSettings.Instance;
            if (settings != null)
            {
                Application.targetFrameRate = settings.TargetFrameRate;
                QualitySettings.vSyncCount = settings.EnableVSync ? 1 : 0;
            }

            Debug.Log($"Game initialized - Version: {settings?.GameVersion ?? "Unknown"}");
        }

        private IEnumerator InitializeGameSystems()
        {
            yield return StartCoroutine(InitializeNetworking());
            yield return StartCoroutine(InitializeAudio());
            yield return StartCoroutine(InitializeUI());

            SetGameState(GameState.MainMenu);
            Debug.Log("All game systems initialized successfully");
        }

        private IEnumerator InitializeNetworking()
        {
            // Initialize PocketBase connection
            Debug.Log("Initializing networking systems...");
            yield return new WaitForSeconds(0.1f); // Placeholder for actual initialization
        }

        private IEnumerator InitializeAudio()
        {
            Debug.Log("Initializing audio systems...");
            yield return new WaitForSeconds(0.1f);
        }

        private IEnumerator InitializeUI()
        {
            Debug.Log("Initializing UI systems...");
            yield return new WaitForSeconds(0.1f);
        }

        public void SetGameState(GameState newState)
        {
            if (currentGameState == newState) return;

            GameState previousState = currentGameState;
            currentGameState = newState;

            Debug.Log($"Game state changed: {previousState} -> {newState}");
            OnGameStateChanged?.Invoke(newState);
        }

        private void OnGUI()
        {
            if (!showDebugInfo || !ProjectSettings.Instance?.DebugMode == true) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Game State: {currentGameState}");
            GUILayout.Label($"FPS: {1f / Time.unscaledDeltaTime:F1}");
            GUILayout.Label($"Time Scale: {Time.timeScale}");
            GUILayout.EndArea();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SetGameState(GameState.Paused);
            }
            else
            {
                SetGameState(GameState.Playing);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && currentGameState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
            }
        }
    }

    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver,
        Settings
    }
}