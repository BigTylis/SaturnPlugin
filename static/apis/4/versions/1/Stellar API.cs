// Imports
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Stellar.APIs.API.SuperThreading;
using System.Collections;
using Unity.Collections;
using System.Text;

// Self imports
using Stellar.APIs.API.General;

// Ambiguous clarifications
using Component = UnityEngine.Component;
using Debug = UnityEngine.Debug;
using Debugger = Stellar.APIs.API.General.Debugger;

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
        /// For game mouse move input events
        /// </summary>
        public class MouseMoveEventArgs : EventArgs
        {
            public MouseMoveEventArgs(float anglex, float angley)
            {
                AngleX = anglex;
                AngleY = angley;
            }
            public float AngleX { get; private set; }
            public float AngleY { get; private set; }
        }
        /// <summary>
        /// A tool class that provides multiple ways to smoothly interpolate between various number structures
        /// </summary>
        public static class TweenTools
        {
            /// <summary>
            /// Linearly interpolates to create smooth easing between start and end
            /// </summary>
            /// <param name="start">The starting value</param>
            /// <param name="end">The ending value. The start value is interpolated towards this value</param>
            /// <param name="t">The speed of interpolation. Must be 0-1. Higher numbers result in quicker speeds</param>
            /// <returns></returns>
            public static float Lerp(float start, float end, float t)
            {
                t = Mathf.Clamp01(t);
                return start + (end - start) * t;
            }
            /// <summary>
            /// Linearly interpolates in reverse to create smooth easing between end and start
            /// </summary>
            /// <param name="start">The starting value. The end value is interpolated towards this value</param>
            /// <param name="end">The ending value</param>
            /// <param name="t">The speed of interpolation. Must be 0-1. Lower numbers result in quicker speeds</param>
            /// <returns></returns>
            public static float InverseLerp(float start, float end, float t)
            {
                t = Mathf.Clamp01(t);
                return end + (start - end) * t;
            }
        }

        /// <summary>
        /// A two dimensional float, storing an x and y
        /// </summary>
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
        /// A collection of game engine based math functions
        /// </summary>
        public struct Mathg
        {
            /// <summary>
            /// Gets the percentage of how far a value is between 2 other values
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
        }

        /// <summary>
        /// Generates a random number using a seed
        /// </summary>
        public static class SeededRandom
        {
            /// <summary>
            /// Returns a random integer between the min and max using a seed
            /// </summary>
            /// <param name="Min">The minimum value</param>
            /// <param name="Max">The maximum value</param>
            /// <param name="Seed">The seed</param>
            /// <returns></returns>
            public static int GenerateInt(int Min, int Max, int Seed)
            {
                System.Random random = new System.Random(Seed);
                return random.Next(Min, Max + 1);
            }
            /// <summary>
            /// Returns a random float between the min and max using a seed
            /// </summary>
            /// <param name="Min">The minimum value</param>
            /// <param name="Max">The maximum value</param>
            /// <param name="Seed">The seed</param>
            /// <returns></returns>
            public static float GenerateFloat(float Min, float Max, int Seed)
            {
                System.Random random = new System.Random(Seed);
                return (float)(Min + random.NextDouble() * (Max - Min));
            }
            /// <summary>
            /// Returns a random Vector2 using a seed
            /// </summary>
            /// <param name="Min">The minimum value</param>
            /// <param name="Max">The maximum value</param>
            /// <param name="Seed">The seed</param>
            /// <returns></returns>
            public static Vector2 GenerateXY(float Min, float Max, int Seed)
            {
                System.Random random = new System.Random(Seed);
                return new Vector2((float)(Min + random.NextDouble() * (Max - Min)), (float)(Min + random.NextDouble() * (Max - Min)));
            }
        }

        /// <summary>
        /// A class that assists in loading and managing Unity assets during runtime
        /// </summary>
        [Obsolete("Depracted. Use Assetables API for future asset retrieving. This class depended on Unity.Addressables package.")]
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
                private readonly Dictionary<string, ProfileInfo> storedProfiles = new();

                internal Profiler()
                {
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
                    startTime = Time.realtimeSinceStartup;
                    startFrameAmount = Time.frameCount;
                    stopwatch.Restart();
                }
                /// <summary>
                /// Stops the profilers profiling and stores the profile info under the specified start name. Also allows for instant printing based on the <see cref="ProfilerPrintType"/> type.
                /// </summary>
                /// <param name="printResult"></param>
                public void Stop(ProfilerPrintType printResult = ProfilerPrintType.Dont)
                {
                    stopwatch.Stop();
                    long totalMemory = GC.GetTotalMemory(false) - startMemory; totalMemory = totalMemory < 0 ? 0 : totalMemory;
                    float totalTimeSeconds = Time.realtimeSinceStartup - startTime;
                    long totalTimeMilliseconds = stopwatch.ElapsedMilliseconds;
                    long totalTimeTicks = stopwatch.ElapsedTicks;
                    int totalFrames = Time.frameCount - startFrameAmount;

                    ProfileInfo profileInfo = new(recordingProfileName, totalMemory, totalTimeSeconds, totalTimeMilliseconds, totalTimeTicks, totalFrames);
                    if (printResult != ProfilerPrintType.Dont) { PrintProfileInfo(profileInfo, printResult); }

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
                    if(storedProfiles.TryGetValue(profileName, out ProfileInfo profileInfo))
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
                    if(profileInfo != null)
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
                        case ProfilerPrintType.PrintAll: { Log($"----- Debug Profile -----\nProfile Name: {info.Name}\nExecution Time: {info.ExecutionTimeSeconds}s | {info.ExecutionTimeMilliseconds}ms | {info.ExecutionTimeTicks} ticks\nMemory Usage: {info.MemoryUsage/1024L/1024L} MBs | {info.MemoryUsage/1024L} KBs | {info.MemoryUsage} Bytes\nFrames Rendered: {info.FrameCount}\n-----------------------"); } break;
                    }
                }
            }

            /// <summary>
            /// Creates a new profiler instance
            /// </summary>
            /// <returns><see cref="Profiler"/></returns>
            public static Profiler CreateProfiler() { return  new Profiler(); }
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
    }

    /// <summary>
    /// Tools for generating HeightMaps/random perlin noise maps
    /// </summary>
    namespace HeightMapping
    {
        /// <summary>
        /// A storage class for containing height maps as floats
        /// </summary>
        public class HeightMap
        {
            internal HeightMap(float[,] map) { this.Map = map; }
            /// <summary>
            /// The plot data of the height map
            /// </summary>
            public float[,] Map { get; }

            bool lowestset = false;
            private float lowestvalue = 0;
            private Float2D lowestvalueposition;
            bool highestset = false;
            private float highestvalue = 0;
            private Float2D highestvalueposition;

            private void UpdateLowest()
            {
                int width = Map.GetLength(0);
                int height = Map.GetLength(1);
                float L = Mathf.Infinity;
                Float2D LPOS = new Float2D(width, height);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        float value = Map[x, y];
                        if (value < L) { L = value; LPOS = new Float2D(x, y); }
                    }
                }

                lowestvalue = L;
                lowestvalueposition = LPOS;
            }
            private void UpdateHighest()
            {
                int width = Map.GetLength(0);
                int height = Map.GetLength(1);
                float H = -Mathf.Infinity;
                Float2D HPOS = new Float2D(0, 0);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        float value = Map[x, y];
                        if (value > H) { H = value; HPOS = new Float2D(x, y); }
                    }
                }

                highestvalue = H;
                highestvalueposition = HPOS;
            }
            /// <summary>
            /// The lowest value in the heightmap
            /// </summary>
            public float Lowest
            {
                get
                {
                    if (!lowestset)
                    {
                        UpdateLowest();
                    }
                    return lowestvalue;
                }
            }
            /// <summary>
            /// The position that the lowest heightmap value resides
            /// </summary>
            public Float2D LowestPosition
            {
                get
                {
                    if (!lowestset)
                    {
                        UpdateLowest();
                    }
                    return lowestvalueposition;
                }
            }
            /// <summary>
            /// The highest value in the heightmap
            /// </summary>
            public float Highest
            {
                get
                {
                    if (!highestset)
                    {
                        UpdateHighest();
                    }
                    return highestvalue;
                }
            }
            /// <summary>
            /// The position that the highest heightmap value resides
            /// </summary>
            public Float2D HighestPosition
            {
                get
                {
                    if (!highestset)
                    {
                        UpdateHighest();
                    }
                    return highestvalueposition;
                }
            }

        }
        /// <summary>
        /// A configuration class for configuring HeightMap generation
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
            public int seed;

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
            /// <summary>
            /// Returns the default HeightMap configuration
            /// </summary>
            /// <returns>The default <see cref="HeightMapConfiguration"/></returns>
            public static HeightMapConfiguration Default()
            {
                return new HeightMapConfiguration();
            }
        }
        /// <summary>
        /// A tool class for generating HeightMaps
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

                Vector2 OffsetVector2 = SeededRandom.GenerateXY(0f, 10000f, config.seed);
                float offsetX = OffsetVector2.x; //Random.Range(0f, 10000f);
                float offsetY = OffsetVector2.y; //Random.Range(0f, 10000f);

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
            /// Generates a Heightmap with default configuration
            /// </summary>
            /// <returns>A <see cref="HeightMap"/> object</returns>
            public static HeightMap GenerateMap()
            {
                return generate(HeightMapConfiguration.Default());
            }
            /// <summary>
            /// Generates a Heightmap with specified configuration
            /// </summary>
            /// <param name="config"><see cref="HeightMapConfiguration"/> with desired configuration</param>
            /// <returns>A <see cref="HeightMap"/> object</returns>
            public static HeightMap GenerateMap(HeightMapConfiguration config)
            {
                return generate(config);
            }
        }
    }

    /// <summary>
    /// Extension classes and methods that extend the capabilities of existing libraries
    /// </summary>
    namespace Extensions
    {
        /// <summary>
        /// A collection of extension methods for the base type <see cref="string"/>.
        /// </summary>
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
                if(Internal.ContextSynchronizer._unityContext == null)
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
            public static void PostQueueOnePerFrame(Action action)
            {
                Internal.ConcurrentQueueManager._singleFrameActions.Enqueue(action);
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
                    internal static readonly ConcurrentQueue<Action> _normalActions = new ConcurrentQueue<Action>();
                    internal static readonly ConcurrentQueue<Action> _singleFrameActions = new ConcurrentQueue<Action>();
                    private readonly List<Action> _pendingSingleFrameActions = new List<Action>();
                    private void Update()
                    {
                        while (_normalActions.TryDequeue(out var action))
                        {
                            action?.Invoke();
                        }

                        if (_pendingSingleFrameActions.Count == 0)
                        {
                            while (_singleFrameActions.TryDequeue(out var act))
                            {
                                _pendingSingleFrameActions.Add(act);
                            }
                        }
                        if (_pendingSingleFrameActions.Count > 0)
                        {
                            var nextAction = _pendingSingleFrameActions[0];
                            _pendingSingleFrameActions.RemoveAt(0);
                            nextAction.Invoke();
                        }
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
                                if(s.Wait(0)) // Not held
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
                        };
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
                        };
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
        /// A super dynamic and efficient class that takes <see cref="Coroutine"/>, <see cref="Task"/>, and <see cref="Thread"/> and combines them into an all in one use threading system. All code is passed into them via constructors allowing for you to write whatever you want into them. <see cref="NovaThread"/> provides enums for status information (<see cref="NovaThreadStatus"/>) and completion state information (<see cref="NovaThreadCompletionStatus"/>), a completion event (<see cref="NovaThread.OnWorkCompleted"/>), a custom <see cref="SuperThreading.PauseToken"/> system for users to implement pausing mechanisms, and the use of <see cref="System.Threading.CancellationTokenSource"/> to allow users to implement cancellation systems (or immediately cancel in <see cref="NovaThread"/>s using <see cref="Coroutine"/> framework).
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
                
                if(coroutineAction != null && monoScript != null)
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

                if(action != null)
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

                if(action != null)
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
                if(ThreadFramework == ThreadType.Coroutine)
                {
                    if(coroutine != null && coroHost != null)
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
                if(ThreadFramework == ThreadType.Task)
                {
                    if(setTask != null)
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
                if(ThreadFramework == ThreadType.Thread)
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
                if(calledCancellation) { throw new InvalidOperationException("Cannot invoke Cancel() on NovaThread instance since it was already cancelled."); }
                if (ThreadFramework == ThreadType.Coroutine)
                {
                    if(coroHost != null && runningCoroutine != null)
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
                if(ThreadFramework == ThreadType.Thread)
                {
                    if(setThread != null)
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

    namespace Internal
    {
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
                internalObj.AddComponent<T>();
            }
        }
    }
}


/// <summary>
/// Desc: Originally part of Stellar.APIs.API but moved so it is cross compatible with a different stellar API. Idk its kinda weird
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
                GameObject found = self.transform.Find(NameOrTag)?.gameObject;
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
                    GameObject _ = self.transform.Find(NameOrTag)?.gameObject;
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