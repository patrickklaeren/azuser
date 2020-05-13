using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;

namespace Azuser.Client.Framework
{
    public interface IMessengerService
    {
        void Send<TMessage>(TMessage message);
        void Register<TMessage>(object recipient, Action<TMessage> action);
        void Unregister(object context);
    }
    
    public class MessengerService : IMessengerService
    {
        private static readonly object CreationLock = new object();
        
        private static IMessengerService _defaultInstance;

        private readonly object _registerLock = new object();

        private Dictionary<Type, List<WeakAction>> _recipients;
        private bool _isCleanupRegistered;

        public static IMessengerService Default
        {
            get
            {
                if (_defaultInstance is null)
                {
                    lock (CreationLock)
                    {
                        _defaultInstance ??= new MessengerService();
                    }
                }

                return _defaultInstance;
            }
        }

        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            lock (_registerLock)
            {
                var messageType = typeof(TMessage);

                _recipients ??= new Dictionary<Type, List<WeakAction>>();

                var recipients = _recipients;

                lock (recipients)
                {
                    List<WeakAction> weakActions;

                    if (!_recipients.ContainsKey(messageType))
                    {
                        weakActions = new List<WeakAction>();
                        _recipients.Add(messageType, weakActions);
                    }
                    else
                    {
                        weakActions = _recipients[messageType];
                    }

                    var weakAction = new WeakAction<TMessage>(recipient, action);

                    weakActions.Add(weakAction);
                }
            }

            RequestCleanup();
        }

        public void Send<TMessage>(TMessage message)
        {
            var messageType = typeof(TMessage);

            if (_recipients != null)
            {
                List<WeakAction> weakActions = null;

                lock (_recipients)
                {
                    if (_recipients.ContainsKey(messageType))
                    {
                        weakActions = _recipients[messageType]
                            .Take(_recipients[messageType].Count)
                            .ToList();
                    }
                }

                if (weakActions != null)
                {
                    SendToList(message, weakActions);
                }
            }

            RequestCleanup();
        }

        public void Unregister(object recipient)
        {
            UnregisterFromLists(recipient, _recipients);
            RequestCleanup();
        }

        public void Unregister<TMessage>(object recipient)
        {
            UnregisterFromLists(recipient, _recipients);
            RequestCleanup();
        }

        private static void CleanupList(IDictionary<Type, List<WeakAction>> weakActions)
        {
            if (weakActions is null)
            {
                return;
            }

            lock (weakActions)
            {
                var listsToRemove = new List<Type>();
                
                foreach (var (typeOfMessage, listeners) in weakActions)
                {
                    var recipientsToRemove = listeners
                        .Where(item => item == null || !item.IsAlive)
                        .ToList();

                    foreach (var recipient in recipientsToRemove)
                    {
                        listeners.Remove(recipient);
                    }

                    if (listeners.Count == 0)
                    {
                        listsToRemove.Add(typeOfMessage);
                    }
                }

                foreach (var key in listsToRemove)
                {
                    weakActions.Remove(key);
                }
            }
        }

        private static void SendToList<TMessage>(TMessage message, IEnumerable<WeakAction> weakActionsAndTokens)
        {
            if (weakActionsAndTokens == null) 
                return;
            
            var list = weakActionsAndTokens.ToList();
            var listClone = list.Take(list.Count).ToList();

            foreach (var item in listClone)
            {
                if (item is IExecuteWithObject executeAction
                    && item.IsAlive
                    && item.Target != null)
                {
                    executeAction.ExecuteWithObject(message);
                }
            }
        }

        private static void UnregisterFromLists(object recipient, Dictionary<Type, List<WeakAction>> lists)
        {
            if (recipient == null
                || lists == null
                || lists.Count == 0)
            {
                return;
            }

            lock (lists)
            {
                foreach (var messageType in lists.Keys)
                {
                    foreach (var item in lists[messageType])
                    {
                        var weakAction = item;

                        if (weakAction != null
                            && recipient == weakAction.Target)
                        {
                            weakAction.MarkForDeletion();
                        }
                    }
                }
            }
        }

        private void RequestCleanup()
        {
            if (_isCleanupRegistered) 
                return;
            
            Action cleanupAction = Cleanup;

            Dispatcher.CurrentDispatcher.BeginInvoke(
                cleanupAction,
                DispatcherPriority.ApplicationIdle,
                null);

            _isCleanupRegistered = true;
        }

        private void Cleanup()
        {
            CleanupList(_recipients);
            _isCleanupRegistered = false;
        }
    }
}