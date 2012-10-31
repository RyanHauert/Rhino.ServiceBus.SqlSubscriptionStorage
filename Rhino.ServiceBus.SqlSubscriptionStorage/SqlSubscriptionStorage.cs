using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Common.Logging;
using Rhino.ServiceBus.Impl;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.MessageModules;
using Rhino.ServiceBus.Messages;

namespace Rhino.ServiceBus
{
    public class SqlSubscriptionStorage : ISubscriptionStorage, IMessageModule
    {
        private readonly string _connectionString;
        private readonly ILog _logger = LogManager.GetLogger(typeof(SqlSubscriptionStorage));

        public SqlSubscriptionStorage()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["Rhino.ServiceBus.SqlSubscriptionStorage"].ConnectionString;
        }

        public void AddLocalInstanceSubscription(IMessageConsumer consumer)
        {
        }

        public bool AddSubscription(string type, string endpoint)
        {
            Subscription((subscription =>
            {
                subscription.AddSubscription(type, endpoint);
                subscription.Commit();
            }));
            RaiseSubscriptionChanged();
            _logger.DebugFormat("Added subscription for {0} message at endpoint {1}", type, endpoint);
            return true;
        }

        public bool ConsumeAddInstanceSubscription(AddInstanceSubscription subscription)
        {
            return true;
        }

        public bool ConsumeAddSubscription(AddSubscription subscription)
        {
            return AddSubscription(subscription.Type, subscription.Endpoint.Uri.AbsoluteUri);
        }

        public bool ConsumeRemoveInstanceSubscription(RemoveInstanceSubscription subscription)
        {
            return true;
        }

        public bool ConsumeRemoveSubscription(RemoveSubscription subscription)
        {
            RemoveSubscription(subscription.Type, subscription.Endpoint.Uri.AbsoluteUri);
            return true;
        }

        public object[] GetInstanceSubscriptions(Type type)
        {
            return new object[0];
        }

        public IEnumerable<Uri> GetSubscriptionsFor(Type type)
        {
            IEnumerable<Uri> subscriptions = new List<Uri>();
            try
            {
                Subscription(subscription =>
                {
                    subscriptions = subscription.GetSubscriptions(type.FullName).ToList();
                    subscription.Commit();
                });
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error getting subscriptions for {0}", type.FullName), ex);
            }
            return subscriptions;
        }

        public void Init(ITransport transport, IServiceBus bus)
        {
            transport.AdministrativeMessageArrived += TransportAdministrativeMessageArrived;
        }

        public void Initialize()
        {
        }

        public void RemoveLocalInstanceSubscription(IMessageConsumer consumer)
        {
        }

        public void RemoveSubscription(string type, string endpoint)
        {
            Subscription(subscription =>
            {
                subscription.RemoveSubscription(type, endpoint);
                subscription.Commit();
            });
            _logger.DebugFormat("Removed subscription for {0} message at endpoint {1}", type, endpoint);
        }

        public void Stop(ITransport transport, IServiceBus bus)
        {
            transport.AdministrativeMessageArrived -= TransportAdministrativeMessageArrived;
        }

        void RaiseSubscriptionChanged()
        {
            var action = SubscriptionChanged;
            if (action == null)
                return;
            action();
        }

        void Subscription(Action<SubscriptionStorageActions> action)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var subscriptionStorageActions = new SubscriptionStorageActions(connection, transaction);
                    action(subscriptionStorageActions);
                }
            }
        }

        bool TransportAdministrativeMessageArrived(CurrentMessageInformation msgInfo)
        {
            var subscription = msgInfo.Message as AddSubscription;
            if (subscription != null)
                return ConsumeAddSubscription(subscription);
            var removeSubscription = msgInfo.Message as RemoveSubscription;
            if (removeSubscription != null)
                return ConsumeRemoveSubscription(removeSubscription);
            var addInstanceSubscription = msgInfo.Message as AddInstanceSubscription;
            if (addInstanceSubscription != null)
                return ConsumeAddInstanceSubscription(addInstanceSubscription);
            var removeInstanceSubscription = msgInfo.Message as RemoveInstanceSubscription;
            if (removeInstanceSubscription != null)
                return ConsumeRemoveInstanceSubscription(removeInstanceSubscription);
            return false;
        }

        public event Action SubscriptionChanged;
    }
}