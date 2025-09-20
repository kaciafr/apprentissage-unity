using UnityEngine;
using System.IO;
using UnityEditor;
public static class Setup
{
    [MenuItem("Tools/Setup/Create Default Folders")]
    public static void CreatetoDefaultFolders()
    {

        Folder.CreateDefault("Project", "Animation", "Art", "Material", "Prefabs", "ScriptableObject", "ScriptableObject", "SettingsBindableAttribute", "Sound");
        UnityEditor.AssetDatabase.Refresh();
    }
        static class Folder{ 
    
        public static void CreateDefault(string root, params string[] folders)
        {
            string fullpath = Path.Combine(Application.dataPath, root);
            foreach ( var folder in folders)
            {
                string path = Path.Combine(fullpath, folder);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                  
                }
            }

        }
        }
}
