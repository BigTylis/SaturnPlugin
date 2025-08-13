// Imports
using UnityEngine;
using System;
using Stellar.APIs.Idify;
using Stellar.APIs.Idify.Internal;
using System.Linq;

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

namespace Stellar.APIs.Idify
{
    /// <summary>
    /// An interface for tagging classes that should support the identification system. <see cref="GameObject"/>s are always counted as <see cref="IIdentifiable"/> despite not having the tag. Keep in mind that <see cref="GameObject"/> IDs can persist whilst custom classes marked as <see cref="IIdentifiable"/> will not have persistent IDs.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// The custom <see cref="IdentifiableObject"/>s ID. Value setting permissions are managed by <see cref="RuntimeIdentifiable"/> attribute.
        /// </summary>
        // Should implement the bridge property that connects changes and a storage variable. Make sure to implement set as protected.
        string Identifier { get;}
    }

    /// <summary>
    /// An implementation of the unique ID system but for any custom class. Using this automatically makes your class a <see cref="IDisposable"/>.
    /// </summary>
    public abstract class IdentifiableObject : IIdentifiable, IDisposable
    {
        internal DynamicStorage.Entry associatedEntry;
        private string identifier;
        public string Identifier
        {
            get { return identifier; }
            protected set
            {
                
                bool RuntimeIdentifiable = Attribute.IsDefined(this.GetType(), typeof(RuntimeIdentifiable));
                if (!RuntimeIdentifiable)
                {
                    if (Application.isPlaying || !Application.isEditor)
                    {
                        throw new AccessViolationException("Attempted to edit the ID of a class implementing IIdentifiable but it can only be edited in the Unity Editor.");
                    }
                }
                if (DynamicStorage.AlreadyExists(typeof(GameObject), value) || string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(value) || value.Any(char.IsWhiteSpace))
                {
                    throw new InvalidOperationException("Attempted to set the ID of a class implementing IIdentifiable to an already existing or invalid ID.");
                }
                identifier = value;
                associatedEntry.ID = identifier;
            }
        }

        /// <summary>
        /// Implement me using :base(ID) for custom IDs!
        /// </summary>
        public IdentifiableObject(string ID = null)
        {
            Type Derived = this.GetType();
            ID = ID ?? DynamicStorage.NewUUID();
            bool idIsUnique = DynamicStorage.AlreadyExists(Derived, ID) ? false : true;
            while (!idIsUnique)
            {
                ID = DynamicStorage.NewUUID();
                idIsUnique = !DynamicStorage.AlreadyExists(Derived, ID);
            }
            this.identifier = ID;
            this.associatedEntry = new(ID, this);
            DynamicStorage.AddEntry(this.associatedEntry);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DynamicStorage.RemoveEntry(this.GetType(), identifier);
            }
        }

        ~IdentifiableObject()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Marks a class implementing <see cref="IIdentifiable"/> to only allow ID modifications at Runtime. Without this attribute the ID can only be changed in the Editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class RuntimeIdentifiable : Attribute { }
}

namespace Stellar.APIs.ObjectHandlingExtensions
{
    // This is an extension for Stellar.APIs.ObjectHandlingExtensions. Strongly recommended to pair with it for more fine tuned control!
    // <summary>
    // Adds extension methods and other global methods to make accessing Unity GameObjects simpler. Extends <see cref="GameObject"/>
    // <summary>
    public static partial class ObjectHandlingExtensions
    {
        // ------------------------------------- RetrieveByID ------------------------------------- //
        /// <summary>
        /// Retrieves an object by it's custom BetterIdentifier identifier. This version always returns <see cref="GameObject"/>. This is an extension from Idify.
        /// </summary>
        /// <param name="ID">Identifier string</param>
        /// <returns>Found <see cref="GameObject"/> or null</returns>
        public static GameObject RetrieveByID(string ID)
        {
            BetterIdentifier[] BetterIdentifierComponents = GameObject.FindObjectsOfType<BetterIdentifier>();
            foreach (BetterIdentifier component in BetterIdentifierComponents)
            {
                if (component.Identifier == ID)
                {
                    return component.gameObject;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves any <see cref="IIdentifiable"/> class by it's ID. This method cannot be used to get <see cref="GameObject"/>s, instead use <see cref="RetrieveByID(string)"/>. This is an extension from Idify.
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="ID"></param>
        /// <returns>Found <see cref="IIdentifiable"/> object as <see cref="T"/></returns>
        public static T RetrieveByID<T>(string ID) where T : class
        {
            DynamicStorage.EntryGrouping group;
            if(typeof(T) == typeof(GameObject))
            {
                GameObject gameObj = RetrieveByID(ID);
                if (gameObj != null)
                {
                    return gameObj as T;
                }
            }
            else
            {
                group = DynamicStorage.storedGroups.FirstOrDefault(group_ => group_.ClassType == typeof(T));
                if (group != null)
                {
                    if (group.entriesLookupManagement.TryGetValue(ID, out var Entry))
                    {
                        return (T) Entry.Class;
                    }
                }
            }
            return default;
        }
    }
}