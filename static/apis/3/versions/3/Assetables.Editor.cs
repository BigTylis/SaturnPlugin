#if UNITY_EDITOR

// Imports
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private Dictionary<string, bool> cachedFoldoutStates = new();

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

            // Search
            List<AssetablesStorage.Entry> filteredAssets = new();
            if (AssetablesStorage.Instance != null)
            {
                searchQuery = EditorGUILayout.TextField("Search Assets", searchQuery);

                string groupQuery = null;
                string assetQuery = searchQuery;
                if (searchQuery.Contains('@'))
                {
                    string query = searchQuery.Replace("@", "");
                    var tokenRes = TokenizeSearchQuery(query);
                    groupQuery = tokenRes.ElementAtOrDefault(0);
                    assetQuery = tokenRes.ElementAtOrDefault(1);
                }

                if (assetQuery == null) assetQuery = string.Empty;
                if (groupQuery != null)
                {
                    List<string> groups = string.IsNullOrEmpty(groupQuery)
                        ? AssetablesStorage.Instance.assets.Where(entry => entry.name.Split('/').Length > 1).Select(entry => entry.name).ToList()
                        : AssetablesStorage.Instance.assets.Where(entry => entry.name.Split('/').Length > 1 && entry.name.Contains(groupQuery, StringComparison.OrdinalIgnoreCase)).Select(entry => entry.name).ToList();

                    List<string> _groups = new();
                    foreach(var g in groups)
                    {
                        var split = g.Split('/');
                        _groups.Add(string.Join(string.Empty, split.Where(v => v != split.LastOrDefault())));
                    }
                    groups = _groups;

                    if (string.IsNullOrEmpty(assetQuery))
                    {
                        filteredAssets.Clear();
                        foreach(var entry in AssetablesStorage.Instance.assets)
                        {
                            if (entry.name == null) continue;

                            var split = entry.name.Split('/');
                            string onlygroup = string.Join(string.Empty, split.Where(v => v != split.LastOrDefault()).ToList());
                            if (split.Length > 1)
                            {
                                foreach(var g in groups)
                                {
                                    if (filteredAssets.Contains(entry)) break;
                                    if (onlygroup.Contains(g, StringComparison.OrdinalIgnoreCase)) filteredAssets.Add(entry);
                                }
                            }
                            else
                            {
                                if(groupQuery == string.Empty) filteredAssets.Add(entry); // Direct rooted entries can only be found by searching empty ""
                            }
                        }
                    }
                    else
                    {
                        filteredAssets.Clear();
                        foreach (var entry in AssetablesStorage.Instance.assets)
                        {
                            if (entry.name == null) continue;

                            var split = entry.name.Split('/');
                            string onlygroup = string.Join(string.Empty, split.Where(v => v != split.LastOrDefault()).ToList());
                            if (split.Length > 1)
                            {
                                foreach (var g in groups)
                                {
                                    if (filteredAssets.Contains(entry)) break;
                                    if (onlygroup.Contains(g, StringComparison.OrdinalIgnoreCase) && split.LastOrDefault().Contains(assetQuery, StringComparison.OrdinalIgnoreCase)) filteredAssets.Add(entry);
                                }
                            }
                            else
                            {
                                if (groupQuery == string.Empty && split.First().Contains(assetQuery, StringComparison.OrdinalIgnoreCase)) filteredAssets.Add(entry);
                            }
                        }
                    }
                }
                else
                {
                    filteredAssets = string.IsNullOrEmpty(searchQuery)
                        ? AssetablesStorage.Instance.assets
                        : AssetablesStorage.Instance.assets.Where(entry => (entry.name.Split('/').LastOrDefault()??string.Empty).Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }

            // Storage Instance
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

            // Determine groups
            List<FoldoutContext> RootFoldouts = new();
            List<AssetablesStorage.Entry> RootEntries = new();
            foreach(var entry in filteredAssets)
            {
                if (entry.name == null) entry.name = string.Empty; // Stop null name from causing issues

                string[] detectedFoldouts = entry.name.Split('/');
                if(detectedFoldouts.Length <= 1)
                {
                    RootEntries.Add(entry);
                    continue;
                }

                FoldoutContext foldout = RootFoldouts.Where(f => f.name == detectedFoldouts.FirstOrDefault()).FirstOrDefault();
                if (foldout == null) 
                { 
                    foldout = new FoldoutContext() { name = detectedFoldouts.FirstOrDefault() };
                    RootFoldouts.Add(foldout);
                }

                int nestedCount = detectedFoldouts.Length - 2; // Root, entry
                if (nestedCount <= 0)
                {
                    foldout.entires.Add(entry);
                    continue;
                }

                int deep = 0;
                var current = foldout;
                while(nestedCount > 0)
                {
                    FoldoutContext innerFoldout = foldout.foldouts.Where(f => f.name == detectedFoldouts.ElementAtOrDefault(deep + 1)).FirstOrDefault();
                    if(innerFoldout == null)
                    {
                        innerFoldout = new FoldoutContext() { name = detectedFoldouts[deep + 1] };
                        current.foldouts.Add(innerFoldout);
                    }
                    current = innerFoldout;
                    nestedCount--;
                    deep++;
                }
                current.entires.Add(entry);
            }


            // Draw
            foreach(var entry in RootEntries) // Entries not in a grouping
                DrawEntry(entry, MarkedForRemoval);
            EditorGUI.indentLevel = -1;
            foreach (var foldout in RootFoldouts) // Entires in a grouping and everything inside that
                DrawFoldableLayout(foldout, MarkedForRemoval);
            EditorGUI.indentLevel = 0;

            // Remove assets
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

            // Add assets
            if (GUILayout.Button("Add Asset"))
            {
                addedChanges += 1;
                addedSinceSave += 1;
                AssetablesStorage.Instance.assets.Add(new AssetablesStorage.Entry());
                EditorUtility.SetDirty(AssetablesStorage.Instance);
            }

            // Save
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

        private void DrawFoldableLayout(FoldoutContext foldout, HashSet<AssetablesStorage.Entry> removalFlush)
        {
            using(FoldoutContext fc = foldout)
            {
                fc.Use(); // Trigger indentation

                string cacheIdentifier = $"{fc.name}_{EditorGUI.indentLevel}";
                if (!cachedFoldoutStates.ContainsKey(cacheIdentifier)) cachedFoldoutStates.Add(cacheIdentifier, false);
                cachedFoldoutStates[cacheIdentifier] = EditorGUILayout.Foldout(cachedFoldoutStates[cacheIdentifier], fc.name);
                if (cachedFoldoutStates[cacheIdentifier]) // open
                {
                    EditorGUI.indentLevel++; // Entry indent 1 further
                    foreach (var entry in fc.entires)
                        DrawEntry(entry, removalFlush);
                    EditorGUI.indentLevel--;

                    foreach(var foldoutcontext in fc.foldouts)
                        DrawFoldableLayout(foldoutcontext, removalFlush);
                }
            }
        }

        private void DrawEntry(AssetablesStorage.Entry entry, HashSet<AssetablesStorage.Entry> removalFlush)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box");

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

            entry.name = EditorGUILayout.DelayedTextField("Name", entry.name);
            entry.asset = EditorGUILayout.ObjectField("Asset", entry.asset, typeof(Object), false);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Remove Asset"))
            {
                removalFlush.Add(entry);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private List<string> TokenizeSearchQuery(string query)
        {
            var matches = Regex.Matches(query, "\"([^\"]*)\"|(\\S+)");
            var results = new List<string>();

            foreach (Match match in matches)
            {
                if (match.Groups[1].Success)
                    results.Add(match.Groups[1].Value); // quoted
                else
                    results.Add(match.Groups[2].Value); // unquoted
            }
            return results;
        }

        private class FoldoutContext : IDisposable
        {
            private int oldIndent;
            public string name = "";
            public List<FoldoutContext> foldouts = new();
            public List<AssetablesStorage.Entry> entires = new();

            public void Use()
            {
                oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel += 1;
            }

            public void Dispose()
            {
                EditorGUI.indentLevel = oldIndent;
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