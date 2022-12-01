namespace SimListener
{
    public class Track : IEquatable<Track>
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

        public override bool Equals(object? obj)
        {
            return obj is not null && Equals(obj as Track);
        }

        public bool Equals(Track? other)
        {
            return other is not null
&& latitude == other.latitude &&
                   longitude == other.longitude &&
                   altitude == other.altitude &&
                   airspeed == other.airspeed &&
                   heading == other.heading;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(latitude, longitude, altitude, airspeed, heading);
        }

        public override string ToString()
        {
            return "Lat = " + latitude +
                   " Long = " + longitude +
                   " Alt = " + altitude +
                   " Speed = " + airspeed +
                   " Head = " + heading;
        }

        public static bool operator ==(Track left, Track right)
        {
            return EqualityComparer<Track>.Default.Equals(left, right);
        }

        public static bool operator !=(Track left, Track right)
        {
            return !(left == right);
        }

        internal bool Zero()
        {
            return latitude == 0 && longitude == 0 && altitude == 0 && airspeed == 0 && heading == 0;
        }
    }
}
