// Imports
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;

// Self imports
using Stellar.APIs.API.Extensions;
using Stellar.APIs.API.Implementers;
using Stellar.APIs.API.Internal;
using Stellar.APIs.API.Math;
using Stellar.APIs.API.SuperThreading;

// Ambiguous clarifications
using Component = UnityEngine.Component;
using Debug = UnityEngine.Debug;
using Debugger = Stellar.APIs.API.General.Debugger;
using Float2D = Stellar.APIs.API.Math.Float2D;
using Object = UnityEngine.Object;
using Segment = Stellar.APIs.API.Math.Segment;
using ColorUtility = UnityEngine.ColorUtility;

/// <summary>
/// Desc: Super epic cool multiuse API for Stellar's games
/// Usage: Includes tools for all sorts of uses throughout the game environment, like making Unity even simpler!
///
///                                                                         ..;=== +.
///                                                                     .:= iiiiii =+=
///                                                                 .= i))=;::+)i = +,
///                                                               ,= i);)I)))I):= i =;
///                                                            .= i ==))))ii)))I:i++
///                                                          +)+))iiiiiiii))I=i+:'
///                                     .,:; ; ++++++;:,.       )iii +:::; iii))+i='
///                                  .:; ++= iiiiiiiiii = ++;.    =::,,,:::= i));= +'
///                                ,;+== ii)))))))))))ii == +;,      ,,,:= i))+=:
///                              ,;+= ii))))))IIIIII))))ii===;.    ,,:= i)= i +
///                             ;+= ii)))IIIIITIIIIII))))iiii=+,   ,:=));=,
///                           ,+= i))IIIIIITTTTTITIIIIII)))I)i=+,,:+i)= i +
///                          ,+i))IIIIIITTTTTTTTTTTTI))IIII))i=::i))i='
///                         ,= i))IIIIITLLTTTTTTTTTTIITTTTIII)+; +i)+i`
///                         = i))IIITTLTLTTTTTTTTTIITTLLTTTII +:i)ii:'
///                        + i))IITTTLLLTTTTTTTTTTTTLLLTTTT +:i)))=,
///                        =))ITTTTTTTTTTTLTTTTTTLLLLLLTi:=)IIiii;
///                       .i)IIITTTTTTTTLTTTITLLLLLLLT);=)I)))))i;
///                       :))IIITTTTTLTTTTTTLLHLLLLL);=)II)IIIIi=:
///                       :i)IIITTTTTTTTTLLLHLLHLL)+=)II)ITTTI)i=
///                       .i)IIITTTTITTLLLHHLLLL);=)II)ITTTTII)i+
///                       =i)IIIIIITTLLLLLLHLL=:i)II)TTTTTTIII)i'
///                     +i)i)))IITTLLLLLLLLT=:i)II)TTTTLTTIII)i;
///                   +ii)i:)IITTLLTLLLLT =; +i)I)ITTTTLTTTII))i;
///                  =;)i =:,=)ITTTTLTTI =:i))I)TTTLLLTTTTTII)i;
///                +i)ii::,  +)IIITI +:+i)I))TTTTLLTTTTTII))=,
///              :=;)i =:,,    ,i++::i))I)ITTTTTTTTTTIIII)=+'
///            .+ii)i=::,,   ,,::=i)))iIITTTTTTTTIIIII)=+
///           ,==)ii=;:,,,,:::= ii)i)iIIIITIIITIIII))i+:'
///          +=:))i ==;:::;= iii)+)=  `:i)))IIIII)ii+'
///        .+=:))iiiiiiii)))+ii;
///       .+=;))iiiiii))); ii +
///      .+= i:)))))))= +ii +
///     .;== i +::::=)i =;
///     ,+== iiiiii +,
///     `+= +++;`
///
/// Credit: Written and Documented entirely by BigTylis
/// </summary>
namespace Stellar.APIs.API
{
    /// <summary>
    /// General use tools for various random situations
    /// </summary>
    namespace General
    {
        /// <summary>
        /// A tool class that provides multiple ways to smoothly interpolate between various number structures
        /// </summary>
        [Obsolete("Currently no uses.")]
        public static class TweenTools
        {

        }

        /// <summary>
        /// A class that assists in loading and managing Unity assets during runtime
        /// </summary>
        [Obsolete("Removed (requires Addressables package). Uses Unity.Addressables for asset retrieving. Its recommended to use Assetables API instead.")]
        public static class AssetHandler
        {
            /*
            /// <summary>
            /// Loads the asset from the given address as T
            /// </summary>
            /// <param name="assetAddress">The address of the asset from the Addressables window</param>
            /// <returns><see cref="UnityEngine.Object"/> as <see cref="Type"/> T or null if failure</returns>
            public static async Task<T> LoadAssetAsync<T>(string assetAddress) where T : UnityEngine.Object
            {
                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetAddress);
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("AssetHandler.LoadAssetAsync -> Successfully loaded asset: " + assetAddress);
                    return handle.Result;
                }
                else
                {
                    Debug.Log("AssetHandler.LoadAssetAsync -> Failed to load asset: " + assetAddress);
                    return null;
                }
            }*/
        }

        /// <summary>
        /// A class that contains methods for generating and using UUIDs
        /// </summary>
        public static class UUIDTool
        {
            /// <summary>
            /// Creates a new UUID string
            /// </summary>
            /// <returns>String</returns>
            public static string GenerateUUIDString()
            {
                return Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Print types for <see cref="Debugger.Profiler"/> profile info
        /// </summary>
        public enum ProfilerPrintType
        {
            Dont = 0,
            MemoryUsage = 1,
            TimeSeconds = 2,
            TimeMilliseconds = 3,
            TimeTicks = 4,
            FramesRendered = 5,
            PrintAll = 6
        }

        /// <summary>
        /// Contains operations useful for debugging the game environment, like logging for off thread contexts or reading memory usage and execution time between operations.
        /// </summary>
        public static class Debugger
        {
            /// <summary>
            /// Allows you to profile debug information such as execution time, memory usage, and frames rendered.
            /// </summary>
            public class Profiler
            {
                /// <summary>
                /// Contains debug info
                /// </summary>
                public class ProfileInfo
                {
                    public string Name;
                    /// <summary>
                    /// In bytes
                    /// </summary>
                    public long MemoryUsage;
                    public float ExecutionTimeSeconds;
                    public float ExecutionTimeMilliseconds;
                    public float ExecutionTimeTicks;
                    public float FrameCount;

                    internal ProfileInfo(string name, long memUse, float exeTimeS, float exeTimeM, float exeTimeT, float frames)
                    {
                        Name = name;
                        MemoryUsage = memUse;
                        ExecutionTimeSeconds = exeTimeS;
                        ExecutionTimeMilliseconds = exeTimeM;
                        ExecutionTimeTicks = exeTimeT;
                        FrameCount = frames;
                    }
                }

                private readonly Stopwatch stopwatch;
                private long startMemory;
                private float startTime;
                private int startFrameAmount;
                private string recordingProfileName;
                private bool useUnityTime; 
                private readonly Dictionary<string, ProfileInfo> storedProfiles = new();

                internal Profiler()
                {
                    try // Check if on a thread other than the main thread, since these wont work.
                    {
                        _ = Time.realtimeSinceStartup;
                        _ = Time.frameCount;
                        useUnityTime = true;
                    }
                    catch { useUnityTime = false; }
                    stopwatch = new Stopwatch();
                }

                /// <summary>
                /// Starts the profilers profiling. The name specified is used for later on referencing this specific profile.
                /// </summary>
                /// <param name="ProfileName"></param>
                public void Start(string ProfileName = "")
                {
                    recordingProfileName = ProfileName;
                    startMemory = GC.GetTotalMemory(false);

                    if (useUnityTime)
                    {
                        startTime = Time.realtimeSinceStartup;
                        startFrameAmount = Time.frameCount;
                    }
                    else
                    {
                        startTime = Stopwatch.GetTimestamp() / Stopwatch.Frequency;
                        startFrameAmount = 0;
                    }

                    stopwatch.Restart();
                }
                /// <summary>
                /// Stops the profilers profiling and stores the profile info under the specified start name. Also allows for instant printing based on the <see cref="ProfilerPrintType"/> type.
                /// </summary>
                /// <param name="printResult"></param>
                public void Stop(ProfilerPrintType printResult = ProfilerPrintType.Dont)
                {
                    stopwatch.Stop();
                    long totalMemory = GC.GetTotalMemory(false) - startMemory;
                    if (totalMemory < 0) totalMemory = 0;

                    double totalTimeSeconds;
                    int totalFrames;

                    if (useUnityTime)
                    {
                        totalTimeSeconds = Time.realtimeSinceStartup - startTime;
                        totalFrames = Time.frameCount - startFrameAmount;
                    }
                    else
                    {
                        double now = Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;
                        totalTimeSeconds = now - startTime;
                        totalFrames = 0;
                    }

                    long totalTimeMilliseconds = stopwatch.ElapsedMilliseconds;
                    long totalTimeTicks = stopwatch.ElapsedTicks;

                    ProfileInfo profileInfo = new(recordingProfileName, totalMemory,
                        (float)totalTimeSeconds, (float)totalTimeMilliseconds, totalTimeTicks, totalFrames);

                    if (printResult != ProfilerPrintType.Dont)
                    {
                        PrintProfileInfo(profileInfo, printResult);
                    }

                    if (storedProfiles.ContainsKey(recordingProfileName))
                    {
                        storedProfiles.Remove(recordingProfileName);
                    }
                    storedProfiles.Add(recordingProfileName, profileInfo);
                }
                /// <summary>
                /// Retrieves the <see cref="ProfileInfo"/> associated with the profile name provided. Or null if there isn't one by that name.
                /// </summary>
                /// <param name="profileName"></param>
                /// <returns></returns>
                public ProfileInfo GetProfileInfo(string profileName)
                {
                    if (storedProfiles.TryGetValue(profileName, out ProfileInfo profileInfo))
                    {
                        return profileInfo;
                    }
                    return null;
                }
                /// <summary>
                /// Logs the info of the <see cref="ProfileInfo"/> by the name provided and prints based off the <see cref="ProfilerPrintType"/>. If no profile exists by the provided name it will give you a warning.
                /// </summary>
                /// <param name="profileName"></param>
                /// <param name="printType"></param>
                public void LogProfileInfo(string profileName, ProfilerPrintType printType)
                {
                    ProfileInfo profileInfo = GetProfileInfo(profileName);
                    if (profileInfo != null)
                    {
                        PrintProfileInfo(profileInfo, printType);
                    }
                    else
                    {
                        LogWarning($"Could not find Profiler profile by name: {profileName}. Failed to log info.");
                    }
                }
                private void PrintProfileInfo(ProfileInfo info, ProfilerPrintType printType)
                {
                    switch (printType)
                    {
                        case ProfilerPrintType.TimeTicks: { Log($"Execution Time: {info.ExecutionTimeTicks}"); } break;
                        case ProfilerPrintType.TimeMilliseconds: { Log($"Execution Time: {info.ExecutionTimeMilliseconds}"); } break;
                        case ProfilerPrintType.TimeSeconds: { Log($"Execution Time: {info.ExecutionTimeSeconds}"); } break;
                        case ProfilerPrintType.MemoryUsage: { Log($"Memory Usage: {info.MemoryUsage / 1024L / 1024L} MBs | {info.MemoryUsage / 1024L} KBs | {info.MemoryUsage} Bytes"); } break;
                        case ProfilerPrintType.FramesRendered: { Log($"Frames Rendered: {info.FrameCount}"); } break;
                        case ProfilerPrintType.PrintAll: { Log($"----- Debug Profile -----\nProfile Name: {info.Name}\nExecution Time: {info.ExecutionTimeSeconds}s | {info.ExecutionTimeMilliseconds}ms | {info.ExecutionTimeTicks} ticks\nMemory Usage: {info.MemoryUsage / 1024L / 1024L} MBs | {info.MemoryUsage / 1024L} KBs | {info.MemoryUsage} Bytes\nFrames Rendered: {info.FrameCount}\n-----------------------"); } break;
                    }
                }
            }

            /// <summary>
            /// Creates a new profiler instance
            /// </summary>
            /// <returns><see cref="Profiler"/></returns>
            public static Profiler CreateProfiler() { return new Profiler(); }
            /// <summary>
            /// Logs a message to the Unity Console in a multithreaded environment.
            /// </summary>
            /// <param name="message"></param>
            public static void Log(object message) { SynchronizationDispatcher.ForcePostImmediate(() => Debug.Log(message)); }
            /// <summary>
            /// Logs a message to the Unity Console in a multithreaded environment.
            /// </summary>
            /// <param name="message"></param>
            public static void Log(object message, UnityEngine.Object context) { SynchronizationDispatcher.ForcePostImmediate(() => Debug.Log(message, context)); }
            /// <summary>
            /// A variant of Debugger.Log that logs a warning message to the Console in a multithreaded environment.
            /// </summary>
            /// <param name="message"></param>
            public static void LogWarning(object message) { SynchronizationDispatcher.ForcePostImmediate(() => Debug.LogWarning(message)); }
            /// <summary>
            /// A variant of Debugger.Log that logs a warning message to the Console in a multithreaded environment.
            /// </summary>
            /// <param name="message"></param>
            public static void LogWarning(object message, UnityEngine.Object context) { SynchronizationDispatcher.ForcePostImmediate(() => Debug.LogWarning(message, context)); }
            /// <summary>
            /// A variant of Debugger.Log that logs an error message to the Console in a multithreaded environment.
            /// </summary>
            /// <param name="message"></param>
            public static void LogError(object message) { SynchronizationDispatcher.ForcePostImmediate(() => Debug.LogError(message)); }
            /// <summary>
            /// A variant of Debugger.Log that logs an error message to the Console in a multithreaded environment.
            /// </summary>
            /// <param name="message"></param>
            public static void LogError(object message, UnityEngine.Object context) { SynchronizationDispatcher.ForcePostImmediate(() => Debug.LogError(message, context)); }
        }

        /// <summary>
        /// A different type of List that has hashing built into it, allowing for all the functionality of list ordering/indexing whilst ensuring unique items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class HashList<T> : IEnumerable<T>
        {
            private Dictionary<T, int> _indexMap = new();
            private List<T> _items = new();

            public int Count => _items.Count;
            public T this[int index] => _items[index];

            public HashList() { }

            /// <summary>
            /// Attempts to add the item. Returns a <see cref="Boolean"/> on the success status.
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool Add(T item)
            {
                if (_indexMap.ContainsKey(item)) return false;

                _indexMap[item] = _items.Count;
                _items.Add(item);
                return true;
            }
            /// <summary>
            /// Attempts to remove an item. Returns a <see cref="Boolean"/> on the success status (false if item didn't exist).
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool Remove(T item)
            {
                if (!_indexMap.TryGetValue(item, out int index)) return false;

                _indexMap.Remove(item);
                _items.RemoveAt(index);

                _rebuildMap(index);

                return true;
            }
            /// <summary>
            /// Attempts to remove an item at the specified index.
            /// </summary>
            /// <param name="item"></param>
            /// <exception cref="IndexOutOfRangeException"></exception>
            /// <returns></returns>
            public bool RemoveAt(int index)
            {
                if (index < 0 || index >= _items.Count)
                    throw new IndexOutOfRangeException("Index out of bounds of list.");

                var item = _items[index];
                _indexMap.Remove(item);
                _items.RemoveAt(index);

                _rebuildMap(index);

                return true;
            }
            /// <summary>
            /// Attempts to insert an item at the specified index. Returns a <see cref="Boolean"/> on the success status (false if item existed).
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool Insert(int index, T item)
            {
                index = System.Math.Clamp(index, 0, _items.Count);
                if (_indexMap.ContainsKey(item))
                    return false;

                _items.Insert(index, item);

                _rebuildMap(index);

                return true;
            }
            public void Clear()
            {
                _items.Clear();
                _indexMap.Clear();
            }
            public bool Contains(T item) => _indexMap.ContainsKey(item);
            public void Sort(Comparison<T> comparison)
            {
                _items.Sort(comparison);

                for (int i = 0; i < _items.Count; i++)
                    _indexMap[_items[i]] = i;
            }
            /// <summary>
            /// Gets the index of the item specified or -1 if it didnt exist in the list.
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public int IndexOf(T item)
            {
                return _indexMap.TryGetValue(item, out int index) ? index : -1;
            }

            public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private void _rebuildMap(int index) { for (int i = index; i < _items.Count; i++) _indexMap[_items[i]] = i; } // rebuild index map
        }

        /// <summary>
        /// Thread safe version of <see cref="HashList{T}"/>, allowing for heavy read/writes in parallel execution.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class ConcurrentHashList<T> : IEnumerable<T>
        {
            private readonly ConcurrentDictionary<T, int> _indexMap = new();
            private readonly List<T> _items = new();
            private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

            public int Count
            {
                get
                {
                    _lock.EnterReadLock();
                    try { return _items.Count; }
                    finally { _lock.ExitReadLock(); }
                }
            }

            public T this[int index]
            {
                get
                {
                    _lock.EnterReadLock();
                    try { return _items[index]; }
                    finally { _lock.ExitReadLock(); }
                }
            }

            /// <summary>
            /// Attempts to add the item. Returns a <see cref="Boolean"/> on the success status.
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool Add(T item)
            {
                if (!_indexMap.TryAdd(item, 0))
                    return false;

                _lock.EnterWriteLock();
                try
                {
                    _indexMap[item] = _items.Count;
                    _items.Add(item);
                    return true;
                }
                finally { _lock.ExitWriteLock(); }
            }
            /// <summary>
            /// Attempts to remove an item. Returns a <see cref="Boolean"/> on the success status (false if item didn't exist).
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool Remove(T item)
            {
                if (!_indexMap.TryRemove(item, out int index))
                    return false;

                _lock.EnterWriteLock();
                try
                {
                    if (index < 0 || index >= _items.Count || !EqualityComparer<T>.Default.Equals(_items[index], item))
                    {
                        // If not found in the right spot (due to race), do full search
                        index = _items.IndexOf(item);
                        if (index == -1) return false;
                    }

                    _items.RemoveAt(index);
                    _rebuildMap(index);
                    return true;
                }
                finally { _lock.ExitWriteLock(); }
            }
            /// <summary>
            /// Attempts to remove an item at the specified index.
            /// </summary>
            /// <param name="item"></param>
            /// <exception cref="IndexOutOfRangeException"></exception>
            /// <returns></returns>
            public bool RemoveAt(int index)
            {
                _lock.EnterWriteLock();
                try
                {
                    if (index < 0 || index >= _items.Count)
                        throw new IndexOutOfRangeException("Index out of bounds of list.");

                    var item = _items[index];
                    _items.RemoveAt(index);
                    _indexMap.TryRemove(item, out _);
                    _rebuildMap(index);
                    return true;
                }
                catch (Exception e) { throw e; } // forward
                finally { _lock.ExitWriteLock(); }
            }
            /// <summary>
            /// Attempts to insert an item at the specified index. Returns a <see cref="Boolean"/> on the success status (false if item existed).
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool Insert(int index, T item)
            {
                if (!_indexMap.TryAdd(item, 0))
                    return false;

                _lock.EnterWriteLock();
                try
                {
                    index = System.Math.Clamp(index, 0, _items.Count);
                    _items.Insert(index, item);
                    _rebuildMap(index);
                    return true;
                }
                finally { _lock.ExitWriteLock(); }
            }

            public void Clear()
            {
                _lock.EnterWriteLock();
                try
                {
                    _items.Clear();
                    _indexMap.Clear();
                }
                finally { _lock.ExitWriteLock(); }
            }

            public bool Contains(T item) => _indexMap.ContainsKey(item);

            /// <summary>
            /// Gets the index of the item specified or -1 if it didnt exist in the list.
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public int IndexOf(T item) => _indexMap.TryGetValue(item, out int index) ? index : -1;

            public void Sort(Comparison<T> comparison)
            {
                _lock.EnterWriteLock();
                try
                {
                    _items.Sort(comparison);
                    for (int i = 0; i < _items.Count; i++)
                        _indexMap[_items[i]] = i;
                }
                finally { _lock.ExitWriteLock(); }
            }

            public IEnumerator<T> GetEnumerator()
            {
                T[] snapshot;
                _lock.EnterReadLock();
                try
                {
                    snapshot = _items.ToArray(); // safe snapshot
                }
                finally { _lock.ExitReadLock(); }

                foreach (var item in snapshot)
                    yield return item;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private void _rebuildMap(int start)
            {
                for (int i = start; i < _items.Count; i++)
                    _indexMap[_items[i]] = i;
            }
        }

        /// <summary>
        /// Allows you to see basic information about the app.
        /// </summary>
        public static class ApplicationUtils
        {
            /// <summary>
            /// Is the app shutting down? (Process exit, domain unload, or Unity quitting)
            /// </summary>
            public static bool IsShuttingDown { get; private set; }

            /// <summary>
            /// The current processes executable path.
            /// </summary>
            public static readonly string ExecutablePath = Process.GetCurrentProcess().MainModule.FileName;

            static ApplicationUtils()
            {
                AppDomain.CurrentDomain.ProcessExit += (_, __) => IsShuttingDown = true;
                AppDomain.CurrentDomain.DomainUnload += (_, __) => IsShuttingDown = true;
                Application.quitting += () => IsShuttingDown = true;
            }
        }
    }

    /// <summary>
    /// Math functions and classes
    /// </summary>
    namespace Math
    {
        /// <summary>
        /// A two dimensional float, storing an x and y
        /// </summary>
        [Obsolete("Use a (float x,float y) tuple instead. This is inefficient, allocates GC overhead for no reason and is honestly so unesseccary.")]
        public class Float2D
        {
            public float X;
            public float Y;
            public Float2D(float x, float y)
            {
                X = x;
                Y = y;
            }
        }

        /// <summary>
        /// Holds a min and max of any numeric value type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public struct Range<T> where T : unmanaged, IComparable<T>
        {
            public T min;
            public T max;
            public Range(T min, T max)
            {
                this.min = min;
                this.max = max;
            }
            public bool IsValid => max.CompareTo(min) >= 0;
            public bool InRange(T value) => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;

            public static API.Math.Range<T> From(T a, T b) => new API.Math.Range<T> { min = a, max = b };
        }

        /// <summary>
        /// A line segment consisting of two Vector2 points.
        /// </summary>
        public struct Segment
        {
            public Vector2 a, b;
            public Segment(Vector2 a, Vector2 b) { this.a = a; this.b = b; }
        }

        /// <summary>
        /// A collection of game(g) math functions.
        /// </summary>
        public static class Mathg
        {
            /// <summary>
            /// Float epsilon for checking if something is basically zero. If the number is less than this epsilon it should be treated as zero.
            /// </summary>
            public const float FEPS_NORMALIZE = 1e-8f;
            /// <summary>
            /// A float epsilon matching the smallest float precision.
            /// </summary>
            public const float FEPS_BASE = 1e-7f;
            /// <summary>
            /// Best float epsilon for equality checks, angle and intersection tests, ext. Subjective.
            /// </summary>
            public const float FEPS_EQ = 1e-6f;
            /// <summary>
            /// Best float epsilon for distance checks. Subjective.
            /// </summary>
            public const float FEPS_DIST = 1e-5f;


            /// <summary>
            /// Gets the percentage of how far a value is between 2 other values.
            /// </summary>
            /// <param name="min">The minimum value</param>
            /// <param name="max">The maxmimum value</param>
            /// <param name="value">The input value that determines the percentage</param>
            /// <returns>A percentage as a <see cref="float"/></returns>
            /// <exception cref="ArgumentException"></exception>
            public static float PercentBetween(float min, float max, float value)
            {
                if (min == max)
                {
                    throw new ArgumentException("Min and Max cannot be the same");
                }
                if (min > max)
                {
                    throw new ArgumentException("Max cannot be less than Min");
                }
                value = Mathf.Clamp(value, min, max);
                float percentage = (value - min) / (max - min);
                return percentage;
            }


            /// <summary>
            /// Allows to to shift linear value mappings to different ranges, which can be particularly useful for normalization systems.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="oldMin">The current map's min</param>
            /// <param name="oldMax">The current map's max</param>
            /// <param name="newMin">The new map's min</param>
            /// <param name="newMax">The new map's max</param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public static float ShiftLinearMapping(float value, float oldMin, float oldMax, float newMin, float newMax)
            {
                if (Mathf.Approximately(oldMin, oldMax)) throw new ArgumentException("oldMin and oldMax should not be equal!");
                return newMin + ((value - oldMin) * (newMax - newMin)) / (oldMax - oldMin);
            }

            /// <summary>
            /// Smoothly interpolates a value within a range using non-linear(cubic hermite) interpolation.
            /// </summary>
            /// <param name="min">The minimum value</param>
            /// <param name="max">The maxmimum value</param>
            /// <param name="t">The input value that gets smoothed</param>
            /// <returns>A value as a <see cref="float"/></returns>
            /// <exception cref="ArgumentException"></exception>
            public static float Smooth(float min, float max, float t)
            {
                if (min == max)
                {
                    throw new ArgumentException("Min and Max cannot be the same");
                }
                if (min > max)
                {
                    throw new ArgumentException("Max cannot be less than Min");
                }
                t = math.clamp(t, min, max);
                float value = Mathf.SmoothStep(min, max, Mathf.InverseLerp(min, max, t));
                return value;
            }

            /// <summary>
            /// Gets the max possible amount of values in a numberic type for [0,1]. Integer types just return their full range since they cannot have precision further than whole numbers.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            /// <exception cref="NotSupportedException"></exception>
            public static double GetNumericUnitResolution<T>() where T : struct
            {
                decimal NextDecimal(decimal value)
                {
                    // smallest representable increment at this value
                    int[] bits = decimal.GetBits(value);
                    bits[3] += 1; // bump last bit
                    return new decimal(bits);
                }

                Type t = typeof(T);
                if (t == typeof(float))
                {
                    float eps = BitConverter.Int32BitsToSingle(
                        BitConverter.SingleToInt32Bits(1.0f) + 1
                    ) - 1.0f;
                    return 1.0 / eps;
                }
                else if (t == typeof(double))
                {
                    double eps = BitConverter.Int64BitsToDouble(
                        BitConverter.DoubleToInt64Bits(1.0) + 1
                    ) - 1.0;
                    return 1.0 / eps;
                }
                else if (t == typeof(decimal))
                {
                    decimal eps = 1.0m;
                    decimal next = NextDecimal(1.0m);
                    eps = next - 1.0m;
                    return (double)(1.0m / eps);
                }
                else if (t == typeof(int))
                {
                    return int.MaxValue - (double)int.MinValue + 1;
                }
                else if (t == typeof(uint))
                {
                    return (double)uint.MaxValue + 1;
                }
                else if (t == typeof(long))
                {
                    return (double)long.MaxValue - (double)long.MinValue + 1;
                }
                else if (t == typeof(ulong))
                {
                    return (double)ulong.MaxValue + 1;
                }
                else
                {
                    throw new NotSupportedException($"Type {t} not supported");
                }
            }

            /// <summary>
            /// Scales epsilon baseE by magnitude to account for number magnitudes.
            /// </summary>
            /// <param name="baseE"></param>
            /// <param name="magnitude"></param>
            /// <returns></returns>
            public static float fEpsilonFrom(float baseE, float magnitude) => BMathg.fEpsilonFrom(baseE, magnitude);
        }

        /// <summary>
        /// A collection of game(g) math functions that are burst optimized. More unsafe and optimized than <see cref="Mathg"/> methods.
        /// </summary>
        [BurstCompile]
        public static class BMathg
        {
            /// <summary>
            /// Scales epsilon baseE by magnitude to account for number magnitudes.
            /// </summary>
            /// <param name="baseE"></param>
            /// <param name="magnitude"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [BurstCompile]
            public static float fEpsilonFrom(float baseE, float magnitude)
            {
                return baseE * math.abs(magnitude);
            }

            /// <summary>
            /// See <see cref="Mathg.PercentBetween(float, float, float)"/>
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [BurstCompile]
            public static float BPercentBetween(float min, float max, float value) => (value - min) / (max - min);

            /// <summary>
            /// See <see cref="Mathg.ShiftLinearMapping(float, float, float, float, float)"/>
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [BurstCompile]
            public static float ShiftLinearMapping(float value, float oldMin, float oldMax, float newMin, float newMax) => newMin + ((value - oldMin) * (newMax - newMin)) / (oldMax - oldMin);
        }

        /// <summary>
        /// Vector 2 math functions, all optimized for burst. 
        /// </summary>
        public static class Vec2Math
        {
            /// <summary>
            /// Detects if a point can be found between two other points that form a line segment.
            /// </summary>
            /// <param name="start"></param>
            /// <param name="end"></param>
            /// <param name="point"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [BurstCompile]
            public static bool IsPointOnSegment(Vector2 start, Vector2 end, Vector2 point)
            {
                return math.all(new bool2(
                    point.x <= math.max(start.x, end.x) && point.x >= math.min(start.x, end.x),
                    point.y <= math.max(start.y, end.y) && point.y >= math.min(start.y, end.y)));
            }

            /// <summary>
            /// Gives you a generic number 0/1/2 describing if a string of connected points strays off center.
            /// </summary>
            /// <param name="p1"></param>
            /// <param name="p2"></param>
            /// <param name="p3"></param>
            /// <returns>0 = no bend, 1 = bends clockwise(left), 2 = bends counterclockwise(right)</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [BurstCompile]
            public static float GetTripletBend(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                float value = (p2.y - p1.y) * (p3.x - p2.x) -
                              (p2.x - p1.x) * (p3.y - p2.y);
                if (math.abs(value) < 1e-6f) return 0; // Is colinear, no bend
                return (value > 0) ? 1 : 2; // Bends clockwise, bends counterclockwise
            }

            #region SegmentsIntersect -> bool
            /// <summary>
            /// Detects if two line segments intersect, givin each segments start and end points.
            /// </summary>
            /// <param name="s1"></param>
            /// <param name="s2"></param>
            /// <param name="e1"></param>
            /// <param name="e2"></param>
            /// <returns></returns>
            public static bool SegmentsIntersect(Vector2 s1, Vector2 s2, Vector2 e1, Vector2 e2)
            {
                return _segmentsIntersect(s1, s2, e1, e2);
            }

            /// <summary>
            /// Detects if two line segments intersect, givin each segment.
            /// </summary>
            /// <param name="s1"></param>
            /// <param name="s2"></param>
            /// <returns></returns>
            public static bool SegmentsIntersect(Segment s1, Segment s2)
            {
                return _segmentsIntersect(s1.a, s1.b, s2.a, s2.b);
            }

            // Internal
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [BurstCompile]
            private static bool _segmentsIntersect(Vector2 s1, Vector2 s2, Vector2 e1, Vector2 e2)
            {
                float o1 = GetTripletBend(s1, s2, e1);
                float o2 = GetTripletBend(s1, s2, e2);
                float o3 = GetTripletBend(e1, e2, s1);
                float o4 = GetTripletBend(e1, e2, s2);

                if (o1 != o2 && o3 != o4) return true;

                if (o1 == 0 && IsPointOnSegment(s1, e1, s2)) return true;
                if (o2 == 0 && IsPointOnSegment(s1, e2, s2)) return true;
                if (o3 == 0 && IsPointOnSegment(e1, s1, e2)) return true;
                if (o4 == 0 && IsPointOnSegment(e1, s2, e2)) return true;
                return false;
            }
            #endregion

            /// <summary>
            /// Computes the squared distance between a single point and a line segment. If the perpendicular projection of the point falls outside the segment, the nearest endpoint is used instead.
            /// </summary>
            /// <param name="p"></param>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [BurstCompile]
            public static float PointSegmentDistanceSqr(Vector2 p, Vector2 a, Vector2 b)
            {
                Vector2 v = b - a;
                Vector2 w = p - a;
                float vv = math.dot(v, v);
                if (vv <= 1e-12f) return math.lengthsq(w); // a==b
                float t = math.clamp(math.dot(w, v) / vv, 0f, 1f);
                Vector2 proj = a + v * t;
                return math.lengthsq(p - proj);
            }

            #region SegmentSegmentDistanceSqr -> float
            /// <summary>
            /// Computes the squared shortest distance between two line segments.
            /// </summary>
            /// <param name="p1"></param>
            /// <param name="p2"></param>
            /// <param name="q1"></param>
            /// <param name="q2"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [BurstCompile]
            public static float SegmentSegmentDistanceSqr(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
            {
                float2 p1f2 = new(p1.x, p1.y);
                float2 p2f2 = new(p2.x, p2.y);
                float2 q1f2 = new(q1.x, q1.y);
                float2 q2f2 = new(q2.x, q2.y);
                return _segmentSegmentDistanceSqr(p1f2, p2f2, q1f2, q2f2);
            }

            /// <summary>
            /// Computes the squared shortest distance between two line segments.
            /// </summary>
            /// <param name="s1"></param>
            /// <param name="s2"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [BurstCompile]
            public static float SegmentSegmentDistanceSqr(Segment s1, Segment s2)
            {
                float2 p1f2 = new(s1.a.x, s1.a.y);
                float2 p2f2 = new(s1.b.x, s1.b.y);
                float2 q1f2 = new(s2.a.x, s2.a.y);
                float2 q2f2 = new(s2.b.x, s2.b.y);
                return _segmentSegmentDistanceSqr(p1f2, p2f2, q1f2, q2f2);
            }

            /// <summary>
            /// Computes the squared shortest distance between two line segments.
            /// </summary>
            /// <param name="p1"></param>
            /// <param name="p2"></param>
            /// <param name="q1"></param>
            /// <param name="q2"></param>
            /// <returns></returns>
            public static float SegmentSegmentDistanceSqr(float2 p1, float2 p2, float2 q1, float2 q2)
            {
                return _segmentSegmentDistanceSqr(p1, p2, q1, q2);
            }

            // Internal
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [BurstCompile]
            private static float _segmentSegmentDistanceSqr(float2 p1, float2 p2, float2 q1, float2 q2)
            {
                float2 u = p2 - p1;
                float2 v = q2 - q1;
                float2 w0 = p1 - q1;

                float a = math.dot(u, u);
                float b = math.dot(u, v);
                float c = math.dot(v, v);
                float d = math.dot(u, w0);
                float e = math.dot(v, w0);

                float D = a * c - b * b; // denom

                float sc, sN, sD = D; // sc = sN / sD
                float tc, tN, tD = D; // tc = tN / tD

                const float SMALL_NUM = 1e-9f;

                // get closest point parameters sN, tN
                if (D < SMALL_NUM) // almost parallel
                {
                    sN = 0f;
                    sD = 1f;
                    tN = e;
                    tD = c;
                }
                else
                {
                    sN = (b * e - c * d);
                    tN = (a * e - b * d);
                }

                // clamp sN within [0, sD]
                if (sN <= 0f)
                {
                    sN = 0f;
                    tN = e;
                    tD = c;
                }
                else if (sN >= sD)
                {
                    sN = sD;
                    tN = e + b;
                    tD = c;
                }

                // clamp tN within [0, tD]
                if (tN <= 0f)
                {
                    tN = 0f;
                    if (-d <= 0f)
                        sN = 0f;
                    else if (-d >= a)
                        sN = sD;
                    else
                    {
                        sN = -d;
                        sD = a;
                    }
                }
                else if (tN >= tD)
                {
                    tN = tD;
                    float tmp = -d + b;
                    if (tmp <= 0f)
                        sN = 0f;
                    else if (tmp >= a)
                        sN = sD;
                    else
                    {
                        sN = tmp;
                        sD = a;
                    }
                }

                sc = (math.abs(sN) < 1e-12f ? 0f : sN / sD);
                tc = (math.abs(tN) < 1e-12f ? 0f : tN / tD);

                float2 dP = w0 + (u * sc) - (v * tc);
                return math.lengthsq(dP);
            }
            #endregion
        }

        public static class NumConvert // hmm
        {
            // --------------------------
            // To UInt
            // --------------------------
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ToUInt(int value) => unchecked((uint)math.clamp(value, 0, int.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ToUInt(uint value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ToUInt(long value) => unchecked((uint)math.clamp(value, 0L, uint.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ToUInt(ulong value) => unchecked((uint)math.clamp(value, 0UL, uint.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ToUInt(short value) => unchecked((uint)math.clamp(value, 0, short.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ToUInt(ushort value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ToUInt(byte value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ToUInt(sbyte value) => unchecked((uint)math.clamp(value, 0, sbyte.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ToUInt(float value) => unchecked((uint)math.clamp(value, 0f, uint.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ToUInt(double value) => unchecked((uint)math.clamp(value, 0.0, uint.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ToUInt(decimal value) => unchecked((uint)Clamp(value, 0m, (decimal)uint.MaxValue));

            // --------------------------
            // To Int
            // --------------------------
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ToInt(int value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ToInt(uint value) => (int)math.clamp(value, 0u, int.MaxValue);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ToInt(long value) => (int)math.clamp(value, int.MinValue, int.MaxValue);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ToInt(ulong value) => (int)math.clamp(value, 0UL, int.MaxValue);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ToInt(short value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ToInt(ushort value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ToInt(byte value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ToInt(sbyte value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ToInt(float value) => (int)math.clamp(value, int.MinValue, int.MaxValue);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ToInt(double value) => (int)math.clamp(value, int.MinValue, int.MaxValue);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ToInt(decimal value) => (int)Clamp(value, int.MinValue, int.MaxValue);

            // --------------------------
            // To Long
            // --------------------------
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ToLong(int value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ToLong(uint value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ToLong(long value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ToLong(ulong value) => unchecked((long)math.clamp(value, 0UL, long.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ToLong(short value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ToLong(ushort value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ToLong(byte value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ToLong(sbyte value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ToLong(float value) => (long)math.clamp(value, long.MinValue, long.MaxValue);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ToLong(double value) => (long)math.clamp(value, long.MinValue, long.MaxValue);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ToLong(decimal value) => (long)Clamp(value, long.MinValue, long.MaxValue);

            // --------------------------
            // To ULong
            // --------------------------
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong ToULong(int value) => unchecked((ulong)math.clamp(value, 0, int.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong ToULong(uint value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong ToULong(long value) => unchecked((ulong)math.clamp(value, 0L, long.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong ToULong(ulong value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong ToULong(short value) => unchecked((ulong)math.clamp(value, 0, short.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong ToULong(ushort value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong ToULong(byte value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong ToULong(sbyte value) => unchecked((ulong)math.clamp(value, 0, sbyte.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong ToULong(float value) => unchecked((ulong)math.clamp(value, 0f, ulong.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong ToULong(double value) => unchecked((ulong)math.clamp(value, 0.0, ulong.MaxValue));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong ToULong(decimal value) => unchecked((ulong)Clamp(value, 0m, ulong.MaxValue));

            // --------------------------
            // To Float
            // --------------------------
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float ToFloat(int value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float ToFloat(uint value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float ToFloat(long value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float ToFloat(ulong value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float ToFloat(short value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float ToFloat(ushort value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float ToFloat(byte value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float ToFloat(sbyte value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float ToFloat(float value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float ToFloat(double value) => (float)value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float ToFloat(decimal value) => (float)value;

            // --------------------------
            // To Double
            // --------------------------
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static double ToDouble(int value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static double ToDouble(uint value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static double ToDouble(long value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static double ToDouble(ulong value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static double ToDouble(short value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static double ToDouble(ushort value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static double ToDouble(byte value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static double ToDouble(sbyte value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static double ToDouble(float value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static double ToDouble(double value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static double ToDouble(decimal value) => (double)value;

            // --------------------------
            // To Decimal
            // --------------------------
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static decimal ToDecimal(int value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static decimal ToDecimal(uint value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static decimal ToDecimal(long value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static decimal ToDecimal(ulong value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static decimal ToDecimal(short value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static decimal ToDecimal(ushort value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static decimal ToDecimal(byte value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static decimal ToDecimal(sbyte value) => value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static decimal ToDecimal(float value) => (decimal)value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static decimal ToDecimal(double value) => (decimal)value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static decimal ToDecimal(decimal value) => value;

            // Helpers
            private static long Clamp(long value, long min, long max) => value < min ? min : (value > max ? max : value);
            private static ulong Clamp(ulong value, ulong min, ulong max) => value < min ? min : (value > max ? max : value);
            private static decimal Clamp(decimal value, decimal min, decimal max) => value < min ? min : (value > max ? max : value);
        }
    }

    /// <summary>
    /// Tools and things for procedural generation systems
    /// </summary>
    namespace Procedural
    {
        #region Heightmap stuff
        /// <summary>
        /// A height map data class. Currenly only supports <see cref="Single"/>.
        /// </summary>
        public class HeightMap : IEnumerable<(int x, int y, float value)>
        {
            // Vars
            /// <summary>
            /// The stored map data. Typically you should just access data through direct enumeration or indexing of the <see cref="HeightMap"/> instance.
            /// </summary>
            public float[,] Map { get; set; }
            public int Width => Map.GetLength(0);
            public int Height => Map.GetLength(1);

            // Constructor
            public HeightMap(float[,] map)
            {
                Map = map;
            }

            // Indexing support
            public float this[int x, int y]
            {
                get => Map[x, y];
                set => Map[x,y] = value;
            }

            // Enumeration support
            public IEnumerator<(int x, int y, float value)> GetEnumerator()
            {
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        yield return (x, y, Map[x, y]);
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<float> GetValueEnumerator()
            {
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        yield return Map[x, y];
            }
            
            // Methods
            /// <summary>
            /// Moves the height map values to a 1 dimensional array.
            /// </summary>
            public float[] As1D()
            {
                var as1D = new float[Map.Length];
                Buffer.BlockCopy(Map, 0, as1D, 0, Map.Length * sizeof(float));
                return as1D;
            }
            /// <summary>
            /// Easy transform method to transform values of the map. Out <see cref="Single"/> is the new value per coordinate.
            /// </summary>
            /// <param name="transform"></param>
            public void Transform(Func<int, int, float, float> transform)
            {
                for(int x = 0; x < Width; x++)
                    for(int y = 0; y < Height; y++)
                        Map[x,y] = transform(x,y, Map[x,y]);
            }
            /// <summary>
            /// Runs a parallel scan with locks to find what coordinate the lowest value is at and what that value is.
            /// </summary>
            public (int x, int y, float value) GetLowestData()
            {
                (int x, int y, float value) result = (-1, -1, float.MaxValue);
                object lock_ = new();

                Parallel.For(0, Width, () => (0, 0, value:float.MaxValue), (x, state, local) =>
                {
                    for(int y = 0; y < Height; y++)
                    {
                        float value = Map[x, y];
                        if (value < local.value) local = (x, y, value);
                    }
                    return local;
                },
                local =>
                {
                    lock (lock_)
                    {
                        if (local.value < result.value) result = local;
                    }
                });
                return result;
            }
            /// <summary>
            /// Runs a parallel scan with locks to find what coordinate the highest value is at and what that value is.
            /// </summary>
            public (int x, int y, float value) GetHighestData()
            {
                (int x, int y, float value) result = (-1, -1, float.MinValue);
                object lock_ = new();

                Parallel.For(0, Width, () => (0, 0, value: float.MinValue), (x, state, local) =>
                {
                    for (int y = 0; y < Height; y++)
                    {
                        float value = Map[x, y];
                        if (value > local.value) local = (x, y, value);
                    }
                    return local;
                },
                local =>
                {
                    lock (lock_)
                    {
                        if (local.value > result.value) result = local;
                    }
                });
                return result;
            }
            public HeightMap Clone()
            {
                var copy = new float[Width, Height];
                Array.Copy(Map, copy, Map.Length);
                return new HeightMap(copy);
            }

            // DEPRACATED
            [Obsolete("Use GetLowestData() instead.")]
            public float Lowest = float.NaN;
            [Obsolete("Use GetHighestData() instead.")]
            public float Highest = float.NaN;
        }

        /// <summary>
        /// A configuration class for <see cref="HeightMapper"/>.
        /// </summary>
        public class HeightMapConfiguration
        {
            /// <summary>
            /// The width of the height map
            /// </summary>
            public int width;
            /// <summary>
            /// The height of the height map
            /// </summary>
            public int height;
            /// <summary>
            /// Controls the zoom level of the noise. Larger scales zoom out, smaller scales zoom in
            /// </summary>
            public float scale;
            /// <summary>
            ///  The number of layers of noise to add together to create more complex patterns
            /// </summary>
            public int octaves;
            /// <summary>
            /// Controls the decrease in amplitude of each octave. A higher value results in rougher values
            /// </summary>
            public float persistence;
            /// <summary>
            /// Controls the increase in frequency of each octave. A higher value results in more frequent features
            /// </summary>
            public float lacunarity;
            /// <summary>
            /// Sets the randomized offset seed. The same seed will return the same map
            /// </summary>
            [Obsolete("Reworked and will no longer function. You must now manually input offsetX and offsetY into HeightMapConfigurations.")]
            public int seed;
            /// <summary>
            /// The offset of the noise in the X coordinate.
            /// </summary>
            public float offsetX;
            /// <summary>
            /// The offset of the noise in the Y coordinate.
            /// </summary>
            public float offsetY;

            /// <summary>
            /// Configuration class for HeightMapper class
            /// </summary>
            /// <param name="width">The width of the height map</param>
            /// <param name="height">The height of the height map</param>
            /// <param name="scale">Controls the zoom level of the noise. Larger scales zoom out, smaller scales zoom in</param>
            /// <param name="octaves">The number of layers of noise to add together to create more complex patterns</param>
            /// <param name="persistence">Controls the decrease in amplitude of each octave. A higher value results in rougher values</param>
            /// <param name="lacunarity"> Controls the increase in frequency of each octave. A higher value results in more frequent features</param>
            /// <param name="seed">Sets the randomized offset seed. The same seed will return the same map</param>
            [Obsolete("This constructor is depracted, the usage of seed is no longer functional.")]
            public HeightMapConfiguration(int width = 256, int height = 256, float scale = 1f, int octaves = 1, float persistence = 0.5f, float lacunarity = 2.0f, int seed = 0)
            {
                this.width = width;
                this.height = height;
                this.scale = scale;
                this.octaves = octaves;
                this.persistence = persistence;
                this.lacunarity = lacunarity;
                this.seed = seed;
            }
            /// <param name="width">The width of the height map</param>
            /// <param name="height">The height of the height map</param>
            /// <param name="scale">Controls the zoom level of the noise. Larger scales zoom out, smaller scales zoom in</param>
            /// <param name="octaves">The number of layers of noise to add together to create more complex patterns</param>
            /// <param name="persistence">Controls the decrease in amplitude of each octave. A higher value results in rougher values</param>
            /// <param name="lacunarity"> Controls the increase in frequency of each octave. A higher value results in more frequent features</param>
            /// <param name="offsetX">The offset applied to the x coordinate plane when retrieving noise values</param>
            /// <param name="offsetY">The offset applied to the y coordinate plane when retrieving noise values</param>
            public HeightMapConfiguration(int width, int height, float scale = 1f, int octaves = 1, float persistence = 0.5f, float lacunarity = 2.0f, float offsetX = 0, float offsetY = 0)
            {
                this.width = width;
                this.height = height;
                this.scale = scale;
                this.octaves = octaves;
                this.persistence = persistence;
                this.lacunarity = lacunarity;
                this.offsetX = offsetX;
                this.offsetY = offsetY;
            }
        }

        /// <summary>
        /// A utility for generating basic/classic heightmaps.
        /// </summary>
        public static class HeightMapper
        {
            private static HeightMap generate(HeightMapConfiguration config)
            {
                float[,] Map = new float[config.width, config.height];

                float CalculatePerlinNoise(int x, int y, float offsetX, float offsetY)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < config.octaves; i++)
                    {
                        float xCoord = (x / (float)config.width * config.scale * frequency) + offsetX;
                        float yCoord = (y / (float)config.height * config.scale * frequency) + offsetY;

                        float perlinValue = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= config.persistence;
                        frequency *= config.lacunarity;
                    }

                    return (noiseHeight + 1) / 2; // Normalize to range [0, 1]
                }

                float offsetX = config.offsetX;
                float offsetY = config.offsetY;

                for (int x = 0; x < config.width; x++)
                {
                    for (int y = 0; y < config.height; y++)
                    {
                        float height = CalculatePerlinNoise(x, y, offsetX, offsetY);
                        Map[x, y] = height;
                    }
                }
                return new HeightMap(Map);
            }
            /// <summary>
            /// Generates a Heightmap with specified configuration
            /// </summary>
            /// <param name="config"><see cref="HeightMapConfiguration"/> with desired configuration</param>
            /// <returns>A <see cref="HeightMap"/> object</returns>
            [Obsolete("This usage is deprecated. For a better, more dynamic version, use the other GenerateMap() overload.")]
            public static HeightMap GenerateMap(HeightMapConfiguration config)
            {
                return generate(config);
            }

            /// <summary>
            /// Generates a Heightmap with specified configuration using the specified noise function.
            /// </summary>
            /// <param name="config">Configuration</param>
            /// <param name="noiseFunc">Noise function. In: <see cref="Single"/> noise x coordinate, <see cref="Single"/> noise y coordinate | Out: <see cref="Single"/> outputted noise value</param>
            /// <returns></returns>
            public static HeightMap GenerateMap(HeightMapConfiguration config, Func<float, float, float> noiseFunc)
            {
                float[,] Map = new float[config.width, config.height];

                float CalculateNoise(int x, int y, float offsetX, float offsetY)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < config.octaves; i++)
                    {
                        float xCoord = (x / (float)config.width * config.scale * frequency) + offsetX;
                        float yCoord = (y / (float)config.height * config.scale * frequency) + offsetY;

                        float noiseValue = Mathg.ShiftLinearMapping(noiseFunc(xCoord, yCoord), 0, 1, -1, 1);
                        noiseHeight += noiseValue * amplitude;

                        amplitude *= config.persistence;
                        frequency *= config.lacunarity;
                    }

                    return Mathf.Clamp(Mathg.ShiftLinearMapping(noiseHeight, -1, 1, 0, 1), 0, 1);
                }

                float offsetX = config.offsetX;
                float offsetY = config.offsetY;

                for (int x = 0; x < config.width; x++)
                {
                    for (int y = 0; y < config.height; y++)
                    {
                        float height = CalculateNoise(x, y, offsetX, offsetY);
                        Map[x, y] = height;
                    }
                }
                return new HeightMap(Map);
            }
        }
        #endregion

        #region Randomness algorithms/utils

        // ------------------ PRNG algorithms ------------------ //
        /// <summary>
        /// (Low/medium quality, medium speed, inefficient, requires state)[2/5] Pseudo random generation class using LCG algorithm (basically <see cref="System.Random"/> internally). However this class is adapted for the game environment and Stellar API internal systems. It is not recommended to intermix this and <see cref="System.Random"/> when making randomizer engines.
        /// Docs Note: PRNG sequence increment cost refers to how many numbers from the internal PRNG sequence are used to give you the desired output.
        /// </summary>
        public class LCGRandom
        {
            const int ReservedSequenceCount = 1; // 1th is for SeededRandom reservations.
            private readonly System.Random random;

            /// <summary>
            /// The current location of the PRNG selector. Starts at 0, signifying no numbers have been used yet.
            /// </summary>
            public int SequenceLocation { get; private set; } = 0;

            public LCGRandom(int seed)
            {
                random = new(seed);
                IncrementSequence(ReservedSequenceCount, true); // Automatically move the sequence past all reserved numbers.
            }

            /// <summary>
            /// Gets a random int between [min,~max]. PRNG sequence increment cost: 1
            /// </summary>
            /// <param name="Min"></param>
            /// <param name="Max"></param>
            /// <param name="Seed"></param>
            /// <returns></returns>
            public int GetIntMinMax(int Min, int Max)
            {
                SequenceLocation += 1;
                return random.Next(Min, Max + 1);
            }

            /// <summary>
            /// Gets a random float between [min,~max]. PRNG sequence increment cost: 1
            /// </summary>
            /// <param name="Min"></param>
            /// <param name="Max"></param>
            /// <param name="Seed"></param>
            /// <returns></returns>
            public float GetFloatMinMax(float Min, float Max)
            {
                SequenceLocation += 1;
                return (float)(Min + random.NextDouble() * (Max - Min + float.Epsilon));
            }

            /// <summary>
            /// Gets a random double between [min,~max]. PRNG sequence increment cost: 1
            /// </summary>
            /// <param name="Min"></param>
            /// <param name="Max"></param>
            /// <param name="Seed"></param>
            /// <returns></returns>
            public double GetDoubleMinMax(double Min, double Max)
            {
                SequenceLocation += 1;
                return (Min + random.NextDouble() * (Max - Min + double.Epsilon));
            }

            /// <summary>
            /// Gets the next int from the PRNG random sequence, which can be any number between [0,<see cref="int.MaxValue"/>). PRNG sequence increment cost: 1
            /// </summary>
            /// <returns></returns>
            public int GetNextInt()
            {
                SequenceLocation += 1;
                return random.Next();
            }

            /// <summary>
            /// Gets the next double from the PRNG random sequence, which can be any number between [0,1). PRNG sequence increment cost: 1
            /// </summary>
            /// <returns></returns>
            public double GetNextDouble()
            {
                SequenceLocation += 1;
                return random.NextDouble();
            }

            /// <summary>
            /// Fills the provided byte[] array using the PRNG random sequence. Each byte can be [0,255].  PRNG sequence increment cost: buffer.Length
            /// </summary>
            /// <param name="buffer"></param>
            public void FillByteBuffer(byte[] buffer)
            {
                SequenceLocation += buffer.Length;
                random.NextBytes(buffer);
            }

            /// <summary>
            /// Automatically increments the PRNG sequence by the amount provided. If allocateForPerformance is true, a byte[] is used to increment the PRNG sequence which utilizes memory but has better performance. If allocateForPerformance is false, a for loop increments the sequence manually, which allocates no memory but has worse performance.
            /// You should only set this to false if you are incrementing by large amounts or need to preserve memory.
            /// </summary>
            /// <param name="inc"></param>
            public void IncrementSequence(int inc, bool allocateForPerformance = true)
            {
                if (inc < 0) throw new ArgumentException("Can't increment by a negative number. Value must be >= 0");
                if (inc == 0) return;

                if (allocateForPerformance)
                {
                    byte[] skipBuffer = new byte[inc];
                    random.NextBytes(skipBuffer);
                }
                else
                {
                    for (int i = 0; i < inc; i++)
                    {
                        random.Next();
                    }
                }
                SequenceLocation += inc;
            }
        }

        /// <summary>
        /// (High quality, very fast, very high efficiency, stateless)[4/5] Pseudo random generation class using SplitMix64 algorithm, which is stateless.
        /// </summary>
        public class SplitMix64Random
        {
            public ulong Seed { get; private set; }
            public SplitMix64Random(ulong seed) { this.Seed = seed; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong From(ulong index, ulong seed = 0)
            {
                ulong z = index + seed + 0x9E3779B97F4A7C15UL;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ulong GetULong(ulong index) => From(index, Seed);

            /// <summary>
            /// Gets an int [min,max]
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public int GetIntMinMax(int min, int max, ulong index)
            {
                ulong value = From(index, Seed);
                return RNGNumConverter.ToRangeInt(value, min, max);
            }
            /// <summary>
            /// Gets a float [min,max]
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public float GetFloatMinMax(float min, float max, ulong index)
            {
                ulong value = From(index, Seed);
                return RNGNumConverter.ToRangeFloat(value, min, max);
            }
            /// <summary>
            /// Gets a double [min,max]
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public double GetDoubleMinMax(double min, double max, ulong index)
            {
                ulong value = From(index, Seed);
                return RNGNumConverter.ToRangeDouble(value, min, max);
            }

            /// <summary>
            /// Changes the seed this instance is using.
            /// </summary>
            /// <param name="seed"></param>
            public void ChangeSeed(ulong seed) => Seed = seed;
        }

        /// <summary>
        /// (Pretty good quality, very fast, efficient, stateless)[3/5] Pseudo random generation class using MurmurHash3 algorithm (directly uses <see cref="MurmurHash3Finalizer"/> to generate outputs), which is stateless.
        /// </summary>
        public class MurmurHash3Random
        {
            public ulong Seed { get; private set; }
            public MurmurHash3Random(ulong seed) { this.Seed = seed; }

            /// <summary>
            /// Gets an int [min,max]
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public int GetIntMinMax(int min, int max, ulong index)
            {
                ulong value = MurmurHash3Finalizer.Scramble(index, Seed);
                return RNGNumConverter.ToRangeInt(value, min, max);
            }
            /// <summary>
            /// Gets a float [min,max]
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public float GetFloatMinMax(float min, float max, ulong index)
            {
                ulong value = MurmurHash3Finalizer.Scramble(index, Seed);
                return RNGNumConverter.ToRangeFloat(value, min, max);
            }
            /// <summary>
            /// Gets a double [min,max]
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public double GetDoubleMinMax(double min, double max, ulong index)
            {
                ulong value = MurmurHash3Finalizer.Scramble(index, Seed);
                return RNGNumConverter.ToRangeDouble(value, min, max);
            }

            /// <summary>
            /// Changes the seed this instance is using.
            /// </summary>
            /// <param name="seed"></param>
            public void ChangeSeed(ulong seed) => Seed = seed;
        }

        /// <summary>
        /// (Bad quality, ultra fast, very high efficiency, requires state)[2/5] Pseudo random generation class using XorShift32 algorithm.
        /// Docs Note: PRNG sequence increment cost refers to how many numbers from the internal PRNG sequence are used to give you the desired output.
        /// </summary>
        public class XorShift32Random
        {
            public uint Seed { get; private set; }
            private uint state;

            /// <summary>
            /// The current location of the PRNG selector. Starts at 0, signifying no numbers have been used yet.
            /// </summary>
            public int SequenceLocation { get; private set; } = 0;

            public XorShift32Random(uint seed)
            {
                Seed = seed != 0 ? seed : 2463534242;
                state = Seed;
            }

            /// <summary>
            /// Gets the next uint. PRNG sequence increment cost: 1
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint NextUInt()
            {
                SequenceLocation += 1;
                uint x = state;
                x ^= x << 13;
                x ^= x >> 17;
                x ^= x << 5;
                state = x;
                return x;
            }

            /// <summary>
            /// Gets an int [min,max]. PRNG sequence increment cost: 1
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public int NextInt(int min, int max) => RNGNumConverter.ToRangeInt(NextUInt(), min, max);
            /// <summary>
            /// Gets a float [min,max]. PRNG sequence increment cost: 1
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public float NextFloat(float min, float max) => RNGNumConverter.ToRangeFloat(NextUInt(), min, max);
            /// <summary>
            /// Gets a double [min,max]. PRNG sequence increment cost: 1
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public double NextDouble(double min, double max) => RNGNumConverter.ToRangeDouble(NextUInt(), min, max);

            /// <summary>
            /// Automatically skips ahead in the sequence the desired amount.
            /// </summary>
            /// <param name="inc"></param>
            public void IncrementSequence(int inc)
            {
                for(int i = 0; i < inc; i++)
                {
                    NextUInt();
                }
                SequenceLocation += inc;
            }
        }

        /// <summary>
        /// (Excellent quality, very fast, efficient, stateless)[5/5] Pseudo random generation class using PCG algorithm, which is stateless. 
        /// </summary>
        public class PCGRandom
        {
            public uint Seed { get; private set; }
            public PCGRandom(uint seed) { Seed = seed; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint From(uint index, uint seed = 0)
            {
                ulong state = (ulong)index * 6364136223846793005UL + seed;
                uint xorshifted = (uint)(((state >> 18) ^ state) >> 27);
                int rot = (int)(state >> 59);
                return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint GetUInt(uint index) => From(index, Seed);

            /// <summary>
            /// Gets an int [min,max]
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public int GetIntMinMax(int min, int max, uint index)
            {
                uint value = From(index, Seed);
                return RNGNumConverter.ToRangeInt(value, min, max);
            }
            /// <summary>
            /// Gets a float [min,max]
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public float GetFloatMinMax(float min, float max, uint index)
            {
                uint value = From(index, Seed);
                return RNGNumConverter.ToRangeFloat(value, min, max);
            }
            /// <summary>
            /// Gets a double [min,max]
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public double GetDoubleMinMax(double min, double max, uint index)
            {
                uint value = From(index, Seed);
                return RNGNumConverter.ToRangeDouble(value, min, max);
            }

            /// <summary>
            /// Changes the seed this instance is using.
            /// </summary>
            /// <param name="seed"></param>
            public void ChangeSeed(uint seed) => Seed = seed;
        }

        // ------------------ RNG Finalizers ------------------ //
        /// <summary>
        /// (Excellent collision resistance, very fast, efficient) Non-cryptographic hash helper using MurmurHash3 finalizer algorithm.
        /// </summary>
        public class MurmurHash3Finalizer
        {
            /// <summary>
            /// Can be used to decrease patterns in PRNG outputs, or can be used as its own PRNG as seen in <see cref=""/>
            /// </summary>
            /// <param name="input"></param>
            /// <param name="seed"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong Scramble(ulong input, ulong seed = 0)
            {
                ulong h = input + seed;
                h ^= h >> 33;
                h *= 0xff51afd7ed558ccdUL;
                h ^= h >> 33;
                h *= 0xc4ceb9fe1a85ec53UL;
                h ^= h >> 33;
                return h;
            }
        }

        // ------------------ Equalization Mappers ------------------ //
        /// <summary>
        /// <see cref="{T}"/> provided determines what RNG number type is mapped from. Map a list containing ranges and singletons of any RNG supported numeric type to ulongs using <see cref="EqualizedRNGMapping{T}.ConstructFrom(List{object}, int)"/>. Then, pick a random ulong from <see cref="EqualizedRNGMapping{T}.Minimum"/> and <see cref="EqualizedRNGMapping{T}.Maximum"/> and retrieve the value it would point to through <see cref="EqualizedRNGMapping{T}.GetRNGInFromMapping(ulong)"/>.
        /// </summary>

        public class EqualizedRNGMapping<T> where T : struct, IComparable<T>
        {
            const string NotSupportedErr = "EqualizedRNGRange only supports numeric RNG types: int, float, and double.";

            private readonly List<(Range<ulong> key, object value)> integerMapping = new();
            private readonly Type RNGNumberType = null;
            private int cachedResolution = -1;

            public bool Constructed { get; private set; } = false;
            public readonly ulong Minimum = 0;
            public ulong Maximum { get; private set; } = 0;

            public EqualizedRNGMapping()
            {
                if (typeof(T) != typeof(int) && typeof(T) != typeof(float) && typeof(T) != typeof(double))
                    throw new NotSupportedException(NotSupportedErr);

                RNGNumberType = typeof(T);
            }

            /// <summary>
            /// Constructs an equalized mapping from singletons and ranges. 
            /// </summary>
            public void ConstructFrom(List<object> rngRange, int constResolution = -1)
            {
                if (RNGNumberType == null) throw new NotSupportedException(NotSupportedErr + " This instance has already been invalidated.");
                if (Constructed) throw new InvalidOperationException("This EqualizedRNGRange has already been constructed, only once allowed.");
                Constructed = true;

                if (RNGNumberType == typeof(int)) ConstructIntMapping(rngRange);
                else if (RNGNumberType == typeof(float)) ConstructFloatMapping(rngRange, constResolution);
                else if (RNGNumberType == typeof(double)) ConstructDoubleMapping(rngRange, constResolution);
            }

            private void ConstructIntMapping(List<object> rngRange)
            {
                ulong cursor = 0;

                foreach (object value in rngRange)
                {
                    Type valueType = value.GetType();
                    ulong min = cursor;
                    ulong max = 0;

                    if (valueType == typeof(int))
                    {
                        max = min;
                        integerMapping.Add((Range<ulong>.From(min, max), value));
                        cursor = max + 1;
                    }
                    else if (valueType == typeof(Range<int>))
                    {
                        Range<int> r = (Range<int>)value;
                        if (!r.IsValid) continue;

                        if (r.min == r.max) max = min; // treat as singleton
                        else max = min + (ulong)(r.max - r.min);

                        integerMapping.Add((Range<ulong>.From(min, max), value));
                        cursor = max + 1;
                    }
                }

                Maximum = cursor == 0 ? 0 : cursor - 1;
            }

            private void ConstructFloatMapping(List<object> rngRange, int constResolution)
            {
                ulong cursor = 0;
                int resolution = constResolution != -1 ? constResolution :
                                 cachedResolution != -1 ? cachedResolution :
                                 (cachedResolution = (int)math.round(Mathg.GetNumericUnitResolution<float>()));

                foreach (object value in rngRange)
                {
                    Type valueType = value.GetType();
                    ulong min = cursor;
                    ulong max = 0;

                    if (valueType == typeof(float))
                    {
                        max = min + (ulong)resolution;
                        integerMapping.Add((Range<ulong>.From(min, max), value));
                        cursor = max + 1;
                    }
                    else if (valueType == typeof(Range<float>))
                    {
                        Range<float> r = (Range<float>)value;
                        if (!r.IsValid) continue;

                        if (r.min == r.max) max = min;
                        else max = min + (ulong)((r.max - r.min) * resolution);

                        integerMapping.Add((Range<ulong>.From(min, max), value));
                        cursor = max + 1;
                    }
                }

                Maximum = cursor == 0 ? 0 : cursor - 1;
            }

            private void ConstructDoubleMapping(List<object> rngRange, int constResolution)
            {
                ulong cursor = 0;
                int resolution = constResolution != -1 ? constResolution :
                                 cachedResolution != -1 ? cachedResolution :
                                 (cachedResolution = (int)math.round(Mathg.GetNumericUnitResolution<double>()));

                foreach (object value in rngRange)
                {
                    Type valueType = value.GetType();
                    ulong min = cursor;
                    ulong max = 0;

                    if (valueType == typeof(double))
                    {
                        max = min + (ulong)resolution;
                        integerMapping.Add((Range<ulong>.From(min, max), value));
                        cursor = max + 1;
                    }
                    else if (valueType == typeof(Range<double>))
                    {
                        Range<double> r = (Range<double>)value;
                        if (!r.IsValid) continue;

                        if (r.min == r.max) max = min;
                        else max = min + (ulong)((r.max - r.min) * resolution);

                        integerMapping.Add((Range<ulong>.From(min, max), value));
                        cursor = max + 1;
                    }
                }

                Maximum = cursor == 0 ? 0 : cursor - 1;
            }

            /// <summary>
            /// Maps an equalized ulong index back to its RNG value or range.
            /// </summary>
            /// <param name="equalizedValue"></param>
            /// <returns>Range, singleton, or null if something went wrong.</returns>
            /// <exception cref="NotSupportedException"></exception>
            /// <exception cref="InvalidOperationException"></exception>
            public object GetRNGInFromMapping(ulong equalizedValue)
            {
                if (RNGNumberType == null) throw new NotSupportedException(NotSupportedErr + " This instance has already been invalidated.");
                if (!Constructed) throw new InvalidOperationException("This instance has not yet been constructed, therefore it cannot be used.");

                int low = 0;
                int high = integerMapping.Count - 1;

                while (low <= high)
                {
                    int mid = low + (high - low) / 2;
                    var mapping = integerMapping[mid];

                    if (mapping.key.InRange(equalizedValue)) return mapping.value;
                    if (equalizedValue < mapping.key.min) high = mid - 1;
                    else low = mid + 1;
                }

                return null;
            }
        }

        // ------------------ Other ------------------ //
        /// <summary>
        /// Generates a random number using a seed. The same seed will always result in a variant of the same number no matter the min/max (its just shifted to fit between restrictions).
        /// </summary>
        [Obsolete("renamed to Randomization")]
        public static class SeededRandom
        {
            public static int GenerateInt(int Min, int Max, int Seed)
            {
                System.Random random = new System.Random(Seed);
                return random.Next(Min, Max + 1);
            }
            public static float GenerateFloat(float Min, float Max, int Seed)
            {
                System.Random random = new System.Random(Seed);
                return (float)(Min + random.NextDouble() * (Max - Min));
            }
            public static double GenerateDouble(double Min, double Max, int Seed)
            {
                System.Random random = new System.Random(Seed);
                return (Min + random.NextDouble() * (Max - Min));
            }
            public static Vector2 GenerateXY(float Min, float Max, int Seed)
            {
                System.Random random = new System.Random(Seed);
                return new Vector2((float)(Min + random.NextDouble() * (Max - Min)), (float)(Min + random.NextDouble() * (Max - Min)));
            }
        }

        /// <summary>
        /// Helper for RNG output conversion. Mostly internal but exposed anyway.
        /// </summary>
        public static class RNGNumConverter
        {
            // 01 conversions
            public static float ToFloat01(uint value) => (value & 0xFFFFFF) / (float)0xFFFFFF;
            public static float ToFloat01(ulong value) => (value >> 40) / (float)0xFFFFFF;
            public static double ToDouble01(uint value) => value / (double)uint.MaxValue;
            public static double ToDouble01(ulong value) => value / (double)ulong.MaxValue;

            // To range
            public static int ToRangeInt(uint value, int min, int max) => min + (int)(value % (uint)(max - min + 1));
            public static int ToRangeInt(ulong value, int min, int max) => min + (int)(value % (ulong)(max - min + 1));
            public static ulong ToRangeULong(ulong value, ulong min, ulong max) => min + (value % (max - min + 1));
            public static double ToRangeDouble(uint value, double min, double max) => min + ToDouble01(value) * (max - min);
            public static double ToRangeDouble(ulong value, double min, double max) => min + ToDouble01(value) * (max - min);
            public static float ToRangeFloat(uint value, float min, float max) => min + ToFloat01(value) * (max - min);
            public static float ToRangeFloat(ulong value, float min, float max) => min + ToFloat01(value) * (max - min);
        }

        /// <summary>
        /// Helper for noise algorithms. Mostly internal but exposed anyway.
        /// </summary>
        public static class NoiseUtils
        {
            /// <summary>
            /// Quintic fade
            /// </summary>
            /// <param name="t"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float Lerp(float a, float b, float t) => a + t * (b - a);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong Hash2i(int x, int y, ulong seed)
            {
                ulong h = (ulong)(x * 374761393 + y * 668265263) ^ seed;
                return MurmurHash3Finalizer.Scramble(h);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float HashFloat(int x, int y, ulong seed)
            {
                return (Hash2i(x, y, seed) & 0xFFFFFF) / (float)0x1000000;
            }
        }

        /// <summary>
        /// Randomization utilities, anything you could need when it comes to randomness. Supports LCG pseudo and crypto ("true").
        /// </summary>
        public static class Randomization
        {
            // IMPORTANT NOTE: OFFSET NORMAL PSEUDO GENERATORS BY 1 IN SEQUENCE, FIRST NUMBER IS RESERVED FOR STATICS.

            #region Pseudo
            /// <summary>
            /// Returns a random integer between the min and max using a seed. The same seed will always give you the same int. (Based on <see cref="System.Random"/>)
            /// </summary>
            /// <param name="Min">The minimum value</param>
            /// <param name="Max">The maximum value</param>
            /// <param name="Seed">The seed</param>
            /// <returns></returns>
            public static int GenerateStaticInt(int Min, int Max, int Seed)
            {
                _validateRange(Min, Max);
                System.Random random = new System.Random(Seed);
                return random.Next(Min, Max + 1);
            }

            /// <summary>
            /// Returns a random float between the min and max using a seed. The same seed will always give you the same float. (Based on <see cref="System.Random"/>)
            /// </summary>
            /// <param name="Min">The minimum value</param>
            /// <param name="Max">The maximum value</param>
            /// <param name="Seed">The seed</param>
            /// <returns></returns>
            public static float GenerateStaticFloat(float Min, float Max, int Seed)
            {
                _validateRange(Min, Max);
                System.Random random = new System.Random(Seed);
                return (float)(Min + random.NextDouble() * (Max - Min + float.Epsilon));
            }

            /// <summary>
            /// Returns a random double between the min and max using a seed. The same seed will always give you the same double. (Based on <see cref="System.Random"/>)
            /// </summary>
            /// <param name="Min">The minimum value</param>
            /// <param name="Max">The maximum value</param>
            /// <param name="Seed">The seed</param>
            /// <returns></returns>
            public static double GenerateStaticDouble(double Min, double Max, int Seed)
            {
                _validateRange(Min, Max);
                System.Random random = new System.Random(Seed);
                return (Min + random.NextDouble() * (Max - Min + double.Epsilon));
            }

            /// <summary>
            /// Returns a random Vector2 using a seed. The same seed will always give you the same Vector2. (Based on <see cref="System.Random"/>)
            /// </summary>
            /// <param name="Min">The minimum value</param>
            /// <param name="Max">The maximum value</param>
            /// <param name="Seed">The seed</param>
            /// <returns></returns>
            public static Vector2 GenerateStaticXY(float Min, float Max, int Seed)
            {
                _validateRange(Min, Max);
                System.Random random = new System.Random(Seed);
                int shifted = (Seed == int.MinValue) ? int.MaxValue : math.abs(Seed) - 1;
                System.Random random2 = new System.Random(shifted);
                return new Vector2((float)(Min + random.NextDouble() * (Max - Min + float.Epsilon)), (float)(Min + random2.NextDouble() * (Max - Min + float.Epsilon)));
            }
            #endregion

            #region Crypto
            /// <summary>
            /// Generates a truely random int seed using Cryptography. Can be negative.
            /// </summary>
            /// <returns></returns>
            public static int GetRandomSeed32()
            {
                byte[] buffer = new byte[4];
                System.Security.Cryptography.RandomNumberGenerator.Fill(buffer);
                return BitConverter.ToInt32(buffer, 0);
            }

            /// <summary>
            /// Generates a truely random uint seed using Cryptography. Cannot be negative.
            /// </summary>
            /// <returns></returns>
            public static uint GetRandomSeedU32()
            {
                byte[] buffer = new byte[4];
                System.Security.Cryptography.RandomNumberGenerator.Fill(buffer);
                return BitConverter.ToUInt32(buffer, 0);
            }

            /// <summary>
            /// Generates a truely random ulong seed using Cryptography. Cannot be negative.
            /// </summary>
            /// <returns></returns>
            public static ulong GetRandomSeed64()
            {
                byte[] buffer = new byte[8];
                System.Security.Cryptography.RandomNumberGenerator.Fill(buffer);
                return BitConverter.ToUInt64(buffer, 0);
            }

            /// <summary>
            /// Generates a truely random int between [min,max] using Cryptography.
            /// </summary>
            /// <param name="Min"></param>
            /// <param name="Max"></param>
            /// <returns></returns>
            public static int TrueGenerateInt(int Min, int Max)
            {
                _validateRange(Min, Max);
                return System.Security.Cryptography.RandomNumberGenerator.GetInt32(Min, Max + 1);
            }

            /// <summary>
            /// Generates a truely random float between [min,max] using Cryptography.
            /// </summary>
            /// <param name="Min"></param>
            /// <param name="Max"></param>
            /// <returns></returns>
            public static float TrueGenerateFloat(float Min, float Max)
            {
                _validateRange(Min, Max);
                byte[] buffer = new byte[4];
                System.Security.Cryptography.RandomNumberGenerator.Fill(buffer);
                uint asUInt = BitConverter.ToUInt32(buffer, 0);
                return (Min + (asUInt / (float)uint.MaxValue) * (Max - Min + float.Epsilon));
            }

            /// <summary>
            /// Generates a truely random double between [min,max] using Cryptography.
            /// </summary>
            /// <param name="Min"></param>
            /// <param name="Max"></param>
            /// <returns></returns>
            public static double TrueGenerateDouble(double Min, double Max)
            {
                _validateRange(Min, Max);
                byte[] buffer = new byte[8];
                System.Security.Cryptography.RandomNumberGenerator.Fill(buffer);
                ulong asULong = BitConverter.ToUInt64(buffer, 0);
                return (Min + (asULong / (double)ulong.MaxValue) * (Max - Min + double.Epsilon));
            }

            /// <summary>
            /// Generates a truely random Vector2 positon between [min,max] using Cryptography.
            /// </summary>
            /// <param name="Min"></param>
            /// <param name="Max"></param>
            /// <returns></returns>
            public static Vector2 TrueGenerateXY(float Min, float Max)
            {
                _validateRange(Min, Max);
                return new Vector2(TrueGenerateFloat(Min, Max), TrueGenerateFloat(Min, Max));
            }
            #endregion

            private static void _validateRange<T>(T Min, T Max) where T : IComparable<T>
            {
                if (Max.CompareTo(Min) < 0) throw new ArgumentException("Min must be <= Max");
            }
        }

        /// <summary>
        /// Noise functions. All outputs are auto-normalized to [0,1] ranges
        /// </summary>
        public static class Noise
        {
            /// <summary>
            /// Simple value noise for blobby but smooth textures.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="scale"></param>
            /// <param name="seed"></param>
            /// <returns></returns>
            public static float ValueNoise2D(float x, float y, float scale, ulong seed)
            {
                x *= scale;
                y *= scale;
                int x0 = (int)math.floor(x);
                int y0 = (int)math.floor(y);
                int x1 = x0 + 1;
                int y1 = y0 + 1;

                float sx = NoiseUtils.Fade(x - x0);
                float sy = NoiseUtils.Fade(y - y0);


                float n0 = NoiseUtils.HashFloat(x0, y0, seed);
                float n1 = NoiseUtils.HashFloat(x1, y0, seed);
                float ix0 = NoiseUtils.Lerp(n0, n1, sx);


                n0 = NoiseUtils.HashFloat(x0, y1, seed);
                n1 = NoiseUtils.HashFloat(x1, y1, seed);
                float ix1 = NoiseUtils.Lerp(n0, n1, sx);

                return NoiseUtils.Lerp(ix0, ix1, sy);
            }
            public static class Gradient
            {
                private static readonly (int, int)[] Grad2 = {(1,1),(-1,1),(1,-1),(-1,-1),(1,0),(-1,0),(0,1),(0,-1)};
                private static float GradDot(int hash, float x, float y)
                {
                    var g = Grad2[hash & 7];
                    return g.Item1 * x + g.Item2 * y;
                }

                /// <summary>
                /// Perlin-style graident noise for creating smooth, natural, flowing patterns.
                /// </summary>
                /// <param name="x"></param>
                /// <param name="y"></param>
                /// <param name="scale"></param>
                /// <param name="seed"></param>
                /// <returns></returns>
                public static float PerlinNoise2D(float x, float y, float scale, ulong seed)
                {
                    x *= scale;
                    y *= scale;
                    int x0 = (int)math.floor(x);
                    int y0 = (int)math.floor(y);
                    int x1 = x0 + 1;
                    int y1 = y0 + 1;


                    float sx = NoiseUtils.Fade(x - x0);
                    float sy = NoiseUtils.Fade(y - y0);


                    ulong h00 = NoiseUtils.Hash2i(x0, y0, seed);
                    ulong h10 = NoiseUtils.Hash2i(x1, y0, seed);
                    ulong h01 = NoiseUtils.Hash2i(x0, y1, seed);
                    ulong h11 = NoiseUtils.Hash2i(x1, y1, seed);


                    float n00 = GradDot((int)h00, x - x0, y - y0);
                    float n10 = GradDot((int)h10, x - x1, y - y0);
                    float n01 = GradDot((int)h01, x - x0, y - y1);
                    float n11 = GradDot((int)h11, x - x1, y - y1);


                    float ix0 = NoiseUtils.Lerp(n00, n10, sx);
                    float ix1 = NoiseUtils.Lerp(n01, n11, sx);


                    return (NoiseUtils.Lerp(ix0, ix1, sy) + 1) * 0.5f;
                }
            }
            
            private static readonly (float, float)[] Grad2_Simplex = { (1, 1), (-1, 1), (1, -1), (-1, -1), (1, 0), (-1, 0), (0, 1), (0, -1) };

            /// <summary>
            /// Higher quality perlin noise with less directional bias and runs more efficiently. Reports better visual quality than perlin.
            /// </summary>
            /// <param name="xin"></param>
            /// <param name="yin"></param>
            /// <param name="scale"></param>
            /// <param name="seed"></param>
            /// <returns></returns>
            public static float SimplexNoise2D(float xin, float yin, float scale, ulong seed)
            {
                xin *= scale;
                yin *= scale;


                float s = (xin + yin) * 0.366025403f;
                int i = (int)math.floor(xin + s);
                int j = (int)math.floor(yin + s);


                float t = (i + j) * 0.211324865f;
                float X0 = i - t;
                float Y0 = j - t;
                float x0 = xin - X0;
                float y0 = yin - Y0;


                int i1 = x0 > y0 ? 1 : 0;
                int j1 = x0 > y0 ? 0 : 1;


                float x1 = x0 - i1 + 0.211324865f;
                float y1 = y0 - j1 + 0.211324865f;
                float x2 = x0 - 1f + 2f * 0.211324865f;
                float y2 = y0 - 1f + 2f * 0.211324865f;


                ulong h0 = NoiseUtils.Hash2i(i, j, seed);
                ulong h1 = NoiseUtils.Hash2i(i + i1, j + j1, seed);
                ulong h2 = NoiseUtils.Hash2i(i + 1, j + 1, seed);


                float n0, n1, n2;


                float t0 = 0.5f - x0 * x0 - y0 * y0;
                if (t0 < 0) n0 = 0; else { t0 *= t0; var g = Grad2_Simplex[(int)h0 & 7]; n0 = t0 * t0 * (g.Item1 * x0 + g.Item2 * y0); }


                float t1 = 0.5f - x1 * x1 - y1 * y1;
                if (t1 < 0) n1 = 0; else { t1 *= t1; var g = Grad2_Simplex[(int)h1 & 7]; n1 = t1 * t1 * (g.Item1 * x1 + g.Item2 * y1); }


                float t2 = 0.5f - x2 * x2 - y2 * y2;
                if (t2 < 0) n2 = 0; else { t2 *= t2; var g = Grad2_Simplex[(int)h2 & 7]; n2 = t2 * t2 * (g.Item1 * x2 + g.Item2 * y2); }


                return BMathg.ShiftLinearMapping(70f * (n0 + n1 + n2), -1, 1, 0, 1);
            }

            /// <summary>
            /// AKA Cellular Noise, it produces natural cell-like patterns.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="scale"></param>
            /// <param name="seed"></param>
            /// <returns></returns>
            public static float WorleyNoise2D(float x, float y, float scale, ulong seed)
            {
                x *= scale;
                y *= scale;
                int xi = (int)math.floor(x);
                int yi = (int)math.floor(y);


                float minDist = float.MaxValue;
                for (int yy = -1; yy <= 1; yy++)
                {
                    for (int xx = -1; xx <= 1; xx++)
                    {
                        int cx = xi + xx;
                        int cy = yi + yy;
                        ulong h = NoiseUtils.Hash2i(cx, cy, seed);
                        float fx = (h & 0xFFFF) / (float)0x10000;
                        float fy = ((h >> 16) & 0xFFFF) / (float)0x10000;
                        float dx = (cx + fx) - x;
                        float dy = (cy + fy) - y;
                        float dist = dx * dx + dy * dy;
                        if (dist < minDist) minDist = dist;
                    }
                }

                return 1f - math.sqrt(minDist);
            }
            
            /// <summary>
            /// Noise layering helper.
            /// </summary>
            public static class Fractal
            {
                /// <summary>
                /// Fractal brownian motion. Helps create smooth layered noise that looks like rolling hills.
                /// </summary>
                /// <param name="noiseFunc"></param>
                /// <param name="x"></param>
                /// <param name="y"></param>
                /// <param name="scale"></param>
                /// <param name="seed"></param>
                /// <param name="octaves"></param>
                /// <param name="lacunarity"></param>
                /// <param name="gain"></param>
                /// <returns></returns>
                public static float FBM2D(Func<float, float, float, ulong, float> noiseFunc, float x, float y, float scale, ulong seed, int octaves = 4, float lacunarity = 2f, float gain = 0.5f)
                {
                    float total = 0, amplitude = 1, maxAmp = 0;
                    for (int i = 0; i < octaves; i++)
                    {
                        total += noiseFunc(x, y, scale, seed + (ulong)i * 1315423911UL) * amplitude;
                        maxAmp += amplitude;
                        amplitude *= gain;
                        scale *= lacunarity;
                    }
                    return total / maxAmp;
                }

                /// <summary>
                /// Takes absolute values of noise layers to create swirling/marble like patterns.
                /// </summary>
                /// <param name="noiseFunc"></param>
                /// <param name="x"></param>
                /// <param name="y"></param>
                /// <param name="scale"></param>
                /// <param name="seed"></param>
                /// <param name="octaves"></param>
                /// <param name="lacunarity"></param>
                /// <param name="gain"></param>
                /// <returns></returns>
                public static float Turbulence2D(Func<float, float, float, ulong, float> noiseFunc, float x, float y, float scale, ulong seed, int octaves = 4, float lacunarity = 2f, float gain = 0.5f)
                {
                    float total = 0, amplitude = 1, maxAmp = 0;
                    for (int i = 0; i < octaves; i++)
                    {
                        total += math.abs(noiseFunc(x, y, scale, seed + (ulong)i * 1315423911UL)) * amplitude;
                        maxAmp += amplitude;
                        amplitude *= gain;
                        scale *= lacunarity;
                    }
                    return total / maxAmp;
                }

                /// <summary>
                /// Similar to <see cref="Fractal.FBM2D()"/>, but with inverted ridges. This creates sharp mountain type maps.
                /// </summary>
                /// <param name="noiseFunc"></param>
                /// <param name="x"></param>
                /// <param name="y"></param>
                /// <param name="scale"></param>
                /// <param name="seed"></param>
                /// <param name="octaves"></param>
                /// <param name="lacunarity"></param>
                /// <param name="gain"></param>
                /// <returns></returns>
                public static float Ridged2D(Func<float, float, float, ulong, float> noiseFunc, float x, float y, float scale, ulong seed, int octaves = 4, float lacunarity = 2f, float gain = 0.5f)
                {
                    float total = 0, amplitude = 1, maxAmp = 0;
                    for (int i = 0; i < octaves; i++)
                    {
                        float n = 1f - math.abs(noiseFunc(x, y, scale, seed + (ulong)i * 1315423911UL));
                        total += n * amplitude;
                        maxAmp += amplitude;
                        amplitude *= gain;
                        scale *= lacunarity;
                    }
                    return total / maxAmp;
                }
            }
        }
        #endregion

        #region Mesh things
        /// <summary>
        /// Mesh math utilities
        /// </summary>
        public static class MeshOperations
        {
            /// <summary>
            /// This generates the correct tangent value for all vertices included in the triangle used.
            /// </summary>
            /// <param name="edge1"></param>
            /// <param name="edge2"></param>
            /// <param name="deltaUV1"></param>
            /// <param name="deltaUV2"></param>
            /// <param name="W"></param>
            /// <returns></returns>
            public static Vector4 GetTriangleTangent(Vector3 edge1, Vector3 edge2, Vector2 deltaUV1, Vector2 deltaUV2, float W = 1f)
            {
                float det = deltaUV1.x * deltaUV2.y - deltaUV2.x * deltaUV1.y;

                if (Mathf.Approximately(det, 0f))
                {
                    Vector3 fallback = Vector3.Cross(edge1, edge2).normalized;
                    Debug.LogWarning("[Debug] Used fallback tangent");
                    return new Vector4(fallback.x, fallback.y, fallback.z, W);
                }

                float f = 1.0f / det;

                Vector3 tangent;
                tangent.x = f * (deltaUV2.y * edge1.x - deltaUV1.y * edge2.x);
                tangent.y = f * (deltaUV2.y * edge1.y - deltaUV1.y * edge2.y);
                tangent.z = f * (deltaUV2.y * edge1.z - deltaUV1.y * edge2.z);

                tangent.Normalize();
                return new Vector4(tangent.x, tangent.y, tangent.z, W);
            }
        }

        /// <summary>
        /// Provides methods to visualize or gain extensive information about meshes.
        /// </summary>
        public static class MeshDebugger
        {
            /// <summary>
            /// Visualizes the normals of all vertices on a mesh. Optional drawing conditions can be specified.
            /// </summary>
            /// <param name="mesh"></param>
            /// <param name="transform"></param>
            /// <param name="length"></param>
            /// <param name="color"></param>
            /// <param name="drawCondition"><see cref="Vector3"/> arg1 is the vertex world position, <see cref="Vector3"/> arg2 is the vertex world normal.</param>
            public static void DrawNormals(Mesh mesh, Transform transform, float length = 0.1f, Color? color = null, Func<Vector3, Vector3, bool> drawCondition = null)
            {
                if (mesh == null || transform == null) return;

                Gizmos.color = color ?? Color.blue;

                var vertices = mesh.vertices;
                var normals = mesh.normals;

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 worldPos = transform.TransformPoint(vertices[i]);
                    Vector3 worldNormal = transform.TransformDirection(normals[i]);

                    if (drawCondition != null)
                    {
                        if (drawCondition(worldPos, worldNormal) == false) continue;
                    }

                    Gizmos.DrawLine(worldPos, worldPos + worldNormal * length);
                }
            }

            /// <summary>
            /// Visualizes the tangents of all vertices on a mesh. Optional drawing conditions.
            /// </summary>
            /// <param name="mesh"></param>
            /// <param name="transform"></param>
            /// <param name="length"></param>
            /// <param name="color"></param>
            /// <param name="drawCondition"><see cref="Vector3"/> arg1 is the vertex world position, <see cref="Vector3"/> arg2 is the tangent direction vector.</param>
            public static void DrawTangents(Mesh mesh, Transform transform, float length = 0.1f, Color? color = null, Func<Vector3, Vector3, bool> drawCondition = null)
            {
                if (mesh == null || transform == null) return;

                Gizmos.color = color ?? Color.red;

                var vertices = mesh.vertices;
                var tangents = mesh.tangents;

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 worldPos = transform.TransformPoint(vertices[i]);
                    Vector3 tangentDir = transform.TransformDirection((Vector3)tangents[i]);

                    if (drawCondition != null)
                    {
                        if (drawCondition(worldPos, tangentDir) == false) continue;
                    }

                    Gizmos.DrawLine(worldPos, worldPos + tangentDir * length);
                }
            }

            /// <summary>
            /// Visualizes the winding order of triangles on a mesh. Optional drawing conditions.
            /// </summary>
            /// <param name="mesh"></param>
            /// <param name="transform"></param>
            /// <param name="length"></param>
            /// <param name="color"></param>
            /// <param name="drawConditions"><see cref="Vector3"/> arg1 is the center of the triangle in world pos, <see cref="Vector3"/> arg2 is the direction the winding order makes it face.</param>
            public static void DrawTriangleWindings(Mesh mesh, Transform transform, float length = 0.1f, Color? color = null, Func<Vector3, Vector3, bool> drawConditions = null)
            {
                if (mesh == null || transform == null) return;

                Gizmos.color = color ?? Color.yellow;

                var vertices = mesh.vertices;
                var triangles = mesh.triangles;

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    Vector3 v0 = transform.TransformPoint(vertices[triangles[i]]);
                    Vector3 v1 = transform.TransformPoint(vertices[triangles[i + 1]]);
                    Vector3 v2 = transform.TransformPoint(vertices[triangles[i + 2]]);

                    Vector3 center = (v0 + v1 + v2) / 3f;
                    Vector3 dir = Vector3.Cross(v1 - v0, v2 - v0).normalized;

                    if (drawConditions != null)
                    {
                        if (drawConditions(center, dir) == false) continue;
                    }

                    Gizmos.DrawLine(center, center + dir * length);
                }
            }

            // Mesh Debug Visualizer
#if UNITY_EDITOR
            [UnityEditor.InitializeOnLoad]
            internal class MeshDebugWindow : UnityEditor.EditorWindow
            {
                // Vars
                public static GameObject[] lastHeldSelections;
                public static MeshDebugWindow current = null;

                #region Editor Prefs
                // IDs
                const string PREFS_DebugRenderRadius = "StellerAPI.Windows.MeshDebugWindow.DebugRenderRadius";
                const string PREFS_ShowNormals = "StellerAPI.Windows.MeshDebugWindow.ShowNormals";
                const string PREFS_ShowWindingOrder = "StellerAPI.Windows.MeshDebugWindow.ShowWindingOrder";
                const string PREFS_ShowTangents = "StellerAPI.Windows.MeshDebugWindow.ShowTangents";
                const string PREFS_IsViewingSharedMesh = "StellerAPI.Windows.MeshDebugWindow.IsViewingSharedMesh";

                const string PREFS_TangentLength = "StellerAPI.Windows.MeshDebugWindow.TangentLength";
                const string PREFS_NormalLength = "StellerAPI.Windows.MeshDebugWindow.NormalLength";
                const string PREFS_WindingOrderLength = "StellerAPI.Windows.MeshDebugWindow.WindingOrderLength";

                const string PREFS_TangentColor = "StellerAPI.Windows.MeshDebugWindow.TangentColor";
                const string PREFS_NormalColor = "StellerAPI.Windows.MeshDebugWindow.NormalColor";
                const string PREFS_WindingOrderColor = "StellerAPI.Windows.MeshDebugWindow.WindingOrderColor";

                const string PREFS_VisualizerEnabled = "StellerAPI.Windows.MeshDebugWindow.VisualizerEnabled";
                const string PREFS_SelectVisualizationEnabled = "StellarAPI.Windows.MeshDebugWindow.SelectVisualizationEnabled";

                const string PREFS_StoredHeldGameObjectID = "StellarAPI.Windows.MeshDebugWindow.HeldSelection";

                // Vars
                public static float renderRadius;
                public static bool showTangents;
                public static bool showWindingOrder;
                public static bool showNormals;
                public static bool isViewingSharedMesh;

                public static float tangentLength;
                public static float normalLength;
                public static float windingOrderLength;

                private static string tangentColor;
                private static string normalColor;
                private static string windingOrderColor;

                public static Color tangentColorCached = Color.black;
                public static Color normalColorCached = Color.black;
                public static Color windingOrderColorCached = Color.black;

                public static bool visualizerEnabled;
                public static bool selectVisualizationEnabled;
                #endregion

                #region Callbacks
                [UnityEditor.MenuItem("Window/Mesh Debug")]
                public static void ShowWindow() // On opened
                {
                    var window = GetWindow<MeshDebugWindow>("Mesh Debug Visualizer");
                    window.minSize = new Vector2(430, 300);
                    window.maxSize = new Vector2(430, 300);
                }
                private MeshDebugWindow() // On opened or open after domain reload
                {
                    current = this;
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        MeshDebugVisualizerGizmoSelector.OnSelectionChanged();
                    };
                }

                static MeshDebugWindow() // At domain reload (EditorPref direct calls not allowed)
                {
                    UnityEditor.EditorApplication.delayCall += () => // After domain reload
                    {
                        renderRadius = UnityEditor.EditorPrefs.GetFloat(PREFS_DebugRenderRadius, 20f);
                        showTangents = UnityEditor.EditorPrefs.GetBool(PREFS_ShowTangents, true);
                        showWindingOrder = UnityEditor.EditorPrefs.GetBool(PREFS_ShowWindingOrder, true);
                        showNormals = UnityEditor.EditorPrefs.GetBool(PREFS_ShowNormals, true);
                        isViewingSharedMesh = UnityEditor.EditorPrefs.GetBool(PREFS_IsViewingSharedMesh, false);

                        tangentLength = UnityEditor.EditorPrefs.GetFloat(PREFS_TangentLength, 0.5f);
                        normalLength = UnityEditor.EditorPrefs.GetFloat(PREFS_NormalLength, 0.5f);
                        windingOrderLength = UnityEditor.EditorPrefs.GetFloat(PREFS_WindingOrderLength, 0.5f);

                        tangentColor = "#" + UnityEditor.EditorPrefs.GetString(PREFS_TangentColor, ColorUtility.ToHtmlStringRGBA(Color.red));
                        normalColor = "#" + UnityEditor.EditorPrefs.GetString(PREFS_NormalColor, ColorUtility.ToHtmlStringRGBA(Color.blue));
                        windingOrderColor = "#" + UnityEditor.EditorPrefs.GetString(PREFS_WindingOrderColor, ColorUtility.ToHtmlStringRGBA(Color.yellow));
                        ColorUtility.TryParseHtmlString(tangentColor, out tangentColorCached);
                        ColorUtility.TryParseHtmlString(normalColor, out normalColorCached);
                        ColorUtility.TryParseHtmlString(windingOrderColor, out windingOrderColorCached);

                        visualizerEnabled = UnityEditor.EditorPrefs.GetBool(PREFS_VisualizerEnabled, false);
                        selectVisualizationEnabled = UnityEditor.EditorPrefs.GetBool(PREFS_SelectVisualizationEnabled, true);

                        // Re-retrieve held selections
                        string saved = UnityEditor.EditorPrefs.GetString(PREFS_StoredHeldGameObjectID, string.Empty);
                        if (saved != string.Empty)
                        {
                            string[] allObjectIds = saved.Split(',');

                            List<GameObject> GameObjects = new();
                            foreach (var gameObject in ObjectHandlingExtensions.ObjectHandlingExtensions.RetrieveAll())
                            {
                                if (allObjectIds.Contains(gameObject.GetInstanceID().ToString())) GameObjects.Add(gameObject);
                            }

                            lastHeldSelections = GameObjects.ToArray();
                            foreach (var gameObject in GameObjects)
                            {
                                if (gameObject.GetComponent<MeshDebugVisualizerGizmo>() == null && MeshDebugWindow.visualizerEnabled)
                                {
                                    gameObject.AddComponent<MeshDebugVisualizerGizmo>();
                                }
                            }
                        }
                    };
                }
                #endregion

                // Methods
                private static void HeldSelectionChanged()
                {
                    if (lastHeldSelections != null)
                    {
                        foreach (var obj in lastHeldSelections)
                        {
                            var old = obj.GetComponent<MeshDebugVisualizerGizmo>();
                            if (old) Object.DestroyImmediate(old);
                        }
                    }

                    lastHeldSelections = UnityEditor.Selection.gameObjects.Where(obj_ => obj_ != null && obj_.scene.IsValid() && obj_.activeInHierarchy).ToArray();
                    if (lastHeldSelections.Length > 0)
                    {
                        string save = string.Empty;
                        foreach (var obj in lastHeldSelections)
                        {
                            if (obj.GetComponent<MeshDebugVisualizerGizmo>() == null && MeshDebugWindow.visualizerEnabled) obj.AddComponent<MeshDebugVisualizerGizmo>();
                            save += obj.GetInstanceID() + ",";
                        }
                        UnityEditor.EditorPrefs.SetString(PREFS_StoredHeldGameObjectID, save);
                    }
                    else UnityEditor.EditorPrefs.SetString(PREFS_StoredHeldGameObjectID, string.Empty);
                }

                // UI
                private void OnGUI()
                {
                    if (!Application.isPlaying)
                    {
                        UnityEditor.EditorGUILayout.HelpBox("This tool is only usable at runtime!", UnityEditor.MessageType.Warning);
                        return;
                    }

                    UnityEditor.EditorGUILayout.BeginVertical();

                    UnityEditor.EditorGUILayout.LabelField("Select a GameObject that has a MeshFilter to see its mesh debug!");

                    UnityEditor.EditorGUI.BeginChangeCheck();
                    visualizerEnabled = UnityEditor.EditorGUILayout.Toggle("Visualizer Enabled", visualizerEnabled);
                    selectVisualizationEnabled = UnityEditor.EditorGUILayout.Toggle("Show Selection Visuals", selectVisualizationEnabled);
                    if (UnityEditor.EditorGUI.EndChangeCheck())
                    {
                        UnityEditor.EditorPrefs.SetBool(PREFS_VisualizerEnabled, visualizerEnabled);
                        UnityEditor.EditorPrefs.SetBool(PREFS_SelectVisualizationEnabled, selectVisualizationEnabled);
                        if (visualizerEnabled)
                        {
                            MeshDebugVisualizerGizmoSelector.OnSelectionChanged();
                        }
                    }

                    UnityEditor.EditorGUI.BeginChangeCheck();
                    renderRadius = Mathf.Max(UnityEditor.EditorGUILayout.FloatField("Debug Render Radius", renderRadius), 0);
                    if (UnityEditor.EditorGUI.EndChangeCheck()) UnityEditor.EditorPrefs.SetFloat(PREFS_DebugRenderRadius, renderRadius);

                    if (GUILayout.Button("Toggle Rendering Mode"))
                    {
                        UnityEditor.SceneView scene = UnityEditor.SceneView.lastActiveSceneView;
                        switch (scene.cameraMode.drawMode)
                        {
                            case (UnityEditor.DrawCameraMode.Textured): { scene.cameraMode = UnityEditor.SceneView.GetBuiltinCameraMode(UnityEditor.DrawCameraMode.TexturedWire); break; }
                            case (UnityEditor.DrawCameraMode.TexturedWire): { scene.cameraMode = UnityEditor.SceneView.GetBuiltinCameraMode(UnityEditor.DrawCameraMode.Wireframe); break; }
                            case (UnityEditor.DrawCameraMode.Wireframe): { scene.cameraMode = UnityEditor.SceneView.GetBuiltinCameraMode(UnityEditor.DrawCameraMode.Textured); break; }
                        }
                    }

                    if (UnityEditor.Selection.activeGameObject == null || UnityEditor.Selection.activeGameObject.GetComponent<MeshFilter>() == null)
                    {
                        if (GUILayout.Button("Clear Held Selection")) HeldSelectionChanged();
                        UnityEditor.EditorGUILayout.HelpBox("You must select an object before you can view settings.", UnityEditor.MessageType.Info);
                    }
                    else
                    {
                        if (GUILayout.Button("Hold Current Selections")) HeldSelectionChanged();

                        UnityEditor.EditorGUI.BeginChangeCheck();
                        isViewingSharedMesh = UnityEditor.EditorGUILayout.Toggle("Viewing Shared Mesh", isViewingSharedMesh);

                        UnityEditor.EditorGUILayout.Space();

                        showNormals = UnityEditor.EditorGUILayout.Toggle("Show Normals", showNormals);
                        UnityEditor.EditorGUILayout.BeginHorizontal();
                        normalLength = Mathf.Max(UnityEditor.EditorGUILayout.FloatField("Ray Length", normalLength), 0);
                        normalColorCached = UnityEditor.EditorGUILayout.ColorField("Ray Color", normalColorCached);
                        UnityEditor.EditorGUILayout.EndHorizontal();

                        UnityEditor.EditorGUILayout.Space();

                        showWindingOrder = UnityEditor.EditorGUILayout.Toggle("Show Winding Order", showWindingOrder);
                        UnityEditor.EditorGUILayout.BeginHorizontal();
                        windingOrderLength = Mathf.Max(UnityEditor.EditorGUILayout.FloatField("Ray Length", windingOrderLength), 0);
                        windingOrderColorCached = UnityEditor.EditorGUILayout.ColorField("Ray Color", windingOrderColorCached);
                        UnityEditor.EditorGUILayout.EndHorizontal();

                        UnityEditor.EditorGUILayout.Space();

                        showTangents = UnityEditor.EditorGUILayout.Toggle("Show Tangents", showTangents);
                        UnityEditor.EditorGUILayout.BeginHorizontal();
                        tangentLength = Mathf.Max(UnityEditor.EditorGUILayout.FloatField("Ray Length", tangentLength), 0);
                        tangentColorCached = UnityEditor.EditorGUILayout.ColorField("Ray Color", tangentColorCached);
                        UnityEditor.EditorGUILayout.EndHorizontal();

                        if (UnityEditor.EditorGUI.EndChangeCheck())
                        {
                            UnityEditor.EditorPrefs.SetBool(PREFS_IsViewingSharedMesh, isViewingSharedMesh);
                            UnityEditor.EditorPrefs.SetBool(PREFS_ShowNormals, showNormals);
                            UnityEditor.EditorPrefs.SetBool(PREFS_ShowWindingOrder, showWindingOrder);
                            UnityEditor.EditorPrefs.SetBool(PREFS_ShowTangents, showTangents);

                            UnityEditor.EditorPrefs.SetFloat(PREFS_NormalLength, normalLength);
                            UnityEditor.EditorPrefs.SetFloat(PREFS_WindingOrderLength, windingOrderLength);
                            UnityEditor.EditorPrefs.SetFloat(PREFS_TangentLength, tangentLength);

                            tangentColor = ColorUtility.ToHtmlStringRGBA(tangentColorCached);
                            normalColor = ColorUtility.ToHtmlStringRGBA(normalColorCached);
                            windingOrderColor = ColorUtility.ToHtmlStringRGBA(windingOrderColorCached);
                            UnityEditor.EditorPrefs.SetString(PREFS_TangentColor, tangentColor);
                            UnityEditor.EditorPrefs.SetString(PREFS_NormalColor, normalColor);
                            UnityEditor.EditorPrefs.SetString(PREFS_WindingOrderColor, windingOrderColor);
                        }
                    }

                    UnityEditor.EditorGUILayout.EndVertical();
                }
            }

            // Handle selections
            [UnityEditor.InitializeOnLoad]
            private static class MeshDebugVisualizerGizmoSelector
            {
                // Vars
                private static GameObject lastSelected;

                // Selection callback
                static MeshDebugVisualizerGizmoSelector()
                {
                    UnityEditor.Selection.selectionChanged += OnSelectionChanged;
                }
                internal static void OnSelectionChanged()
                {
                    if (!Application.isPlaying) return;
                    if (MeshDebugWindow.current != null) MeshDebugWindow.current.Repaint();

                    if (lastSelected != null)
                    {
                        if (MeshDebugWindow.lastHeldSelections == null)
                        {
                            var old = lastSelected.GetComponent<MeshDebugVisualizerGizmo>();
                            if (old) Object.DestroyImmediate(old);
                        }
                        else
                        {
                            if (!MeshDebugWindow.lastHeldSelections.Contains(lastSelected))
                            {
                                var old = lastSelected.GetComponent<MeshDebugVisualizerGizmo>();
                                if (old) Object.DestroyImmediate(old);
                            }
                        }
                    }

                    lastSelected = UnityEditor.Selection.activeGameObject;
                    if (lastSelected != null)
                    {
                        if (lastSelected.GetComponent<MeshDebugVisualizerGizmo>() == null && MeshDebugWindow.visualizerEnabled) lastSelected.AddComponent<MeshDebugVisualizerGizmo>();
                    }
                }
            }

            // Visualizer Component
            [DisallowMultipleComponent]
            private class MeshDebugVisualizerGizmo : MonoBehaviour
            {
                private void draw(GameObject gameObject, Transform transform)
                {
                    var meshFilter = gameObject.GetComponent<MeshFilter>();
                    if (meshFilter)
                    {
                        Mesh mesh;
                        if (MeshDebugWindow.isViewingSharedMesh) mesh = meshFilter.sharedMesh;
                        else mesh = meshFilter.mesh;

                        Vector3 cameraPosition = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;

                        if (MeshDebugWindow.showNormals)
                        {
                            DrawNormals(mesh, transform, MeshDebugWindow.normalLength, MeshDebugWindow.normalColorCached, (pos, normal) => (pos - cameraPosition).magnitude <= MeshDebugWindow.renderRadius);
                        }
                        if (MeshDebugWindow.showWindingOrder)
                        {
                            DrawTriangleWindings(mesh, transform, MeshDebugWindow.windingOrderLength, MeshDebugWindow.windingOrderColorCached, (triPos, dir) => (triPos - cameraPosition).magnitude <= MeshDebugWindow.renderRadius);
                        }
                        if (MeshDebugWindow.showTangents)
                        {
                            DrawTangents(mesh, transform, MeshDebugWindow.tangentLength, MeshDebugWindow.tangentColorCached, (pos, tanDir) => (pos - cameraPosition).magnitude <= MeshDebugWindow.renderRadius);
                        }
                    }
                }
                private void OnDrawGizmos()
                {
                    if (MeshDebugWindow.visualizerEnabled == false) return;
                    if (MeshDebugWindow.lastHeldSelections != null)
                    {
                        foreach (var obj in MeshDebugWindow.lastHeldSelections)
                        {
                            draw(obj, obj.transform);
                        }
                    }

                    if (UnityEditor.Selection.activeGameObject && MeshDebugWindow.selectVisualizationEnabled)
                    {
                        if (MeshDebugWindow.lastHeldSelections == null)
                        {
                            draw(UnityEditor.Selection.activeGameObject, UnityEditor.Selection.activeGameObject.transform);
                        }
                        else
                        {
                            if (!MeshDebugWindow.lastHeldSelections.Contains(UnityEditor.Selection.activeGameObject))
                            {
                                draw(UnityEditor.Selection.activeGameObject, UnityEditor.Selection.activeGameObject.transform);
                            }
                        }
                    }
                }
            }
#endif
        }
        #endregion

        /// <summary>
        /// An intelligent 2 dimensional pathing algorithm. Controlled externally via <see cref="VNScript"/>s which you MUST set in <see cref="VectorNetwork.ActiveVNScript"/>. Read <see cref="VNScript"/> documentation, and check all of your available data to gain the most control over the network. <see cref="VectorNetwork.Solver"/> is exposed for more manual control and potential uses outside the network :) .  Please use <see cref="Dispose"/> for proper cleanup when done using.
        /// </summary>
        public class VectorNetwork : IDisposable
        {
            bool disposed = false;

            #region Spawn Cycle Vars
            private readonly List<Branch> LastCycleActingNodeSpawnBranches = new(); // This keeps track of branches that will cause new nodes to spawn off of them.
            private readonly List<Branch> LastCycleActingBranchSpawnBranches = new(); // This keeps track of branches that will cause new branches to spawn off of them.
            private readonly List<Node> ThisCycleActingNodes = new();
            private readonly List<Branch> ThisCycleActingBranches = new();
            private int actingBranchNodeSpawnsIndex = 0;
            private int actingBranchBranchSpawnsIndex = 0;
            private int actingNodesIndex = 0;
            private int actingBranchIndex = 0;
            /// <summary>
            /// This is true if the network has run out of available actions. Ex: If all outmost paths decide that they wont continue forward with another <see cref="Node"/> or <see cref="Branch"/> then the network is dead, since nothing can generate any further. ALWAYS AVAILABLE
            /// </summary>
            public bool NoFurtherActions { get; private set; } = false;
            private int throttleCycleExecutionCount = 0;
            #endregion

            #region Internal Storage Vars
            private readonly List<Node> Nodes;
            private readonly List<Branch> Branches;
            private readonly NativeList<SegmentWrap> UnmanagedSegmentRefs;
            #endregion

            #region Exposed Vars Backing Fields
            private float _LineExtensionQuality = 3;
            private float _RadialCastQuality = 2;
            private float _ComputationOutputPrecision = 0.05f;
            private int _MaxCyclesPerSlice = 40;
            private int _ThreadDelayPerThrottle = 1;
            #endregion

            #region Exposed Public Vars
            /// <summary>
            /// Controls how many samples are taken per unit when determining max segment cast distance. DEFAULT: 3, ALWAYS CHANGEABLE
            /// </summary>
            public float LineExtensionQuality
            {
                get { return _LineExtensionQuality; }
                set { _LineExtensionQuality = math.max(0.01f, value); }
            }
            /// <summary>
            /// Controls how many samples are taken when doing broad radius sweeps using degree ranges. DEFAULT: 2, ALWAYS CHANGEABLE
            /// </summary>
            public float RadialCastQuality
            {
                get { return _RadialCastQuality; }
                set { _RadialCastQuality = math.max(0.01f, value); }
            }
            /// <summary>
            /// Controls how outputs are combined to simplify the output versatality. If two different directions (in order) have lengths within this distance of each other they will combine into the lower length. DEFAULT: 0.05, ALWAYS CHANGEABLE
            /// </summary>
            public float ComputationOutputPrecision
            {
                get { return _ComputationOutputPrecision; }
                set { _ComputationOutputPrecision = math.max(0, value); }
            }
            /// <summary>
            /// Should the network throttle cycles to avoid overloading the CPU. NOTE: Do not enable throttling if this network is running on a Unity Thread! DEFAULT: false, ALWAYS CHANGEABLE
            /// </summary>
            public bool UseThrottling = false;
            /// <summary>
            /// Max amount of cycles ran before throttling kicks in each time. DEFAULT: 40, ALWAYS CHANGEABLE, ONLY APPLIES IF: <see cref="VectorNetwork.UseThrottling"/> = true.
            /// </summary>
            public int MaxCyclesPerSlice
            {
                get { return _MaxCyclesPerSlice; }
                set { _MaxCyclesPerSlice = math.max(value, 1); }
            }
            /// <summary>
            /// The amount of milliseconds the algorithm throttles for each time the cycles are sliced. Recommended: leave at 1. DEFAULT: 1, ALWAYS CHANGABLE, ONLY APPLIES IF: <see cref="VectorNetwork.UseThrottling"/> = true.
            /// </summary>
            public int ThreadDelayPerThrottle
            {
                get { return _ThreadDelayPerThrottle; }
                set { _ThreadDelayPerThrottle = math.max(value, 1); }
            }

            /// <summary>
            /// Internally known as BufferRadius. The "thickness" or buffer zone around each segment to expand its hit radius. SET IN CONSTRUCTOR
            /// </summary>
            public readonly float LineThickness;
            /// <summary>
            /// The <see cref="Node"/> that lies at the beginning of the whole network, and ultimately starts off its expansion. SET IN CONSTRUCTOR
            /// </summary>
            public Node NetworkOrigin { get; private set; }
            /// <summary>
            /// The current step in the current lifecycle. A lifecycle consists of 4 periods/steps: 
            /// 1 - branches generate their nodes if instructed last lifecycle, 
            /// 2 - branches generate their branches if instructed last lifecycle, 
            /// 3 - new nodes from this lifecycle pick a branch count to generate and generate all of them per node, 
            /// 4 - all branches new to this lifecycle determine if they should spawn nodes and branches respectively on the next lifecycle.
            /// Always reads from what is currently happening, since network generation is entirely linear. ALWAYS AVAILABLE
            /// </summary>
            public int LifecyclePeriod { get; private set; }
            /// <summary>
            /// The <see cref="VNScript"/> currently in use to control the network's growth. Make sure to fill out all properties of the <see cref="VNScript"/> that are marked as "required", as failure to do so makes it automatically choose to not spawn/act when it finds empty instructions. DEFAULT: null, ALWAYS CHANGEABLE
            /// </summary>
            public VNScript ActiveVNScript = null;

            /// <summary>
            /// Gets all <see cref="Node"/>s in the entire network as an <see cref="IReadOnlyList{T}"/>. ALWAYS AVAILABLE
            /// </summary>
            public IReadOnlyList<Node> GetNodes => Nodes;
            /// <summary>
            /// Gets all <see cref="Branch"/>es in the entire network as an <see cref="IReadOnlyList{T}"/>. ALWAYS AVAILABLE.
            /// </summary>
            public IReadOnlyList<Branch> GetBranches => Branches;
            /// <summary>
            /// Exposes the internally stored <see cref="SegmentWrap"/>s unmanaged collection as a <see cref="NativeArray{T}"/>. Normally, these are just accessed via <see cref="Branch.ActualSegment"/> which uses a pointer index to reference a specific <see cref="SegmentWrap"/> from this collection. ALWAYS AVAILABLE
            /// </summary>
            public NativeArray<SegmentWrap> GetUnmanagedSegmentRefs => UnmanagedSegmentRefs.AsArray();
            /// <summary>
            /// Tracks how many lifecycles the network has completed. ALWAYS AVAILABLE
            /// </summary>
            public int LifecyclesCompleted { get; private set; } = 0;
            #endregion

            /// <param name="origin">The origin position in 2D space</param>
            /// <param name="baseDirection">Controls the direction that the network origin <see cref="Node"/> uses. For more info on what this direction means, see the docs for <see cref="Node"/></param>
            /// <param name="lineThickness">See <see cref="VectorNetwork.LineThickness"/></param>
            /// <param name="vnScript">See <see cref="VectorNetwork.ActiveVNScript"/></param>
            /// <param name="initialBranchCapacity">Controls how much space should be pre-allocated in the list that stores <see cref="Branch"/>es</param>
            /// <param name="initialNodeCapacity">Controls how much space should be pre-allocated in the list that stores <see cref="Node"/>s</param>
            /// <param name="giveNetworkOriginAFakeParent">Should the network origin <see cref="Node"/> be given a "fake" <see cref="Node.Parent"/>, since it doesnt typically have one? This is useful if your <see cref="VNScript"/> configuration relies on <see cref="Node"/>s ALWAYS having a valid parent</param>
            /// <param name="fakeOriginParentCustomComponents">Only applies if you are using a fake origin parent. Controls custom components that are pre-added to the fake origin parent <see cref="Branch"/></param>
            public VectorNetwork(Vector2 origin, Vector2 baseDirection, float lineThickness, VNScript vnScript = null, int initialBranchCapacity = 0, int initialNodeCapacity = 0, bool giveNetworkOriginAFakeParent = false, Dictionary<string,object> fakeOriginParentCustomComponents = null)
            {
                // Normalize baseDirection
                baseDirection = baseDirection.normalized;

                // Init storage
                Branches = new(initialBranchCapacity);
                Nodes = new(initialNodeCapacity);
                UnmanagedSegmentRefs = new NativeList<SegmentWrap>(initialBranchCapacity, Allocator.Persistent);

                // If simulated parent
                Branch simulatedParent = giveNetworkOriginAFakeParent ? new Branch(origin, baseDirection, false, this) : null;
                if(simulatedParent != null)
                {
                    SegmentWrap simulatedSegment = new SegmentWrap(new Segment(origin, origin), UnmanagedSegmentRefs.Length);
                    UnmanagedSegmentRefs.Add(simulatedSegment);
                    simulatedParent.SegmentPtrIndex = UnmanagedSegmentRefs.Length - 1;
                }
                if (fakeOriginParentCustomComponents != null)
                {
                    foreach (var component in fakeOriginParentCustomComponents) simulatedParent.CustomComponents.Add(component.Key, component.Value);
                }

                // Create origin
                NetworkOrigin = new Node(origin, baseDirection)
                {
                    FractalIndex = 1,
                    SurfaceDepthIndex = 0, // branches add +1 to depth index when generating off of nodes
                    Parent = simulatedParent
                };

                LineThickness = lineThickness;

                LifecyclePeriod = 3; // Start at node branch count period since periods 1 & 2 would do nothing anyway.
                ActiveVNScript = vnScript;
                ThisCycleActingNodes.Add(NetworkOrigin); // Make sure the Network Origin is set to act
                Nodes.Add(NetworkOrigin);
            }

            #region Generation Methods and Stuff
            /// <summary>
            /// Moves the network lifecycle forward.
            /// </summary>
            /// <returns><see cref="(bool PeriodCompleted, bool LifecycleCompleted, bool NoMoreNetworkActions)"/>. PeriodCompleted is true if the current period of the current lifecycle has just completed (<see cref="VectorNetwork.LifecyclePeriod"/> increments by 1). LifecycleCompleted is true if the current cycle has just completed (<see cref="VectorNetwork.LifecyclePeriod"/> reset to 1). NoMoreNetworkActions is true when the entire network has run out of available actions/"moves" and is essentially dead.</returns>
            public (bool PeriodCompleted, bool LifecycleCompleted, bool NoMoreNetworkActions) StepCycle()
            {
                if (NoFurtherActions) return (PeriodCompleted: false, LifecycleCompleted: false, NoMoreNetworkActions: true);

                bool periodCompleted = false;
                bool lifecycleCompleted = false;
                bool noMoreNetworkActions = false;

                if (UseThrottling)
                {
                    if(throttleCycleExecutionCount >= MaxCyclesPerSlice)
                    {
                        throttleCycleExecutionCount = 0;
                        Thread.Sleep(ThreadDelayPerThrottle);
                    }
                    throttleCycleExecutionCount++;
                }

                switch (LifecyclePeriod)
                {
                    case 1: // Branches that should spawn nodes spawn nodes
                        {
                            if(LastCycleActingNodeSpawnBranches.Count <= 0) { LifecyclePeriod = 2; periodCompleted = true; break; } // Cut short, no actors
                            
                            Branch thisBranch = LastCycleActingNodeSpawnBranches[actingBranchNodeSpawnsIndex];
                            actingBranchNodeSpawnsIndex++;

                            if (thisBranch.IsGeneratingNode)
                            {
                                Vector2 baseDirection = (thisBranch.ActualSegment.Segment.b - thisBranch.ActualSegment.Segment.a).normalized;
                                Node newNode = new Node(thisBranch.ActualSegment.Segment.b, baseDirection)
                                {
                                    Parent = thisBranch,
                                    FractalIndex = thisBranch.FractalDepth + 1,
                                    SurfaceDepthIndex = thisBranch.SurfaceDepth,
                                };
                                ActiveVNScript?.OnNodeCreation?.Invoke(this, newNode);

                                ThisCycleActingNodes.Add(newNode);
                                Nodes.Add(newNode);
                            }

                            if(actingBranchNodeSpawnsIndex > LastCycleActingNodeSpawnBranches.Count - 1) { LifecyclePeriod = 2; actingBranchNodeSpawnsIndex = 0; LastCycleActingNodeSpawnBranches.Clear(); periodCompleted = true; } // Early detect next cycle wont have any and move on
                            break;
                        }
                    case 2: // Branches that should spawn branches spawn branches
                        {
                            if(LastCycleActingBranchSpawnBranches.Count <= 0) { LifecyclePeriod = 3; periodCompleted = true; break; } // Cut short, no actors

                            Branch thisBranch = LastCycleActingBranchSpawnBranches[actingBranchBranchSpawnsIndex];
                            actingBranchBranchSpawnsIndex++;

                            if (thisBranch.IsGeneratingBranch)
                            {
                                if (ActiveVNScript?.NewBranchOnCycle != null) // script must have instructions otherwise no branches are permitted
                                {
                                    Vector2 baseDirection = (thisBranch.ActualSegment.Segment.b - thisBranch.ActualSegment.Segment.a).normalized;
                                    Branch newBranch = new Branch(thisBranch.ActualSegment.Segment.b, baseDirection, false, this)
                                    {
                                        Parent = thisBranch,
                                        Node = thisBranch.Node,
                                        FractalDepth = thisBranch.Node.FractalIndex,
                                        SurfaceDepth = thisBranch.SurfaceDepth + 1,
                                        LocalDepth = thisBranch.LocalDepth + 1
                                    };

                                    BranchValidation branchValidationInfo = ActiveVNScript.NewBranchOnCycle(this, newBranch);
                                    if (branchValidationInfo.CanSpawn)
                                    {
                                        newBranch.DeltaDegree = branchValidationInfo.DeltaAngle;
                                        baseDirection.Rotate((branchValidationInfo.DeltaAngle * -1) * math.TORADIANS); // *-1 is for normalizing to VectorNetwork rotation system
                                        
                                        SegmentWrap newSegment = new SegmentWrap(new Segment(newBranch.Origin, newBranch.Origin + baseDirection * branchValidationInfo.Length), UnmanagedSegmentRefs.Length);
                                        UnmanagedSegmentRefs.Add(newSegment);
                                        newBranch.SegmentPtrIndex = UnmanagedSegmentRefs.Length - 1;

                                        ThisCycleActingBranches.Add(newBranch);
                                        thisBranch.Node.Connected.Add(newBranch);
                                        Branches.Add(newBranch);
                                    }
                                }
                            }

                            if (actingBranchBranchSpawnsIndex > LastCycleActingBranchSpawnBranches.Count - 1) { LifecyclePeriod = 3; actingBranchBranchSpawnsIndex = 0; LastCycleActingBranchSpawnBranches.Clear(); periodCompleted = true; } // Early detect next cycle wont have any and move on
                            break;
                        }
                    case 3: // Nodes find out how many branches they should get and act on it per cycle until no more remain
                        {
                            if (ThisCycleActingNodes.Count <= 0) { LifecyclePeriod = 4; periodCompleted = true; break; } // Cut short, no actors

                            Node thisNode = ThisCycleActingNodes[actingNodesIndex];
                            if (ActiveVNScript?.NewBranchCountForNodeOnCycle != null && ActiveVNScript?.NewBranchOnCycle != null)
                            {
                                if (thisNode.RemainingUngeneratedBranches == -1) // signal for get count request
                                {
                                    int genBranchesCount = ActiveVNScript.NewBranchCountForNodeOnCycle(this, thisNode);
                                    thisNode.RemainingUngeneratedBranches = genBranchesCount;
                                    thisNode.InitialUngeneratedBranches = genBranchesCount;
                                }

                                if (thisNode.RemainingUngeneratedBranches > 0) // Do a branch if instructed
                                {
                                    Branch newBranch = new Branch(thisNode.Position, thisNode.BaseDirection, true, this)
                                    {
                                        Parent = thisNode.Parent,
                                        Node = thisNode,
                                        FractalDepth = thisNode.FractalIndex,
                                        SurfaceDepth = thisNode.SurfaceDepthIndex + 1,
                                        LocalDepth = 1
                                    };

                                    BranchValidation branchValidationInfo = ActiveVNScript.NewBranchOnCycle(this, newBranch);
                                    if (branchValidationInfo.CanSpawn)
                                    {
                                        Vector2 baseDirectionCopy = new Vector2(thisNode.BaseDirection.x, thisNode.BaseDirection.y);
                                        newBranch.DeltaDegree = branchValidationInfo.DeltaAngle;
                                        baseDirectionCopy.Rotate((branchValidationInfo.DeltaAngle * -1) * math.TORADIANS); // *-1 is for normalizing to VectorNetwork rotation system

                                        SegmentWrap newSegment = new SegmentWrap(new Segment(newBranch.Origin, newBranch.Origin + baseDirectionCopy * branchValidationInfo.Length), UnmanagedSegmentRefs.Length);
                                        UnmanagedSegmentRefs.Add(newSegment);
                                        newBranch.SegmentPtrIndex = UnmanagedSegmentRefs.Length - 1;

                                        ThisCycleActingBranches.Add(newBranch);
                                        thisNode.Connected.Add(newBranch);
                                        Branches.Add(newBranch);
                                    }

                                    thisNode.RemainingUngeneratedBranches--;
                                }
                            }
                            else thisNode.RemainingUngeneratedBranches = 0; // if bad script info just signal to move on with 0 as shown below

                            if (thisNode.RemainingUngeneratedBranches == 0) actingNodesIndex++; // Only move to the next node after this node has got a gen count and acted on them.
                            if (actingNodesIndex > ThisCycleActingNodes.Count - 1) { LifecyclePeriod = 4; actingNodesIndex = 0; ThisCycleActingNodes.Clear(); periodCompleted = true; } // Early detect next cycle wont have any and move on
                            break;
                        }
                    case 4: // Branches determine if they should spawn nodes and branches next cycle
                        {
                            if(ThisCycleActingBranches.Count <= 0) { LifecyclePeriod = 1; LifecyclesCompleted++; periodCompleted = true; lifecycleCompleted = true; break; }
                            
                            Branch thisBranch = ThisCycleActingBranches[actingBranchIndex];
                            actingBranchIndex++;

                            if (ActiveVNScript?.BranchSpawnsNewNodeNextCycle != null)
                            {
                                bool shouldCreateNode = ActiveVNScript.BranchSpawnsNewNodeNextCycle(this, thisBranch);
                                thisBranch.IsGeneratingNode = shouldCreateNode;
                                if(shouldCreateNode) LastCycleActingNodeSpawnBranches.Add(thisBranch);
                            }
                            if (ActiveVNScript?.BranchSpawnsAnotherBranchNextCycle != null)
                            {
                                bool shouldCreateBranch = ActiveVNScript.BranchSpawnsAnotherBranchNextCycle(this, thisBranch);
                                thisBranch.IsGeneratingBranch = shouldCreateBranch;
                                if (shouldCreateBranch) LastCycleActingBranchSpawnBranches.Add(thisBranch);
                            }

                            if(actingBranchIndex > ThisCycleActingBranches.Count - 1) { LifecyclePeriod = 1; LifecyclesCompleted++; actingBranchIndex = 0; ThisCycleActingBranches.Clear(); periodCompleted = true; lifecycleCompleted = true; }
                            break;
                        }
                }

                if (lifecycleCompleted) // if the lifecycle just finished and there are no actors for next cycle then the network has no more actions
                {
                    if (LastCycleActingNodeSpawnBranches.Count <= 0 && LastCycleActingBranchSpawnBranches.Count <= 0)
                    {
                        NoFurtherActions = true;
                        noMoreNetworkActions = true;
                    }
                }

                return (PeriodCompleted: periodCompleted, LifecycleCompleted: lifecycleCompleted, NoMoreNetworkActions: noMoreNetworkActions);
            }

            /// <summary>
            /// Automatically moves the lifecycle forward one lifecycle period.
            /// </summary>
            /// <returns><see cref="(bool LifecycleCompleted, bool NoMoreNetworkActions)"/>. LifecycleCompleted is true if the current cycle has just completed (<see cref="VectorNetwork.LifecyclePeriod"/> reset to 1). NoMoreNetworkActions is true when the entire network has run out of available actions/"moves" and is essentially dead.</returns>
            public (bool LifecycleCompleted, bool NoMoreNetworkActions) StepPeriod()
            {
                bool lifecycleCompleted = false;
                bool periodCompleted = false;

                while(!periodCompleted && !NoFurtherActions) // exit when network runs out of actions or period is completed
                {
                    var result = StepCycle();
                    periodCompleted = result.PeriodCompleted;
                    lifecycleCompleted = result.LifecycleCompleted;
                }
                return (LifecycleCompleted: lifecycleCompleted, NoMoreNetworkActions: NoFurtherActions);
            }

            /// <summary>
            /// Automatically completes one network lifecycle.
            /// </summary>
            /// <returns><see cref="Boolean"/> is true when the entire network has run out of available actions/"moves" and is essentially dead.</returns>
            public bool StepLifecycle()
            {
                bool lifecycleCompleted = false;
                while (!lifecycleCompleted && !NoFurtherActions) // exit when network runs out of actions or lifecycle is complete
                {
                    var result = StepPeriod();
                    lifecycleCompleted = result.LifecycleCompleted;
                }
                return NoFurtherActions;
            }


            private readonly List<int> segmentExcludeAllocation = new(); // exclusively for getExcludedSegments_()
            private void getExcludedSegments_(Branch branch)
            {
                segmentExcludeAllocation.Clear(); // free list
                if (branch.LocalDepth == 1) // For branches directly on the node, they may intersect with other branches directly on the node. Excludes all segments on the same node
                {
                    for (int i = 0; i < branch.Node.Connected.Count; i++)
                    {
                        segmentExcludeAllocation.Add(branch.Node.Connected[i].ActualSegment.ID);
                    }
                }
                if (branch.Parent != null) segmentExcludeAllocation.Add(branch.Parent.ActualSegment.ID); // Get node parent branch since it also intersects
            }
            /// <summary>
            /// Simplified exposure of the Solver.GetAvailableRanges() method. This allows you to get a range of movement you can choose from for where new branches should go to. This overload allows for a broad length and degree sweep (relies on <see cref="VectorNetwork.RadialCastQuality"/>).
            /// </summary>
            /// <param name="branchFrom"></param>
            /// <param name="lengthRange"></param>
            /// <param name="degreeRange"></param>
            /// <returns></returns>
            public List<BranchRange> ComputeRangeFor(Branch branch, Range<float> lengthRange, Range<float> degreeRange)
            {
                getExcludedSegments_(branch);
                return Solver.GetAvailableRanges(branch.Origin, branch.Direction, lengthRange, degreeRange, UnmanagedSegmentRefs, segmentExcludeAllocation, LineThickness, LineExtensionQuality, RadialCastQuality, ComputationOutputPrecision);
            }

            /// <summary>
            /// Simplified exposure of the Solver.GetAvailableRanges() method. This allows you to get a range of movement you can choose from for where new branches should go to. This overload allows for specific degree checks with each degree having its own max and min allowed lengths.
            /// </summary>
            /// <param name="branchFrom"></param>
            /// <param name="degLens"></param>
            /// <returns></returns>
            public List<DegLen> ComputeRangeFor(Branch branch, DegLen[] degLens)
            {
                getExcludedSegments_(branch);
                return Solver.GetAvailableRanges(branch.Origin, branch.Direction, degLens, UnmanagedSegmentRefs, segmentExcludeAllocation, LineThickness, LineExtensionQuality);
            }

            /// <summary>
            /// Simplified exposure of the Solver.GetAvailableRanges() method. This allows you to get a range of movement you can choose from for where new branches should go to. This overload allows for specific degree checks with every cast obeying the same inputted legth range.
            /// </summary>
            /// <param name="branchFrom"></param>
            /// <param name="lengthRange"></param>
            /// <param name="degrees"></param>
            /// <returns></returns>
            public List<DegLen> ComputeRangeFor(Branch branch, Range<float> lengthRange, float[] degrees)
            {
                getExcludedSegments_(branch);
                return Solver.GetAvailableRanges(branch.Origin, branch.Direction, lengthRange, degrees, UnmanagedSegmentRefs, segmentExcludeAllocation, LineThickness, LineExtensionQuality);
            }
            #endregion

            #region VectorNetwork Exclusive Objects
            /// <summary>
            /// A point of multiple branching paths. Fundamentally different than a <see cref="Branch"/>, its purpose is to allow for the creation of multiple <see cref="Branch"/>es all originating from a single point.
            /// </summary>
            public class Node
            {
                /// <summary>
                /// Custom components for this node. Allows you to store anything you want on the instance.
                /// </summary>
                public readonly Dictionary<string, object> CustomComponents = new();
                /// <summary>
                /// The position of the node, AKA origin of <see cref="Branch"/>es it spawns.
                /// </summary>
                public Vector2 Position { get; private set; }
                /// <summary>
                /// The direction which <see cref="Branch"/>es use as a starting point to add to their delta angles. Ex: If this points towards <see cref="Vector2.up"/> and a branch chooses a delta angle of 90, the branch will spawn facing towards <see cref="Vector2.right"/>. Delta angles use a normalization system (*-1) where negatives face left, and positives face right.
                /// </summary>
                public Vector2 BaseDirection { get; private set; }
                /// <summary>
                /// The branch this node originates from. If this value is null, it signifies that this node is the network origin.
                /// </summary>
                public Branch Parent { get; internal set; } = null;
                /// <summary>
                /// The index of this node's fractal depth. Called "index" instead of depth because nodes are the fracture point. It still works the same as <see cref="Branch.FractalDepth"/>.
                /// </summary>
                public int FractalIndex { get; internal set; }
                /// <summary>
                /// The surface depth passed (not incremented) from the parent branch of this node. Network origin has a value of 1 for this, since no parent branch would exist.
                /// </summary>
                public int SurfaceDepthIndex { get; internal set; }

                internal readonly List<Branch> Connected = new(); // Branches that originate from this node.
                /// <summary>
                /// Gives an <see cref="IReadOnlyList{T}"/> of all <see cref="Branch"/>es DIRECTLY connected to this node. If you want to find out if a branch is under the hierarchy of a node but not just connected directly, use <see cref="Branch.Node"/> from within that instance.
                /// </summary>
                public IReadOnlyList<Branch> GetConnected => Connected;

                /// <summary>
                /// How many branches this node has left to generate. Default is -1 to signal this node needs a count set and has not yet recieved it.
                /// </summary>
                public int RemainingUngeneratedBranches { get; internal set; } = -1;
                /// <summary>
                /// The initial amount of <see cref="Branch"/>es this node was instructed to generate. <see cref="Node.RemainingUngeneratedBranches"/> displays the amount remaining. Default is -1 to signal this node needs a count set and has not yet recieved it.
                /// </summary>
                public int InitialUngeneratedBranches { get; internal set; } = -1;

                internal Node(Vector2 position, Vector2 baseDirection)
                {
                    Position = position;
                    BaseDirection = baseDirection;
                }
            }
            /// <summary>
            /// A single segment of the network. Despite being called "Branch" it represents one individual line/piece.
            /// </summary>
            public class Branch
            {
                /// <summary>
                /// Custom components for this branch. Allows you to store anything you want on the instance.
                /// </summary>
                public readonly Dictionary<string, object> CustomComponents = new();
                /// <summary>
                /// <see cref="VectorNetwork.Node"/> this branch originates from. Usable at init time (<see cref="VNScript.NewBranchOnCycle"/>).
                /// </summary>
                public Node Node { get; internal set; }
                /// <summary>
                ///  The branch this branch is connected to. If directly connected to a <see cref="VectorNetwork.Node"/>, this value is the <see cref="VectorNetwork.Node"/>s parent. Usable at init time (<see cref="VNScript.NewBranchOnCycle"/>).
                /// </summary>
                public Branch Parent { get; internal set; }
                /// <summary>
                /// The amount of <see cref="VectorNetwork.Node"/>s deep this branch is. The network origin has a starting depth of 1. Usable at init time (<see cref="VNScript.NewBranchOnCycle"/>).
                /// </summary>
                public int FractalDepth { get; internal set; }
                /// <summary>
                ///  How many branches deep this branch is, counting every branch back to the network origin. Branches directly connected to the network origin start at 1. Usable at init time (<see cref="VNScript.NewBranchOnCycle"/>).
                /// </summary>
                public int SurfaceDepth { get; internal set; }
                /// <summary>
                /// How many branches deep this branch is, counting every branch back to the last <see cref="VectorNetwork.Node"/>. Branches directly connected to the <see cref="VectorNetwork.Node"/> start at 1. Usable at init time (<see cref="VNScript.NewBranchOnCycle"/>).
                /// </summary>
                public int LocalDepth { get; internal set; }
                /// <summary>
                /// The origin position of this branch. Usable at init time (<see cref="VNScript.NewBranchOnCycle"/>).
                /// </summary>
                public Vector2 Origin { get; private set; }
                /// <summary>
                /// The base direction this branch used. Explained more at <see cref="VectorNetwork.Node.BaseDirection"/>. Usable at init time (<see cref="VNScript.NewBranchOnCycle"/>).
                /// </summary>
                public Vector2 Direction { get; private set; } // Base direction (before delta change)
                /// <summary>
                /// Amount of degrees to turn FROM Direction where positive turns right and negative turns left. Normalization and more explained in <see cref="Node.BaseDirection"/>. NOT USABLE AT INIT TIME (<see cref="VNScript.NewBranchOnCycle"/>).
                /// </summary>
                public float DeltaDegree { get; internal set; }
                /// <summary>
                /// The actual line segment generated (a wrapped version of it). NOT USABLE AT INIT TIME (<see cref="VNScript.NewBranchOnCycle"/>).
                /// </summary>
                public SegmentWrap ActualSegment { get { return NetworkRef.UnmanagedSegmentRefs[SegmentPtrIndex]; } }
                /// <summary>
                /// If this branch was determined to spawn a node or not. Set before <see cref="IsGeneratingBranch"/>. NOT USABLE AT INIT TIME (<see cref="VNScript.NewBranchOnCycle"/>).
                /// </summary>
                public bool IsGeneratingNode { get; internal set; }
                /// <summary>
                /// If this branch was determined to stem into another branch or not. Set after <see cref="IsGeneratingNode"/> and will be unset if used before. NOT USABLE AT INIT TIME (<see cref="VNScript.NewBranchOnCycle"/>).
                /// </summary>
                public bool IsGeneratingBranch { get; internal set; }
                /// <summary>
                /// True if spawned from a node, false if spawned from another branch. Usable at init time (<see cref="VNScript.NewBranchOnCycle"/>).
                /// </summary>
                public bool SpawnedFromNode { get; private set; }

                internal int SegmentPtrIndex;
                private readonly VectorNetwork NetworkRef;

                internal Branch(Vector2 origin, Vector2 direction, bool spawnedFromNode, VectorNetwork networkRef)
                {
                    Origin = origin;
                    Direction = direction;
                    SpawnedFromNode = spawnedFromNode;
                    NetworkRef = networkRef;
                }
            }

            /// <summary>
            /// Vector Network "Script". Input <see cref="Func{TResult}"/>s to control how the network grows.
            /// </summary>
            public class VNScript
            {
                #region Conditionals
                /// <summary>
                /// REQUIRED! Called each time a node needs to know how many branches should now extend off of it when newly created. In: <see cref="VectorNetwork"/> calling network, <see cref="Node"/> calling node | Out: <see cref="Int32"/> branch generation count
                /// </summary>
                public Func<VectorNetwork, Node, int> NewBranchCountForNodeOnCycle { get; set; } = null;

                /// <summary>
                /// REQUIRED! Called every time a new branch is being prepared to generate, the output determines if the branch should generate and what its direction and length should be. In: <see cref="VectorNetwork"/> calling network, <see cref="Branch"/> the currently configuring branch before changes | Out: <see cref="BranchValidation"/> branch spawn info
                /// </summary>
                public Func<VectorNetwork, Branch, BranchValidation> NewBranchOnCycle { get; set; } = null;

                /// <summary>
                /// REQUIRED! Called at the end of the lifecycle after <see cref="VNScript.BranchSpawnsNewNodeNextCycle"/>. Determines if the current branch should have another branch come off of it. In: <see cref="VectorNetwork"/> calling network, <see cref="Branch"/> calling branch | Out: <see cref="Boolean"/> should spawn another branch
                /// </summary>
                public Func<VectorNetwork, Branch, bool> BranchSpawnsAnotherBranchNextCycle { get; set; } = null;

                /// <summary>
                /// REQUIRED! Called at the end of the lifecycle but before <see cref="VNScript.BranchSpawnsAnotherBranchNextCycle"/>. Determines if a new node will be created at the end of the current branch. In: <see cref="VectorNetwork"/> calling network, <see cref="Branch"/> calling branch | Out: <see cref="Boolean"/> should create node
                /// </summary>
                public Func<VectorNetwork, Branch, bool> BranchSpawnsNewNodeNextCycle { get; set; } = null;
                #endregion

                #region Callbacks
                /// <summary>
                /// Callback that fires whenever a new node is generated. In: <see cref="VectorNetwork"/> calling network, <see cref="Node"/> newly created node
                /// </summary>
                public Action<VectorNetwork, Node> OnNodeCreation { get; set; } = null;
                #endregion
            }

            /// <summary>
            /// Wrapped version of <see cref="Math.Segment"/> allowing for the storage of identification info. This is used to ensure connecting segments dont cut off all paths in solver calculations.
            /// </summary>
            public readonly struct SegmentWrap
            {
                /// <summary>
                /// The real <see cref="Math.Segment"/>.
                /// </summary>
                public readonly Segment Segment;
                /// <summary>
                /// The segment exclusion ID of this wrapped <see cref="Math.Segment"/>. Exposed for manual exclusion usage in Solver.GetAvailableRanges() calls.
                /// </summary>
                public readonly int ID;
                public SegmentWrap(Segment segment, int ID)
                {
                    Segment = segment;
                    this.ID = ID;
                }
            }

            /// <summary>
            /// Contains the range of lengths valid for a range of degrees.
            /// </summary>
            public struct BranchRange
            {
                public Range<float> degrees;
                public Range<float> length;
                public BranchRange(Range<float> length, Range<float> degrees)
                {
                    this.length = length;
                    this.degrees = degrees;
                }
            }

            /// <summary>
            /// Contains the range of lengths valid for a single degree.
            /// </summary>
            public struct DegLen
            {
                public float degree;
                public Range<float> lengthR;
                public DegLen(float degree, Range<float> lengthR)
                {
                    this.degree = degree;
                    this.lengthR = lengthR;
                }
            }

            /// <summary>
            /// Result for <see cref="VNScript"/>s to give the network information about a new <see cref="Branch"/>.
            /// </summary>
            public struct BranchValidation
            {
                public bool CanSpawn { get; set; }
                public float DeltaAngle { get; set; }
                public float Length { get; set; }
            }
            #endregion

            /// <summary>
            /// VectorNetwork internal algorithm solver. Available for use, it allows you to compute ranges of open space and the distance extended outward in any radial size / cone. Obstacles are segments.
            /// </summary>
            public static class Solver
            {
                // Hybrid computing values
                const int SEG_COMPUTATION_WORKLOAD_THLD = 128; // the amount of segments required to increment batching by 1
                const int SEG_COMPUTATION_BATCH_MIN = 16;
                const int SEG_COMPUTATION_BATCH_MAX = 128;

                const int RANGE_COMPUTATION_GROUP_FAC = 16; // factor for how many degrees of the initially available range are grouped into a batch at a time
                const int RANGE_COMPUTATION_BATCH_MIN = 4;
                const int RANGE_COMPUTATION_BATCH_MAX = 16;

                const float MERGE_COMPUTATION_PREALLOCATION_FAC = 0.16f; // factor that controls the intial capcity of the merge results list. Formula is as follows: (int)math.ceil(rangesCount / MERGE_COMPUTATION_PREALLOCATION_FAC)

                [BurstCompile]
                public struct GatherSegments : IJobParallelFor
                {
                    [ReadOnly] public NativeList<SegmentWrap> inSegments;
                    [ReadOnly][DeallocateOnJobCompletion] public NativeArray<int> inExcludedSegmentIDs;
                    [WriteOnly] public NativeQueue<Segment>.ParallelWriter outSegments;

                    public float validSegmentDistSqr;
                    public Vector2 origin;

                    public void Execute(int i)
                    {
                        SegmentWrap current = inSegments[i];
                        if (inExcludedSegmentIDs.Contains(current.ID)) return;

                        if ((origin - current.Segment.a).sqrMagnitude <= validSegmentDistSqr || (origin - current.Segment.b).sqrMagnitude <= validSegmentDistSqr)
                        {
                            outSegments.Enqueue(current.Segment);
                        }
                        else
                        {
                            float distSqr = Vec2Math.PointSegmentDistanceSqr(origin, current.Segment.a, current.Segment.b);
                            if (distSqr <= validSegmentDistSqr)
                            {
                                outSegments.Enqueue(current.Segment);
                            }
                        }
                    }
                }

                [BurstCompile]
                public struct GetRanges : IJobParallelFor
                {
                    [ReadOnly][DeallocateOnJobCompletion] public NativeArray<Segment> inSegments;
                    [WriteOnly] public NativeStream.Writer outRangesStream;

                    public Vector2 origin;
                    public Vector2 direction;
                    public Range<float> degreeRange;
                    public Range<float> lengthRange;

                    public float DegStepC;

                    public float LenIterationsStatic;
                    public float LenStepCStatic;

                    public float bufferRadius;
                    public float bufferSqr;

                    public void Execute(int i)
                    {
                        float DegStep = degreeRange.min + i * DegStepC;
                        if (DegStep > degreeRange.max + 1e-6f) return; // Exit if steps outside maximum range

                        Vector2 dir = new(direction.x, direction.y);
                        dir.Rotate((DegStep * -1) * math.TORADIANS); // DegStep *-1 is to normalize degrees from the VectorNetwork rotation system, which uses negative = left, positive = right

                        bool validLength = true;
                        float LenStep = lengthRange.max;
                        for (int i2 = 0; i2 < LenIterationsStatic; i2++)
                        {
                            bool intersected = false;
                            for (int i3 = 0; i3 < inSegments.Length; i3++)
                            {
                                Segment current = inSegments[i3];
                                Vector2 rayA = origin;
                                Vector2 rayB = origin + dir * LenStep;

                                if (bufferRadius <= 0f)
                                {
                                    intersected = Vec2Math.SegmentsIntersect(current.a, current.b, rayA, rayB);
                                }
                                else
                                {
                                    float distSqr = Vec2Math.SegmentSegmentDistanceSqr(current.a, current.b, rayA, rayB);
                                    if (distSqr <= bufferSqr + 1e-8f)
                                    {
                                        intersected = true;
                                        break;
                                    }
                                }
                            }

                            if (!intersected) break; // Exit if no intersections happened

                            LenStep -= LenStepCStatic;
                            if (LenStep < lengthRange.min - 1e-6f)
                            {
                                validLength = false;
                                break; // No valid lengths
                            }
                        }

                        outRangesStream.BeginForEachIndex(i);
                        if (validLength)
                        {
                            outRangesStream.Write(new DegLen(DegStep, Range<float>.From(lengthRange.min, LenStep)));
                        }
                        else
                        {
                            outRangesStream.Write(new DegLen(DegStep, Range<float>.From(0f, 0f)));
                        }
                        outRangesStream.EndForEachIndex();
                    }
                }

                /// <summary>
                /// Get full range of movement for ranges of numbers with specified quality.
                /// </summary>
                /// <param name="origin">Origin</param>
                /// <param name="direction">Direction</param>
                /// <param name="lengthRange">Min/Max length</param>
                /// <param name="degreeRange">Min/Max degree change</param>
                /// <param name="segments">Segments</param>
                /// <param name="bufferRadius">Line "thickness"</param>
                /// <param name="lengtheningQuality">Controls the number of samples when determining radial length. Greater than 1 increases sample count less than 1 lowers it. Must be greater than 0. Ex: 0.5 is 2 times less samples, 2 is 2 times more samples.</param>
                /// <param name="radialQuality">Controls the number of samples when iterating around the radial range. Greater than 1 increases sample count less than 1 lowers it. Must be greater than 0. Ex: 0.5 is 2 times less samples, 2 is 2 times more samples.</param>
                /// <param name="outputPrecisionThreshold">Controls range merging. If two numbers have max lengths with a distance less than or equal to this number, they will be merged into a single <see cref="BranchRange"/>.</param>
                /// <returns></returns>
                public static List<BranchRange> GetAvailableRanges(
                    Vector2 origin, Vector2 direction,
                    Range<float> lengthRange, Range<float> degreeRange,
                    NativeList<SegmentWrap> segments,
                    List<int> excludedSegmentIDs,
                    float bufferRadius,
                    float lengtheningQuality = 3, // > 0
                    float radialQuality = 2, // > 0
                    float outputPrecisionThreshold = 0.05f) // >= 0
                {
                    // Get applicable segments
                    float validSegmentDist = lengthRange.max + bufferRadius + 1e-6f;
                    Segment[] validSegments = ComputeValidSegments(segments, excludedSegmentIDs, origin, validSegmentDist);

                    // Get valid ranges
                    List<DegLen> ranges;
                    if (validSegments.Length > 0)
                    {
                        float bufferSqr = bufferRadius * bufferRadius;

                        float degSpan = degreeRange.max - degreeRange.min;
                        int DegIterations = math.max(1, (int)math.round(degSpan * radialQuality) + 1);
                        float DegStepC = (DegIterations > 1) ? (degSpan / (DegIterations - 1)) : 0f;

                        ranges = new List<DegLen>(DegIterations); // pre allocate

                        float lenSpan = lengthRange.max - lengthRange.min;
                        int LenIterationsStatic = math.max(1, (int)math.round(lenSpan * lengtheningQuality) + 1);
                        float LenStepCStatic = (LenIterationsStatic > 1) ? (lenSpan / (LenIterationsStatic - 1)) : 0f;

                        NativeStream gotRangesStream = new(DegIterations, Allocator.TempJob);
                        GetRanges getRangesJob = new()
                        {
                            inSegments = new NativeArray<Segment>(validSegments, Allocator.TempJob),
                            origin = origin,
                            direction = direction,
                            degreeRange = degreeRange,
                            lengthRange = lengthRange,
                            outRangesStream = gotRangesStream.AsWriter(),
                            DegStepC = DegStepC,
                            LenStepCStatic = LenStepCStatic,
                            LenIterationsStatic = LenIterationsStatic,
                            bufferRadius = bufferRadius,
                            bufferSqr = bufferSqr
                        };

                        int batchCount = math.clamp((int)math.round((float)DegIterations / (float)RANGE_COMPUTATION_GROUP_FAC), RANGE_COMPUTATION_BATCH_MIN, RANGE_COMPUTATION_BATCH_MAX);
                        JobHandle getRangesHandle = getRangesJob.Schedule(DegIterations, batchCount);
                        getRangesHandle.Complete();

                        var reader = gotRangesStream.AsReader();
                        for (int i = 0; i < reader.ForEachCount; i++)
                        {
                            int itemCount = reader.BeginForEachIndex(i);
                            if (itemCount == 0) continue;
                            for (int i2 = 0; i2 < itemCount; i2++)
                            {
                                ranges.Add(reader.Read<DegLen>());
                            }
                            reader.EndForEachIndex();
                        }
                        gotRangesStream.Dispose();

#if false // synchronous version for reference.
                        float DegStep = degreeRange.min; // Current deg step
                        for (int i = 0; i < DegIterations; i++)
                        {
                            if (DegStep > degreeRange.max + 1e-6f) break; // Exit if precision issues cause steps outside maximum range

                            Vector2 dir = new Vector2(direction.x, direction.y);
                            dir.Rotate((DegStep * -1) * math.TORADIANS); // DegStep *-1 is to normalize degrees from the VectorNetwork rotation system, which uses negative = left, positive = right

                            bool validLength = true;
                            float LenStep = lengthRange.max; // Current len step
                            for (int i2 = 0; i2 < LenIterationsStatic; i2++)
                            {
                                bool intersected = false;
                                for (int i3 = 0; i3 < validSegments.Length; i3++)
                                {
                                    Segment current = validSegments[i3];
                                    Vector2 rayA = origin;
                                    Vector2 rayB = origin + dir * LenStep;

                                    if (bufferRadius <= 0f)
                                    {
                                        intersected = Vec2Math.SegmentsIntersect(current.a, current.b, rayA, rayB);
                                    }
                                    else
                                    {
                                        // check distance between two segments <= bufferRadius
                                        float distSqr = Vec2Math.SegmentSegmentDistanceSqr(current.a, current.b, rayA, rayB);
                                        // if bufferRadius is 0 this reduces to checking distance = 0
                                        if (distSqr <= bufferSqr + 1e-8f)
                                        {
                                            intersected = true;
                                            break;
                                        }
                                    }
                                }

                                if (!intersected) break; // Exit if no intersections happened at this length, so it's good

                                LenStep -= LenStepCStatic;
                                if (LenStep < lengthRange.min - 1e-6f)
                                {
                                    validLength = false;
                                    break; // No valid lengths. Exit to make sure due to precision issues.
                                }
                            }

                            if (validLength)
                            {
                                ranges.Add((DegStep, new Range<float>(lengthRange.min, LenStep)));
                            }
                            else
                            {
                                ranges.Add((DegStep, new Range<float>(0f, 0f)));
                            }

                            DegStep += DegStepC;
                        }
#endif
                    }
                    else return new() { new BranchRange(lengthRange, degreeRange) };

                    if (ranges.Count == 0) return new(); // Empty

                    // Turn ranges into merged BranchRanges
                    return ComputeMergedResults(ranges, lengthRange.min, outputPrecisionThreshold);
                }

                /// <summary>
                /// Get range of movement for specific directions (relative to inputted direction). All lengths controlled by lengthRange selected. 
                /// </summary>
                /// <param name="origin">Origin</param>
                /// <param name="direction">Direction</param>
                /// <param name="lengthRange">Min/Max length</param>
                /// <param name="degrees">Specific degrees to test</param>
                /// <param name="segments">Segments</param>
                /// <param name="bufferRadius">Line "thickness"</param>
                /// <param name="lengtheningQuality">Controls the number of samples when determining length. Greater than 1 increases sample count less than 1 lowers it. Must be greater than 0. Ex: 0.5 is 2 times less samples, 2 is 2 times more samples.</param>
                /// <returns></returns>
                public static List<DegLen> GetAvailableRanges(
                    Vector2 origin, Vector2 direction,
                    Range<float> lengthRange,
                    float[] degrees,
                    NativeList<SegmentWrap> segments,
                    List<int> excludedSegmentIDs,
                    float bufferRadius,
                    float lengtheningQuality = 3)
                {
                    // Get applicable segments
                    float validSegmentDist = lengthRange.max + bufferRadius + 1e-6f;
                    Segment[] validSegments = ComputeValidSegments(segments, excludedSegmentIDs, origin, validSegmentDist);

                    // Get ranges
                    List<DegLen> ranges = new(degrees.Length);

                    float lenSpan = lengthRange.max - lengthRange.min;
                    int LenIterationsStatic = math.max(1, (int)math.round(lenSpan * lengtheningQuality) + 1);
                    float LenStepCStatic = (LenIterationsStatic > 1) ? (lenSpan / (LenIterationsStatic - 1)) : 0f;

                    float bufferSqr = bufferRadius * bufferRadius;

                    for (int i = 0; i < degrees.Length; i++)
                    {
                        float thisDegree = degrees[i];

                        Vector2 dir = new Vector2(direction.x, direction.y);
                        dir.Rotate((thisDegree * -1) * math.TORADIANS); // DegStep *-1 is to normalize degrees from the VectorNetwork rotation system, which uses negative = left, positive = right

                        bool validLength = true;
                        float LenStep = lengthRange.max;
                        for (int i2 = 0; i2 < LenIterationsStatic; i2++)
                        {
                            bool intersected = false;
                            for (int i3 = 0; i3 < validSegments.Length; i3++)
                            {
                                Segment current = validSegments[i3];
                                Vector2 rayA = origin;
                                Vector2 rayB = origin + dir * LenStep;

                                if (bufferRadius <= 0f)
                                {
                                    intersected = Vec2Math.SegmentsIntersect(current.a, current.b, rayA, rayB);
                                }
                                else
                                {
                                    float distSqr = Vec2Math.SegmentSegmentDistanceSqr(current.a, current.b, rayA, rayB);
                                    if (distSqr <= bufferSqr + 1e-8f)
                                    {
                                        intersected = true;
                                        break;
                                    }
                                }
                            }

                            if (!intersected) break; // Exit if no intersections happened

                            LenStep -= LenStepCStatic;
                            if (LenStep < lengthRange.min - 1e-6f)
                            {
                                validLength = false;
                                break; // No valid lengths
                            }
                        }

                        if (validLength)
                        {
                            ranges.Add(new DegLen(thisDegree, Range<float>.From(lengthRange.min, LenStep)));
                        }
                    }
                    return ranges;
                }

                /// <summary>
                /// Get range of movement for specific directions (relative to inputted direction) with length ranges specified by each <see cref="DegLen"/>.
                /// </summary>
                /// <param name="origin">Origin</param>
                /// <param name="direction">Direction</param>
                /// <param name="degLens">Specific degree and length ranges to test</param>
                /// <param name="segments">Segments</param>
                /// <param name="bufferRadius">Line "thickness"</param>
                /// <param name="lengtheningQuality">Controls the number of samples when determining length. Greater than 1 increases sample count less than 1 lowers it. Must be greater than 0. Ex: 0.5 is 2 times less samples, 2 is 2 times more samples.</param>
                /// <returns></returns>
                public static List<DegLen> GetAvailableRanges(
                    Vector2 origin, Vector2 direction,
                    DegLen[] degLens,
                    NativeList<SegmentWrap> segments,
                    List<int> excludedSegmentIDs,
                    float bufferRadius,
                    float lengtheningQuality = 3)
                {
                    // Get longest deglen max and treat that as the lengthRange.max
                    float furthestDeglenLength = 0f;
                    for (int i = 0; i < degLens.Length; i++)
                    {
                        var current_max = degLens[i].lengthR.max;
                        if (current_max > furthestDeglenLength)
                        {
                            furthestDeglenLength = current_max;
                        }
                    }

                    // Get applicable segments
                    float validSegmentDist = furthestDeglenLength + bufferRadius + 1e-6f;
                    Segment[] validSegments = ComputeValidSegments(segments, excludedSegmentIDs, origin, validSegmentDist);

                    // Get ranges
                    List<DegLen> ranges = new(degLens.Length);
                    float bufferSqr = bufferRadius * bufferRadius;

                    for (int i = 0; i < degLens.Length; i++)
                    {
                        DegLen thisDegLen = degLens[i];

                        Vector2 dir = new Vector2(direction.x, direction.y);
                        dir.Rotate((thisDegLen.degree * -1) * math.TORADIANS); // DegStep *-1 is to normalize degrees from the VectorNetwork rotation system, which uses negative = left, positive = right

                        float lenSpan = thisDegLen.lengthR.max - thisDegLen.lengthR.min;
                        int LenIterations = math.max(1, (int)math.round(lenSpan * lengtheningQuality) + 1);
                        float LenStepCStatic = (LenIterations > 1) ? (lenSpan / (LenIterations - 1)) : 0f;

                        bool validLength = true;
                        float LenStep = thisDegLen.lengthR.max;
                        for (int i2 = 0; i2 < LenIterations; i2++)
                        {
                            bool intersected = false;
                            for (int i3 = 0; i3 < validSegments.Length; i3++)
                            {
                                Segment current = validSegments[i3];
                                Vector2 rayA = origin;
                                Vector2 rayB = origin + dir * LenStep;

                                if (bufferRadius <= 0f)
                                {
                                    intersected = Vec2Math.SegmentsIntersect(current.a, current.b, rayA, rayB);
                                }
                                else
                                {
                                    float distSqr = Vec2Math.SegmentSegmentDistanceSqr(current.a, current.b, rayA, rayB);
                                    if (distSqr <= bufferSqr + 1e-8f)
                                    {
                                        intersected = true;
                                        break;
                                    }
                                }
                            }

                            if (!intersected) break; // Exit if no intersections happened

                            LenStep -= LenStepCStatic;
                            if (LenStep < thisDegLen.lengthR.min - 1e-6f)
                            {
                                validLength = false;
                                break; // No valid lengths
                            }
                        }

                        if (validLength)
                        {
                            ranges.Add(new DegLen(thisDegLen.degree, Range<float>.From(thisDegLen.lengthR.min, LenStep)));
                        }
                    }
                    return ranges;
                }

                /// <summary>
                /// Get segments that are within range or cross through the radius specified.
                /// </summary>
                /// <param name="segments"></param>
                /// <param name="origin"></param>
                /// <param name="validSegmentDist"></param>
                /// <returns></returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static Segment[] ComputeValidSegments(NativeList<SegmentWrap> segments, List<int> excludedSegmentIDs, Vector2 origin, float validSegmentDist)
                {
                    float validSegmentDistSqr = validSegmentDist * validSegmentDist;
                    if (segments.Length < SEG_COMPUTATION_WORKLOAD_THLD)
                    {
                        List<Segment> validSegments = new();
                        for (int i = 0; i < segments.Length; i++)
                        {
                            var current = segments[i];
                            if (excludedSegmentIDs.Contains(current.ID)) continue;

                            if ((origin - current.Segment.a).sqrMagnitude <= validSegmentDistSqr || (origin - current.Segment.b).sqrMagnitude <= validSegmentDistSqr)
                            {
                                validSegments.Add(current.Segment);
                            }
                            else // Failed easy test: sample the segment to see if any point is near enough
                            {
                                float distSqr = Vec2Math.PointSegmentDistanceSqr(origin, current.Segment.a, current.Segment.b);
                                if (distSqr <= validSegmentDistSqr)
                                {
                                    validSegments.Add(current.Segment);
                                }
                            }
                        }
                        return validSegments.ToArray();
                    }
                    else
                    {
                        int batchSize = math.clamp(segments.Length / SEG_COMPUTATION_WORKLOAD_THLD, SEG_COMPUTATION_BATCH_MIN, SEG_COMPUTATION_BATCH_MAX);

                        NativeQueue<Segment> gatheredSegs = new NativeQueue<Segment>(Allocator.TempJob);
                        GatherSegments gatherSegsJob = new()
                        {
                            inSegments = segments,
                            inExcludedSegmentIDs = new NativeArray<int>(excludedSegmentIDs.ToArray(), Allocator.TempJob),
                            origin = origin,
                            validSegmentDistSqr = validSegmentDistSqr,
                            outSegments = gatheredSegs.AsParallelWriter()
                        };

                        JobHandle gatherSegsHandle = gatherSegsJob.Schedule(segments.Length, batchSize);
                        gatherSegsHandle.Complete();

                        Segment[] validSegments;
                        using (NativeArray<Segment> copy = gatheredSegs.ToArray(Allocator.Persistent))
                        {
                            validSegments = copy.ToArray();
                        }
                        gatheredSegs.Dispose();
                        return validSegments;
                    }
                }

                /// <summary>
                /// Get merged range results as <see cref="BranchRange"/>s.
                /// </summary>
                /// <param name="ranges"></param>
                /// <param name="minimumLength"></param>
                /// <param name="outputPrecisionThreshold"></param>
                /// <returns></returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static List<BranchRange> ComputeMergedResults(List<DegLen> ranges, float minimumLength, float outputPrecisionThreshold)
                {
                    List<BranchRange> results = new((int)math.ceil(ranges.Count / MERGE_COMPUTATION_PREALLOCATION_FAC));

                    bool mChainExists = false;
                    float mChainDegI = 0f;
                    float mChainDegF = 0f;
                    float mChainLowestMaxLen = 0f;

                    void MergeChainEnded(bool wholeChainInvalid = false)
                    {
                        mChainExists = false;
                        if (wholeChainInvalid) return;
                        results.Add(new BranchRange(Range<float>.From(minimumLength, mChainLowestMaxLen), Range<float>.From(mChainDegI, mChainDegF)));
                    }

                    for (int i = 0; i < ranges.Count; i++)
                    {
                        var current = ranges[i];

                        bool chainStartedThisIteration = false;
                        if (!mChainExists) // Always start a chain if none exists
                        {
                            chainStartedThisIteration = true;
                            mChainExists = true;
                            mChainDegI = current.degree;
                            mChainDegF = current.degree;
                            mChainLowestMaxLen = current.lengthR.max;
                        }

                        if (current.lengthR.max == 0) // invalidated range
                        {
                            MergeChainEnded(chainStartedThisIteration); // don't post the chain if it was created invalid (this iteration)
                            continue;
                        }

                        if (i + 1 > ranges.Count - 1) // stop and finalize chain if no more comparisons
                        {
                            mChainDegF = current.degree;
                            if (current.lengthR.max < mChainLowestMaxLen) mChainLowestMaxLen = current.lengthR.max;
                            MergeChainEnded();
                            break;
                        }

                        var next = ranges[i + 1];
                        if (next.lengthR.max == 0) continue; // skip the check so the chain remains untouched and the invalidated range gets caught next iteration
                        if (math.abs(current.lengthR.max - next.lengthR.max) <= outputPrecisionThreshold)
                        {
                            mChainDegF = next.degree;
                            if (next.lengthR.max < mChainLowestMaxLen) mChainLowestMaxLen = next.lengthR.max;
                        }
                        else // chain ends because the delta length is above the threshold
                        {
                            mChainDegF = current.degree;
                            if (current.lengthR.max < mChainLowestMaxLen) mChainLowestMaxLen = current.lengthR.max;
                            MergeChainEnded();
                        }
                    }
                    return results;
                }
            }

            public void Dispose()
            {
                if (disposed) throw new ObjectDisposedException("VectorNetwork");
                disposed = true;

                UnmanagedSegmentRefs.Dispose();
                GC.SuppressFinalize(this);
            }

            ~VectorNetwork()
            {
                UnmanagedSegmentRefs.Dispose();
            }
        }
    }

    /// <summary>
    /// Provides various standalone implementations that custom classes can use
    /// </summary>
    namespace Implementers
    {
        /// <summary>
        /// Allows you to implement a simple flexible exception handling system.
        /// </summary>
        public abstract class FlexibleCatcher
        {
            /// <summary>
            /// When true, discovered exceptions are thrown in their encapsulating method. Otherwise, they are not thrown. All exceptions are stored in <see cref="LastFailureException"/> regardless.
            /// </summary>
            public bool RoughExceptions { get; protected set; } = true;
            /// <summary>
            /// The last caught exception.
            /// </summary>
            public Exception LastFailureException { get; protected set; } = null;
            /// <summary>
            /// Provides <see cref="FlexibleCatcher"/> instance, context (Caller method name), error
            /// </summary>
            public event Action<FlexibleCatcher, string, Exception> OnError;
            /// <summary>
            /// Handle the exception. Is overridable.
            /// </summary>
            /// <param name="exception"></param>
            /// <param name="context"></param>
            protected virtual void handleException(Exception exception, [CallerMemberName] string context = null)
            {
                OnError?.Invoke(this, context, exception);
                LastFailureException = exception;
                if (RoughExceptions) throw exception;
            }
        }

        /// <summary>
        /// Use on an interface to restrict its allowed implementors to specific types.
        /// </summary>
        [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
        public sealed class InterfaceImplementationEnforcementAttribute : Attribute
        {
            public Type[] AllowedImplementors { get; }
            public InterfaceImplementationEnforcementAttribute(params Type[] allowedImplementors)
            {
                if (allowedImplementors == null || allowedImplementors.Length == 0) throw new ArgumentException("You must specify at least one enforcement type.");
                AllowedImplementors = allowedImplementors;
            }

#if UNITY_EDITOR
            /// <summary>
            /// Enforces <see cref="InterfaceImplementationEnforcementAttribute"/> system
            /// </summary>
            [UnityEditor.InitializeOnLoad]
            internal static class ImplementationEnforcer
            {
                static ImplementationEnforcer()
                {
                    var assemblies = Reusables.GetNonInternalAssemblies();
                    var interfaces = assemblies
                        .SelectMany(a => a.GetTypes())
                        .Where(t => t.IsInterface && t.GetCustomAttributes(typeof(InterfaceImplementationEnforcementAttribute), true)
                        .Any());

                    foreach (var interface_ in interfaces)
                    {
                        var implementors = assemblies
                            .SelectMany(a => a.GetTypes())
                            .Where(t => interface_.IsAssignableFrom(t) && !t.IsInterface);

                        foreach (var impl in implementors)
                        {
                            var attrRef = (InterfaceImplementationEnforcementAttribute)interface_.GetCustomAttributes(typeof(InterfaceImplementationEnforcementAttribute), true).First();

                            bool isAllowed = attrRef.AllowedImplementors.Any(checkingType => checkingType.IsAssignableFrom(impl));
                            if (!isAllowed) { Debug.LogError($"Type '{impl.FullName}' implements interface '{interface_.FullName}' but is not permitted to."); }
                        }
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Marks a definition as not implemented with an optional explanation. If Warn is true then a message is logged into the Unity console letting you know this definition is not implemented.
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
        public sealed class NotImplementedAttribute : Attribute
        {
            public string Explanation { get; }
            public bool Warn { get; }
            public NotImplementedAttribute(string explanation = "", bool warn = true)
            {
                Explanation = explanation;
                Warn = warn;
            }

#if UNITY_EDITOR
            [UnityEditor.InitializeOnLoad]
            internal static class NotImplementedAttributeEnforcer
            {
                static NotImplementedAttributeEnforcer()
                {
                    var assemblies = Reusables.GetNonInternalAssemblies();
                    foreach (var assembly in assemblies)
                    {
                        foreach (var type in assembly.GetTypes())
                        {
                            var objectAttr = type.GetCustomAttribute<NotImplementedAttribute>();
                            if (objectAttr != null)
                            {
                                if (objectAttr.Warn)
                                {
                                    string typeString = string.Empty;
                                    if (type.IsInterface) typeString = "Interface";
                                    if (type.IsClass) typeString = "Class";
                                    if (type.IsStruct()) typeString = "Struct";
                                    if (type.IsEnum) typeString = "Enum";

                                    string warning = $"{typeString} '{type.FullName}' is not implemented";
                                    warning += objectAttr.Explanation != string.Empty ? $": '{objectAttr.Explanation}'" : ".";

                                    Debug.LogWarning(warning);
                                }
                            }

                            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                            {
                                var methodAttr = method.GetCustomAttribute<NotImplementedAttribute>();
                                if (methodAttr != null)
                                {
                                    if (methodAttr.Warn)
                                    {
                                        string warning = $"Method '{type.FullName}.{method.Name}()' is not implemented";
                                        warning += methodAttr.Explanation != string.Empty ? $": '{methodAttr.Explanation}'" : ".";

                                        Debug.LogWarning(warning);
                                    }
                                }
                            }
                        }
                    }
                }
            }
#endif
        }
    }

    /// <summary>
    /// Extension classes and methods that extend the capabilities of existing libraries
    /// </summary>
    namespace Extensions
    {
        public static class StringExtensions
        {
            /// <summary>
            /// Converts <see cref="string"/> self into a <see cref="NativeArray{byte}"/> with persistent allocation.
            /// </summary>
            /// <param name="self"></param>
            /// <returns><see cref="NativeArray{T}"/></returns>
            public static NativeArray<byte> ToNativeByteArray(this string self)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(self);
                NativeArray<byte> result = new(bytes.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                result.CopyFrom(bytes);
                return result;
            }
            /// <summary>
            /// Converts <see cref="string"/> self into a <see cref="NativeArray{byte}"/>.
            /// </summary>
            /// <param name="self"></param>
            /// <returns><see cref="NativeArray{T}"/></returns>
            public static NativeArray<byte> ToNativeByteArray(this string self, Allocator allocatorMode)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(self);
                NativeArray<byte> result = new(bytes.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                result.CopyFrom(bytes);
                return result;
            }
            /// <summary>
            /// Converts <see cref="string"/> self into a <see cref="NativeArray{byte}"/>.
            /// </summary>
            /// <param name="self"></param>
            /// <returns><see cref="NativeArray{T}"/></returns>
            public static NativeArray<byte> ToNativeByteArray(this string self, Allocator allocatorMode, NativeArrayOptions nativeArrayOptions)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(self);
                NativeArray<byte> result = new(bytes.Length, Allocator.Persistent, nativeArrayOptions);
                result.CopyFrom(bytes);
                return result;
            }
        }
        public static class DictionaryExtensions
        {
            /// <summary>
            /// Tries to update the specified key with a new value if it exists. Success status is returned as a <see cref="Boolean"/>.
            /// </summary>
            /// <typeparam name="TKey"></typeparam>
            /// <typeparam name="TValue"></typeparam>
            /// <param name="dict"></param>
            /// <param name="key"></param>
            /// <param name="updateValueFactory"></param>
            /// <returns></returns>
            public static bool TryUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue, TValue> updateValueFactory)
            {
                if (dict.TryGetValue(key, out var oldValue))
                {
                    dict[key] = updateValueFactory(key, oldValue);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Tries to update the specified key with a new value if it exists. Success status is returned as a <see cref="Boolean"/>.
            /// </summary>
            /// <typeparam name="TKey"></typeparam>
            /// <typeparam name="TValue"></typeparam>
            /// <param name="dict"></param>
            /// <param name="key"></param>
            /// <param name="updateValueFactory"></param>
            /// <returns></returns>
            public static bool TryUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
            {
                if (dict.TryGetValue(key, out var _))
                {
                    dict[key] = value;
                    return true;
                }
                return false;
            }
        }
        public static class TypeExtensions
        {
            public static bool IsStruct(this Type type) => type.IsValueType && !type.IsPrimitive;
            public static bool IsNumeric(this Type type)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        return true;
                    default:
                        return false;
                }
            }
        }
        public static class Vector3Extensions
        {
            public static bool Approximately(this Vector3 self, Vector3 vector, float tolerance = 0.0001f)
            {
                return ((self - vector).sqrMagnitude < (tolerance * tolerance));
            }
        }
        public static class Vector2Extensions
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Rotate(this ref Vector2 self, float radians)
            {
                float cos = math.cos(radians);
                float sin = math.sin(radians);
                float x = self.x;
                float y = self.y;
                self.x = x * cos - y * sin;
                self.y = x * sin + y * cos;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetRotation(this ref Vector2 self, float radians)
            {
                float magnitude = math.length(self);
                self.x = math.cos(radians) * magnitude;
                self.y = math.sin(radians) * magnitude;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool Approximately(this Vector2 self, Vector2 vector, float tolerance = 0.0001f)
            {
                return (self - vector).sqrMagnitude <= tolerance * tolerance;
            }
        }
    }

    ///<summary>
    /// Threading/Task tools for maximizing the possibilities of async operations and simplifying their processes
    /// </summary>
    namespace SuperThreading
    {
        /// <summary>
        /// An action fowarding class for posting actions to Unity's main thread, useful for fowarding code that cannot run on other asynchronous threads.
        /// </summary>
        public static class SynchronizationDispatcher
        {
            /// <summary>
            /// Posts an action to Unity's main thread. Has fine control over the timing of posts, always executing before the next frame renders. Each post goes into a queue.
            /// This method cannot function without Unity Update() calls.
            /// </summary>
            /// <param name="action">Thread action</param>
            public static void PostToQueue(Action action)
            {
                Internal.ConcurrentQueueManager._normalActions.Enqueue(action);
            }
            /// <summary>
            /// Posts an action to Unity's main thread. No control over the timing of execution and runs as soon as possible, without queueing. This method, unlike <see cref="PostToQueue(Action)"/> and <see cref="PostQueueOnePerFrame(Action)"/>
            /// can execute without Unity Update() calls.
            /// </summary>
            /// <param name="action">Thread action</param>
            public static void ForcePostImmediate(Action action)
            {
                if (Internal.ContextSynchronizer._unityContext == null)
                {
                    action?.Invoke();
                }
                else
                {
                    Internal.ContextSynchronizer._unityContext?.Post(_ => action(), null);
                }
            }
            /// <summary>
            /// Posts an action to Unity's main thread. Has fine control over the timing of posts, each post executing on the next frame, in order. Each post is queued and one is ran per frame.
            /// This method cannot function without Unity Update() calls.
            /// </summary>
            /// <param name="action">Thread action</param>
            public static void PostQueueOnePerFrame(Action action, string queueID = "")
            {
                if (!Internal.ConcurrentQueueManager._onePerFrameActionQueues.ContainsKey(queueID)) Internal.ConcurrentQueueManager._onePerFrameActionQueues.TryAdd(queueID, new());
                Internal.ConcurrentQueueManager._onePerFrameActionQueues[queueID].Enqueue(action);
            }

            /// <summary>
            /// [Internal] Internal manager of these methods.
            /// </summary>
            internal static class Internal
            {
                // Internal
                internal class ConcurrentQueueManager : MonoBehaviour
                {
                    private static ConcurrentQueueManager _instance;
                    internal static readonly ConcurrentQueue<Action> _normalActions = new();
                    internal static readonly ConcurrentDictionary<string, ConcurrentQueue<Action>> _onePerFrameActionQueues = new();
                    private void Update()
                    {
                        while (_normalActions.TryDequeue(out var action))
                        {
                            action?.Invoke();
                        }
                        
                        List<string> emptyKeys = new();
                        foreach(var kvp in _onePerFrameActionQueues)
                        {
                            if (kvp.Value.TryDequeue(out Action action))
                            {
                                action?.Invoke();
                            }
                            else emptyKeys.Add(kvp.Key);
                        }
                        for (int i = 0; i < emptyKeys.Count; i++) _onePerFrameActionQueues.TryRemove(emptyKeys[i], out var _);
                    }
                }
                // Internal
                internal static class ContextSynchronizer
                {
                    internal static SynchronizationContext _unityContext;

                    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
                    private static void Initialize()
                    {
                        _unityContext = SynchronizationContext.Current;
                    }
                }
            }
        }

        /// <summary>
        /// Sync <see cref="ConcurrentLocker"/> lock types
        /// </summary>
        public enum NormalLockType
        {
            Sync = 0,
            Skip = 1
        }
        /// <summary>
        /// Async <see cref="ConcurrentLocker"/> lock types
        /// </summary>
        public enum ConcurrentLockType
        {
            ConcurrentSync = 0,
            ConcurrentSkip = 1
        }

        /// <summary>
        /// Used to provide context about the completion state of a <see cref="NovaThread.OnWorkCompleted"/> event.
        /// </summary>
        public enum NovaThreadCompletionStatus
        {
            RanToComplete = 0,
            Cancelled = 1
        }
        /// <summary>
        /// Describes the current state of a <see cref="NovaThread"/>
        /// </summary>
        public enum NovaThreadStatus
        {
            WaitingForExecution = 0,
            Running = 1,
            Paused = 2,
            Completed = 3
        }

        /// <summary>
        /// Used to provided context about which operation triggered a <see cref="NovaThread.OnPauseStateChanged"/> event.
        /// </summary>
        public enum NovaThreadPauseTriggerType
        {
            Paused = 0,
            Resumed = 1
        }

        /// <summary>
        /// A task management class that helps manage task execution order and reduction.
        /// </summary>
        public static class ConcurrentLocker
        {
            private readonly static ConcurrentDictionary<string, SemaphoreSlim> syncLocks = new();
            private readonly static ConcurrentDictionary<string, SemaphoreSlim> concurrentSyncLocks = new();
            private readonly static ConcurrentDictionary<string, SemaphoreSlim> skipLocks = new();
            private readonly static ConcurrentDictionary<string, SemaphoreSlim> concurrentSkipLocks = new();

            private static SemaphoreSlim GetOrCreateLock(string lockID, ConcurrentDictionary<string, SemaphoreSlim> dictionary)
            {
                return dictionary.GetOrAdd(lockID, _ => new SemaphoreSlim(1, 1));
            }

            /// <summary>
            /// Checks if the specified lock ID is in use for the specified <see cref="NormalLockType"/>
            /// </summary>
            /// <param name="type">The <see cref="NormalLockType"/></param>
            /// <param name="lockID">Lock ID string</param>
            /// <returns>True if the lock is available, False if the lock is in use.</returns>
            /// <exception cref="ArgumentException"></exception>
            public static bool CheckLock(NormalLockType type, string lockID)
            {
                if (string.IsNullOrEmpty(lockID)) { throw new ArgumentException("LockID string cannot be null or empty."); }
                switch (type)
                {
                    case NormalLockType.Sync: { if (syncLocks.TryGetValue(lockID, out SemaphoreSlim s)) { return s.Wait(0); } else { return true; } };
                    case NormalLockType.Skip: { if (skipLocks.TryGetValue(lockID, out SemaphoreSlim s)) { return s.Wait(0); } else { return true; } };
                }
                return true;
            }
            /// <summary>
            /// Checks if the specified lock ID is in use for the specified <see cref="ConcurrentLockType"/>
            /// </summary>
            /// <param name="type">The <see cref="ConcurrentLockType"/></param>
            /// <param name="lockID">Lock ID string</param>
            /// <returns>True if the lock is available, False if the lock is in use.</returns>
            /// /// <exception cref="ArgumentException"></exception>
            public static async Task<bool> CheckLock(ConcurrentLockType type, string lockID)
            {
                if (string.IsNullOrEmpty(lockID)) { throw new ArgumentException("LockID string cannot be null or empty."); }
                switch (type)
                {
                    case ConcurrentLockType.ConcurrentSync: { if (concurrentSyncLocks.TryGetValue(lockID, out SemaphoreSlim s)) { return await s.WaitAsync(0); } else { return true; } };
                    case ConcurrentLockType.ConcurrentSkip: { if (concurrentSkipLocks.TryGetValue(lockID, out SemaphoreSlim s)) { return await s.WaitAsync(0); } else { return true; } };
                }
                return true;
            }
            /// <summary>
            /// Checks if a lock exists by ID and removes it from the list, handling disposal. NOTE: Applying force destroy when threads are using the lock can cause a PERMENANT deadlock/dangling thread, use at your own risk.
            /// </summary>
            /// <param name="type">The <see cref="NormalLockType"/></param>
            /// <param name="lockID">Lock ID string</param>
            /// <param name="ForceDestroy">(Discouraged) Should it be destroyed even if in use?</param>
            /// <returns><see cref="bool"/> operation success</returns>
            /// <exception cref="ArgumentException"></exception>
            public static bool DestroyLock(NormalLockType type, string lockID, bool ForceDestroy = false)
            {
                if (string.IsNullOrEmpty(lockID)) { throw new ArgumentException("LockID string cannot be null or empty."); }
                switch (type)
                {
                    case NormalLockType.Sync:
                        {
                            if (syncLocks.TryGetValue(lockID, out SemaphoreSlim s))
                            {
                                if (s.Wait(0)) // Not held
                                {
                                    syncLocks.TryRemove(lockID, out SemaphoreSlim _);
                                    s.Dispose();
                                    return true;
                                }
                                else
                                {
                                    if (ForceDestroy == true)
                                    {
                                        syncLocks.TryRemove(lockID, out SemaphoreSlim _);
                                        s.Release();
                                        s.Dispose();
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            return true;
                        }
                        ;
                    case NormalLockType.Skip:
                        {
                            if (skipLocks.TryGetValue(lockID, out SemaphoreSlim s))
                            {
                                if (s.Wait(0)) // Not held
                                {
                                    skipLocks.TryRemove(lockID, out SemaphoreSlim _);
                                    s.Dispose();
                                    return true;
                                }
                                else
                                {
                                    if (ForceDestroy == true)
                                    {
                                        skipLocks.TryRemove(lockID, out SemaphoreSlim _);
                                        s.Release();
                                        s.Dispose();
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            return true;
                        }
                        ;
                }
                return true;
            }
            /// <summary>
            /// Checks if a lock exists by ID and removes it from the list, handling disposal. NOTE: Applying force destroy when threads are using the lock can cause a PERMENANT deadlock/dangling thread, use at your own risk.
            /// </summary>
            /// <param name="type">The <see cref="ConcurrentLockType"/></param>
            /// <param name="lockID">Lock ID string</param>
            /// <param name="ForceDestroy">(Discouraged) Should it be destroyed even if in use?</param>
            /// <returns><see cref="bool"/> operation success</returns>
            /// <exception cref="ArgumentException"></exception>
            public static async Task<bool> DestroyLock(ConcurrentLockType type, string lockID, bool ForceDestroy = false)
            {
                if (string.IsNullOrEmpty(lockID)) { throw new ArgumentException("LockID string cannot be null or empty."); }
                switch (type)
                {
                    case ConcurrentLockType.ConcurrentSync:
                        {
                            if (concurrentSyncLocks.TryGetValue(lockID, out SemaphoreSlim s))
                            {
                                if (await s.WaitAsync(0)) // Not held
                                {
                                    concurrentSyncLocks.TryRemove(lockID, out SemaphoreSlim _);
                                    s.Dispose();
                                    return true;
                                }
                                else
                                {
                                    if (ForceDestroy == true)
                                    {
                                        concurrentSyncLocks.TryRemove(lockID, out SemaphoreSlim _);
                                        s.Release();
                                        s.Dispose();
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            return true;
                        }
                        ;
                    case ConcurrentLockType.ConcurrentSkip:
                        {
                            if (concurrentSkipLocks.TryGetValue(lockID, out SemaphoreSlim s))
                            {
                                if (await s.WaitAsync(0)) // Not held
                                {
                                    concurrentSkipLocks.TryRemove(lockID, out SemaphoreSlim _);
                                    s.Dispose();
                                    return true;
                                }
                                else
                                {
                                    if (ForceDestroy == true)
                                    {
                                        concurrentSkipLocks.TryRemove(lockID, out SemaphoreSlim _);
                                        s.Release();
                                        s.Dispose();
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            return true;
                        }
                        ;
                }
                return true;
            }

            /// <summary>
            /// Creates a blocking lock around the specified non asynchronous <see cref="Action"/>. This lock type will halt code execution until the lock is released if aquired by another operation. The LockID determines which lock is used to manage the <see cref="Action"/>.
            /// </summary>
            /// <param name="lockID">The ID of the lock for this <see cref="Action"/></param>
            /// <param name="action">The operation to execute</param>
            /// <exception cref="InvalidOperationException"></exception>
            public static void SyncLock(string lockID, Action action)
            {
                if (action.Method.IsDefined(typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute), false))
                {
                    throw new InvalidOperationException("Cannot pass an async method to SyncLock. Use SyncLockAsync instead.");
                }
                var _lock = GetOrCreateLock(lockID, syncLocks);
                _lock.Wait();
                try
                {
                    action();
                }
                finally
                {
                    try
                    {
                        _lock.Release();
                    }
                    catch (SemaphoreFullException) { }
                }
            }
            /// <summary>
            /// Creates a blocking lock around the specified asynchronous <see cref="Action"/> <see cref="Task"/>. This lock type will halt code execution until the lock is released if aquired by another operation. The LockID determines which lock is used to manage the <see cref="Action"/> <see cref="Task"/>.
            /// </summary>
            /// <param name="lockID">The ID of the lock for this <see cref="Action"/> <see cref="Task"/></param>
            /// <param name="action">The operation to execute</param>
            public static async Task ConcurrentSyncLock(string lockID, Func<Task> action)
            {
                var _lock = GetOrCreateLock(lockID, concurrentSyncLocks);
                await _lock.WaitAsync();
                try
                {
                    await action();
                }
                finally
                {
                    try
                    {
                        _lock.Release();
                    }
                    catch (SemaphoreFullException) { }
                }
            }
            /// <summary>
            /// Creates a skipping lock around the specified non asynchronous <see cref="Action"/>. This lock type will make it so if locked, any further calling operations will be cancelled until the lock is available again. The LockID determines which lock is used to manage the <see cref="Action"/>.
            /// </summary>
            /// <param name="lockID">The ID of the lock for this <see cref="Action"/></param>
            /// <param name="action">The operation to execute</param>
            /// <exception cref="InvalidOperationException"></exception>
            public static void SkipLock(string lockID, Action action)
            {
                if (action.Method.IsDefined(typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute), false))
                {
                    throw new InvalidOperationException("Cannot pass an async method to SyncLock. Use SyncLockAsync instead.");
                }
                var _lock = GetOrCreateLock(lockID, skipLocks);
                bool aquired = _lock.Wait(0);
                if (aquired)
                {
                    try
                    {
                        action();
                    }
                    finally
                    {
                        try
                        {
                            _lock.Release();
                        }
                        catch (SemaphoreFullException) { }
                    }
                }
            }
            /// <summary>
            /// Creates a skipping lock around the specified asynchronous <see cref="Action"/> <see cref="Task"/>. This lock type will make it so if locked, any further calling operations will be cancelled until the lock is available again. The LockID determines which lock is used to manage the <see cref="Action"/> <see cref="Task"/>.
            /// </summary>
            /// <param name="lockID">The ID of the lock for this <see cref="Action"/></param>
            /// <param name="action">The operation to execute</param>
            public static async Task ConcurrentSkipLock(string lockID, Func<Task> action)
            {
                var _lock = GetOrCreateLock(lockID, concurrentSkipLocks);
                bool aquired = await _lock.WaitAsync(0);
                if (aquired)
                {
                    try
                    {
                        await action();
                    }
                    finally
                    {
                        try
                        {
                            _lock.Release();
                        }
                        catch (SemaphoreFullException) { }
                    }
                }
            }
        }

        /// <summary>
        /// Allows for users to dynamically add pausing mechanisms into passed <see cref="NovaThread"/> constructor code. When calling <see cref="NovaThread.Pause"/> and <see cref="NovaThread.Resume"/> on a <see cref="NovaThread"/>, <see cref="IsPaused"/> will be updated.
        /// </summary>
        public class PauseToken
        {
            private volatile bool _isPaused = false;

            public bool IsPaused
            {
                get { return _isPaused; }
                internal set { _isPaused = value; }
            }

            public void Pause() => IsPaused = true;
            public void Resume() => IsPaused = false;
        }

        /// <summary>
        /// A super multitasking class that takes <see cref="Coroutine"/>, <see cref="Task"/>, and <see cref="Thread"/> and combines them into an all in one use threading system. All code is passed into them via constructors allowing for you to write whatever you want into them. <see cref="NovaThread"/> provides enums for status information (<see cref="NovaThreadStatus"/>) and completion state information (<see cref="NovaThreadCompletionStatus"/>), a completion event (<see cref="NovaThread.OnWorkCompleted"/>), a custom <see cref="SuperThreading.PauseToken"/> system for users to implement pausing mechanisms, and the use of <see cref="System.Threading.CancellationTokenSource"/> to allow users to implement cancellation systems (or immediately cancel in <see cref="NovaThread"/>s using <see cref="Coroutine"/> framework).
        /// </summary>
        public class NovaThread
        {
            // ----- Events -----
            public delegate void NovaThreadFinished(NovaThreadCompletionStatus status);
            public event NovaThreadFinished OnWorkCompleted;

            public delegate void NovaThreadPauseStateChanged(NovaThreadPauseTriggerType trigger);
            public event NovaThreadPauseStateChanged OnPauseStateChanged;


            // ----- Enum -----
            /// <summary>
            /// Describes the framework type that a <see cref="NovaThread"/> is using.
            /// </summary>
            public enum ThreadType
            {
                Coroutine = 0,
                Thread = 1,
                Task = 2
            }

            // ----- Nova Thread ANY -----
            private readonly TaskCompletionSource<bool> TaskCompleted = new();
            /// <summary>
            /// The framework type that the current <see cref="NovaThread"/> instance is using.
            /// </summary>
            public readonly ThreadType ThreadFramework;
            private readonly PauseToken PauseToken = null;
            private readonly CancellationTokenSource CancellationTokenSource = null;
            private bool Executed;
            private volatile bool calledCancellation = false;

            private volatile NovaThreadStatus _state;
            /// <summary>
            /// The execution state of the <see cref="NovaThread"/>.
            /// </summary>
            public NovaThreadStatus State
            {
                get { return _state; }
                private set { _state = value; }
            }

            // ----- Nova Thread Coro Framework -----
            private readonly MonoBehaviour coroHost;
            private IEnumerator coroutine;
            private volatile Coroutine runningCoroutine;
            private Coroutine awaiterCoroutine;

            // ----- Nova Thread Task Framework -----
            private readonly Func<Task> setTask;

            // ----- Nova Thread THREAD Framework -----
            private readonly Thread setThread;

            // ----- Constructors -----
            // Coroutines
            /// <summary>
            /// Initialize a <see cref="NovaThread"/> in Unity <see cref="Coroutine"/> framework. Read constructor arguments for more info.
            /// </summary>
            /// <param name="monoScript">The host script for the running <see cref="Coroutine"/></param>
            /// <param name="coroutineAction">The <see cref="Coroutine"/> passed to execute</param>
            /// <param name="ExecuteImmediate">If the <see cref="NovaThread"/> should execute while constructed or not</param>
            /// <exception cref="ArgumentException"></exception>
            public NovaThread(MonoBehaviour monoScript, Func<IEnumerator> coroutineAction, bool ExecuteImmediate = false)
            {
                State = NovaThreadStatus.WaitingForExecution;
                ThreadFramework = ThreadType.Coroutine;
                coroHost = monoScript;
                Executed = ExecuteImmediate;

                if (coroutineAction != null && monoScript != null)
                {
                    coroutine = coroutineAction();
                    if (ExecuteImmediate)
                    {
#pragma warning disable CS4014
                        RunCoroutine();
#pragma warning restore CS4014
                    }
                }
                else
                {
                    throw new ArgumentException("NovaThread(Monobehavior, Func<IEnumerator>) constructor requires an existing mono script and coroutine but one was null.");
                }
            }
            /// <summary>
            /// Initialize a <see cref="NovaThread"/> in Unity <see cref="Coroutine"/> framework. Read constructor arguments for more info.
            /// </summary>
            /// <param name="monoScript">The host script for the running <see cref="Coroutine"/></param>
            /// <param name="coroutineAction">The <see cref="Coroutine"/> passed to execute</param>
            /// <param name="pauseToken">The passed <see cref="SuperThreading.PauseToken"/> to manage thread pausing. This can be null if you prefer not to include pausing</param>
            /// <param name="ExecuteImmediate">If the <see cref="NovaThread"/> should execute while constructed or not</param>
            /// <exception cref="ArgumentException"></exception>
            public NovaThread(MonoBehaviour monoScript, Func<IEnumerator> coroutineAction, PauseToken pauseToken, bool ExecuteImmediate = false)
            {
                State = NovaThreadStatus.WaitingForExecution;
                ThreadFramework = ThreadType.Coroutine;
                coroHost = monoScript;
                PauseToken = pauseToken;
                Executed = ExecuteImmediate;

                if (coroutineAction != null && monoScript != null)
                {
                    coroutine = coroutineAction();
                    if (ExecuteImmediate)
                    {
#pragma warning disable CS4014
                        RunCoroutine();
#pragma warning restore CS4014
                    }
                }
                else
                {
                    throw new ArgumentException("NovaThread(Monobehavior, Func<IEnumerator>) constructor requires an existing mono script and coroutine but one was null.");
                }
            }

            // Tasks
            /// <summary>
            /// Initialize a <see cref="NovaThread"/> in <see cref="Task"/> framework. Read constructor arguments for more info.
            /// </summary>
            /// <param name="action">The asynchronous <see cref="Action"/> passed to execute</param>
            /// <param name="ExecuteImmediate">If the <see cref="NovaThread"/> should execute while constructed or not</param>
            /// <exception cref="ArgumentException"></exception>
            public NovaThread(Func<Task> action, bool ExecuteImmediate)
            {
                State = NovaThreadStatus.WaitingForExecution;
                ThreadFramework = ThreadType.Task;
                Executed = ExecuteImmediate;

                if (action != null)
                {
                    setTask = action;
                    if (ExecuteImmediate)
                    {
#pragma warning disable CS4014
                        RunTask();
#pragma warning restore CS4014
                    }
                }
                else
                {
                    throw new ArgumentException("NovaThread(Func<Task>) constructor requires a non null asynchronous action.");
                }
            }
            /// <summary>
            /// Initialize a <see cref="NovaThread"/> in <see cref="Task"/> framework. Read constructor arguments for more info.
            /// </summary>
            /// <param name="action">The asynchronous <see cref="Action"/> passed to execute</param>
            /// <param name="pauseToken">The passed <see cref="SuperThreading.PauseToken"/> to manage thread pausing. This can be null if you prefer not to include pausing</param>
            /// <param name="cancellationTokenSource">The passed <see cref="System.Threading.CancellationTokenSource"/> to manage thread cancellation</param>
            /// <param name="ExecuteImmediate">If the <see cref="NovaThread"/> should execute while constructed or not</param>
            /// <exception cref="ArgumentException"></exception>
            public NovaThread(Func<Task> action, PauseToken pauseToken = null, CancellationTokenSource cancellationTokenSource = null, bool ExecuteImmediate = false)
            {
                State = NovaThreadStatus.WaitingForExecution;
                ThreadFramework = ThreadType.Task;
                this.PauseToken = pauseToken;
                this.CancellationTokenSource = cancellationTokenSource;
                Executed = ExecuteImmediate;

                if (action != null)
                {
                    setTask = action;
                    if (ExecuteImmediate)
                    {
#pragma warning disable CS4014
                        RunTask();
#pragma warning restore CS4014
                    }
                }
                else
                {
                    throw new ArgumentException("NovaThread(Func<Task>) constructor requires a non null asynchronous action.");
                }
            }

            // Threads
            /// <summary>
            /// Initialize a <see cref="NovaThread"/> in <see cref="Thread"/> framework. Read constructor arguments for more info.
            /// </summary>
            /// <param name="action">The <see cref="Action"/> passed to execute. Can not be asynchronous.</param>
            /// <param name="IsBackground">If True, this thread will be force closed upon Application exit. If False, this thread will be recognized as a main thread and will persist after the game closes requiring manual stoppage. This is what was shown in the Unity game environment and may behave differently in a compiled game.</param>
            /// <param name="ExecuteImmediate">If the <see cref="NovaThread"/> should execute while constructed or not</param>
            /// <exception cref="ArgumentException"></exception>
            public NovaThread(Action action, bool IsBackground, bool ExecuteImmediate)
            {
                if (action.Method.IsDefined(typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute), false))
                {
                    throw new ArgumentException("NovaThread(Action) constructor does not support async actions.");
                }

                State = NovaThreadStatus.WaitingForExecution;
                ThreadFramework = ThreadType.Thread;
                Executed = ExecuteImmediate;

                if (action != null)
                {
                    setThread = new Thread(new ThreadStart(action));
                    setThread.IsBackground = IsBackground;
                    if (ExecuteImmediate)
                    {
#pragma warning disable CS4014
                        RunThread();
#pragma warning restore CS4014
                    }
                }
                else
                {
                    throw new ArgumentException("NovaThread(Action) constructor requires a non null, non async action.");
                }
            }
            /// <summary>
            /// Initialize a <see cref="NovaThread"/> in <see cref="Thread"/> framework. Read constructor arguments for more info.
            /// </summary>
            /// <param name="action">The <see cref="Action"/> passed to execute. Can not be asynchronous.</param>
            /// <param name="IsBackground">If True, this thread will be force closed upon Application exit. If False, this thread will be recognized as a main thread and will persist after the game closes requiring manual stoppage. This is what was shown in the Unity game environment and may behave differently in a compiled game.</param>
            /// <param name="pauseToken">The passed <see cref="SuperThreading.PauseToken"/> to manage thread pausing. This can be null if you prefer not to include pausing</param>
            /// <param name="cancellationTokenSource">The passed <see cref="System.Threading.CancellationTokenSource"/> to manage thread cancellation. This can be null if you prefer not to include cancellation</param>
            /// <param name="ExecuteImmediate">If the <see cref="NovaThread"/> should execute while constructed or not</param>
            /// <exception cref="ArgumentException"></exception>
            public NovaThread(Action action, bool IsBackground, PauseToken pauseToken = null, CancellationTokenSource cancellationTokenSource = null, bool ExecuteImmediate = false)
            {
                if (action.Method.IsDefined(typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute), false))
                {
                    throw new ArgumentException("NovaThread(Action) constructor does not support async actions.");
                }

                State = NovaThreadStatus.WaitingForExecution;
                ThreadFramework = ThreadType.Thread;
                Executed = ExecuteImmediate;
                this.PauseToken = pauseToken;
                this.CancellationTokenSource = cancellationTokenSource;

                if (action != null)
                {
                    setThread = new Thread(new ThreadStart(action));
                    setThread.IsBackground = IsBackground;
                    if (ExecuteImmediate)
                    {
#pragma warning disable CS4014
                        RunThread();
#pragma warning restore CS4014
                    }
                }
                else
                {
                    throw new ArgumentException("NovaThread(Action) constructor requires a non null, non async action.");
                }
            }

            // ----- Coroutine Specific Methods -----
            private async Task RunCoroutine()
            {
                Executed = true;
                State = NovaThreadStatus.Running;
                awaiterCoroutine = coroHost.StartCoroutine(RunCoroutineToCompletion());
                await TaskCompleted.Task;
                State = NovaThreadStatus.Completed;
                OnWorkCompleted?.Invoke(calledCancellation == true ? NovaThreadCompletionStatus.Cancelled : NovaThreadCompletionStatus.RanToComplete);
            }
            private IEnumerator RunCoroutineToCompletion()
            {
                runningCoroutine = coroHost.StartCoroutine(coroutine);
                yield return runningCoroutine;
                TaskCompleted.SetResult(true);
            }

            // ----- Task Specific Methods -----
            private async Task RunTask()
            {
                Executed = true;
                State = NovaThreadStatus.Running;
                await setTask();
                State = NovaThreadStatus.Completed;
                OnWorkCompleted?.Invoke(calledCancellation == true ? NovaThreadCompletionStatus.Cancelled : NovaThreadCompletionStatus.RanToComplete);
            }

            // ----- Thread Specific Methods -----
            private async Task RunThread()
            {
                Executed = true;
                State = NovaThreadStatus.Running;
                setThread.Start();
                await Task.Run(() => setThread.Join());
                State = NovaThreadStatus.Completed;
                OnWorkCompleted?.Invoke(calledCancellation == true ? NovaThreadCompletionStatus.Cancelled : NovaThreadCompletionStatus.RanToComplete);
            }

            // ----- Nova Thread Methods -----
            /// <summary>
            /// Starts the <see cref="NovaThread"/>. Throws <see cref="InvalidOperationException"/> if the <see cref="NovaThread"/> has already started, finished, or been cancelled.
            /// </summary>
            /// <exception cref="InvalidOperationException"></exception>
            public void Run()
            {
                if (Executed) { throw new InvalidOperationException("Cannot invoke Run() on NovaThread instance since it was already ran."); }
                if (ThreadFramework == ThreadType.Coroutine)
                {
                    if (coroutine != null && coroHost != null)
                    {
#pragma warning disable CS4014
                        RunCoroutine();
#pragma warning restore CS4014
                    }
                    else
                    {
                        throw new InvalidOperationException("Cannot run NovaThread of coroutine type. Coroutine or Mono script are missing.");
                    }
                }
                if (ThreadFramework == ThreadType.Task)
                {
                    if (setTask != null)
                    {
#pragma warning disable CS4014
                        RunTask();
#pragma warning restore CS4014
                    }
                    else
                    {
                        throw new InvalidOperationException("Cannot run NovaThread of task type. Func<Task> provided is null.");
                    }
                }
                if (ThreadFramework == ThreadType.Thread)
                {
                    if (setThread != null)
                    {
#pragma warning disable CS4014
                        RunThread();
#pragma warning restore CS4014
                    }
                    else
                    {
                        throw new InvalidOperationException("Cannot run NovaThread of thread type. Action provided is null.");
                    }
                }
            }
            /// <summary>
            /// Invokes cancellation within the <see cref="NovaThread"/>. For <see cref="NovaThread"/>s using <see cref="Coroutine"/> framework, this will work immediately and cancels the thread without any strings attached. For <see cref="NovaThread"/>s using any other framework type than coro, this method will call <see cref="System.Threading.CancellationTokenSource.Cancel()"/> on the provided <see cref="System.Threading.CancellationTokenSource"/>. If no <see cref="System.Threading.CancellationTokenSource"/> is provided and the current framework requires one for cancellation, this method will throw an <see cref="InvalidOperationException"/>.
            /// </summary>
            /// <exception cref="InvalidOperationException"></exception>
            public void Cancel()
            {
                if (!Executed) { throw new InvalidOperationException("Cannot invoke Cancel() on NovaThread instance since it was never started."); }
                if (calledCancellation) { throw new InvalidOperationException("Cannot invoke Cancel() on NovaThread instance since it was already cancelled."); }
                if (ThreadFramework == ThreadType.Coroutine)
                {
                    if (coroHost != null && runningCoroutine != null)
                    {
                        calledCancellation = true;
                        coroHost.StopCoroutine(runningCoroutine);
                        coroHost.StopCoroutine(awaiterCoroutine);
                        TaskCompleted.SetResult(true);
                    }
                }
                if (ThreadFramework == ThreadType.Task)
                {
                    if (setTask != null)
                    {
                        calledCancellation = true;
                        this.CancellationTokenSource.Cancel();
                    }
                }
                if (ThreadFramework == ThreadType.Thread)
                {
                    if (setThread != null)
                    {
                        calledCancellation = true;
                        this.CancellationTokenSource.Cancel();
                    }
                }
            }
            /// <summary>
            /// Calls <see cref="SuperThreading.PauseToken.Pause()"/> on the provided <see cref="SuperThreading.PauseToken"/>, which can be used to manage dynamic pausing systems (you have to set this up). If no <see cref="SuperThreading.PauseToken"/> is provided and this method is called it will log an error to the console but not throw an exception. If the <see cref="NovaThread"/> was never started or was cancelled, an <see cref="InvalidOperationException"/> will be thrown.
            /// </summary>
            /// <exception cref="InvalidOperationException"></exception>
            public void Pause()
            {
                if (!Executed || calledCancellation) { throw new InvalidOperationException("Cannot invoke Pause() on NovaThread instance since it is in a state that doesn't support this."); }
                if (PauseToken != null)
                {
                    State = NovaThreadStatus.Paused;
                    PauseToken.Pause();
                    OnPauseStateChanged?.Invoke(NovaThreadPauseTriggerType.Paused);
                }
                else
                {
                    Debugger.LogError("Cannot invoke Pause() on NovaThread without a PauseToken. Consider passing a PauseToken into the constructor to implement pausing mechanisms.\n" + new StackTrace());
                }
            }
            /// <summary>
            /// Calls <see cref="SuperThreading.PauseToken.Resume()"/> on the provided <see cref="SuperThreading.PauseToken"/>, which can be used to manage dynamic pausing systems (you have to set this up). If no <see cref="SuperThreading.PauseToken"/> is provided and this method is called it will log an error to the console but not throw an exception. If the <see cref="NovaThread"/> was never started or was cancelled, an <see cref="InvalidOperationException"/> will be thrown.
            /// </summary>
            /// <exception cref="InvalidOperationException"></exception>
            public void Resume()
            {
                if (!Executed || calledCancellation) { throw new InvalidOperationException("Cannot invoke Resume() on NovaThread instance since it is in a state that doesn't support this."); }
                if (PauseToken != null)
                {
                    State = NovaThreadStatus.Running;
                    PauseToken.Resume();
                    OnPauseStateChanged?.Invoke(NovaThreadPauseTriggerType.Resumed);
                }
                else
                {
                    Debugger.LogError("Cannot invoke Resume() on NovaThread without a PauseToken. Consider passing a PauseToken into the constructor to implement pausing mechanisms.\n" + new StackTrace());
                }
            }
        }
    }

    namespace Unsafe
    {
        /// <summary>
        /// Lets you control memory initialization strategy on allocation for <see cref="NativeChanneledArray{T}"/>
        /// </summary>
        public enum NativeChanneledArrayOptions
        {
            /// <summary>
            /// Leave all memory at the allocated location untouched, which helps with performance. It is impossible to know what is stored here however, so you shouldnt attempt to read these values.
            /// </summary>
            Uninitialized = 0,
            /// <summary>
            /// Clear all memory originally stored at the location on allocation.
            /// </summary>
            ClearMemory = 1,
            /// <summary>
            /// Fills all values in all channels of the ChanneledArray with the provided <see cref="{T}"/> bufferEmptyValue, or default if none is provided.
            /// </summary>
            DefaultFill = 2
        }

        /// <summary>
        /// A custom native container that provides per-thread array-style storage by "splitting" the buffer into multiple channels. Requires <see cref="NativeDisableUnsafePtrRestrictionAttribute"/> and <see cref="NativeDisableContainerSafetyRestrictionAttribute"/> attributes to function with typical per thread access.
        /// <para>
        /// Typical usage is to set <paramref name="channelCount"/> equal to the parallel iteration count,
        /// so that each job iteration has exclusive access to its own channel.
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [NativeContainer]
        [NativeContainerSupportsDeallocateOnJobCompletion]
        [NativeContainerSupportsMinMaxWriteRestriction]
        public unsafe struct NativeChanneledArray<T> : IDisposable where T : unmanaged
        {
            // Allocation
            [NativeDisableUnsafePtrRestriction]
            internal void* m_Buffer;
            internal T* m_Reinterpretation;

            internal int m_Length;
            internal Allocator m_AllocatorLabel;
            internal readonly int typeSize;

            // Safety system
            internal AtomicSafetyHandle m_Safety;
            [NativeSetClassTypeToNullOnSchedule]
            internal DisposeSentinel m_DisposeSentinel;

            // Exposed values
            /// <summary>
            /// Total length, <see cref="ChannelSize"/> * <see cref="ChannelCount"/>.
            /// </summary>
            public int Length => m_Length;
            public readonly int ChannelSize;
            public readonly int ChannelCount;

            public NativeChanneledArray(int channelCount, int channelBufferSize, Allocator allocator, NativeChanneledArrayOptions options = NativeChanneledArrayOptions.DefaultFill, T bufferEmptyValue = default)
            {
                if (channelCount < 0) throw new ArgumentOutOfRangeException(nameof(channelCount));
                if (channelBufferSize < 0) throw new ArgumentOutOfRangeException(nameof(channelBufferSize));
                if (allocator == Allocator.None) throw new ArgumentException("Allocator must be Temp, TempJob, or Persistent");

                ChannelSize = channelBufferSize;
                ChannelCount = channelCount;

                // Allocate array buffer
                typeSize = UnsafeUtility.SizeOf<T>();
                long memSize = (channelCount * (long)channelBufferSize) * typeSize;
                m_Buffer = UnsafeUtility.Malloc(memSize, UnsafeUtility.AlignOf<T>(), allocator);
                m_Length = channelCount * channelBufferSize;
                m_AllocatorLabel = allocator;

                // Reinterpret
                m_Reinterpretation = (T*)m_Buffer;

                // Create Dispose Sentinel
                DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 1, allocator);

                // Apply memory strategy
                switch (options)
                {
                    case NativeChanneledArrayOptions.DefaultFill: { UnsafeUtility.MemCpyReplicate(m_Buffer, &bufferEmptyValue, typeSize, m_Length); break; }
                    case NativeChanneledArrayOptions.ClearMemory: { UnsafeUtility.MemClear(m_Buffer, memSize); break; }
                }
            }

            public T this[int channelIndex, int index]
            {
                get
                {
                    validateInputs(channelIndex, index);
                    AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
                    return m_Reinterpretation[(channelIndex * ChannelSize) + index];
                }
                set
                {
                    validateInputs(channelIndex, index);
                    AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
                    m_Reinterpretation[(channelIndex * ChannelSize) + index] = value;
                }
            }

            /// <summary>
            /// Gets the pointer at the address of the specified index in channel.
            /// </summary>
            public unsafe T* GetPointer(int channelIndex, int index)
            {
                validateInputs(channelIndex, index);
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
                return m_Reinterpretation + (channelIndex * ChannelSize) + index;
            }

            public void Dispose()
            {
                if (m_Buffer == null) return;

                // Dispose the sentinel and release memory
                DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
                UnsafeUtility.Free(m_Buffer, m_AllocatorLabel);
                m_Buffer = null;
                m_Length = 0;
            }

            private void validateInputs(int channelIndex, int index)
            {
                if (channelIndex < 0 || channelIndex > ChannelCount - 1) throw new IndexOutOfRangeException("Channel Index is out of range.");
                if (index < 0 || index > ChannelSize - 1) throw new IndexOutOfRangeException("Index is out of range.");
            }
        }
    }

    namespace Internal
    {
        internal static class Reusables
        {
            public static IEnumerable<Assembly> GetNonInternalAssemblies()
            {
                return AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.FullName.StartsWith("Unity") && !a.FullName.StartsWith("Microsoft") && !a.FullName.StartsWith("System"));
            }
        }
        internal static class StellarAPIGameObject
        {
            private static readonly GameObject _ = new GameObject("StellarAPIObject - LEAVE ME BE >:[");
            private static readonly TaskCompletionSource<bool> await_ = new();
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            private static void Initialize()
            {
                _.AddComponent<SynchronizationDispatcher.Internal.ConcurrentQueueManager>();
                UnityEngine.Object.DontDestroyOnLoad(_);
                await_.SetResult(true);
            }
            public static async Task<GameObject> Get()
            {
                await await_.Task;
                return _;
            }
        }
        /// <summary>
        /// Gives access to potentially damaging operations within the API. Use carefully.
        /// </summary>
        public static class DangerousModifier
        {
            /// <summary>
            /// Adds the specified <see cref="MonoBehaviour"/> component to the APIs internal <see cref="GameObject"/>. Ensure that the added component does not potentially break the object.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static async Task AddComponentToInternalObject<T>() where T : MonoBehaviour
            {
                GameObject internalObj = await StellarAPIGameObject.Get();
                var tcs = new TaskCompletionSource<bool>(false);
                SynchronizationDispatcher.ForcePostImmediate(() =>
                {
                    if(internalObj != null) internalObj.AddComponent<T>();
                    tcs.SetResult(true);
                });
                await tcs.Task;
            }
        }
    }
}


/// <summary>
/// Desc: Object management extensions for better game control. Some other raw script APIs can extend it as well via its partial class nature.
/// Usage: Contains tools that make managing Unity GameObjects even easier!
/// 
/// :]
/// 
/// Credit: Written and Documented entirely by BigTylis
/// </summary>
namespace Stellar.APIs.ObjectHandlingExtensions
{
    /// <summary>
    /// Adds extension methods and other global methods to make accessing Unity GameObjects simpler. Extends <see cref="GameObject"/>
    /// </summary>  
    public static partial class ObjectHandlingExtensions
    {
        private static System.Reflection.MethodInfo FindObjectsOfType_methodinfo = null;
        // ------------------------------------- GetChild ------------------------------------- //
        /// <summary>
        /// Finds the first child with a matching name or tag. If SearchByName is true, it will look by name, otherwise by tag.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="NameOrTag">Name or tag to search by</param>
        /// <param name="SearchByName">True for search by name, false for search by tag</param>
        /// <returns>Found <see cref="GameObject"/> or null</returns>
        public static GameObject GetChild(this GameObject self, string NameOrTag, bool SearchByName)
        {
            if (SearchByName)
            {
                Transform transform = self.transform.Find(NameOrTag);
                GameObject found = transform != null ? transform.gameObject : null;
                return found;
            }
            else
            {
                foreach (Transform child in self.transform)
                {
                    if (child.CompareTag(NameOrTag))
                    {
                        return child.gameObject;
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// Finds the first child with a matching name or tag. If SearchByName is true, it will look by name, otherwise by tag. If FirstOnly is true, all children meeting the conditions will be included.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="NameOrTag">Name or tag to search by</param>
        /// <param name="SearchByName">True for search by name, false for search by tag</param>
        /// <param name="FirstOnly">Controls if only the first child should be the result, or all children meeting the conditions. <see cref="false"/> by default</param>
        /// <returns><see cref="HashSet{GameObject}"/></returns>
        public static HashSet<GameObject> GetChild(this GameObject self, string NameOrTag, bool SearchByName, bool FirstOnly = false)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();
            if (SearchByName)
            {
                if (FirstOnly)
                {
                    Transform transform = self.transform.Find(NameOrTag);
                    GameObject _ = transform != null ? transform.gameObject : null;
                    if (_ != null) { found.Add(_); }
                    return found;
                }
                else
                {
                    foreach (Transform child in self.transform)
                    {
                        if (child.name == NameOrTag)
                        {
                            found.Add(child.gameObject);
                        }
                    }
                    return found;
                }
            }
            else
            {
                foreach (Transform child in self.transform)
                {
                    if (child.CompareTag(NameOrTag))
                    {
                        found.Add(child.gameObject);
                        if (FirstOnly) { break; }
                    }
                }
                return found;
            }
        }
        /// <summary>
        /// Finds the first child with a matching InstanceID.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="InstanceID">(<see cref="int"/>) Unity InstanceID</param>
        /// <returns>Found <see cref="GameObject"/> or null</returns>
        public static GameObject GetChild(this GameObject self, int InstanceID)
        {
            foreach (Transform child in self.transform)
            {
                if (child.gameObject.GetInstanceID() == InstanceID)
                {
                    return child.gameObject;
                }
            }
            return null;
        }
        /// <summary>
        /// Finds the first child containing the matching component type.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="ComponentType"><see cref="Component"/> type</param>
        /// <returns>Found <see cref="GameObject"/> or null</returns>
        public static GameObject GetChild(this GameObject self, Type ComponentType)
        {
            Component retrieved = self.transform.GetComponentInChildren(ComponentType, true);
            if (retrieved != null)
            {
                return retrieved.gameObject;
            }
            return null;
        }
        /// <summary>
        /// Finds the first child containing the matching component type. If FirstOnly is true, all children meeting the conditions will be included.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="ComponentType"><see cref="Component"/> type</param>
        /// <param name="FirstOnly">Controls if only the first child should be the result, or all children meeting the conditions. False by default</param>
        /// <returns><see cref="HashSet{GameObject}"/></returns>
        public static HashSet<GameObject> GetChild(this GameObject self, Type ComponentType, bool FirstOnly = false)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();
            Component[] components = self.transform.GetComponentsInChildren(ComponentType, true);

            foreach (Component component in components)
            {
                found.Add(component.gameObject);
                if (FirstOnly) { break; }
            }
            return found;
        }
        /// <summary>
        /// Finds the first child meeting any of the custom <see cref="Func{GameObject, bool}"/> conditions in the specified params list.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="customConditions">params <see cref="Func{GameObject, bool}"/>[]</param>
        /// <returns>Found <see cref="GameObject"/> or null</returns>
        public static GameObject GetChild(this GameObject self, params Func<GameObject, bool>[] customConditions)
        {
            foreach (Transform child in self.transform)
            {
                bool meetsCondition = customConditions.Any(condition => condition(child.gameObject));
                if (meetsCondition)
                {
                    return child.gameObject;
                }
            }
            return null;
        }
        /// <summary>
        /// Finds the first child meeting any of the custom <see cref="Func{GameObject, bool}"/> conditions in the specified params list. If FirstOnly is true, all children meeting the conditions will be included.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="FirstOnly">Controls if only the first child should be the result, or all children meeting the conditions. False by default</param>
        /// <param name="customConditions">params <see cref="Func{GameObject, bool}"/>[]</param>
        /// <returns><see cref="HashSet{GameObject}"/></returns>
        public static HashSet<GameObject> GetChild(this GameObject self, bool FirstOnly = false, params Func<GameObject, bool>[] customConditions)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();
            foreach (Transform child in self.transform)
            {
                bool meetsCondition = customConditions.Any(condition => condition(child.gameObject));
                if (meetsCondition)
                {
                    found.Add(child.gameObject);
                    if (FirstOnly) { break; }
                }
            }
            return found;
        }
        /// <summary>
        /// Finds the first child meeting any of the custom <see cref="Func{GameObject, bool}"/> conditions in the specified params list. If RequiresAllConditions is true, then ALL custom conditions must return true for the object to be included. If FirstOnly is true, all children meeting the conditions will be included.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="FirstOnly">Controls if only the first child should be the result, or all children meeting the conditions. False by default</param>
        /// <param name="RequiresAllConditions">Controls if all or one custom condition need to be true for an object to be a valid result. False by default</param>
        /// <param name="customConditions">params <see cref="Func{GameObject, bool}"/>[]</param>
        /// <returns><see cref="HashSet{GameObject}"/></returns>
        public static HashSet<GameObject> GetChild(this GameObject self, bool FirstOnly = false, bool RequiresAllConditions = false, params Func<GameObject, bool>[] customConditions)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();
            foreach (Transform child in self.transform)
            {
                bool meetsCondition = RequiresAllConditions ?
                    customConditions.All(condition => condition(child.gameObject)) :
                    customConditions.Any(condition => condition(child.gameObject));

                if (meetsCondition)
                {
                    found.Add(child.gameObject);
                    if (FirstOnly) { break; }
                }
            }
            return found;
        }
        // ------------------------------------- GetChildren ------------------------------------- //
        /// <summary>
        /// Gets all child objects of an object.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of all child objects</returns>
        public static HashSet<GameObject> GetChildren(this GameObject self)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();
            foreach (Transform child in self.transform)
            {
                found.Add(child.gameObject);
            }
            return found;
        }
        // ------------------------------------- IsChildOf ------------------------------------- //
        /// <summary>
        /// Tests if an object is a child of another object.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="parent">Parent <see cref="GameObject"/></param>
        /// <returns>Result <see cref="bool"/></returns>
        public static bool IsChildOf(this GameObject self, GameObject parent)
        {
            return self.transform.parent == parent.transform;
        }

        // ------------------------------------- IsDescendantOf ------------------------------------- //
        /// <summary>
        /// Tests if an object is a descendant of another object.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="ancestor">Ancestor <see cref="GameObject"/></param>
        /// <returns></returns>
        public static bool IsDescendantOf(this GameObject self, GameObject ancestor)
        {
            return self.transform.IsChildOf(ancestor.transform); // Unity's child test searches like a descendant search ._.?
        }
        // ------------------------------------- IsParentOf ------------------------------------- //
        /// <summary>
        /// Tests if an object is a parent of another object.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="child">Child <see cref="GameObject"/></param>
        /// <returns>Result <see cref="bool"/></returns>
        public static bool IsParentOf(this GameObject self, GameObject child)
        {
            return child.transform.parent == self.transform;
        }
        // ------------------------------------- RetrieveAll ------------------------------------- //
        /// <summary>
        /// Gets all objects matching the name or tag specified.
        /// </summary>
        /// <param name="NameOrTag">A string that can represent a name OR unity tag</param>
        /// <param name="SearchByName">Boolean that controls if the NameOrTag string is in fact a name or tag. If true, it will be recognized as a name</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> RetrieveAll(string NameOrTag, bool SearchByName)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();
            GameObject[] GameObjectsInGame = GameObject.FindObjectsOfType<GameObject>(true);
            if (SearchByName)
            {
                foreach (GameObject gameObject in GameObjectsInGame)
                {
                    if (gameObject.name == NameOrTag)
                    {
                        found.Add(gameObject);
                    }
                }
            }
            else
            {
                found = GameObject.FindGameObjectsWithTag(NameOrTag).ToHashSet<GameObject>();
            }

            return found;
        }
        /// <summary>
        /// Gets all objects matching any of the names or tags in the list specified.
        /// </summary>
        /// <param name="SearchByName">Boolean that controls if the NamesOrTags param string[] is in fact an array of names or tags. If true, it will be recognized as a list of name</param>
        /// <param name="NamesOrTags">A param string[] that can represent a list of names or tags</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> RetrieveAll(bool SearchByName, params string[] NamesOrTags)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();
            GameObject[] GameObjectsInGame = GameObject.FindObjectsOfType<GameObject>(true);
            foreach (GameObject gameObject in GameObjectsInGame)
            {
                if (SearchByName)
                {
                    if (Array.Exists(NamesOrTags, element => element == gameObject.name))
                    {
                        found.Add(gameObject);
                    }
                }
                else
                {
                    if (Array.Exists(NamesOrTags, element => gameObject.CompareTag(element)))
                    {
                        found.Add(gameObject);
                    }
                }

            }
            return found;
        }
        /// <summary>
        /// Gets all objects matching any name or tag from either list given.
        /// </summary>
        /// <param name="Names">A string array of names to search by</param>
        /// <param name="Tags">A string array of tags to search by</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> RetrieveAll(string[] Names, string[] Tags)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();
            GameObject[] GameObjectsInGame = GameObject.FindObjectsOfType<GameObject>(true);
            foreach (GameObject gameObject in GameObjectsInGame)
            {
                if (Array.Exists(Names, element => element == gameObject.name) || Array.Exists(Tags, element => gameObject.CompareTag(element)))
                {
                    found.Add(gameObject);
                }
            }
            return found;
        }
        /// <summary>
        /// Gets all objects with the same <see cref="Type"/> specified.
        /// </summary>
        /// <param name="ComponentType">A <see cref="Type"/> to search by</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> RetrieveAll(Type ComponentType)
        {
            if (FindObjectsOfType_methodinfo == null)
            {
                FindObjectsOfType_methodinfo = typeof(UnityEngine.Object).GetMethod("FindObjectsOfType", new Type[] { typeof(bool) });
            }
            var genericMethod = FindObjectsOfType_methodinfo.MakeGenericMethod(ComponentType);
            Component[] componentsFound = (Component[])genericMethod.Invoke(null, new object[] { true });
            HashSet<GameObject> found = new HashSet<GameObject>();
            foreach (Component component in componentsFound)
            {
                found.Add(component.gameObject);
            }
            return found;
        }
        /// <summary>
        /// Gets all objects with the same <see cref="Type"/> from a list of types specified.
        /// </summary>
        /// <param name="ComponentTypes">An array of <see cref="Type"/>s to search by</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> RetrieveAll(Type[] ComponentTypes)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();
            if (FindObjectsOfType_methodinfo == null)
            {
                FindObjectsOfType_methodinfo = typeof(UnityEngine.Object).GetMethod("FindObjectsOfType", new Type[] { typeof(bool) });
            }
            foreach (Type type in ComponentTypes)
            {
                var genericMethod = FindObjectsOfType_methodinfo.MakeGenericMethod(type);
                Component[] componentsFound = (Component[])genericMethod.Invoke(null, new object[] { true });
                HashSet<GameObject> retrievedObjects = new HashSet<GameObject>();
                foreach (Component component in componentsFound)
                {
                    retrievedObjects.Add(component.gameObject);
                }
                found.UnionWith(retrievedObjects);
            }
            return found;
        }
        /// <summary>
        /// Gets all objects meeting any of the custom <see cref="Func{GameObject, bool}"/> conditions.
        /// </summary>
        /// <param name="customConditions"><see cref="Func{GameObject, bool}"/> conditions</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> RetrieveAll(params Func<GameObject, bool>[] customConditions)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();
            GameObject[] GameObjectsInGame = GameObject.FindObjectsOfType<GameObject>(true);
            foreach (GameObject gameObject in GameObjectsInGame)
            {
                bool shouldInclude = customConditions.Any(condition => condition(gameObject));
                if (shouldInclude)
                {
                    found.Add(gameObject);
                }
            }
            return found;
        }
        /// <summary>
        /// Gets all objects meeting any of the custom <see cref="Func{GameObject, bool}"/> conditions. If RequiresAllConditions is true, all conditions must be met.
        /// </summary>
        /// <param name="RequiresAllConditions">Controls weather all conditions or any conditions must be met to validate a result</param>
        /// <param name="customConditions"><see cref="Func{GameObject, bool}"/> conditions</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> RetrieveAll(bool RequiresAllConditions = false, params Func<GameObject, bool>[] customConditions)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();
            GameObject[] GameObjectsInGame = GameObject.FindObjectsOfType<GameObject>(true);
            foreach (GameObject gameObject in GameObjectsInGame)
            {
                bool shouldInclude = RequiresAllConditions ?
                    customConditions.All(condition => condition(gameObject)) :
                    customConditions.Any(condition => condition(gameObject));

                if (shouldInclude)
                {
                    found.Add(gameObject);
                }
            }
            return found;
        }
        /// <summary>
        /// Gets all objects.
        /// </summary>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> RetrieveAll()
        {
            return GameObject.FindObjectsOfType<GameObject>(true).ToHashSet();
        }
        // ------------------------------------- RetrieveByInstanceID ------------------------------------- //
        /// <summary>
        /// Finds an object with a unity InstanceID matching the one provided.
        /// </summary>
        /// <param name="InstanceID">Unity InstanceID integer</param>
        /// <returns>Retrieved <see cref="GameObject"/></returns>
        public static GameObject RetrieveByInstanceID(int InstanceID)
        {
            GameObject[] GameObjectsInGame = GameObject.FindObjectsOfType<GameObject>(true);
            foreach (GameObject gameObject in GameObjectsInGame)
            {
                if (gameObject.GetInstanceID() == InstanceID)
                {
                    return gameObject;
                }
            }
            return null;
        }
        // ------------------------------------- GetParent ------------------------------------- //
        /// <summary>
        /// Gets the parent of the object.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <returns>Parent <see cref="GameObject"/></returns>
        public static GameObject GetParent(this GameObject self)
        {
            Transform parentTransform = self.transform.parent;
            return parentTransform != null ? parentTransform.gameObject : null;
        }
        // ------------------------------------- SetParent ------------------------------------- //
        /// <summary>
        /// Changes the parent of a <see cref="GameObject"/> to the specified object.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="NewParent">New parent <see cref="GameObject"/></param>
        /// <param name="KeepOriginalWorldPosition">If the object should NOT be transformed to the new parent's position (keep it's position). Default is true</param>
        public static void SetParent(this GameObject self, GameObject NewParent, bool KeepOriginalWorldPosition = true)
        {
            self.transform.SetParent(NewParent.transform, KeepOriginalWorldPosition);
        }
        // ------------------------------------- GetDescendants ------------------------------------- //
        /// <summary>
        /// Recursively retrieves all lower objects in the hierarchy.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> GetDescendants(this GameObject self)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();

            void Search(GameObject target)
            {
                HashSet<GameObject> children = target.GetChildren();
                foreach (GameObject child in children)
                {
                    found.Add(child);
                    Search(child);
                }
            }
            Search(self);
            return found;
        }
        /// <summary>
        /// Recursively retrieves all lower objects in the hierarchy EXCLUDING any meeting any of the <see cref="Func{GameObject, bool}"/> conditions.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="excludeConditions"><see cref="Func{GameObject, bool}"/> conditions</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> GetDescendants(this GameObject self, params Func<GameObject, bool>[] excludeConditions)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();

            void Search(GameObject target)
            {
                HashSet<GameObject> children = target.GetChildren();
                foreach (GameObject child in children)
                {
                    bool shouldExclude = excludeConditions.Any(condition => condition(child));
                    if (!shouldExclude)
                    {
                        found.Add(child);
                        Search(child);
                    }
                }
            }
            Search(self);
            return found;
        }
        /// <summary>
        /// Recursively retrieves all lower objects in the hierarchy EXCLUDING any meeting the <see cref="Func{GameObject, bool}"/> conditions. If RequiresAllConditions is true, all conditions must be met for an object to be excluded.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="RequiresAllConditions">Controls weather all conditions or one condition must be met to exclude an object</param>
        /// <param name="excludeConditions"><see cref="Func{GameObject, bool}"/> conditions</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> GetDescendants(this GameObject self, bool RequiresAllConditions = false, params Func<GameObject, bool>[] excludeConditions)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();

            void Search(GameObject target)
            {
                HashSet<GameObject> children = target.GetChildren();
                foreach (GameObject child in children)
                {
                    bool shouldExclude = RequiresAllConditions ?
                        excludeConditions.All(condition => condition(child)) :
                        excludeConditions.Any(condition => condition(child));

                    if (!shouldExclude)
                    {
                        found.Add(child);
                        Search(child);
                    }
                }
            }
            Search(self);
            return found;
        }
        // ------------------------------------- GetAncestors ------------------------------------- //
        /// <summary>
        /// Retrieves all higher objects in the hierarchy to the scene root.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> GetAncestors(this GameObject self)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();

            void Search(GameObject target)
            {
                GameObject parent = target.GetParent();
                if (parent != null)
                {
                    found.Add(parent);
                    Search(parent);
                }
            }
            Search(self);
            return found;
        }
        /// <summary>
        /// Retrieves all higher objects in the hierarchy to the scene root cutting off at any meeting any of the <see cref="Func{GameObject, bool}"/> conditions.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="excludeConditions"><see cref="Func{GameObject, bool}"/> conditions</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> GetAncestors(this GameObject self, params Func<GameObject, bool>[] excludeConditions)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();

            void Search(GameObject target)
            {
                GameObject parent = target.GetParent();
                if (parent != null)
                {
                    bool shouldExclude = excludeConditions.Any(condition => condition(parent));
                    if (!shouldExclude)
                    {
                        found.Add(parent);
                        Search(parent);
                    }
                }
            }
            Search(self);
            return found;
        }
        /// <summary>
        /// Retrieves all higher objects in the hierarchy to the scene root cutting off at any meeting the <see cref="Func{GameObject, bool}"/> conditions. If RequiresAllConditions is true, all conditions must be met for an object to be excluded.
        /// </summary>
        /// <param name="self"><see cref="GameObject"/> self</param>
        /// <param name="RequiresAllConditions">Controls weather all conditions or one condition must be met to exclude an object</param>
        /// <param name="excludeConditions"><see cref="Func{GameObject, bool}"/> conditions</param>
        /// <returns>A <see cref="HashSet{GameObject}"/> of objects</returns>
        public static HashSet<GameObject> GetAncestors(this GameObject self, bool RequiresAllConditions, params Func<GameObject, bool>[] excludeConditions)
        {
            HashSet<GameObject> found = new HashSet<GameObject>();

            void Search(GameObject target)
            {
                GameObject parent = target.GetParent();
                if (parent != null)
                {
                    bool shouldExclude = RequiresAllConditions ?
                        excludeConditions.All(condition => condition(parent)) :
                        excludeConditions.Any(condition => condition(parent));
                    if (!shouldExclude)
                    {
                        found.Add(parent);
                        Search(parent);
                    }
                }
            }
            Search(self);
            return found;
        }
    }
}