using NUnit.Framework;
namespace Wintermarkt.Tests
{
    public sealed class MarketTests
    {
        [Test] public void KrampusScaresAndIsDrivenOff() { MarketTestCases.KrampusScaresAndIsDrivenOff(); }
        [Test] public void BurstEventSpawnsVisitors() { MarketTestCases.BurstEventSpawnsVisitors(); }
        [Test] public void StormShortensPatience() { MarketTestCases.StormShortensPatience(); }
        [Test] public void PowerCutCallsEveryStall() { MarketTestCases.PowerCutCallsEveryStall(); }
        [Test] public void HelpedStallIsBoosted() { MarketTestCases.HelpedStallIsBoosted(); }
        [Test] public void MissedHelpClosesStall() { MarketTestCases.MissedHelpClosesStall(); }
        [Test] public void HelpCallsGrowWithStalls() { MarketTestCases.HelpCallsGrowWithStalls(); }
        [Test] public void MoodIsRecorded() { MarketTestCases.MoodIsRecorded(); }
        [Test] public void RemoveRefundsAndFreesSpace() { MarketTestCases.RemoveRefundsAndFreesSpace(); }
        [Test] public void MissedTargetCostsAStarNotTheGame() { MarketTestCases.MissedTargetCostsAStarNotTheGame(); }
        [Test] public void MetTargetsGiveEveryStar() { MarketTestCases.MetTargetsGiveEveryStar(); }
        [Test] public void ReputationZeroEndsTheEvening() { MarketTestCases.ReputationZeroEndsTheEvening(); }
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
