// Imports
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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
    /// <summary>
    /// Stores all Assetables asset data and handles method API calls
    /// </summary>
    public partial class AssetablesStorage : ScriptableObject
    {
        #region Internal
        internal List<Entry> assets = new();

        private AssetablesStorage() { }
        public static AssetablesStorage Instance { get; private set; }

        private static TaskCompletionSource<bool> InitializeCompletedTask = new TaskCompletionSource<bool>();

        private static void SetInstance(AssetablesStorage newinstance)
        {
            if (Instance == null)
            {
                Instance = newinstance;
                if (!InitializeCompletedTask.Task.IsCompleted)
                {
                    InitializeCompletedTask.SetResult(true); // For signaling initialized to scripts using the API
                }
            }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void LoadInstance()
        {
            AssetablesStorage retrieved = Resources.Load<AssetablesStorage>("Assetables Storage");
            if (retrieved != null)
            {
                SetInstance(retrieved);
            }
        }
        #endregion

        // ----------------------------- METHODS ----------------------------- //

        /// <summary>
        /// Allows scripts to wait for the API to load before executing methods from it
        /// </summary>
        /// <remarks>You must use this to await the Singleton initialization IF and only if you are retreiving an asset at or around the same time as <see cref="RuntimeInitializeOnLoadMethodAttribute"/> calls</remarks>
        /// <returns></returns>
        public static async Task AwaitAPIInit() // <------------------------------------------ IMPORTANT, README
        {
            await InitializeCompletedTask.Task;
        }

        /// <summary>
        /// Gets the asset associated with the provided Assetables name
        /// </summary>
        /// <param name="name">The name to search by</param>
        /// <remarks>Make sure to check if you need <see cref="AwaitAPIInit"/></remarks>
        /// <returns>The resulting asset as an <see cref="Object"/></returns>
        public Object GetAsset(string name)
        {
            Entry entry = assets.Find(x => x.name == name);
            if(entry != null)
                return entry.asset;

            Debug.Log("[Assetables] An asset by the name " + name + " could not be found!");
            return null;
        }

        /// <summary>
        /// Gets the <see cref="AssetablesStorage.Entry"/> associated with the provided Assetables name. Entry objects provide additional data, including the name you searched by and the asset <see cref="Type"/>
        /// </summary>
        /// <param name="name">The name to search by</param>
        /// <remarks>Make sure to check if you need <see cref="AwaitAPIInit"/></remarks>
        /// <returns>The resulting Assetables <see cref="AssetablesStorage.Entry"/></returns>
        public Entry GetEntry(string name)
        {
            Entry entry = assets.Find(x => x.name == name);
            if (entry != null)
            {
                if (entry.asset != null)
                {
                    entry.type = entry.asset.GetType();
                }
                return entry;
            }
            Debug.Log("[Assetables] An asset by the name " + name + " could not be found!");
            return null;
        }

        // ----------------------------- CLASSES ----------------------------- //

        /// <summary>
        /// Asset name, object, and type storage entry
        /// </summary>
        [System.Serializable]
        public class Entry
        {
            public string name;
            public Object asset;
            public Type type;
        }
    }
}