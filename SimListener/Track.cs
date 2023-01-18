namespace SimListener
{
    public class Track
    {
        private double latitude = 0;
        private double longitude = 0;
        private double altitude = 0;
        private double airspeed = 0;
        private double heading = 0;
        public string Latitude
        {
            get => latitude.ToString();
            set => _ = double.TryParse(value, out latitude);
        }
        public string Longitude
        {
            get => longitude.ToString();
            set => _ = double.TryParse(value, out longitude);
        }
        public string Altitude
        {
            get => altitude.ToString();
            set => _ = double.TryParse(value, out altitude);
        }
        public string Airspeed
        {
            get => airspeed.ToString();
            set => _ = double.TryParse(value, out airspeed);
        }
        public string Heading
        {
            get => heading.ToString();
            set => _ = double.TryParse(value, out heading);
        }
        internal bool Zero()
        {
            return latitude == 0 && longitude == 0 && altitude == 0 && airspeed == 0 && heading == 0;
        }
    }
}
