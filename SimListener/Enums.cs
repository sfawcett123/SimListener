namespace SimListener
{
    internal enum DEFINITION
    {
        Dummy = 0,
        MAX_DEFINITIONS = 100
    };
    internal enum REQUEST
    {
        Dummy = 0,
        ResultStructure,
        MAX_REQUESTS = 100
    };
    internal enum Event
    {
        RECUR_1SEC,
    }
    internal enum Requests
    {
        SIMULATION,
        AIRCRAFT_LOADED,
    }

    public partial class Connect : IDisposable
    {
        /// <summary> Constants for SimConnect messages
        /// These constants are used to identify messages sent by SimConnect.
        /// They are used in the message loop to process incoming SimConnect messages.
        /// </summary>   

        private const uint WM_USER_SIMCONNECT = 0x0402;
    }
}
