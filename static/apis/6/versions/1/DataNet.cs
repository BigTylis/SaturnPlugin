// Defines
#define WINDOWS

// Imports
using Open.Nat;

//using Stellar.APIs.API.General;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Desc: Data transfer and networking API
/// Usage: Allows you to transfer data between local machine processes, over local networks, and across the internet using UPnP
/// 
///
///          %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%@          
///         %%========================================%%
///         %#=*%%%+%%%#*%%%+==================*%#====#%         
///         %#=*%%%+#%%*+%%%+=================#%#%%===#%         
///         %#=+###=###*+###+=================+%%%*===#%         
///         %#========================================#%         
///         %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
///                              %%
///          %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
///         %%========================================#%         
///         %#=*%%%+#%%*+%%%+=================+#%%+===#%         
///         %#=*%%%+%%%#+%%%+=================#% %%===#%         
///         %#=*%%%+#%%*+%%%+=================+###+===#%         
///         %%========================================#%         
///          %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
///                              %%
///         %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%@         
///         %#========================================#%         
///         %#=+###=*##++###+=================+%%%*===#%         
///         %#=*%%%=%%%*+%%%+=================#% %%===#%         
///         %#=*%%%+%%%*+%%%+==================+**+===#%         
///         %%+====================================== +%%
///          %%%%%%%%%%%%%%%%%%%%%%%%%%@@@@@@@@%%@%%@%%
///                              %%
///                              %%
///                             %%%%
///             %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
///                             %%%%
///
///
/// Credit: Written and Documented entirely by BigTylis
/// </summary>
namespace Stellar.APIs.DataNet
{
    public sealed class AlreadyConnectedException : Exception
    {
        public AlreadyConnectedException() { }
        public AlreadyConnectedException(string message) : base(message) { }
    }
    public enum ConnectionStatus
    {
        NotConnected,
        Attempting,
        Connected
    }

    /// <summary>
    /// Allows you to implement a simple flexible exception handling system. Copied from Stellar.APIs.API to be dependency independant.
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
    /// Thread safe version of Stellar.APIs.API.HashList, allowing for heavy read/writes in parallel execution. Copied from Stellar.APIs.API to be dependency independant.
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
    /// Data client for local machine data transfer between processes.
    /// </summary>
    public class DataPipeClient : FlexibleCatcher, IDisposable
    {
        private bool _disposed = false;

        internal NamedPipeClientStream Stream;
        private CancellationTokenSource DisconnectToken;
        public readonly string ClientName = string.Empty;
        private volatile bool Disconnecting = false;

        private volatile ConnectionStatus _connectionstatus = ConnectionStatus.NotConnected;
        public ConnectionStatus ConnectionStatus
        {
            get { return _connectionstatus; }
            internal set
            {
                _connectionstatus = value;
                ConnectionStateChanged?.Invoke(value);
            }
        }

        public event Action<ConnectionStatus> ConnectionStateChanged;
        public event Action ClientConnected;
        public event Action ClientDisconnected;
        public event Action<byte[]> DataReceived;

        /// <summary>
        /// Initialize new <see cref="DataPipeClient"/>
        /// </summary>
        /// <param name="clientName">Clients name. This name is important for connections in <see cref="DataPipeServer"/></param>
        /// <param name="roughExceptions">If true, exceptions will be thrown inside of their failed method. Otherwise they are not immediately thrown, but can still be viewed in <see cref="LastFailureException"/> along with a <see cref="Boolean"/> failure status provided by most methods</param>
        public DataPipeClient(string clientName, bool roughExceptions = true)
        {
            ClientName = clientName;
            RoughExceptions = roughExceptions;
        }
        /// <summary>
        /// Initialize new <see cref="DataPipeClient"/> and immediately try to connect to a server with the specified timeout (ms).
        /// </summary>
        /// <param name="clientName">Clients name. This name is important for connections in <see cref="DataPipeServer"/></param>
        /// <param name="connectionTimeout">Timeout in milliseconds</param>
        /// <param name="roughExceptions">If true, exceptions will be thrown inside of their failed method. Otherwise they are not immediately thrown, but can still be viewed in <see cref="LastFailureException"/> along with a <see cref="Boolean"/> failure status provided by most methods</param>
        public DataPipeClient(string clientName, int connectionTimeout, bool roughExceptions = true)
        {
            ClientName = clientName;
            RoughExceptions = roughExceptions;
            Task.Run(async () => await ConnectToServerAsync(connectionTimeout));
        }

        public async Task<bool> ConnectToServerAsync(int timeout)
        {
            if (_disposed) throw new ObjectDisposedException($"DataPipeClient {ClientName}");
            if (Disconnecting) { handleException(new InvalidOperationException("Cannot attempt to connect to server while a disconnect operation is running.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Attempting) { handleException(new InvalidOperationException("Cannot attempt to connect to server while already trying.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Connected) { handleException(new AlreadyConnectedException($"Already connected to server.")); return false; }
            if (Stream == null) { create(); }

            try
            {
                ConnectionStatus = ConnectionStatus.Attempting;
                await Stream.ConnectAsync(timeout);
                ConnectionStatus = ConnectionStatus.Connected;
                ClientConnected?.Invoke();
#pragma warning disable CS4014, CS1998
                Task.Run(async () => beginListening());
#pragma warning restore CS4014, CS1998
            }
            catch (Exception e)
            {
                ConnectionStatus = ConnectionStatus.NotConnected;
                handleException(e);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Connect with optional cancellation support
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task<bool> ConnectToServerAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException($"DataPipeClient {ClientName}");
            if (Disconnecting) { handleException(new InvalidOperationException("Cannot attempt to connect to server while a disconnect operation is running.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Attempting) { handleException(new InvalidOperationException("Cannot attempt to connect to server while already trying.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Connected) { handleException(new AlreadyConnectedException($"Already connected to server.")); return false; }
            if (Stream == null) { create(); }

            try
            {
                ConnectionStatus = ConnectionStatus.Attempting;
                await Stream.ConnectAsync(cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    ConnectionStatus = ConnectionStatus.Connected;
                    ClientConnected?.Invoke();
#pragma warning disable CS4014, CS1998
                    Task.Run(async () => beginListening());
#pragma warning restore CS4014, CS1998
                }
                else
                {
                    ConnectionStatus = ConnectionStatus.NotConnected;
                    handleException(new TaskCanceledException("Task was cancelled before it could complete."));
                }
            }
            catch (Exception e)
            {
                ConnectionStatus = ConnectionStatus.NotConnected;
                handleException(e);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Connect with optional cancellation support
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task<bool> ConnectToServerAsync(CancellationToken cancellationToken, int timeout = 5000)
        {
            if (_disposed) throw new ObjectDisposedException($"DataPipeClient {ClientName}");
            if (Disconnecting) { handleException(new InvalidOperationException("Cannot attempt to connect to server while a disconnect operation is running.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Attempting) { handleException(new InvalidOperationException("Cannot attempt to connect to server while already trying.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Connected) { handleException(new AlreadyConnectedException($"Already connected to server.")); return false; }
            if (Stream == null) { create(); }

            try
            {
                ConnectionStatus = ConnectionStatus.Attempting;
                await Stream.ConnectAsync(timeout, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    ConnectionStatus = ConnectionStatus.Connected;
                    ClientConnected?.Invoke();
#pragma warning disable CS4014, CS1998
                    Task.Run(async () => beginListening());
#pragma warning restore CS4014, CS1998
                }
                else
                {
                    ConnectionStatus = ConnectionStatus.NotConnected;
                    handleException(new TaskCanceledException("Task was cancelled before it could complete."));
                }
            }
            catch (Exception e)
            {
                ConnectionStatus = ConnectionStatus.NotConnected;
                handleException(e);
                return false;
            }
            return true;
        }
        public async Task<bool> DisconnectAsync()
        {
            if (_disposed) throw new ObjectDisposedException($"DataPipeClient {ClientName}");
            if (Disconnecting) { handleException(new InvalidOperationException("Cannot attempt to disconnect while another disconnect operation is already running")); return false; }
            if (ConnectionStatus == ConnectionStatus.NotConnected) { handleException(new InvalidOperationException("Cannot attempt to disconnect when not connected to begin with.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Attempting) { handleException(new InvalidOperationException("Cannot attempt to disconnect at the same time as trying to connect to a server. To cancel connections, instead use CancellationTokens.")); return false; }

            Disconnecting = true;
            try
            {
                DisconnectToken?.Cancel();
                await Stream?.FlushAsync();
            }
            catch { }
            finally
            {
                Stream?.Dispose();
                Stream = null;
                DisconnectToken?.Dispose();
                DisconnectToken = null;
                ConnectionStatus = ConnectionStatus.NotConnected;
                ClientDisconnected?.Invoke();
                Disconnecting = false;
            }
            return true;
        }
        public bool Disconnect()
        {
            if (_disposed) throw new ObjectDisposedException($"DataPipeClient {ClientName}");
            if (Disconnecting) { handleException(new InvalidOperationException("Cannot attempt to disconnect while another disconnect operation is already running")); return false; }
            if (ConnectionStatus == ConnectionStatus.NotConnected) { handleException(new InvalidOperationException("Cannot attempt to disconnect when not connected to begin with.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Attempting) { handleException(new InvalidOperationException("Cannot attempt to disconnect at the same time as trying to connect to a server. To cancel connections, instead use CancellationTokens.")); return false; }

            Disconnecting = true;
            try
            {
                DisconnectToken?.Cancel();
                Stream?.Flush();
            }
            catch { }
            finally
            {
                Stream?.Dispose();
                Stream = null;
                DisconnectToken?.Dispose();
                DisconnectToken = null;
                ConnectionStatus = ConnectionStatus.NotConnected;
                ClientDisconnected?.Invoke();
                Disconnecting = false;
            }
            return true;
        }

        /// <summary>
        /// Write in raw byte data
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> WriteDataAsync(byte[] message)
        {
            return await write(message);
        }
        /// <summary>
        /// Automatically convert a string into byte data then write
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> WriteDataAsync(string message)
        {
            return await write(Encoding.UTF8.GetBytes(message));
        }

        public void Dispose()
        {
            if (_disposed) throw new ObjectDisposedException($"DataPipeClient {ClientName}");
            _disposed = true;

            try
            {
                DisconnectToken?.Cancel();
                Stream?.Flush();
            }
            catch { }
            finally
            {
                Stream?.Dispose();
                DisconnectToken?.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        private async Task<bool> write(byte[] message)
        {
            if (_disposed) throw new ObjectDisposedException($"DataPipeClient {ClientName}");
            if (Stream == null || this.ConnectionStatus == ConnectionStatus.NotConnected || this.ConnectionStatus == ConnectionStatus.Attempting || Disconnecting) return false;

            try
            {
                await DataStreamHelper.WriteStreamAsync(Stream, message, DisconnectToken.Token);
            }
            catch (Exception e)
            {
                await DisconnectAsync();
                handleException(e);
                return false;
            }

            return true;
        }

        private async Task beginListening()
        {
            while (true)
            {
                if (Stream == null || DisconnectToken == null) break;
                if (DisconnectToken.IsCancellationRequested) break;

                try
                {
                    byte[] message = await DataStreamHelper.ReadStreamAsync(Stream, DisconnectToken.Token);
                    DataReceived?.Invoke(message);
                }
                catch (OperationCanceledException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception e)
                {
                    await DisconnectAsync();
                    handleException(e, "beginListening");
                    break;
                }
            }
        }
        private void create()
        {
            Stream = new(".", ClientName, PipeDirection.InOut, PipeOptions.Asynchronous);
            DisconnectToken = new();
        }
    }

    /// <summary>
    /// Data server for local machine data transfer between processes.
    /// </summary>
    public class DataPipeServer : FlexibleCatcher, IDisposable
    {
        private bool _disposed = false;

        internal ConcurrentDictionary<string, ManagedPipeServerStream> clients = new();

        public event Action<string> ClientJoined;
        public event Action<string> ClientLeft;
        public event Action<string, byte[]> ClientSentData;

        /// <summary>
        /// Initialize new <see cref="DataPipeServer"/>
        /// </summary>
        /// <param name="roughExceptions">If true, exceptions will be thrown inside of their failed method. Otherwise they are not immediately thrown, but can still be viewed in <see cref="LastFailureException"/> along with a <see cref="Boolean"/> failure status provided by most methods</param>
        public DataPipeServer(bool roughExceptions = true)
        {
            RoughExceptions = roughExceptions;
        }

        public IReadOnlyDictionary<string, ManagedPipeServerStream> GetConnectedClients() => clients;
        /// <summary>
        /// Wait for an attempted connection from the specified client. This may hang indefinitely if the client never connects, since there is no timeout.
        /// </summary>
        /// <param name="clientName"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task<bool> TryEstablishConnectionAsync(string clientName)
        {
            if (_disposed) throw new ObjectDisposedException($"DataPipeServer");
            if (clients.ContainsKey(clientName)) { handleException(new AlreadyConnectedException($"A connection by the name '{clientName}' already exists. Connections must be unique.")); return false; }
            try
            {
                ManagedPipeServerStream newConnectionStream = new(new NamedPipeServerStream(clientName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous));
                clients[clientName] = newConnectionStream;

                await newConnectionStream.Stream.WaitForConnectionAsync();
                newConnectionStream.IsConnected = true;
                ClientJoined?.Invoke(clientName);
#pragma warning disable CS4014, CS1998
                Task.Run(async () => newListener(newConnectionStream, clientName));
#pragma warning restore CS4014, CS1998
            }
            catch (Exception e)
            {
                await TryBreakConnectionAsync(clientName);
                handleException(e);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Wait for an attempted connection from the specified client with supported cancellation. This may hang indefinitely if the client never connects, which is why the <see cref="CancellationToken"/> is important.
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task<bool> TryEstablishConnectionAsync(string clientName, CancellationToken cancellationToken)
        {
            if (_disposed) throw new ObjectDisposedException($"DataPipeServer");
            if (clients.ContainsKey(clientName)) { handleException(new AlreadyConnectedException($"A connection by the name '{clientName}' already exists. Connections must be unique.")); return false; }
            try
            {
                ManagedPipeServerStream newConnectionStream = new(new NamedPipeServerStream(clientName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous));
                clients[clientName] = newConnectionStream;

                await newConnectionStream.Stream.WaitForConnectionAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    handleException(new OperationCanceledException("Operation canceled before it could complete."));
                    return false;
                }
                else
                {
                    newConnectionStream.IsConnected = true;
                    ClientJoined?.Invoke(clientName);
#pragma warning disable CS4014, CS1998
                    Task.Run(async () => newListener(newConnectionStream, clientName));
#pragma warning restore CS4014, CS1998
                }
            }
            catch (Exception e)
            {
                await TryBreakConnectionAsync(clientName);
                handleException(e);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Removes the clients <see cref="ManagedPipeServerStream"/> from the connection pool, then disposes of it
        /// </summary>
        /// <param name="clientName"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task<bool> TryBreakConnectionAsync(string clientName)
        {
            if (_disposed) throw new ObjectDisposedException($"DataPipeServer");
            if (!clients.TryRemove(clientName, out var connectionStream)) { handleException(new Exception($"No connection by the name '{clientName}' is registered."), true); return false; }

            await connectionStream.DisposeAsync();
            ClientLeft?.Invoke(clientName);
            return true;
        }
        /// <summary>
        /// Removes the clients <see cref="ManagedPipeServerStream"/> from the connection pool, then disposes of it
        /// </summary>
        /// <param name="clientName"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public bool TryBreakConnection(string clientName)
        {
            if (_disposed) throw new ObjectDisposedException($"DataPipeServer");
            if (!clients.TryRemove(clientName, out ManagedPipeServerStream connectionStream)) { handleException(new Exception($"No connection by the name '{clientName}' is registered."), true); return false; }

            connectionStream.Dispose();
            ClientLeft?.Invoke(clientName);
            return true;
        }
        /// <summary>
        /// Write to client in raw byte data
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WriteDataToClientAsync(string clientName, byte[] message)
        {
            return await write(clientName, message);
        }
        /// <summary>
        /// Automatically convert a string into byte data then write to client
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> WriteDataToClientAsync(string clientName, string message)
        {
            return await write(clientName, Encoding.UTF8.GetBytes(message));
        }
        /// <summary>
        /// Checks if the specified client name is stored in the connection pool and gives their connection stream if they are
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="managedStream"></param>
        /// <returns></returns>
        public bool TryGetClientConnectionStream(string clientName, out ManagedPipeServerStream managedStream)
        {
            if (clients.TryGetValue(clientName, out var managed))
            {
                managedStream = managed;
                return true;
            }
            managedStream = null;
            return false;
        }

        public void Dispose()
        {
            if (_disposed) throw new ObjectDisposedException($"DataPipeServer");
            _disposed = true;

            foreach (var client in clients.Values)
            {
                try
                {
                    client?.Dispose();
                }
                catch { }
            }
            clients.Clear();

            GC.SuppressFinalize(this);
        }

        private async Task<bool> write(string clientName, byte[] message)
        {
            if (_disposed) throw new ObjectDisposedException($"DataPipeServer");
            if (!clients.TryGetValue(clientName, out ManagedPipeServerStream stream)) { handleException(new Exception($"Cant write to a connection by the name '{clientName}'. None is registered.")); return false; }

            try
            {
                await DataStreamHelper.WriteStreamAsync(stream.Stream, message, stream.DisconnectToken.Token);
            }
            catch (Exception e)
            {
                // Disconnect on failure
                await TryBreakConnectionAsync(clientName);
                handleException(e);
                return false;
            }

            return true;
        }
        private async Task newListener(ManagedPipeServerStream managedStream, string clientName)
        {
            while (true)
            {
                if (managedStream.Stream == null || managedStream.DisconnectToken == null) break;
                if (managedStream.DisconnectToken.IsCancellationRequested) break;

                try
                {
                    byte[] message = await DataStreamHelper.ReadStreamAsync(managedStream.Stream, managedStream.DisconnectToken.Token);
                    ClientSentData?.Invoke(clientName, message);
                }
                catch (OperationCanceledException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception e)
                {
                    await TryBreakConnectionAsync(clientName);
                    handleException(e, "newListener");
                    break;
                }
            }
        }

        protected override void handleException(Exception exception, [CallerMemberName] string context = null)
        {
            base.handleException(exception, context);
        }
        private void handleException(Exception exception, bool dontThrow, [CallerMemberName] string context = null)
        {
            if (dontThrow) LastFailureException = exception;
            else base.handleException(exception, context);
        }

        /// <summary>
        /// A wrapper for <see cref="NamedPipeServerStream"/> that contains the raw stream
        /// </summary>
        public class ManagedPipeServerStream : IDisposable
        {
            private bool _disposed = false;

            public readonly NamedPipeServerStream Stream;
            public CancellationTokenSource DisconnectToken;
            public bool IsConnected = false;

            public ManagedPipeServerStream(NamedPipeServerStream stream)
            {
                Stream = stream;
                DisconnectToken = new();
            }

            public void Dispose()
            {
                if (_disposed) throw new ObjectDisposedException("ManagedPipeServerStream");
                _disposed = true;

                try
                {
                    DisconnectToken.Cancel();
                    Stream.Flush();
                    Stream.Disconnect();
                }
                catch { }
                finally
                {
                    DisconnectToken?.Dispose();
                    Stream?.Dispose();
                }

                GC.SuppressFinalize(this);
            }
            public async Task DisposeAsync()
            {
                if (_disposed) throw new ObjectDisposedException("ManagedPipeServerStream");
                _disposed = true;

                try
                {
                    DisconnectToken.Cancel();
                    await Stream.FlushAsync();
                    Stream.Disconnect();
                }
                catch { }
                finally
                {
                    DisconnectToken.Dispose();
                    await Stream.DisposeAsync();
                }

                GC.SuppressFinalize(this);
            }
        }
    }

    /// <summary>
    /// Enables UDP Discovery systems on the local network (LAN)
    /// </summary>
    public class LanBroadcaster : FlexibleCatcher
    {
        private UdpClient broadcaster;
        private volatile bool running = false;
        public readonly int Port;

        /// <summary>
        /// Fires when a remote listener responds to the broadcast
        /// </summary>
        public event Action<byte[], IPEndPoint> ListenerReplied;

        /// <summary>
        /// Fires whenever an exception is thrown in the broadcast loop. These are non fatal.
        /// </summary>
        public event Action<Exception> OnBroadcastLoopError;

        [Obsolete("API debugging event.")]
        public event Action<Exception> OnMisconfigurationError;

        /// <summary>
        /// Initialize new <see cref="LanBroadcaster"/>
        /// </summary>
        /// <param name="discoveryPort">The port to broadcast from</param>
        /// <param name="roughExceptions">If true, exceptions will be thrown inside of their failed method. Otherwise they are not immediately thrown, but can still be viewed in <see cref="LastFailureException"/> along with a <see cref="Boolean"/> failure status provided by most methods</param>
        /// <param name="start">Should the broadcaster immediately start broadcasting</param>
        public LanBroadcaster(int discoveryPort, bool roughExceptions = false, bool start = false)
        {
            Port = discoveryPort;
            RoughExceptions = roughExceptions;
#pragma warning disable CS4014
            if (start) BroadcastAsync();
#pragma warning restore CS4014
        }

        /// <summary>
        /// Starts broadcasting to the network, routing all recieved data and endpoints through <see cref="ListenerReplied"/>
        /// </summary>
        /// <returns></returns>
        public void BroadcastAsync()
        {
            if (running) { handleException(new InvalidOperationException("Cant start broadcast when one is already running from this instance.")); return; }
            broadcaster = new UdpClient(Port);
            broadcaster.Ttl = 1;
            running = true;

#pragma warning disable CS4014
            Task.Run(async () =>
            {
                while (running)
                {
                    try
                    {
                        var result = await broadcaster.ReceiveAsync();
                        ListenerReplied?.Invoke(result.Buffer, result.RemoteEndPoint);
                    }
                    catch (ObjectDisposedException) { return; }
                    catch(SocketException e)
                    {
                        if (e.ErrorCode == 995) return;
                        OnBroadcastLoopError?.Invoke(e);
                    }
                    catch(IOException e)
                    {
                        OnBroadcastLoopError?.Invoke(e);
                    }
                    catch(InvalidOperationException e)
                    {
                        OnMisconfigurationError?.Invoke(e);
                        break;
                    }
                    catch(ArgumentException e)
                    {
                        OnMisconfigurationError?.Invoke(e);
                        break;
                    }
                    catch(Exception e)
                    {
                        OnBroadcastLoopError?.Invoke(e);
                        break;
                    }
                }
            });
#pragma warning restore CS4014
        }
        /// <summary>
        /// Sends a response back to a remote listener that the broadcaster picked up. (Recommended use case) Use this inside of your <see cref="ListenerReplied"/> handles to respond to listeners that have provided valid data. Ex: listener sends a known broadcast signal so you reply back with info about a server.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="remoteClientEndpoint"></param>
        /// <returns></returns>
        public async Task<bool> RespondAsync(byte[] response, IPEndPoint remoteClientEndpoint)
        {
            if (!running || broadcaster == null) { handleException(new InvalidOperationException("Cannot send message response when broadcaster is not live.")); return false; }

            try
            {
                await broadcaster.SendAsync(response, response.Length, remoteClientEndpoint);
            }
            catch (Exception e)
            {
                handleException(e);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Sends a response back to a remote listener that the broadcaster picked up. (Recommended use case) Use this inside of your <see cref="ListenerReplied"/> handles to respond to listeners that have provided valid data. Ex: listener sends a known broadcast signal so you reply back with info about a server.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="remoteClientEndpoint"></param>
        /// <returns></returns>
        public bool Respond(byte[] response, IPEndPoint remoteClientEndpoint)
        {
            if (!running || broadcaster == null) { handleException(new InvalidOperationException("Cannot send message response when broadcaster is not live.")); return false; }

            try
            {
                broadcaster.Send(response, response.Length, remoteClientEndpoint);
            }
            catch (Exception e)
            {
                handleException(e);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Immediately stops the broadcast.
        /// </summary>
        public void Stop()
        {
            if (!running) { handleException(new InvalidOperationException("Cant stop broadcast when none is started.")); return; }
            running = false;
            broadcaster.Close();
            broadcaster = null;
        }

        /// <summary>
        /// Method for listener systems to detect broadcasts. Sends out a message signal to broadcasters and gathers all repliers that match the provided isBroadcasterPredicate condition within the timeout time. Any broadcasters that didn't reply within the timeout time will not be included in the result, making the timeout a cutoff time limit for responses.
        /// </summary>
        /// <param name="discoveryPort">Port to test for broadcasts from</param>
        /// <param name="request">Data</param>
        /// <param name="isBroadcasterPredicate">A predicate to test if a broadcasters response validates it. Return true to accept this result</param>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <param name="cancellationToken">Allows for cancelling the operation mid execution</param>
        /// <param name="onError">Custom error handling function. Routes exceptions into the func instead of throwning them in the method. Return true to consume the error in your function</param>
        /// <returns></returns>
        public static async Task<IReadOnlyCollection<LanBroadcastResult>> DiscoverBroadcastersAsync(int discoveryPort, byte[] request, Func<IPEndPoint, byte[], bool> isBroadcasterPredicate, int timeoutMs = 2000, CancellationToken cancellationToken = default, Func<Exception, bool> onError = null)
        {
            List<LanBroadcastResult> foundBroadcasters = new();

            using (UdpClient searchClient = new())
            {
                searchClient.EnableBroadcast = true;

                IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
                await searchClient.SendAsync(request, request.Length, broadcastEndpoint);

                var timeoutTokenSource = new CancellationTokenSource(timeoutMs);
                var linkedCts = cancellationToken != null ? CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, cancellationToken) : timeoutTokenSource;

                try
                {
                    int startTime = Environment.TickCount;
                    while (!linkedCts.Token.IsCancellationRequested)
                    {
                        int elapsed = Environment.TickCount - startTime;
                        int remaining = timeoutMs - elapsed;
                        if (remaining <= 0) break;

                        var recieveTask = searchClient.ReceiveAsync();
                        var delayTask = Task.Delay(remaining, linkedCts.Token);

                        var completed = await Task.WhenAny(recieveTask, delayTask);
                        if (completed == recieveTask)
                        {
                            var result = recieveTask.Result;
                            if (isBroadcasterPredicate(result.RemoteEndPoint, result.Buffer))
                            {
                                foundBroadcasters.Add(new LanBroadcastResult(result.RemoteEndPoint, result.Buffer));
                            }
                        }
                        else break;
                    }
                }
                catch (SocketException) when (timeoutTokenSource.IsCancellationRequested) { }
                catch (ObjectDisposedException) { return foundBroadcasters.AsReadOnly(); }
                catch (Exception e)
                {
                    if (onError == null || !onError(e)) throw;
                }

                timeoutTokenSource.Dispose();
                linkedCts.Dispose();
            }
            return foundBroadcasters.AsReadOnly();
        }
    }
    /// <summary>
    /// A listener broadcast check result. Contains the replied broadcasters <see cref="IPEndPoint"/> and response data.
    /// </summary>
    public class LanBroadcastResult
    {
        public readonly IPEndPoint EndPoint;
        public readonly byte[] DataRecieved;
        public LanBroadcastResult(IPEndPoint endPoint, byte[] dataRecieved)
        {
            EndPoint = endPoint;
            DataRecieved = dataRecieved;
        }
    }

    /// <summary>
    /// Data client for TCP data transfer.
    /// </summary>
    public class DataTCPClient : FlexibleCatcher, IDisposable
    {
        private bool _disposed = false;

        internal TcpClient client;
        internal NetworkStream stream;
        private volatile bool Disconnecting = false;
        private CancellationTokenSource DisconnectToken;
        public readonly string Hostname;
        public readonly int Port;

        private volatile ConnectionStatus _connectionstatus = ConnectionStatus.NotConnected;
        public ConnectionStatus ConnectionStatus
        {
            get { return _connectionstatus; }
            internal set
            {
                _connectionstatus = value;
                ConnectionStateChanged?.Invoke(value);
            }
        }

        public event Action<ConnectionStatus> ConnectionStateChanged;
        public event Action ClientConnected;
        public event Action ClientDisconnected;
        public event Action<byte[]> DataReceived;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="roughExceptions">If true, exceptions will be thrown inside of their failed method. Otherwise they are not immediately thrown, but can still be viewed in <see cref="LastFailureException"/> along with a <see cref="Boolean"/> failure status provided by most methods</param>
        public DataTCPClient(string hostname, int port, bool roughExceptions = true)
        {
            RoughExceptions = roughExceptions;
            Hostname = hostname;
            Port = port;
        }

        public async Task<bool> ConnectAsync(int timeout = 5000, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException("DataTCPClient");
            if (Disconnecting) { handleException(new InvalidOperationException("Cannot attempt to make a connection while a disconnect operation is running.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Attempting) { handleException(new InvalidOperationException("Cannot attempt to connect to server while already trying.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Connected) { handleException(new AlreadyConnectedException($"Cannot attempt to connect while already connected.")); return false; }
            if (client == null) { create(); }

            try
            {
                ConnectionStatus = ConnectionStatus.Attempting;
                var connectionTask = client.ConnectAsync(Hostname, Port);
                if (await Task.WhenAny(connectionTask, Task.Delay(timeout, cancellationToken)) != connectionTask)
                {
                    if (cancellationToken.IsCancellationRequested) { handleException(new OperationCanceledException("Task was cancelled before it could complete.")); return false; }
                    else { handleException(new TimeoutException("Connection timed out.")); return false; }
                }
                stream = client.GetStream();
                ConnectionStatus = ConnectionStatus.Connected;
                ClientConnected?.Invoke();
#pragma warning disable CS4014, CS1998
                Task.Run(async () => beginListening());
#pragma warning restore CS4014, CS1998
            }
            catch (Exception e)
            {
                ConnectionStatus = ConnectionStatus.NotConnected;
                handleException(e);
                return false;
            }
            return true;
        }
        public bool Connect()
        {
            if (_disposed) throw new ObjectDisposedException("DataTCPClient");
            if (Disconnecting) { handleException(new InvalidOperationException("Cannot attempt to make a connection while a disconnect operation is running.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Attempting) { handleException(new InvalidOperationException("Cannot attempt to connect to server while already trying.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Connected) { handleException(new AlreadyConnectedException($"Cannot attempt to connect while already connected.")); return false; }
            if (client == null) { create(); }

            try
            {
                ConnectionStatus = ConnectionStatus.Attempting;
                client.Connect(Hostname, Port);
                stream = client.GetStream();
                ConnectionStatus = ConnectionStatus.Connected;
                ClientConnected?.Invoke();
#pragma warning disable CS4014, CS1998
                Task.Run(async () => beginListening());
#pragma warning restore CS4014, CS1998
            }
            catch (Exception e)
            {
                ConnectionStatus = ConnectionStatus.NotConnected;
                handleException(e);
                return false;
            }
            return true;
        }
        public async Task<bool> DisconnectAsync()
        {
            if (_disposed) throw new ObjectDisposedException("DataTCPClient");
            if (Disconnecting) { handleException(new InvalidOperationException("Cannot attempt to disconnect while another disconnect operation is already running.")); return false; }
            if (ConnectionStatus == ConnectionStatus.NotConnected) { handleException(new InvalidOperationException("Cannot attempt to disconnect when not connected to begin with.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Attempting) { handleException(new InvalidOperationException("Cannot attempt to disconnect at the same time as trying to connect to a server. To cancel connections, instead use CancellationTokens.")); return false; }

            Disconnecting = true;
            try
            {
                DisconnectToken?.Cancel();
                await stream.FlushAsync();
                client.Close();
            }
            catch { }
            finally
            {
                DisconnectToken?.Dispose();
                client?.Dispose();
                DisconnectToken = null;
                client = null;
                stream = null;
                ConnectionStatus = ConnectionStatus.NotConnected;
                ClientDisconnected?.Invoke();
                Disconnecting = false;
            }
            return true;
        }
        public bool Disconnect()
        {
            if (_disposed) throw new ObjectDisposedException("DataTCPClient");
            if (Disconnecting) { handleException(new InvalidOperationException("Cannot attempt to disconnect while another disconnect operation is already running.")); return false; }
            if (ConnectionStatus == ConnectionStatus.NotConnected) { handleException(new InvalidOperationException("Cannot attempt to disconnect when not connected to begin with.")); return false; }
            if (ConnectionStatus == ConnectionStatus.Attempting) { handleException(new InvalidOperationException("Cannot attempt to disconnect at the same time as trying to connect to a server. To cancel connections, instead use CancellationTokens.")); return false; }

            Disconnecting = true;
            try
            {
                DisconnectToken?.Cancel();
                stream.Flush();
                client.Close();
            }
            catch { }
            finally
            {
                DisconnectToken?.Dispose();
                client?.Dispose();
                DisconnectToken = null;
                client = null;
                stream = null;
                ConnectionStatus = ConnectionStatus.NotConnected;
                ClientDisconnected?.Invoke();
                Disconnecting = false;
            }
            return true;
        }

        /// <summary>
        /// Write in raw byte data
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> WriteDataAsync(byte[] message)
        {
            return await write(message);
        }
        /// <summary>
        /// Automatically convert a string into byte data then write
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> WriteDataAsync(string message)
        {
            return await write(Encoding.UTF8.GetBytes(message));
        }

        public void Dispose()
        {
            if (_disposed) throw new ObjectDisposedException("DataTCPClient");
            _disposed = true;

            try
            {
                DisconnectToken?.Cancel();
                stream?.Flush();
                client?.Close();
            }
            catch { }
            finally
            {
                DisconnectToken?.Dispose();
                client.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        private async Task beginListening()
        {
            while (true)
            {
                if (stream == null || DisconnectToken == null || client == null) break;
                if (DisconnectToken.IsCancellationRequested) break;

                try
                {
                    byte[] message = await DataStreamHelper.ReadStreamAsync(stream, DisconnectToken.Token);
                    DataReceived?.Invoke(message);
                }
                catch (ObjectDisposedException)
                {
                    await DisconnectAsync();
                    break;
                }
                catch (OperationCanceledException) { break; }
                catch (TimeoutException) { }
                catch (Exception e)
                {
                    await DisconnectAsync();
                    handleException(e);
                    break;
                }
            }
        }
        private void create()
        {
            client = new();
            DisconnectToken = new();
        }

        private async Task<bool> write(byte[] message)
        {
            if (_disposed) throw new ObjectDisposedException($"DataTCPClient");
            if (stream == null || this.ConnectionStatus == ConnectionStatus.NotConnected || this.ConnectionStatus == ConnectionStatus.Attempting || Disconnecting) return false;

            try
            {
                await DataStreamHelper.WriteStreamAsync(stream, message, DisconnectToken.Token);
            }
            catch (Exception e)
            {
                // Disconnect on failure
                await DisconnectAsync();
                handleException(e);
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Data server for TCP.
    /// </summary>
    public class DataTCPServer : FlexibleCatcher, IDisposable
    {
        private bool _disposed = false;

        internal TcpListener server;
        private volatile bool running = false;
        private CancellationTokenSource StopToken;
        private ConcurrentDictionary<ManagedClient, string> clientConnections = new();
        public readonly int Port;

        public event Action<ManagedClient, byte[]> ReceivedData;
        public event Action<ManagedClient> ClientConnected;
        public event Action<ManagedClient> ClientDisconnected;
        /// <summary>
        /// Called when the <see cref="RunServer()"/> async loop throws an exception.
        /// </summary>
        public event Action<Exception> OnServerLoopError;

        /// <param name="port"></param>
        /// <param name="roughExceptions">If true, exceptions will be thrown inside of their failed method. Otherwise they are not immediately thrown, but can still be viewed in <see cref="LastFailureException"/> along with a <see cref="Boolean"/> failure status provided by most methods</param>
        public DataTCPServer(int port, bool roughExceptions = true) { Port = port; RoughExceptions = roughExceptions; }

        public IReadOnlyDictionary<ManagedClient, string> GetConnectedClients() => clientConnections;

        /// <summary>
        /// Open server and allow for connections.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void RunServer()
        {
            if (_disposed) throw new ObjectDisposedException($"DataTCPServer");
            if (running) { handleException(new InvalidOperationException("Cannot start server when its already started.")); }
            running = true;

            server = new(IPAddress.Any, Port);
            StopToken = new CancellationTokenSource();
            server.Start();

#pragma warning disable CS4014
            Task.Run(async () =>
            {
                while (!StopToken.IsCancellationRequested)
                {
                    try
                    {
                        TcpClient newTCPClient = await server.AcceptTcpClientAsync();

                        CancellationTokenSource clientToken = new();
                        CancellationTokenSource linkedToken = CancellationTokenSource.CreateLinkedTokenSource(clientToken.Token, StopToken.Token);

                        ManagedClient newManagedClient = new(newTCPClient, clientToken);
                        clientConnections.TryAdd(newManagedClient, GenerateUniqueGuid());
                        ClientConnected?.Invoke(newManagedClient);
                        Task.Run(() => HandleClient(newManagedClient, linkedToken.Token));
                    }
                    catch(ObjectDisposedException)
                    {
                        await StopAsync();
                        break;
                    }
                    catch (OperationCanceledException) { break; }
                    catch (SocketException) { }
                    catch (IOException) { }
                    catch (Exception e)
                    {
                        OnServerLoopError?.Invoke(e);
                        await StopAsync();
                        handleException(e);
                        break;
                    }
                }
            });
#pragma warning restore CS4014
        }

        /// <summary>
        /// Stop server, disconnecting all clients.
        /// </summary>
        /// <returns><see cref="Boolean"/> success</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<bool> StopAsync()
        {
            if (_disposed) throw new ObjectDisposedException($"DataTCPServer");
            if (!running) { handleException(new InvalidOperationException("Cannot stop server when it was never started.")); return false; }
            running = false;

            try
            {
                StopToken?.Cancel();
                server?.Stop();

                foreach (var connection in clientConnections.Keys)
                {
                    await connection.CloseAsync();
                    ClientDisconnected?.Invoke(connection);
                }
            }
            catch (Exception e)
            {
                handleException(e);
                return false;
            }
            finally
            {
                StopToken = null;
                server = null;
                foreach(var client in clientConnections.Keys) ClientDisconnected?.Invoke(client);
                clientConnections.Clear();
            }
            return true;
        }

        /// <summary>
        /// Stop server, disconnecting all clients.
        /// </summary>
        /// <returns><see cref="Boolean"/> success</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public bool Stop()
        {
            if (_disposed) throw new ObjectDisposedException($"DataTCPServer");
            if (!running) { handleException(new InvalidOperationException("Cannot stop server when it was never started.")); return false; }
            running = false;

            try
            {
                StopToken?.Cancel();
                server?.Stop();

                foreach (var connection in clientConnections.Keys)
                {
                    connection.Close();
                    ClientDisconnected?.Invoke(connection);
                }
            }
            catch (Exception e)
            {
                handleException(e);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if client exists by Guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public bool TryGetClient(string guid, out ManagedClient client)
        {
            if (_disposed) throw new ObjectDisposedException($"DataTCPServer");

            client = clientConnections.FirstOrDefault(kvp => kvp.Value == guid).Key;
            return client != null;
        }

        /// <summary>
        /// Write to client in raw byte data
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WriteToClientAsync(ManagedClient client, byte[] data)
        {
            return await write(client, data);
        }

        /// <summary>
        /// Automatically convert a string into byte data then write to client
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> WriteToClientAsync(ManagedClient client, string message)
        {
            return await write(client, Encoding.UTF8.GetBytes(message));
        }

        public void DisconnectClient(ManagedClient client)
        {
            if (_disposed) throw new ObjectDisposedException($"DataTCPServer");

            try
            {
                notifyDisconnect(client);
                client.Close();
            }
            catch { }
        }
        public async Task DisconnectClientAsync(ManagedClient client)
        {
            if (_disposed) throw new ObjectDisposedException($"DataTCPServer");

            try
            {
                notifyDisconnect(client);
                await client.CloseAsync();
            }
            catch { }
        }

        public void Dispose()
        {
            if (_disposed) throw new ObjectDisposedException($"DataTCPServer");
            _disposed = true;

            try
            {
                if (running) Stop();
                else
                {
                    server?.Stop();
                }
            }
            catch { }
            finally
            {
                server = null;
                StopToken?.Dispose();
                StopToken = null;
                foreach (var client in clientConnections.Keys) ClientDisconnected?.Invoke(client);
                clientConnections.Clear();
            }
            GC.SuppressFinalize(this);
        }

        private async Task HandleClient(ManagedClient client, CancellationToken cancellationToken)
        {
            try
            {
                NetworkStream stream = client.Client.GetStream();
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        byte[] data = await DataStreamHelper.ReadStreamAsync(stream, cancellationToken);
                        ReceivedData?.Invoke(client, data);
                    }
                    catch (ObjectDisposedException) { break; }
                    catch (OperationCanceledException) { break; }
                    catch (TimeoutException) { }
                    catch(Exception e)
                    {
                        handleException(e, "HandleClient");
                        break;
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                notifyDisconnect(client);
                client.Close();
            }
        }

        private string GenerateUniqueGuid()
        {
            string guid = string.Empty;
            while (true)
            {
                guid = Guid.NewGuid().ToString();
                if (!clientConnections.Values.Contains(guid)) break;
            }
            return guid;
        }

        private async Task<bool> write(ManagedClient client, byte[] message)
        {
            if (_disposed) throw new ObjectDisposedException($"DataTCPServer");

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(StopToken?.Token ?? default, client.ClientHandleToken.Token);

            try
            {
                await DataStreamHelper.WriteStreamAsync(client.Client.GetStream(), message, linked.Token);
            }
            catch (Exception e)
            {
                // Disconnect on failure
                await DisconnectClientAsync(client);
                handleException(e);
                return false;
            }

            return true;
        }

        private void notifyDisconnect(ManagedClient client)
        {
            if(clientConnections.TryRemove(client, out _))
                ClientDisconnected?.Invoke(client);
        }

        /// <summary>
        /// Managed TCPClient data
        /// </summary>
        public sealed class ManagedClient
        {
            private bool closed = false;
            public readonly TcpClient Client;
            public readonly CancellationTokenSource ClientHandleToken;

            public ManagedClient(TcpClient client, CancellationTokenSource clientHandleToken)
            {
                Client = client;
                ClientHandleToken = clientHandleToken;
            }

            public void Close()
            {
                if (closed) throw new InvalidOperationException("Already closed.");
                closed = true;

                try
                {
                    ClientHandleToken.Cancel();
                    NetworkStream stream = Client.GetStream();
                    stream.Flush();
                    Client.Close();
                    stream.Dispose();
                    ClientHandleToken.Dispose();
                }
                catch { }
            }
            public async Task CloseAsync()
            {
                if (closed) throw new InvalidOperationException("Already closed.");
                closed = true;

                try
                {
                    ClientHandleToken.Cancel();
                    NetworkStream stream = Client.GetStream();
                    await stream.FlushAsync();
                    Client.Close();
                    stream.Dispose();
                    ClientHandleToken.Dispose();
                }
                catch { }
            }

            public override int GetHashCode() => Client?.GetHashCode() ?? 0;
            public override bool Equals(object obj) => obj is ManagedClient other && Client == other.Client;
        }
    }

    /// <summary>
    /// Data client for UDP data transfer.
    /// </summary>
    public class DataUDPClient : FlexibleCatcher, IDisposable
    {
        private bool _disposed = false;

        internal UdpClient client;
        public readonly string Hostname;
        public readonly int Port;
        private volatile bool listening = false;
        private CancellationTokenSource disconnectToken = new();

        public event Action<IPEndPoint, byte[]> DataReceived;
        public event Action<Exception> OnReceiveLoopErrored;

        /// <summary>
        /// Initialization of this object will immediately being accepting inbound data. Disconnects are undetectable by default, you must implement a heartbeat or some other sort of monitoring system.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port">The remote hostnames port</param>
        /// <param name="listenPort">The port to listen for data on</param>
        /// <param name="startListening">Should the client immediately start listening for data</param>
        /// <param name="roughExceptions">If true, exceptions will be thrown inside of their failed method. Otherwise they are not immediately thrown, but can still be viewed in <see cref="LastFailureException"/> along with a <see cref="Boolean"/> failure status provided by most methods</param>
        /// <param name="maxRouterHops">The maximum amount of jumps between routers this packet is allowed to make before being dropped. Ex: 1 would restrict it to LAN, while 30 would allow it to go 30 routers across the internet</param>
        /// <param name="dataLoopback">Does data sent to multicast groups also get sent back to yourself</param>
        /// <param name="dontFragmentPackets">If true packets are prevented from being fragmented if too large. This will cause them to be dropped instead.</param>
        /// <param name="lockBoundAddress">When true, only one DataUDPClient can listen on the current address/port. Otherwise multiple can listen allowing for local multicast</param>
        public DataUDPClient(string hostname, int port, int listenPort, bool startListening = false, bool roughExceptions = true, short maxRouterHops = 255, bool allowNATTraversal = true, bool dataLoopback = false, bool dontFragmentPackets = false, bool lockBoundAddress = true)
        {
            RoughExceptions = roughExceptions;
            Hostname = hostname;
            Port = port;
            client = new UdpClient(listenPort);
            client.EnableBroadcast = false; // use LANBroadcaster for this

            client.Ttl = maxRouterHops;
            client.AllowNatTraversal(allowNATTraversal);
            client.MulticastLoopback = dataLoopback;
            client.DontFragment = dontFragmentPackets;
            client.ExclusiveAddressUse = lockBoundAddress;

#pragma warning disable CS1998
            if (startListening)
            {
                listening = true;
                Task.Run(async () => beginListening());
            }
#pragma warning restore CS1998
        }

        public void JoinMulticast(int ifIndex, IPAddress multicastAddr) => client.JoinMulticastGroup(ifIndex, multicastAddr);
        public void JoinMulticast(IPAddress multicastAddr) => client.JoinMulticastGroup(multicastAddr);
        public void JoinMulticast(IPAddress multicastAddr, int timeToLive) => client.JoinMulticastGroup(multicastAddr, timeToLive);
        public void JoinMulticast(IPAddress multicastAddr, IPAddress localAddress) => client.JoinMulticastGroup(multicastAddr, localAddress);

        public void DropMulticast(IPAddress multicastAddr) => client.DropMulticastGroup(multicastAddr);
        public void DropMulticast(IPAddress multicastAddr, int ifIndex) => client.DropMulticastGroup(multicastAddr, ifIndex);

        /// <summary>
        /// Starts the client's data listening loop.
        /// </summary>
        public void StartListening()
        {
            if (listening) throw new InvalidOperationException("Already listening.");
            listening = true;
#pragma warning disable CS1998
            Task.Run(async () => beginListening());
#pragma warning restore CS1998
        }

        public void StopListening()
        {
            if (!listening) throw new InvalidOperationException("Can't stop listening when the client never was in the first place.");
            listening = false;
            disconnectToken.Cancel();
            client.Close();
        }

        /// <summary>
        /// Write in raw byte data
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Success = did all bytes send, Sent = the amount that actually sent</returns>
        public async Task<(bool success, int sent)> SendDataAsync(byte[] message)
        {
            int result = await writeAsync(message);
            return (result == message.Length, result);
        }

        /// <summary>
        /// Automatically convert a string into byte data then write
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Success = did all bytes send, Sent = the amount that actually sent</returns>
        public async Task<(bool success, int sent)> SendDataAsync(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            int result = await writeAsync(data);
            return (result == message.Length, result);
        }

        /// <summary>
        /// Write in raw byte data
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Success = did all bytes send, Sent = the amount that actually sent</returns>
        public (bool success, int sent) SendData(byte[] message)
        {
            int result = write(message);
            return (result == message.Length, result);
        }

        /// <summary>
        /// Automatically convert a string into byte data then write
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Success = did all bytes send, Sent = the amount that actually sent</returns>
        public (bool success, int sent) SendData(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            int result = write(data);
            return (result == message.Length, result);
        }

        public void Dispose()
        {
            if (_disposed) throw new ObjectDisposedException("DataUDPClient");
            _disposed = true;

            try
            {
                disconnectToken.Cancel();
                disconnectToken.Dispose();
                client.Close();
                client.Dispose();
            }
            catch { }
            finally
            {
                disconnectToken = null;
                client = null;
            }
            GC.SuppressFinalize(this);
        }

        private async Task beginListening()
        {
            while (!disconnectToken.IsCancellationRequested)
            {
                try
                {
                    UdpReceiveResult incoming = await client.ReceiveAsync();
                    DataReceived?.Invoke(incoming.RemoteEndPoint, incoming.Buffer);
                }
                catch (SocketException e) { OnReceiveLoopErrored?.Invoke(e); }
                catch (IOException e) { OnReceiveLoopErrored?.Invoke(e); }
                catch (OperationCanceledException) { break; }
                catch (Exception e)
                {
                    OnReceiveLoopErrored?.Invoke(e);
                    Dispose();
                    handleException(e, "beginListening");
                    break;
                }
            }
        }

        private async Task<int> writeAsync(byte[] data)
        {
            if (_disposed) throw new ObjectDisposedException("DataUDPClient");
            try
            {
                int result = await client.SendAsync(data, data.Length, Hostname, Port);
                return result;
            }
            catch(Exception e)
            {
                Dispose();
                handleException(e);
                return 0;
            }
        }

        private int write(byte[] data)
        {
            if (_disposed) throw new ObjectDisposedException("DataUDPClient");
            try
            {
                int result = client.Send(data, data.Length, Hostname, Port);
                return result;
            }
            catch (Exception e)
            {
                Dispose();
                handleException(e);
                return 0;
            }
        }
    }

    /// <summary>
    /// Data server for UDP.
    /// </summary>
    public class DataUDPServer : FlexibleCatcher, IDisposable
    {
        private bool _disposed = false;

        internal UdpClient server;
        private volatile bool listening = false;
        private CancellationTokenSource StopToken = new CancellationTokenSource();
        private ConcurrentHashList<IPEndPoint> connectedAddresses = new();
        private Predicate<IPEndPoint> AllowConnectCondition;
        public readonly int Port;
        public readonly bool AcceptAllPackets;

        public event Action<IPEndPoint, byte[]> DataReceived;
        public event Action<IPEndPoint> ClientConnected;
        public event Action<IPEndPoint> ClientDisconnected;
        /// <summary>
        /// Called when the <see cref="RunServer()"/> async loop throws an exception.
        /// </summary>
        public event Action<Exception> OnReceiveLoopErrored;

        /// <param name="port">Port to listen to data on</param>
        /// <param name="roughExceptions">If true, exceptions will be thrown inside of their failed method. Otherwise they are not immediately thrown, but can still be viewed in <see cref="LastFailureException"/> along with a <see cref="Boolean"/> failure status provided by most methods</param>
        public DataUDPServer(int port, Predicate<IPEndPoint> allowConnectCondition, bool startServer = false, bool roughExceptions = true)
        {
            Port = port;
            AllowConnectCondition = allowConnectCondition;
            RoughExceptions = roughExceptions;

            server = new UdpClient(port);
            server.EnableBroadcast = false;
            server.Ttl = 255;
            server.AllowNatTraversal(true);
            server.MulticastLoopback = false;
            server.DontFragment = false;
            server.ExclusiveAddressUse = false;

            if (startServer) RunServer();
        }

        public IReadOnlyList<IPEndPoint> GetConnectedClients() => (IReadOnlyList<IPEndPoint>)connectedAddresses;

        public void RunServer()
        {
            if (_disposed) throw new ObjectDisposedException("DataUDPServer");
            if (listening) throw new InvalidOperationException("Already listening.");
            listening = true;
#pragma warning disable CS1998
            Task.Run(async () => beginListening());
#pragma warning restore CS1998
        }

        public void StopServer()
        {
            StopToken.Cancel();
            Dispose();
        }

        /// <summary>
        /// Write to client in raw byte data
        /// </summary>
        /// <returns></returns>
        public async Task<(bool success, int sent)> WriteToClientAsync(IPEndPoint client, byte[] data)
        {
            int result = await writeAsync(client, data);
            return (result == data.Length, result);
        }

        /// <summary>
        /// Automatically convert a string into byte data then write to client
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<(bool success, int sent)> WriteToClientAsync(IPEndPoint client, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            int result = await writeAsync(client, data);
            return (result == data.Length, result);
        }

        /// <summary>
        /// Write to client in raw byte data
        /// </summary>
        /// <returns></returns>
        public (bool success, int sent) WriteToClient(IPEndPoint client, byte[] data)
        {
            int result = write(client, data);
            return (result == data.Length, result);
        }

        /// <summary>
        /// Automatically convert a string into byte data then write to client
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public (bool success, int sent) WriteToClient(IPEndPoint client, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            int result = write(client, data);
            return (result == data.Length, result);
        }

        /// <summary>
        /// Removes a client endpoint from the UDP trust pool (<see cref="connectedAddresses"/>).
        /// </summary>
        /// <param name="client"></param>
        /// <returns><see cref="Boolean"/> did address exist</returns>
        public bool DisconnectClient(IPEndPoint client)
        {
            if (_disposed) throw new ObjectDisposedException("DataUDPServer");
            bool success = connectedAddresses.Remove(client);
            if (success) ClientDisconnected?.Invoke(client);
            return success;
        }

        public void Dispose()
        {
            if (_disposed) throw new ObjectDisposedException($"DataUDPServer");
            _disposed = true;

            try
            {
                connectedAddresses.Clear();
                foreach(var client in connectedAddresses) ClientDisconnected?.Invoke(client);
                StopToken.Cancel();
                StopToken.Dispose();
                server.Close();
                server.Dispose();
            }
            catch { }
            finally
            {
                StopToken = null;
                server = null;
            }
            GC.SuppressFinalize(this);
        }

        private async Task beginListening()
        {
            while (!StopToken.IsCancellationRequested)
            {
                try
                {
                    UdpReceiveResult incoming = await server.ReceiveAsync();
                    IPEndPoint client = incoming.RemoteEndPoint;
                    if (!connectedAddresses.Contains(client))
                    {
                        if (AllowConnectCondition.Invoke(client))
                        {
                            connectedAddresses.Add(client);
                            ClientConnected?.Invoke(client);
                            DataReceived?.Invoke(client, incoming.Buffer);
                        }
                    }
                    else DataReceived?.Invoke(client, incoming.Buffer);
                }
                catch(SocketException e) { OnReceiveLoopErrored?.Invoke(e); }
                catch(IOException e) { OnReceiveLoopErrored?.Invoke(e); }
                catch (OperationCanceledException) { break; }
                catch (Exception e)
                {
                    OnReceiveLoopErrored?.Invoke(e);
                    Dispose();
                    handleException(e, "beginListening");
                    break;
                }
            }
        }

        private int write(IPEndPoint client, byte[] message)
        {
            if (_disposed) throw new ObjectDisposedException("DataUDPServer");
            try
            {
                int result = server.Send(message, message.Length, client);
                return result;
            }
            catch(Exception e)
            {
                Dispose();
                handleException(e);
                return 0;
            }
        }

        private async Task<int> writeAsync(IPEndPoint client, byte[] message)
        {
            if (_disposed) throw new ObjectDisposedException("DataUDPServer");
            try
            {
                int result = await server.SendAsync(message, message.Length, client);
                return result;
            }
            catch (Exception e)
            {
                Dispose();
                handleException(e);
                return 0;
            }
        }
    }

    /// <summary>
    /// Tool class for opening external ports on the router, allowing for data to be sent across the internet.
    /// </summary>
    public static class UPnPManager
    {
        private static object discovererInitLock = new object();
        private static NatDiscoverer discoverer;
        private static NatDevice UPnPdevice;
        private static NatDevice PMPdevice;
        private static ConcurrentDictionary<int, ManagedMapping> Ports = new(); // external port, mapping

        public static IReadOnlyList<int> ForwardedPorts => Ports.Keys.ToList();

        /// <summary>
        /// Open a public port on default configuration.
        /// </summary>
        /// <param name="privatePort"></param>
        /// <param name="publicPort"></param>
        /// <param name="description"></param>
        /// <param name="ErrorHandle"></param>
        /// <returns><see cref="Boolean"/> success status</returns>
        public static async Task<bool> OpenPortAsync(int privatePort, int publicPort, string description = "None provided", Action<Exception> ErrorHandle = null) => await openport(privatePort, publicPort, PortMapper.Upnp, Protocol.Tcp, 5000, default, description);
        /// <summary>
        /// Open a public port.
        /// </summary>
        /// <param name="privatePort"></param>
        /// <param name="publicPort"></param>
        /// <param name="routerMappingType"></param>
        /// <param name="protocol"></param>
        /// <param name="routerTimeout"></param>
        /// <param name="description"></param>
        /// <param name="ErrorHandle"></param>
        /// <returns><see cref="Boolean"/> success status</returns>
        public static async Task<bool> OpenPortAsync(int privatePort, int publicPort, PortMapper routerMappingType, Protocol protocol, int routerTimeout, string description = "None provided", Action<Exception> ErrorHandle = null) => await openport(privatePort, publicPort, routerMappingType, protocol, routerTimeout, default, description);
        /// <summary>
        /// Open a public port.
        /// </summary>
        /// <param name="privatePort"></param>
        /// <param name="publicPort"></param>
        /// <param name="routerMappingType"></param>
        /// <param name="protocol"></param>
        /// <param name="routerTimeout"></param>
        /// <param name="description"></param>
        /// <param name="ErrorHandle"></param>
        /// <returns><see cref="Boolean"/> success status</returns>
        public static async Task<bool> OpenPortAsync(int privatePort, int publicPort, PortMapper routerMappingType, Protocol protocol, int routerTimeout, CancellationTokenSource cancellationTokenSource, string description = "None provided", Action<Exception> ErrorHandle = null) => await openport(privatePort, publicPort, routerMappingType, protocol, routerTimeout, cancellationTokenSource, description, ErrorHandle);

        /// <summary>
        /// Close an open public port.
        /// </summary>
        /// <param name="publicPort"></param>
        /// <returns><see cref="Boolean"/> success status</returns>
        public static async Task<bool> ClosePortAsync(int publicPort)
        {
            if (Ports.TryRemove(publicPort, out var managedMapping))
            {
                NatDevice _device;
                if (managedMapping.RouterMappingType == PortMapper.Upnp)
                {
                    if (UPnPdevice == null) return false;
                    else _device = UPnPdevice;
                }
                else
                {
                    if (PMPdevice == null) return false;
                    else _device = PMPdevice;
                }

                try
                {
                    await _device.DeletePortMapAsync(managedMapping.Mapping);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            else return false;
        }
        /// <summary>
        /// Close all open public ports.
        /// </summary>
        /// <returns></returns>
        public static async Task CloseAllPortsAsync()
        {
            foreach (int port in ForwardedPorts.ToList())
            {
                await ClosePortAsync(port);
            }
        }

        private static async Task<bool> openport(int privatePort, int publicPort, PortMapper routerMappingType, Protocol protocol, int routerTimeout = 5000, CancellationTokenSource cancellationToken = default, string description = "None provided", Action<Exception> ErrorHandle = null)
        {
            lock (discovererInitLock) { if (discoverer == null) discoverer = new NatDiscoverer(); }
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken?.Token ?? CancellationToken.None, new CancellationTokenSource(routerTimeout).Token);

            try
            {
                NatDevice _device;
                if (routerMappingType == PortMapper.Upnp)
                {
                    if (UPnPdevice == null) UPnPdevice = await discoverer.DiscoverDeviceAsync(routerMappingType, linkedTokenSource);
                    _device = UPnPdevice;
                }
                else
                {
                    if (PMPdevice == null) PMPdevice = await discoverer.DiscoverDeviceAsync(routerMappingType, linkedTokenSource);
                    _device = PMPdevice;
                }

                if (Ports.TryGetValue(publicPort, out var _)) return false;
                var mapping = new Mapping(protocol, privatePort, publicPort, 0, description);
                await _device.CreatePortMapAsync(mapping);

                var managed = new ManagedMapping(mapping, routerMappingType);
                Ports.TryAdd(publicPort, managed);
                return true;
            }
            catch (Exception e)
            {
                ErrorHandle?.Invoke(e);
                return false;
            }
        }
        private class ManagedMapping
        {
            public readonly Mapping Mapping;
            public readonly PortMapper RouterMappingType;
            public ManagedMapping(Mapping mapping, PortMapper routerMappingType)
            {
                Mapping = mapping;
                RouterMappingType = routerMappingType;
            }
        }
    }

    /// <summary>
    /// Tool class for checking information about an IP endpoint.
    /// </summary>
    public static class IPResolver
    {
        private static IPAddress cached_PublicIPv4 = null;

        /// <summary>
        /// Checks if the public endpoint provided is remote or from inside the host machine. If the endpoint is on the LAN it will still be considered local machine, since machines share a public IP.
        /// </summary>
        /// <param name="endpoint">Standalone IP or IP:Port</param>
        /// <param name="timeout"></param>
        /// <param name="serviceProvider">The service provider to use for the <see cref="GetIPv4Public(int, PublicIPv4Provider)"/> call made when comparing the endpoint address.</param>
        /// <returns><see cref="EndpointCheckResult"/></returns>
        public static async Task<EndpointCheckResult> ResolvePublic(string endpoint, int timeout, PublicIPv4Provider serviceProvider = PublicIPv4Provider.ipify)
        {
            return await resolvePublic(endpoint, new CancellationTokenSource(timeout).Token, serviceProvider);
        }

        /// <summary>
        /// Checks if the public endpoint provided is remote or from inside the host machine. If the endpoint is on the LAN it will still be considered local machine, since machines share a public IP.
        /// </summary>
        /// <param name="endpoint">Standalone IP or IP:Port</param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="serviceProvider">The service provider to use for the <see cref="GetIPv4Public(int, PublicIPv4Provider)"/> call made when comparing the endpoint address.</param>
        /// <returns><see cref="EndpointCheckResult"/></returns>
        public static async Task<EndpointCheckResult> ResolvePublic(string endpoint, CancellationToken cancellationToken, int timeout = 1000, PublicIPv4Provider serviceProvider = PublicIPv4Provider.ipify)
        {
            return await resolvePublic(endpoint, CancellationTokenSource.CreateLinkedTokenSource(new CancellationTokenSource(timeout).Token, cancellationToken).Token, serviceProvider);
        }

        /// <summary>
        /// Checks if the public endpoint provided is remote or from inside the host machine. If the endpoint is on the LAN it will still be considered local machine, since machines share a public IP.
        /// </summary>
        /// <param name="endpoint">Standalone IP or IP:Port</param>
        /// <param name="cancellationToken"></param>
        /// <param name="serviceProvider">The service provider to use for the <see cref="GetIPv4Public(int, PublicIPv4Provider)"/> call made when comparing the endpoint address.</param>
        /// <returns><see cref="EndpointCheckResult"/></returns>
        public static async Task<EndpointCheckResult> ResolvePublic(string endpoint, CancellationToken cancellationToken = default, PublicIPv4Provider serviceProvider = PublicIPv4Provider.ipify)
        {
            return await resolvePublic(endpoint, cancellationToken, serviceProvider);
        }

        private static async Task<EndpointCheckResult> resolvePublic(string endpoint, CancellationToken cancellationToken, PublicIPv4Provider serviceProvider)
        {
            var split = endpoint.Split(':');
            string host = split[0];
            if (!IPAddress.TryParse(host, out _)) return new()
            {
                Exception = new ArgumentException("Endpoint must be a valid IP address. Both standalone or IP:Port are supported."),
                Status = EndpointStatus.Unreachable
            };

            var localIps = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                    n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                        .Select(a => a.Address)
                        .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                        .ToArray();

            try
            {
                var hostIPs = Dns.GetHostAddresses(host)
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                    .ToArray();

                if (hostIPs.Length == 0)
                    return new EndpointCheckResult { Status = EndpointStatus.Unreachable };

                IPAddress targetIP = hostIPs.ElementAt(0); // was failing to trace before? Why?

                // Is local
                if (IPAddress.IsLoopback(targetIP) || localIps.Any(ip => ip.Equals(targetIP)))
                    return new EndpointCheckResult { Status = EndpointStatus.Local, ReachableAddress = IPAddress.Parse("127.0.0.1") };

                // Machine public check
                var publicIP = (await GetIPv4Public(cancellationToken, serviceProvider)).Address;
                if (publicIP.Equals(targetIP))
                    return new EndpointCheckResult { Status = EndpointStatus.Local, ReachableAddress = IPAddress.Parse("127.0.0.1") };

                return new EndpointCheckResult { Status = EndpointStatus.Remote, ReachableAddress = targetIP };
            }
            catch (Exception ex)
            {
                return new EndpointCheckResult { Status = EndpointStatus.Unreachable, Exception = ex };
            }
        }


        /// <summary>
        /// Gets the best connectable IPv4 of the current machine that other machines on the LAN can use to connect.
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetIPv4LAN()
        {
            var allInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni =>
                    ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                    ni.GetIPProperties().UnicastAddresses.Any(u =>
                        u.Address.AddressFamily == AddressFamily.InterNetwork)).ToList();

            var prioritized = allInterfaces
                .OrderByDescending(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .ThenByDescending(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                .ThenByDescending(ni => ni.GetIPProperties().GatewayAddresses.Any())
                .ThenByDescending(ni => ni.Speed)
                .ToList();

            foreach (var ni in prioritized)
            {
                var addr = ni.GetIPProperties().UnicastAddresses
                    .Select(u => u.Address)
                    .FirstOrDefault(ip =>
                        ip.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(ip) &&
                        !ip.ToString().StartsWith("169.254")); // exclude APIPA

                if (addr != null)
                    return addr;
            }

            var fallback = NetworkInterface.GetAllNetworkInterfaces()
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Select(u => u.Address)
                .FirstOrDefault(ip =>
                    ip.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(ip) &&
                    !ip.ToString().StartsWith("169.254"));

            return fallback;
        }

        /// <summary>
        /// Gets your public IPv4 using an external service, selected by the <see cref="PublicIPv4Provider"/> argument. Rate limits do not matter, since results are cached. Make sure to check any anamolies documented on service provider types, as these may differentiate your results.
        /// </summary>
        /// <returns></returns>
        public static async Task<(IPAddress Address, Exception Error)> GetIPv4Public(int timeout, PublicIPv4Provider serviceProvider = PublicIPv4Provider.ipify)
        {
            return await getIPv4Public(new CancellationTokenSource(timeout).Token, serviceProvider);
        }

        /// <summary>
        /// Gets your public IPv4 using an external service, selected by the <see cref="PublicIPv4Provider"/> argument. Rate limits do not matter, since results are cached. Make sure to check any anamolies documented on service provider types, as these may differentiate your results.
        /// </summary>
        /// <returns></returns>
        public static async Task<(IPAddress Address, Exception Error)> GetIPv4Public(CancellationToken cancellationToken, int timeout = 1000, PublicIPv4Provider serviceProvider = PublicIPv4Provider.ipify)
        {
            return await getIPv4Public(CancellationTokenSource.CreateLinkedTokenSource(new CancellationTokenSource(timeout).Token, cancellationToken).Token, serviceProvider);
        }

        /// <summary>
        /// Gets your public IPv4 using an external service, selected by the <see cref="PublicIPv4Provider"/> argument. Rate limits do not matter, since results are cached. Make sure to check any anamolies documented on service provider types, as these may differentiate your results.
        /// </summary>
        /// <returns></returns>
        public static async Task<(IPAddress Address, Exception Error)> GetIPv4Public(CancellationToken cancellationToken = default, PublicIPv4Provider serviceProvider = PublicIPv4Provider.ipify)
        {
            return await getIPv4Public(cancellationToken, serviceProvider);
        }

        private static async Task<(IPAddress Address, Exception Error)> getIPv4Public(CancellationToken cancellationToken, PublicIPv4Provider serviceProvider)
        {
            if (cached_PublicIPv4 != null) return (cached_PublicIPv4, null);
            try
            {
                using (var http = new HttpClient())
                {
                    string endpoint = string.Empty;
                    switch (serviceProvider)
                    {
                        case (PublicIPv4Provider.ipify): { endpoint = "https://api.ipify.org/"; break; }
                        case (PublicIPv4Provider.ifconfig): { endpoint = "https://ifconfig.me/ip"; break; }
                    }

                    HttpResponseMessage response = await http.GetAsync(endpoint, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var ip = await response.Content.ReadAsStringAsync();

                    IPAddress resultIP;
                    bool success = IPAddress.TryParse(ip, out resultIP);
                    resultIP = success ? resultIP : null;
                    cached_PublicIPv4 = resultIP;

                    return (resultIP, null);
                }
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        }

        /// <summary>
        /// The API service to use to obtain your public IPv4.
        /// </summary>
        public enum PublicIPv4Provider
        {
            ipify = 0,
            ifconfig = 1
        }

        public struct EndpointCheckResult
        {
            public EndpointStatus Status;
            public Exception Exception;
            public IPAddress ReachableAddress;
        }

        public enum EndpointStatus
        {
            /// <summary>
            /// External on the internet.
            /// </summary>
            Remote,
            /// <summary>
            /// Failed to reach for whatever reason.
            /// </summary>
            Unreachable,
            /// <summary>
            /// On this PC.
            /// </summary>
            Local
        }

        /// <summary>
        /// Get a subnet representation of IPv4
        /// </summary>
        private static IPAddress GetSubnet(IPAddress ip, IPAddress mask)
        {
            var ipBytes = ip.GetAddressBytes();
            var maskBytes = mask.GetAddressBytes();
            var result = new byte[ipBytes.Length];
            for (int i = 0; i < ipBytes.Length; i++)
                result[i] = (byte)(ipBytes[i] & maskBytes[i]);
            return new IPAddress(result);
        }
    }

    public static class Firewall
    {
        public enum FirewallRuleDirection
        {
            In = 0,
            Out = 1
        }

        public enum FirewallRuleAction
        {
            Allow = 0,
            Block = 1
        }

        [Flags]
        public enum NetworkProfile
        {
            None = 0,
            Domain = 1 << 0,
            Private = 1 << 1,
            Public = 1 << 2,
            Any = Domain | Private | Public
        }

#if WINDOWS
        /// <summary>
        /// Adds an inbound firewall rule for Windows operating systems. Requires elevated permissions.
        /// </summary>
        /// <param name="askForElevation">(true) Should the method run a shell to obtain exlcusive permission just for this command or (false) assume the current process is already elevated</param>
        /// <param name="ruleName"></param>
        /// <param name="port">Local port</param>
        /// <param name="action">Should this rule allow or block these specifications</param>
        /// <param name="description"></param>
        /// <param name="direction"></param>
        /// <param name="profiles">The network profiles this rule works on</param>
        /// <param name="protocol"></param>
        /// <param name="remoteIps">Specific remote IPs to allow. Provide null for any. Ranges not yet supported.</param>
        /// <param name="remotePorts">Specific remote ports to allow. Provide null for any. Ranges not yet supported.</param>
        /// <param name="UPnPNATEnabled">Is UPnP/NAT traversal enabled for this rule</param>
        /// <param name="exception">The error outputted if any</param>
        /// <remarks>Can only add rules to the current executable!</remarks>
        /// <returns></returns>
        public static (bool Success, int Exitcode, Exception Error) AddInRuleWin(bool askForElevation, string ruleName, int port, FirewallRuleAction action, Protocol protocol, string description = "", bool UPnPNATEnabled = false, NetworkProfile profiles = NetworkProfile.Any, IPAddress[] remoteIps = null, int[] remotePorts = null) =>
            addrulewin(askForElevation, ruleName, port, FirewallRuleDirection.In, action, protocol, description, UPnPNATEnabled, profiles, remoteIps, remotePorts);

        /// <summary>
        /// Adds an outbound firewall rule for Windows operating systems. Requires elevated permissions.
        /// </summary>
        /// <param name="askForElevation">(true) Should the method run a shell to obtain exlcusive permission just for this command or (false) assume the current process is already elevated</param>
        /// <param name="ruleName"></param>
        /// <param name="action">Should this rule allow or block these specifications</param>
        /// <param name="description"></param>
        /// <param name="direction"></param>
        /// <param name="profiles">The network profiles this rule works on</param>
        /// <param name="protocol"></param>
        /// <param name="remoteIps">Specific remote IPs to allow. Provide null for any. Ranges not yet supported.</param>
        /// <param name="remotePorts">Specific remote ports to allow. Provide null for any. Ranges not yet supported.</param>
        /// <param name="UPnPNATEnabled">Is UPnP/NAT traversal enabled for this rule</param>
        /// <param name="exception">The error outputted if any</param>
        /// <remarks>Can only add rules to the current executable!</remarks>
        /// <returns></returns>
        public static (bool Success, int Exitcode, Exception Error) AddOutRuleWin(bool askForElevation, string ruleName, FirewallRuleAction action, Protocol protocol, string description = "", NetworkProfile profiles = NetworkProfile.Any, IPAddress[] remoteIps = null, int[] remotePorts = null) =>
            addrulewin(askForElevation, ruleName, 0, FirewallRuleDirection.Out, action, protocol, description, false, profiles, remoteIps, remotePorts);

        private static (bool Success, int Exitcode, Exception Error) addrulewin(bool askForElevation, string ruleName, int port, FirewallRuleDirection direction, FirewallRuleAction action, Protocol protocol, string description = "", bool UPnPNATEnabled = false, NetworkProfile profiles = NetworkProfile.Any, IPAddress[] remoteIps = null, int[] remotePorts = null)
        {
            if (!askForElevation && !ProcessHelper.IsProcessElevated())
                return (false, 1, new UnauthorizedAccessException("The current process does not have elevated permission."));

            try
            {
                string localportS = direction == FirewallRuleDirection.In ? $"localport={port}" : "";
                string directionS = direction == FirewallRuleDirection.In ? "in" : "out";
                string actionS = action == FirewallRuleAction.Allow ? "allow" : "block";
                string edgeS = direction == FirewallRuleDirection.In ? $"edge={(UPnPNATEnabled ? "yes" : "no")}" : "";
                string descriptionS = string.IsNullOrEmpty(description) ? "" : $"description=\"{description}\"";

                string profileS;
                if (profiles.HasFlag(NetworkProfile.Any))
                    profileS = "any";
                else
                {
                    List<string> profileFlags = new();

                    if (profiles.HasFlag(NetworkProfile.Domain))
                        profileFlags.Add("domain");
                    if (profiles.HasFlag(NetworkProfile.Private))
                        profileFlags.Add("private");
                    if (profiles.HasFlag(NetworkProfile.Public))
                        profileFlags.Add("public");

                    profileS = string.Join(",", profileFlags);
                }

                string remoteipSt;

                if (remoteIps == null)
                    remoteipSt = "any";
                else
                    remoteipSt = string.Join(',', remoteIps.Select(address => address.ToString()));

                string remoteportSt;

                if (remotePorts == null)
                    remoteportSt = "any";
                else
                    remoteportSt = string.Join(',', remotePorts.Select(port => port.ToString()));

                string arguments = $"advfirewall firewall add rule name=\"{ruleName}\" {descriptionS} dir={directionS} action={actionS} protocol={protocol.ToString()} {localportS} program=\"{Process.GetCurrentProcess().MainModule.FileName}\" {edgeS} profile={profileS} remoteip={remoteipSt} remoteport={remoteportSt}";

                var info = new ProcessStartInfo("netsh", arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = askForElevation,
                    RedirectStandardError = !askForElevation,
                    Verb = askForElevation ? "runas" : ""
                };

                using (var proc = Process.Start(info))
                {
                    proc.WaitForExit();

                    if (!askForElevation && proc.ExitCode != 0)
                    {
                        return (false, proc.ExitCode, new Exception(proc.StandardError.ReadToEnd()));
                    }

                    return (proc.ExitCode == 0, proc.ExitCode, null);
                }
            }
            catch (Exception ex)
            {
                return (false, 1, ex);
            }
        }

        /// <summary>
        /// Removes a firewall rule for Windows operating systems. Requires elevated permissions.
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="port">Local port</param>
        /// <param name="protocol"></param>
        /// <remarks>Can only remove rules attached to the current executable!</remarks>
        /// <returns></returns>
        public static (bool Success, int Exitcode, Exception Error) RemoveRuleWin(bool askForElevation, string ruleName, int? port = null, FirewallRuleDirection? direction = null, Protocol? protocol = null)
        {
            if (!askForElevation && !ProcessHelper.IsProcessElevated())
                return (false, 1, new UnauthorizedAccessException("The current process does not have elevated permission."));

            try
            {
                List<string> argsList = new()
                {
                    "advfirewall firewall delete rule",
                    $"name=\"{ruleName}\"",
                    $"program=\"{Process.GetCurrentProcess().MainModule.FileName}\""
                };

                if (port.HasValue)
                    argsList.Add($"localport={port.Value}");
                if (direction.HasValue)
                {
                    string directionS = direction == FirewallRuleDirection.In ? "in" : "out";
                    argsList.Add($"dir={directionS}");
                }
                if (protocol.HasValue)
                    argsList.Add($"protocol={protocol.Value}");

                string arguments = string.Join(" ", argsList);

                var info = new ProcessStartInfo("netsh", arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = askForElevation,
                    RedirectStandardError = !askForElevation,
                    Verb = askForElevation ? "runas" : ""
                };

                using var proc = Process.Start(info);
                proc.WaitForExit();

                if(!askForElevation && proc.ExitCode != 0)
                {
                    return (false, proc.ExitCode, new Exception(proc.StandardError.ReadToEnd()));
                }

                return (proc.ExitCode == 0, proc.ExitCode, null);
            }
            catch (Exception ex)
            {
                return (false, 1, ex);
            }
        }

        /// <summary>
        /// Checks if a firewall rule exists.
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static (bool Exists, string ActualOutput, int Exitcode, Exception Error) QueryRuleWin(string ruleName, FirewallRuleDirection? direction = null)
        {
            try
            {
                List<string> argsList = new()
                {
                    "advfirewall firewall show rule",
                    $"name=\"{ruleName}\""
                };

                if (direction.HasValue)
                {
                    string directionS = direction == FirewallRuleDirection.In ? "in" : "out";
                    argsList.Add($"dir={directionS}");
                }

                string arguments = string.Join(" ", argsList);

                var info = new ProcessStartInfo("netsh", arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                using var proc = Process.Start(info);
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                return (proc.ExitCode == 0 && !output.Contains("No rules match the specified criteria."), output, proc.ExitCode, null);
            }
            catch (Exception ex)
            {
                return (false, string.Empty, 1, ex);
            }
        }
#endif
    }

    /// <summary>
    /// Helper class for <see cref="Stream"/>s. Use <see cref="CancellationToken"/> in these operations wisely, misuse could result in data corruption.
    /// </summary>
    public static class DataStreamHelper
    {
        /// <summary>
        /// Turns raw byte data into two writes, one containing info about the entire length of the data and one being the data itself. Works with <see cref="ReadStreamAsync(Stream, CancellationToken)"/> to ensure data recieved triggers only fire when the FULL data has been recieved (instead of chunks that were automatically split).
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task WriteStreamAsync(Stream stream, byte[] message, CancellationToken cancellationToken = default)
        {
            byte[] lengthBytes = BitConverter.GetBytes(message.Length); // 4 bytes

            if (!BitConverter.IsLittleEndian) Array.Reverse(lengthBytes); // Ensure compatability

            await stream.WriteAsync(lengthBytes, 0, 4, cancellationToken);
            await stream.WriteAsync(message, 0, message.Length, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        /// <summary>
        /// Pieces together data writes that were sent using <see cref="WriteStreamAsync(Stream, byte[], CancellationToken)"/>. When used with its companion method it should never give incomplete data chunks.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public static async Task<byte[]> ReadStreamAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            byte[] lengthBuffer = new byte[4];
            int read = await stream.ReadAsync(lengthBuffer, 0, 4, cancellationToken);
            if (read < 4)
                throw new IOException("Failed to read message length.");

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(lengthBuffer);

            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
            byte[] messageBuffer = new byte[messageLength];
            int totalRead = 0;

            while (totalRead < messageLength)
            {
                int bytesRead = await stream.ReadAsync(messageBuffer, totalRead, messageLength - totalRead, cancellationToken);
                if (bytesRead == 0)
                    throw new IOException("Unexpected disconnect while reading.");
                totalRead += bytesRead;
            }
            return messageBuffer;
        }
    }

    /// <summary>
    /// This helps check if the current process is elevated by getting its token information. Used for additional debugging when firewall rules are attempted to be added/removed with assumption that the current process is already elevated.
    /// </summary>
    public static class ProcessHelper
    {
#if WINDOWS
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetTokenInformation(IntPtr tokenHandle, TOKEN_INFORMATION_CLASS tokenInfoClass, ref TOKEN_ELEVATION tokenInfo, int tokenInformationLength, out int returnLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_ELEVATION
        {
            public int TokenIsElevated;
        }

        private enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenElevation = 20
        }

        public static bool IsProcessElevated()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);

            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                return false;

            var token = identity.Token;
            TOKEN_ELEVATION elevation = new();
            int elevationSize = Marshal.SizeOf(typeof(TOKEN_ELEVATION));
            if (!GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenElevation, ref elevation, elevationSize, out int _))
                return false;

            return elevation.TokenIsElevated != 0;
        }
#endif
    }

#if UNITY_EDITOR
    internal static class CompileFlags
    {
        const string define = "WINDOWS";

        static CompileFlags()
        {
            var buildTarget = UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget)
                .Split(';')
                .Select(d => d.Trim())
                .ToList();

            bool windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (windows && !defines.Contains(define))
            {
                defines.Add(define);
                UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, string.Join(";", defines));
            }
            else if (!windows && defines.Contains(define))
            {
                defines.Remove(define);
                UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, string.Join(";", defines));
            }
        }
    }
#endif
}