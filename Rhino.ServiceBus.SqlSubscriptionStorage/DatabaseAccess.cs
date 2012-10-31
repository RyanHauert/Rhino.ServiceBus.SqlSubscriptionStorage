using System;
using System.Data;
using System.Data.SqlClient;

namespace Rhino.ServiceBus
{
    internal class DatabaseAccess : IDatabaseAccess
    {
        private readonly string _connectionString;
        private readonly Func<SqlConnection, SqlTransaction, ISubscriptionStorageActions> _storageActionsFactory;

        public DatabaseAccess(
            string connectionString,
            Func<SqlConnection, SqlTransaction, ISubscriptionStorageActions> storageActionsFactory)
        {
            _connectionString = connectionString;
            _storageActionsFactory = storageActionsFactory;
        }

        public void Subscription(Action<ISubscriptionStorageActions> action)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    ISubscriptionStorageActions subscriptionStorageActions = _storageActionsFactory(connection,
                        transaction);
                    action(subscriptionStorageActions);
                }
            }
        }
    }
}