using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Core.Services
{
    /// <summary>
    /// Service locator pattern implementation for dependency injection
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

        #region Registration

        /// <summary>
        /// Register a service instance
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            Type serviceType = typeof(T);

            if (_services.ContainsKey(serviceType))
            {
                Debug.LogWarning($"Service {serviceType.Name} is already registered. Overwriting...");
            }

            _services[serviceType] = service;
            Debug.Log($"Service registered: {serviceType.Name}");
        }

        /// <summary>
        /// Register a singleton service with factory function
        /// </summary>
        public static void RegisterSingleton<T>(Func<T> factory) where T : class
        {
            Type serviceType = typeof(T);

            if (_singletons.ContainsKey(serviceType))
            {
                Debug.LogWarning($"Singleton {serviceType.Name} is already registered. Overwriting...");
            }

            _singletons[serviceType] = factory;
            Debug.Log($"Singleton factory registered: {serviceType.Name}");
        }

        /// <summary>
        /// Register a singleton service instance
        /// </summary>
        public static void RegisterSingleton<T>(T instance) where T : class
        {
            Type serviceType = typeof(T);

            if (_singletons.ContainsKey(serviceType))
            {
                Debug.LogWarning($"Singleton {serviceType.Name} is already registered. Overwriting...");
            }

            _singletons[serviceType] = instance;
            Debug.Log($"Singleton instance registered: {serviceType.Name}");
        }

        #endregion

        #region Resolution

        /// <summary>
        /// Resolve a service instance
        /// </summary>
        public static T Resolve<T>() where T : class
        {
            Type serviceType = typeof(T);

            // Check singletons first
            if (_singletons.TryGetValue(serviceType, out object singletonObj))
            {
                // If it's a factory function, create the instance
                if (singletonObj is Func<T> factory)
                {
                    T instance = factory();
                    _singletons[serviceType] = instance; // Replace factory with instance
                    return instance;
                }

                return singletonObj as T;
            }

            // Check regular services
            if (_services.TryGetValue(serviceType, out object serviceObj))
            {
                return serviceObj as T;
            }

            Debug.LogError($"Service {serviceType.Name} not found! Make sure to register it first.");
            return null;
        }

        /// <summary>
        /// Try to resolve a service without throwing errors
        /// </summary>
        public static bool TryResolve<T>(out T service) where T : class
        {
            service = Resolve<T>();
            return service != null;
        }

        /// <summary>
        /// Check if a service is registered
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            Type serviceType = typeof(T);
            return _services.ContainsKey(serviceType) || _singletons.ContainsKey(serviceType);
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Unregister a service
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            Type serviceType = typeof(T);

            if (_services.Remove(serviceType))
            {
                Debug.Log($"Service unregistered: {serviceType.Name}");
            }

            if (_singletons.TryGetValue(serviceType, out object singleton))
            {
                if (singleton is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _singletons.Remove(serviceType);
                Debug.Log($"Singleton unregistered: {serviceType.Name}");
            }
        }

        /// <summary>
        /// Clear all registered services
        /// </summary>
        public static void Clear()
        {
            // Dispose singletons that implement IDisposable
            foreach (var singleton in _singletons.Values)
            {
                if (singleton is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _services.Clear();
            _singletons.Clear();
            Debug.Log("All services cleared");
        }

        #endregion

        #region Debug

        /// <summary>
        /// Get count of registered services
        /// </summary>
        public static int GetServiceCount()
        {
            return _services.Count + _singletons.Count;
        }

        /// <summary>
        /// Get all registered service types
        /// </summary>
        public static Type[] GetRegisteredServiceTypes()
        {
            var types = new List<Type>();
            types.AddRange(_services.Keys);
            types.AddRange(_singletons.Keys);
            return types.ToArray();
        }

        #endregion
    }

    #region Service Interfaces

    /// <summary>
    /// Base interface for all services
    /// </summary>
    public interface IService
    {
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
    }

    /// <summary>
    /// Base service implementation
    /// </summary>
    public abstract class BaseService : IService, IDisposable
    {
        public bool IsInitialized { get; private set; }

        public virtual void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning($"{GetType().Name} is already initialized");
                return;
            }

            OnInitialize();
            IsInitialized = true;
            Debug.Log($"{GetType().Name} initialized");
        }

        public virtual void Shutdown()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning($"{GetType().Name} is not initialized");
                return;
            }

            OnShutdown();
            IsInitialized = false;
            Debug.Log($"{GetType().Name} shut down");
        }

        public virtual void Dispose()
        {
            if (IsInitialized)
            {
                Shutdown();
            }
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnShutdown() { }
    }

    #endregion
}