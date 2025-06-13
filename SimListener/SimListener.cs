using System.Runtime.InteropServices;

namespace SimListener
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ResultStructure
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string sValue;
    };

    internal class SimvarRequest 
    {
        public DEFINITION eDef = DEFINITION.Dummy;
        public REQUEST eRequest = REQUEST.Dummy;

        public string? sName { get; set; }
        public bool bIsString { get; set; }
        public double dValue
        {
            set { m_dValue = value; }
            get { return m_dValue; }
        }
        private double m_dValue = 0.0;
        public string? sValue
        {
            set { m_sValue = value; }

            get => m_sValue;
        }
        private string? m_sValue = null;

        public string? sUnits { get; set; }

        public bool bPending = true;
        public bool bStillPending
        {
            set { m_bStillPending = value; }
            get { return m_bStillPending; }
        }
        private bool m_bStillPending = false;

    };
    /// <summary>  
    /// This class is used to pass data from the simulator to the listener.  
    /// </summary>  
    public class SimulatorData : EventArgs
    {
        /// <summary>  
        /// Gets or sets the time when the data was received.  
        /// </summary>  
        public DateTime TimeReached { get; set; }

        /// <summary>  
        /// Gets or sets the aircraft data as a list of dictionaries containing key-value pairs.  
        /// </summary>  
        public List<Dictionary<string, string>> AircraftData { get; set; } = new List<Dictionary<string, string>>();
    }
}
