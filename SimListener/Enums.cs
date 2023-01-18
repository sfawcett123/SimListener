namespace SimListener
{
    public enum ErrorCodes
    {
        OK,
        INVALID_DATA_REQUEST,
    }
    internal enum DEFINITION
    {
        Dummy = 0
    };
    internal enum REQUEST
    {
        Dummy = 0,
        Struct1
    };
    internal enum EVENT
    {
        RECUR_1SEC,
    }
    internal enum REQUESTS
    {
        SIMULATION,
        AIRCRAFT_LOADED,
    }
}
