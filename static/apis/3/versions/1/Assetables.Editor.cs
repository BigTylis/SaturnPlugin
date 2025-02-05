#if UNITY_EDITOR

// Imports
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

// Ambiguous Clarifications
using Object = UnityEngine.Object;

/// <summary>
/// Desc: A tool for managing Unity assets similarly to Addressables. Except less complicated (and not broken/buggy randomly).
/// Usage: Adds a window for managing asset references. Then later retrieving them during runtime.
/// 
///           +--------------+
///          /|             /|
///         / |            / |
///        *--+-----------*  |
///        |  |           |  |
///        |  |           |  |       
///        |  |           |  |
///        |  +-----------+--+
///        | /            | /
///        |/             |/
///        *--------------*
///        
/// Credit: Written and Documented entirely by BigTylis
/// </summary>
namespace Stellar.APIs.Assetables
{
    public partial class AssetablesStorage : ScriptableObject
    {
        /// <summary>
        /// [Internal] Loads the singleton instance from the saved <see cref="AssetablesStorage"/>
        /// 
        /// VERSION REVISED INTO EDITOR ONLY CODE
        /// </summary>
        [InitializeOnLoadMethod]
        private static void LoadInstanceEDITOR()
        {
            AssetablesStorage retrieved = Resources.Load<AssetablesStorage>("Assetables Storage");
            if (retrieved != null)
            {
                SetInstanceEDITOR(retrieved);
            }
        }

        /// <summary>
        /// [Internal] For managing Assetables singleton instances
        /// 
        /// VERSION REVISED INTO EDITOR ONLY CODE
        /// </summary>
        /// <param name="newinstance">The <see cref="AssetablesStorage"/> instance to use</param>
        internal static void SetInstanceEDITOR(AssetablesStorage newinstance)
        {
            if (Instance == null)
            {
                Instance = newinstance;
                Debug.Log("[Assetables] Ready to go!");
                if (!InitializeCompletedTask.Task.IsCompleted)
                {
                    InitializeCompletedTask.SetResult(true); // For signaling initialized to scripts using the API
                }
            }
            else
            {
                if (newinstance != Instance)
                {
                    Debug.Log("[Assetables] An assetables instance already exists. To create a new one, remove the original. (You will see an error below, this is normal!)");
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(newinstance));
                }
            }
        }
    }

    /// <summary>
    /// Custom editor window found in Window -> Asset Management -> Assetables Manager
    /// </summary>
    internal class AssetManagementWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private string searchQuery = "";
        private int addedChanges = 0;
        private int addedSinceSave = 0;
        private int removedChanges = 0;
        private int removedSinceSave = 0;

        [MenuItem("Window/Asset Management/Assetables Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetManagementWindow>("Assetables Management");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("This window should not be used during runtime. To use it, please return to edit mode.", MessageType.Warning);
                return;
            }

            string LastGuiName = GUI.GetNameOfFocusedControl();

            List<AssetablesStorage.Entry> filteredAssets = new();
            if (AssetablesStorage.Instance != null)
            {
                searchQuery = EditorGUILayout.TextField("Search Assets", searchQuery);
                filteredAssets = string.IsNullOrEmpty(searchQuery)
                ? AssetablesStorage.Instance.assets
                : AssetablesStorage.Instance.assets.Where(entry => entry.name.Contains(searchQuery, System.StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (AssetablesStorage.Instance == null)
            {
                if (GUILayout.Button("Create New Storage"))
                {
                    AssetablesStorage newStorage = ScriptableObject.CreateInstance<AssetablesStorage>();
                    AssetablesStorage.SetInstanceEDITOR(newStorage);

                    if (!Directory.Exists("Assets/Resources"))
                    {
                        Directory.CreateDirectory("Assets/Resources");
                    }
                    string path = "Assets/Resources/Assetables Storage.asset";
                    AssetDatabase.CreateAsset(newStorage, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Debug.Log("[Assetables] New storage created! You can now manage assets :)");
                }
                EditorGUILayout.HelpBox("Create an AssetablesStorage object in your assets folder or click the button above to begin managing assets.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            HashSet<AssetablesStorage.Entry> MarkedForRemoval = new();

            foreach (var entry in filteredAssets)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();

                if (!string.IsNullOrWhiteSpace(entry.name))
                {
                    if (AssetablesStorage.Instance.assets.Any(e => e != entry && e.name == entry.name))
                    {
                        EditorGUILayout.HelpBox("Entry names must be unique!", MessageType.Warning);
                    }
                }
                if (entry.asset == null)
                {
                    EditorGUILayout.HelpBox("All entries must have a selected asset!", MessageType.Warning);
                }

                entry.name = EditorGUILayout.TextField("Name", entry.name);
                entry.asset = EditorGUILayout.ObjectField("Asset", entry.asset, typeof(Object), false);
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Remove Asset"))
                {
                    MarkedForRemoval.Add(entry);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            foreach (var entry in MarkedForRemoval)
            {
                AssetablesStorage.Instance.assets.Remove(entry);
            }
            removedChanges += MarkedForRemoval.Count;
            removedSinceSave += MarkedForRemoval.Count;
            if (MarkedForRemoval.Count > 0)
            {
                EditorUtility.SetDirty(AssetablesStorage.Instance);
                MarkedForRemoval.Clear();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add Asset"))
            {
                addedChanges += 1;
                addedSinceSave += 1;
                AssetablesStorage.Instance.assets.Add(new AssetablesStorage.Entry());
                EditorUtility.SetDirty(AssetablesStorage.Instance);
            }

            if (GUILayout.Button("Save Changes"))
            {
                bool MatchingNames = false;
                bool NoneAsset = false;
                foreach (var entry in AssetablesStorage.Instance.assets)
                {
                    if (string.IsNullOrWhiteSpace(entry.name))
                    {
                        Debug.Log("[Assetables] Names cannot be an empty string");
                        return;
                    }
                    if (AssetablesStorage.Instance.assets.Any(e => e != entry && e.name == entry.name))
                    {
                        MatchingNames = true;
                    }
                    if (entry.asset == null)
                    {
                        NoneAsset = true;
                    }
                }
                if (MatchingNames)
                {
                    Debug.Log("[Assetables] All names must be unique");
                    return;
                }
                if (NoneAsset)
                {
                    Debug.Log("[Assetables] All entries must have an existing asset");
                    return;
                }

                EditorUtility.SetDirty(AssetablesStorage.Instance);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[Assetables] Saved changes");
                addedChanges = Mathf.Max(addedChanges - removedSinceSave, 0);
                removedChanges = Mathf.Max(removedChanges - addedSinceSave, 0);
                Debug.Log("[Assetables] Added " + addedChanges + ", removed " + removedChanges);
                addedChanges = 0;
                addedSinceSave = 0;
                removedChanges = 0;
                removedSinceSave = 0;
            }
        }
    }

    /// <summary>
    /// An <see cref="AssetPostprocessor"/> that prevents the creation of more than one of the singleton <see cref="AssetablesStorage"/> instances
    /// </summary>
    abstract internal class AssetablesPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var asset in importedAssets)
            {
                var assetObject = AssetDatabase.LoadAssetAtPath<AssetablesStorage>(asset);
                if (assetObject != null)
                {
                    AssetablesStorage.SetInstanceEDITOR(assetObject);
                }
            }
        }
    }
}

#endif