using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Rhino.ServiceBus
{
    internal class SubscriptionStorageActions : ISubscriptionStorageActions
    {
        private readonly SqlConnection connection;
        private readonly SqlTransaction transaction;

        internal SubscriptionStorageActions(SqlConnection connection, SqlTransaction transaction)
        {
            this.connection = connection;
            this.transaction = transaction;
        }

        public bool AddSubscription(string type, string subscriberUri)
        {
            var sqlCommand = new SqlCommand("[RSB].[AddSubscription]", connection, transaction);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@messageTypeName", type);
            sqlCommand.Parameters.AddWithValue("@subscriberUri", subscriberUri);
            return Convert.ToBoolean(sqlCommand.ExecuteScalar());
        }

        public void Commit()
        {
            transaction.Commit();
        }

        public IEnumerable<Uri> GetSubscriptions(string type)
        {
            var sqlCommand = new SqlCommand("[RSB].[GetAllSubscriptionsForType]", connection, transaction)
            {
                CommandType = CommandType.StoredProcedure
            };
            sqlCommand.Parameters.AddWithValue("@messageTypeName", type);
            using (var sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.SingleResult))
            {
                while (sqlDataReader.Read())
                    yield return new Uri(sqlDataReader.GetString(0));
            }
        }

        public void RemoveSubscription(string type, string subscriberUri)
        {
            var sqlCommand = new SqlCommand("[RSB].[RemoveSubscription]", connection, transaction);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@messageTypeName", type);
            sqlCommand.Parameters.AddWithValue("@subscriberUri", subscriberUri);
            sqlCommand.ExecuteNonQuery();
        }
    }
}