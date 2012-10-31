using System;

namespace Rhino.ServiceBus
{
    internal interface IDatabaseAccess
    {
        void Subscription(Action<ISubscriptionStorageActions> action);
    }
}