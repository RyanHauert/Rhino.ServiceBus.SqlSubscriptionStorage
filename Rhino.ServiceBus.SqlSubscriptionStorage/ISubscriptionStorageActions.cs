using System;
using System.Collections.Generic;

namespace Rhino.ServiceBus
{
    internal interface ISubscriptionStorageActions
    {
        bool AddSubscription(string type, string subscriberUri);
        void Commit();
        IEnumerable<Uri> GetSubscriptions(string type);
        void RemoveSubscription(string type, string subscriberUri);
    }
}