namespace HeartRateZoneService.Domain.HeartRateZoneService
{

    public class HeartRateZoneReached
    {
        public HeartRateZoneReached(Guid deviceId, HeartRateZone zone, DateTime dateTime, int heartRate, int maxHeartRate)
        {
            DeviceId = deviceId;
            Zone = zone;
            DateTime = dateTime;
            HeartRate = heartRate;
            MaxHeartRate = maxHeartRate;
        }

        public Guid DeviceId { get; set; }
        public HeartRateZone Zone { get; set; }
        public DateTime DateTime { get; set; }
        public int HeartRate { get; set; }
        public int MaxHeartRate { get; set; }
    }


    public enum HeartRateZone
    {
        None = 0,
        Zone1,
        Zone2,
        Zone3,
        Zone4,
        Zone5
    }
    
    public static class HeartRateExtensions
    {
        public static HeartRateZone GetHeartRateZone(this HeartRate heartRate, int maxHeartRate)
        {
            if (heartRate.Value < (0.5 * maxHeartRate))
            {
                return HeartRateZone.None;
            }
            if ((0.50 * maxHeartRate) >= heartRate.Value && heartRate.Value <= (0.59 * maxHeartRate))
            {
                return HeartRateZone.Zone1;
            }
            if ((0.60 * maxHeartRate) >= heartRate.Value && heartRate.Value <= (0.69 * maxHeartRate))
            {
                return HeartRateZone.Zone2;
            }
            if ((0.70 * maxHeartRate) >= heartRate.Value && heartRate.Value <= (0.79 * maxHeartRate))
            {
                return HeartRateZone.Zone3;
            }
            if ((0.80 * maxHeartRate) >= heartRate.Value && heartRate.Value <= (0.89 * maxHeartRate))
            {
                return HeartRateZone.Zone4;
            }
            return HeartRateZone.Zone5;
        }
    }
}
