﻿namespace SimListener
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
    }
}
