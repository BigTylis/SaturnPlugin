// Imports
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

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
    // Better Identifier
    /// <summary>
    /// Idify's custom component, used for storing the identifiers and referencing uniqueness. This class cannot be used as is. It is inherited by Stellar.APIs.Idify.BetterIdentifier, so use that for actual use.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public abstract class IdifyAPI : MonoBehaviour
    {
        [SerializeField] private string identifier;
        internal DynamicStorage.GameObjectEntry associatedEntry;
        public string Identifier
        {
            get { return identifier; }
            set
            {
                if (Application.isPlaying || !Application.isEditor) // Only editable in editor
                {
                    throw new AccessViolationException("Attempted to edit the ID of a class implementing IIdentifiable but it can only be edited in the Unity Editor.");
                }

                if (DynamicStorage.AlreadyExists(value))
                {
                    Debug.Log("[Idify] Another object already has this ID. Please ensure your ID is unique.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(value))
                {
                    Debug.Log("[Idify] IDs cannot be empty or whitespace.");
                    return;
                }
                if (value.Any(char.IsWhiteSpace))
                {
                    Debug.Log("[Idify] IDs cannot contain any whitespace (spaces, tabs, ext.).");
                    return;
                }

                identifier = value;
                associatedEntry.ID = identifier;
            }
        }

        [SerializeField] private bool ComponentInitialized = false;
        private void Reset()
        {
            if (!ComponentInitialized)
            {
                bool idisunique = false;
                string newID = string.Empty;

                while (!idisunique)
                {
                    newID = DynamicStorage.NewUUID();
                    idisunique = !DynamicStorage.AlreadyExists(newID);
                }

                identifier = newID;
                associatedEntry = new(identifier);
                DynamicStorage.AddEntry(associatedEntry);
                ComponentInitialized = true;
            }
        }

        private void Awake() // Runtime component creation support, custom IDs are not supported.
        {
            if (!Application.isPlaying) return;

            if (!ComponentInitialized)
            {
                bool idisunique = false;
                string newID = string.Empty;

                while (!idisunique)
                {
                    newID = DynamicStorage.NewUUID();
                    idisunique = !DynamicStorage.AlreadyExists(newID);
                }

                identifier = newID;
                associatedEntry = new(identifier);
                DynamicStorage.AddEntry(associatedEntry);
                ComponentInitialized = true;
            }
        }

        private void OnEnable()
        {
            if (ComponentInitialized == false)
                return;

            if (!Application.isPlaying && Application.isEditor)
            {
                associatedEntry = new(identifier);
                DynamicStorage.AddEntry(associatedEntry);
            }
        }

        private void OnDestroy()
        {
            if(associatedEntry != null)
            {
                DynamicStorage.RemoveEntry(associatedEntry);
            }
        }
    }

    /// <summary>
    /// An internal class used for checking the uniqueness of added and edited entries.
    /// </summary>
    public static class DynamicStorage
    {
        internal class Entry
        {
            public string ID;
            public GameObject GameObject;
            public IIdentifiable Class;
            public Type ClassType;
            public Entry(string id, GameObject gameobject)
            {
                ID = id;
                GameObject = gameobject;
                ClassType = typeof(GameObject);
            }
            public Entry(string id, IIdentifiable any)
            {
                ID = id;
                Class = any;
                ClassType = any.GetType();
            }
        }
        internal class GameObjectEntry
        {
            public string ID;
            public GameObjectEntry(string id)
            {
                ID = id;
            }
        }

        internal class EntryGrouping
        {
            public ConcurrentBag<Entry> entries = new();
            public ConcurrentDictionary<string, Entry> entriesLookupManagement = new();
            public Type ClassType;
            public EntryGrouping(Type classType)
            {
                ClassType = classType;
            }
        }

        internal static ConcurrentBag<EntryGrouping> storedGroups = new();
        internal static HashSet<GameObjectEntry> gameObjectIDs = new();

        /// <summary>
        /// Gets the group of the specified class type
        /// </summary>
        /// <param name="ClassType">Class type</param>
        /// <returns><see cref="EntryGrouping"/> group</returns>
        internal static EntryGrouping GetSearchGroup(Type ClassType)
        {
            EntryGrouping searchGroup = null;
            EntryGrouping matchingGroup = storedGroups.FirstOrDefault(group => group.ClassType == ClassType);
            if (matchingGroup != null)
            {
                searchGroup = matchingGroup;
            }

            if (searchGroup == null)
            {
                searchGroup = new EntryGrouping(ClassType);
                storedGroups.Add(searchGroup);
            }

            return searchGroup;
        }

        /// <summary>
        /// Adds an entry to its group. Entry class types are split into groups to improve lookup speed.
        /// </summary>
        /// <param name="entry">New entry</param>
        internal static void AddEntry(Entry entry)
        {
            EntryGrouping grouping = GetSearchGroup(entry.ClassType);
            if (grouping != null)
            {
                grouping.entries.Add(entry);
                grouping.entriesLookupManagement[entry.ID] = entry;
            }
        }
        /// <summary>
        /// Adds a <see cref="GameObject"/> ID entry. This system works differently and is less complex than the <see cref="IIdentifiable"/> system.
        /// </summary>
        /// <param name="ID"></param>
        internal static void AddEntry(GameObjectEntry entry)
        {
            if (!gameObjectIDs.Contains(entry))
            {
                gameObjectIDs.Add(entry);
            }
        }

        /// <summary>
        /// Removes an entry from its group, if it exists.
        /// </summary>
        /// <param name="ClassType">Class type for group search</param>
        /// <param name="ID">String ID</param>
        internal static void RemoveEntry(Type ClassType, string ID)
        {
            EntryGrouping grouping = GetSearchGroup(ClassType);

            if (grouping != null && grouping.entriesLookupManagement.ContainsKey(ID))
            {
                if(grouping.entriesLookupManagement.TryGetValue(ID, out var entry))
                {
                    List<Entry> temp = grouping.entries.ToList(); temp.Remove(entry);
                    grouping.entries = new ConcurrentBag<Entry>(temp);
                    grouping.entriesLookupManagement.TryRemove(ID, out _);
                }
            }
        }
        /// <summary>
        /// Removes a <see cref="GameObject"/> ID entry. This system works differently and is less complex than the <see cref="IIdentifiable"/> system.
        /// </summary>
        /// <param name="ID"></param>
        internal static void RemoveEntry(GameObjectEntry entry)
        {
            if (gameObjectIDs.Contains(entry))
            {
                gameObjectIDs.Remove(entry);
            }
        }
        
        /// <summary>
        /// Checks if an ID is already in use anywhere in the game.
        /// </summary>
        /// <param name="ID">The ID to search for</param>
        /// <returns><see cref="bool"/> result</returns>
        internal static bool AlreadyExists(Type ClassType, string ID)
        {
            EntryGrouping grouping = GetSearchGroup(ClassType);
            if (grouping != null)
            {
                return grouping.entriesLookupManagement.ContainsKey(ID);
            }

            return false;
        }
        /// <summary>
        /// Checks if a <see cref="GameObject"/> ID entry exists. This system works differently and is less complex than the <see cref="IIdentifiable"/> system.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static bool AlreadyExists(string ID)
        {
            return gameObjectIDs.FirstOrDefault(e => e.ID == ID) != null;
        }

        /// <summary>
        /// Generates a new UUID from System.Guid
        /// </summary>
        /// <returns>UUID string</returns>
        internal static string NewUUID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}