using NUnit.Framework;
namespace Wintermarkt.Tests
{
    public sealed class MarketTests
    {
        [Test] public void NoArrivalsBeforeFirstBuilding() { MarketTestCases.NoArrivalsBeforeFirstBuilding(); }
        [Test] public void PlacementIsAtomic() { MarketTestCases.PlacementIsAtomic(); }
        [Test] public void NavigationRejectsWalls() { MarketTestCases.NavigationRejectsWalls(); }
        [Test] public void ServicePaysExactlyOnce() { MarketTestCases.ServicePaysExactlyOnce(); }
        [Test] public void QueueSlotsAreUnique() { MarketTestCases.QueueSlotsAreUnique(); }
        [Test] public void UpgradeAndPause() { MarketTestCases.UpgradeAndPause(); }
        [Test] public void UpgradeImprovesThroughput() { MarketTestCases.UpgradeImprovesThroughput(); }
        [Test] public void FullRunStaysBounded() { MarketTestCases.FullRunStaysBounded(); }
        [Test] public void InvalidDataFails() { MarketTestCases.InvalidDataFails(); }
    }
}
