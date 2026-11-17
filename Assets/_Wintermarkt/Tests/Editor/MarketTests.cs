using NUnit.Framework;
namespace Wintermarkt.Tests
{
    public sealed class MarketTests
    {
        [Test] public void MoodIsRecorded() { MarketTestCases.MoodIsRecorded(); }
        [Test] public void RemoveRefundsAndFreesSpace() { MarketTestCases.RemoveRefundsAndFreesSpace(); }
        [Test] public void MissedWaveTargetEndsFestival() { MarketTestCases.MissedWaveTargetEndsFestival(); }
        [Test] public void MetTargetsAdvanceAndWin() { MarketTestCases.MetTargetsAdvanceAndWin(); }
        [Test] public void ArrivalFactorSpeedsArrivals() { MarketTestCases.ArrivalFactorSpeedsArrivals(); }
        [Test] public void VisitorVisitsEveryWish() { MarketTestCases.VisitorVisitsEveryWish(); }
        [Test] public void MissingWishDisappoints() { MarketTestCases.MissingWishDisappoints(); }
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
