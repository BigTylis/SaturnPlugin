#if UNITY_EDITOR

// Imports
using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Desc: An API tool for giving GameObjects unique and constant identification, then retrieving them through fast simple lookups.
/// Usage: Simply add the BetterIdentifier component to GameObjects you would like to register. During runtime, retrieve them by their IDs using Stellar.APIs.ObjectHandlingExtensions.ObjectHandlingExtensions
/// 
///                ______
///            .- '      `-.           
///          .'            `.         
///         /                \        
///        ;                  ;`       
///        |                  |;
///        ;                 ;|
///        '\               / ;       
///         \`.           .' /        
///          `.`-._____.- ' .'
///            / /`_____.- '           
///           / / /
///          / / /
///         / / /
///        / / /
///       / / /
///      / / /
///     / / /
///    / / /
///   / / /
///   \/_/
///   
/// Credit: Written and Documented entirely by BigTylis
/// </summary>
namespace Stellar.APIs.Idify.Internal
{
    /// <summary>
    /// Modifies the inspector GUI so that custom component properties appear correctly.
    /// </summary>
    [CustomEditor(typeof(BetterIdentifier))]
    public class BetterIdentifierInspectorGUI : Editor
    {
        private string BetterIdentifierComponentIcon = "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";
        private Texture2D componentIcon;

        // Do icon stuff
        private void OnEnable()
        {
            byte[] imageBytes = Convert.FromBase64String(BetterIdentifierComponentIcon);
            componentIcon = new Texture2D(2, 2);
            componentIcon.LoadImage(imageBytes);
        }
        public override void OnInspectorGUI()
        {
            BetterIdentifier component = (BetterIdentifier)target;

            // Gui
            if (!Application.isPlaying)
            {
                GUILayout.Label(new GUIContent("           Modify identifier below or use the button to re-roll its auto generated one!", componentIcon), GUILayout.Height(64));
            }
            else
            {
                GUILayout.Label(new GUIContent("           This GameObjects ID:", componentIcon), GUILayout.Height(64));
            }

            EditorGUI.BeginDisabledGroup(Application.isPlaying); // Disable this field if in runtime
            string newName = EditorGUILayout.TextField(component.Identifier);

            if (GUILayout.Button("Generate Random"))
            {
                string randomID = DynamicStorage.NewUUID();
                component.Identifier = randomID;
            }
            else
            {
                if (newName != component.Identifier)
                {
                    component.Identifier = newName;
                }
            }

            EditorGUI.EndDisabledGroup();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(component);
            }
        }

        public override string GetInfoString()
        {
            return "Better Identifier";
        }
    }

    /// <summary>
    /// Adds an ID reference list window at Window -> General -> IDify Used IDs
    /// </summary>
    public class IdifyCustomWindow : EditorWindow
    {
        private Vector2 scrollPosition;

        [MenuItem("Window/General/IDify Used IDs")]
        public static void ShowWindow()
        {
            var window = GetWindow<IdifyCustomWindow>("Idify Used IDs List");
            window.minSize = new Vector2(100, 600);
        }

        private void OnGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("This window does not work during runtime. Return to the editor for it to work.", MessageType.Warning);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var entry in DynamicStorage.gameObjectIDs)
            {
                //EditorGUILayout.LabelField(entry.ID);
                if (GUILayout.Button(entry.ID))
                {
                    BetterIdentifier[] identifiableObjects = UnityEngine.Object.FindObjectsByType<BetterIdentifier>(FindObjectsSortMode.None);
                    foreach(var obj in identifiableObjects)
                    {
                        if(obj.Identifier == entry.ID) { Selection.activeObject = obj.gameObject; break; }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}

#endif