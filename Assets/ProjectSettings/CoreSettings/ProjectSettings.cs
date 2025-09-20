using UnityEngine;

namespace Project.Settings
{
    /// <summary>
    /// Central configuration for project-wide settings
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectSettings", menuName = "Project/Settings/Project Settings")]
    public class ProjectSettings : ScriptableObject
    {
        [Header("Game Configuration")]
        [SerializeField] private string gameVersion = "1.0.0";
        [SerializeField] private string buildVersion = "1.0.0.0";
        [SerializeField] private bool debugMode = false;

        [Header("PocketBase Configuration")]
        [SerializeField] private string pocketBaseUrl = "http://localhost:8090";
        [SerializeField] private bool enableRealtime = true;
        [SerializeField] private int connectionTimeout = 30;

        [Header("Performance Settings")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool enableVSync = true;
        [SerializeField] private int maxConcurrentNetworkRequests = 5;

        public string GameVersion => gameVersion;
        public string BuildVersion => buildVersion;
        public bool DebugMode => debugMode;
        public string PocketBaseUrl => pocketBaseUrl;
        public bool EnableRealtime => enableRealtime;
        public int ConnectionTimeout => connectionTimeout;
        public int TargetFrameRate => targetFrameRate;
        public bool EnableVSync => enableVSync;
        public int MaxConcurrentNetworkRequests => maxConcurrentNetworkRequests;

        private static ProjectSettings _instance;
        public static ProjectSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ProjectSettings>("ProjectSettings");
                    if (_instance == null)
                    {
                        Debug.LogError("ProjectSettings asset not found in Resources folder!");
                    }
                }
                return _instance;
            }
        }
    }
}