using BusinessLayer.Helpers;

namespace BusinessLayer.Tests.Operations;

public class BranchScheduleTimeTests
{
    [Fact]
    public void ToUtc_ConvertsBangkokNineAmToTwoAmUtc()
    {
        var utc = BranchScheduleTime.ToUtc(new DateOnly(2026, 7, 24), new TimeSpan(9, 0, 0));

        Assert.Equal(new DateTime(2026, 7, 24, 2, 0, 0, DateTimeKind.Utc), utc);
    }

    [Fact]
    public void ToLocal_ConvertsUtcBackToBangkokNineAm()
    {
        var local = BranchScheduleTime.ToLocal(new DateTime(2026, 7, 24, 2, 0, 0, DateTimeKind.Utc));

        Assert.Equal(new DateTime(2026, 7, 24, 9, 0, 0), local);
    }
}
