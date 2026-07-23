namespace BusinessLayer.Helpers;

public class AttendanceSettings
{
    public int CheckInEarlyMinutes { get; set; } = 30;
    public int CheckInLateWindowMinutes { get; set; } = 60;
    public int LateThresholdMinutes { get; set; } = 1;
    public int EarlyLeaveThresholdMinutes { get; set; } = 1;
    public int AbsentAfterStartMinutes { get; set; } = 60;
}
