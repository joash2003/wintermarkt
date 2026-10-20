using System;
using Wintermarkt.Tests;
class MarketRunner
{
    static int Main()
    {
        int failed = 0;
        foreach (var test in MarketTestCases.All)
        {
            try { test(); Console.WriteLine("PASS " + test.Method.Name); }
            catch (Exception e) { failed++; Console.WriteLine("FAIL " + test.Method.Name + ": " + e); }
        }
        Console.WriteLine("Tests: " + MarketTestCases.All.Length + ", failed: " + failed);
        return failed == 0 ? 0 : 1;
    }
}
