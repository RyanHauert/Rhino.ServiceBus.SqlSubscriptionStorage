using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Mocks;
using Xunit;

namespace Rhino.ServiceBus.SqlSubscriptionStorageTests
{
    public class UsingCache
    {
        [Fact]
        public void SubscriptionIsReturnedFromCacheWhenExceptionIsThrown()
        {
            string type = typeof(object).FullName;
            const string endpoint = "null://test";

            var storageActions = MockRepository.GenerateMock<ISubscriptionStorageActions>();
            storageActions.Expect(x => x.GetSubscriptions(type))
                .Return(new[] { new Uri(endpoint) });

            var databaseAccess = MockRepository.GenerateMock<IDatabaseAccess>();
            databaseAccess.Expect(x => x.Subscription(Arg<Action<ISubscriptionStorageActions>>.Is.Anything))
                .WhenCalled(x => ((Action<ISubscriptionStorageActions>)x.Arguments[0]).Invoke(storageActions));

            var subscriptionStorage = new SqlSubscriptionStorage(databaseAccess);
            subscriptionStorage.GetSubscriptionsFor(typeof(object));

            databaseAccess.Expect(x => x.Subscription(Arg<Action<ISubscriptionStorageActions>>.Is.Anything))
                .Throw(new Exception());

            IEnumerable<Uri> subscriptions = subscriptionStorage.GetSubscriptionsFor(typeof(object));
            Assert.NotNull(subscriptions);
            Assert.Equal(1, subscriptions.Count());
            Assert.Equal(new Uri(endpoint), subscriptions.First());
        }
    }
}