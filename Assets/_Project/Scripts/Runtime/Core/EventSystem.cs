using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Core.Events
{
    /// <summary>
    /// Global event system for decoupled communication between systems
    /// </summary>
    public static class EventSystem
    {
        private static readonly Dictionary<Type, List<IEventListener>> _listeners = new Dictionary<Type, List<IEventListener>>();
        private static readonly Dictionary<Type, List<Action<IGameEvent>>> _actions = new Dictionary<Type, List<Action<IGameEvent>>>();

        #region Public API

        /// <summary>
        /// Subscribe to an event type with a callback action
        /// </summary>
        public static void Subscribe<T>(Action<T> callback) where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (!_actions.ContainsKey(eventType))
                _actions[eventType] = new List<Action<IGameEvent>>();

            _actions[eventType].Add(evt => callback((T)evt));
        }

        /// <summary>
        /// Subscribe an event listener to an event type
        /// </summary>
        public static void Subscribe<T>(IEventListener<T> listener) where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (!_listeners.ContainsKey(eventType))
                _listeners[eventType] = new List<IEventListener>();

            _listeners[eventType].Add(listener);
        }

        /// <summary>
        /// Unsubscribe from an event type
        /// </summary>
        public static void Unsubscribe<T>(Action<T> callback) where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (_actions.ContainsKey(eventType))
            {
                _actions[eventType].RemoveAll(action => action.Method == callback.Method);
            }
        }

        /// <summary>
        /// Unsubscribe an event listener
        /// </summary>
        public static void Unsubscribe<T>(IEventListener<T> listener) where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (_listeners.ContainsKey(eventType))
            {
                _listeners[eventType].Remove(listener);
            }
        }

        /// <summary>
        /// Publish an event to all subscribers
        /// </summary>
        public static void Publish<T>(T gameEvent) where T : IGameEvent
        {
            Type eventType = typeof(T);

            // Notify action subscribers
            if (_actions.ContainsKey(eventType))
            {
                foreach (var action in _actions[eventType])
                {
                    try
                    {
                        action.Invoke(gameEvent);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error invoking event action for {eventType}: {ex}");
                    }
                }
            }

            // Notify listener subscribers
            if (_listeners.ContainsKey(eventType))
            {
                foreach (var listener in _listeners[eventType])
                {
                    try
                    {
                        if (listener is IEventListener<T> typedListener)
                        {
                            typedListener.OnEventReceived(gameEvent);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error invoking event listener for {eventType}: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// Clear all subscribers (useful for scene transitions)
        /// </summary>
        public static void Clear()
        {
            _listeners.Clear();
            _actions.Clear();
        }

        /// <summary>
        /// Clear subscribers for a specific event type
        /// </summary>
        public static void Clear<T>() where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (_listeners.ContainsKey(eventType))
                _listeners[eventType].Clear();

            if (_actions.ContainsKey(eventType))
                _actions[eventType].Clear();
        }

        #endregion

        #region Debug

        public static int GetSubscriberCount<T>() where T : IGameEvent
        {
            Type eventType = typeof(T);
            int count = 0;

            if (_listeners.ContainsKey(eventType))
                count += _listeners[eventType].Count;

            if (_actions.ContainsKey(eventType))
                count += _actions[eventType].Count;

            return count;
        }

        #endregion
    }

    #region Interfaces

    public interface IGameEvent
    {
        DateTime Timestamp { get; }
    }

    public interface IEventListener
    {
    }

    public interface IEventListener<in T> : IEventListener where T : IGameEvent
    {
        void OnEventReceived(T gameEvent);
    }

    #endregion

    #region Base Event Class

    public abstract class BaseGameEvent : IGameEvent
    {
        public DateTime Timestamp { get; private set; }

        protected BaseGameEvent()
        {
            Timestamp = DateTime.UtcNow;
        }
    }

    #endregion

    #region Example Events

    public class GameStateChangedEvent : BaseGameEvent
    {
        public GameState PreviousState { get; }
        public GameState NewState { get; }

        public GameStateChangedEvent(GameState previousState, GameState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    public class PlayerConnectedEvent : BaseGameEvent
    {
        public string PlayerId { get; }
        public string PlayerName { get; }

        public PlayerConnectedEvent(string playerId, string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;
        }
    }

    public class PlayerDisconnectedEvent : BaseGameEvent
    {
        public string PlayerId { get; }

        public PlayerDisconnectedEvent(string playerId)
        {
            PlayerId = playerId;
        }
    }

    #endregion
}