namespace SimListener
{
    internal class TrackList : List<Track>
    {
        public List<Track> List()
        {
            return this;
        }
        public void AddTrack(Track _point)
        {
            if (Count > 0)
            {
                if (this.Last().Equals(_point) == false)
                {
                    Add(_point);
                }
                else
                if (_point.Zero() == false)
                {
                    Add(_point);
                }
            }
        }

    }
}
