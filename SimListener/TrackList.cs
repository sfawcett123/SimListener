using System.Collections.ObjectModel;

namespace SimListener
{
    internal class TrackList : List<Track>
    {
        public List<Track> List()
        {
            return this;
        }
        public void AddTrack(Track newpoint)
        {
            // If the last point added isnt the same as this new point then
            // if the newpoint isnt a zero point then add it to the list
            if ( Count>0 && !this.Last().Equals(newpoint) && !newpoint.Zero() ) 
                Add(newpoint);
        }

        public static Track ConstructTrack(ObservableCollection<SimListener> lSimvarRequests )
        {
            Track track = new Track();

            foreach (SimListener oSimvarRequest in lSimvarRequests)
            {
                if (oSimvarRequest.Value is null)
                    continue;

                if (oSimvarRequest.Parameter == "PLANE LONGITUDE")
                {
                    track.Longitude = oSimvarRequest.Value;
                }

                if (oSimvarRequest.Parameter == "PLANE LATITUDE")
                {
                    track.Latitude = oSimvarRequest.Value;
                }

                if (oSimvarRequest.Parameter == "AIRSPEED TRUE")
                {
                    track.Airspeed = oSimvarRequest.Value;
                }

                if (oSimvarRequest.Parameter == "PLANE ALTITUDE")
                {
                    track.Altitude = oSimvarRequest.Value;
                }

                if (oSimvarRequest.Parameter == "PLANE HEADING DEGREES TRUE")
                {
                    track.Heading = oSimvarRequest.Value;
                }
            }

            return track;
        }
    }
}
