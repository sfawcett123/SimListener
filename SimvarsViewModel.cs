using Microsoft.FlightSimulator.SimConnect;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace SimListener
{
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
        RECUR_1SEC ,
    }
    internal enum REQUESTS
    {
        SIMULATION,
        AIRCRAFT_LOADED,
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ResultStructure
    {
        // this is how you declare a fixed size string
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string sValue;

        // other definitions can be added to this struct
        // ...
    };
    internal class SimvarRequest : IEquatable<SimvarRequest?>
    {
        public DEFINITION eDef = DEFINITION.Dummy;
        public REQUEST eRequest = REQUEST.Dummy;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Parameter { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Measure { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public bool bIsString = false;

        public bool bPending = true;
        public bool bStillPending = false;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Value { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as SimvarRequest);
        }

        public bool Equals(SimvarRequest? other)
        {
            return other is not null &&
                   Parameter == other.Parameter &&
                   Measure == other.Measure;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Parameter);
        }

        public static bool operator ==(SimvarRequest? left, SimvarRequest? right)
        {
            return EqualityComparer<SimvarRequest>.Default.Equals(left, right);
        }

        public static bool operator !=(SimvarRequest? left, SimvarRequest? right)
        {
            return !(left == right);
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
    internal class TrackList : List<Track>
    {
        public List<Track> List()
        {
            return this;
        }
        public void AddTrack(Track _point)
        {
            if (this.Count > 0)
                if (this.Last().Equals(_point) == false) this.Add(_point);
            else
                if (_point.Zero() == false) this.Add(_point);
        }

    }
    public class Track : IEquatable<Track>
    {
        private double latitude =0;
        private double longitude=0;
        private double altitude = 0;
        private double airspeed = 0;
        private double heading = 0;
        public string Latitude {
            get
            {
                return latitude.ToString();
            }
            set
            {
                _ = double.TryParse(value, out latitude);
            }
        }
        public string Longitude
        {
            get
            {
                return longitude.ToString();
            }
            set
            {
                _ = double.TryParse(value, out longitude);
            }
        }
        public string Altitude
        {
            get
            {
                return altitude.ToString();
            }
            set
            {
                _ = double.TryParse(value, out altitude);
            }
        }
        public string Airspeed
        {
            get
            {
                return airspeed.ToString();
            }
            set
            {
                _ = double.TryParse(value, out airspeed);
            }
        }
        public string Heading
        {
            get
            {
                return heading.ToString();
            }
            set
            {
                _ = double.TryParse(value, out heading);
            }
        }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override bool Equals(object obj)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return Equals(obj as Track);
#pragma warning restore CS8604 // Possible null reference argument.
        }

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        public bool Equals(Track other)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
            return latitude == other.latitude &&
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
            return "Lat = "    + latitude +
                   " Long = "  + longitude +
                   " Alt = "   + altitude +
                   " Speed = " + airspeed +
                   " Head = "  + heading ;
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
            return  latitude == 0 && longitude == 0 && altitude == 0 && airspeed == 0 && heading == 0;
        }
    }
    public class SimvarsViewModel
    {
        #region Public
        public bool Connected { get; set; }
        #endregion

        #region Private
        private const int WM_USER_SIMCONNECT = 0x0402;
        private SimConnect m_oSimConnect;
        private readonly ObservableCollection<SimvarRequest> lSimvarRequests;
        private readonly ObservableCollection<uint> lObjectIDs;
        private string AircaftLoaded = "Unknown";
        private uint m_iCurrentDefinition = 0;
        private uint m_iCurrentRequest = 0;
        private readonly TrackList _TrackList = new() ;
        #endregion Private

        #region Private Methods
        private void ReceiveSimConnectMessage()
        {
            m_oSimConnect?.ReceiveMessage();
        }
        private bool RegisterToSimConnect(SimvarRequest _oSimvarRequest)
        {
            if (m_oSimConnect != null)
            {
                if (_oSimvarRequest.bIsString)
                {
                    m_oSimConnect.AddToDataDefinition(_oSimvarRequest.eDef, _oSimvarRequest.Parameter, "", SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    m_oSimConnect.RegisterDataDefineStruct<ResultStructure>(_oSimvarRequest.eDef);
                }
                else
                {
                    m_oSimConnect.AddToDataDefinition(_oSimvarRequest.eDef, _oSimvarRequest.Parameter, _oSimvarRequest.Measure, SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    m_oSimConnect.RegisterDataDefineStruct<double>(_oSimvarRequest.eDef);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Connected = true;

            // Register pending requests
            if (lSimvarRequests != null)
            {
                foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
                {
                    if (oSimvarRequest.bPending)
                    {
                        oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
                        oSimvarRequest.bStillPending = oSimvarRequest.bPending;
                    }
                }
            }
        }
        private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Disconnect();
        }
        private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            // throw new Exception( data.dwException.ToString() );
        }
        private void SimConnect_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_SYSTEM_STATE data)
        {
            if ((REQUESTS)data.dwRequestID == REQUESTS.AIRCRAFT_LOADED) AircaftLoaded = data.szString;
        }
        private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {

            uint iRequest = data.dwRequestID;
            uint iObject = data.dwObjectID;

            if (iObject == 1)
            {
                if (lObjectIDs != null)
                {
                    if (!lObjectIDs.Contains(iObject))
                    {
                        lObjectIDs.Add(iObject);
                    }
                }

                if (lSimvarRequests != null)
                {
                    foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
                    {

                        if (iRequest == (uint)oSimvarRequest.eRequest)
                        {
                            if (oSimvarRequest.bIsString)
                            {
                                ResultStructure result = (ResultStructure)data.dwData[0];

                                oSimvarRequest.Value = result.sValue;
                            }
                            else
                            {
                                oSimvarRequest.Value = ((double)data.dwData[0]).ToString();

                            }

                            oSimvarRequest.bPending = false;
                            oSimvarRequest.bStillPending = false;
                        }
                    }
                }
            }
        }

        #endregion

        #region Public Methods
        public void Disconnect()
        {
            if (m_oSimConnect != null)
            {
                /// Dispose serves the same purpose as SimConnect_Close()
                m_oSimConnect.Dispose();
            }

            Connected = false;

            // Set all requests as pending
            if ( lSimvarRequests != null)
            {
                foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
                {
                    oSimvarRequest.bPending = true;
                    oSimvarRequest.bStillPending = true;
                }
            }
        }
        public void Connect()
        {
            try
            {
                /// The constructor is similar to SimConnect_Open in the native API
                m_oSimConnect = new SimConnect("SimListener", (IntPtr)null, WM_USER_SIMCONNECT, null, 0);

                /// Listen to connect and quit msgs
                m_oSimConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
                m_oSimConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);

                /// Listen to exceptions
                m_oSimConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);

                /// Catch a simobject data request
                m_oSimConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(SimConnect_OnRecvSimobjectDataBytype);

                m_oSimConnect.OnRecvSystemState += new SimConnect.RecvSystemStateEventHandler(SimConnect_OnRecvEvent);
                m_oSimConnect.SubscribeToSystemEvent(EVENT.RECUR_1SEC, "1sec");
                
                AddRequest("PLANE LATITUDE" , "degrees", false);
                AddRequest("PLANE LONGITUDE", "degrees", false);
                AddRequest("AIRSPEED TRUE", "knots"  , false);
                AddRequest("PLANE ALTITUDE" , "feet"   , false);
                AddRequest("PLANE HEADING DEGREES TRUE", "degrees", false);  

                Connected = true;

            }
            catch (COMException)
            {
                Connected = false;
            }
        }
        public Dictionary<string, string> AircraftData()
        {
            Track tempTrack = new();

            ReceiveSimConnectMessage();

            Dictionary<string, string> ReturnValue = new()
            {
                { "Connected"     , Connected.ToString() },
                { "AircaftLoaded" ,  AircaftLoaded }
            };


            m_oSimConnect?.RequestSystemState( REQUESTS.AIRCRAFT_LOADED, "AircraftLoaded");

            if (lSimvarRequests != null)
            {
                foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
                {
                    if (oSimvarRequest.Parameter == "PLANE LONGITUDE") tempTrack.Longitude = oSimvarRequest.Value;
                    if (oSimvarRequest.Parameter == "PLANE LATITUDE") tempTrack.Latitude  = oSimvarRequest.Value;
                    if (oSimvarRequest.Parameter == "AIRSPEED TRUE") tempTrack.Airspeed   = oSimvarRequest.Value;
                    if (oSimvarRequest.Parameter == "PLANE ALTITUDE") tempTrack.Altitude = oSimvarRequest.Value;
                    if (oSimvarRequest.Parameter == "PLANE HEADING DEGREES TRUE") tempTrack.Heading   = oSimvarRequest.Value;

                    if (ReturnValue.ContainsKey(oSimvarRequest.Parameter) != true)
                        ReturnValue.Add(oSimvarRequest.Parameter, oSimvarRequest.Value);

                    if (!oSimvarRequest.bPending)
                    {
                        m_oSimConnect?.RequestDataOnSimObjectType(oSimvarRequest.eRequest, oSimvarRequest.eDef, 0, SIMCONNECT_SIMOBJECT_TYPE.ALL );

                        oSimvarRequest.bPending = true;
                    }
                    else
                    {
                        oSimvarRequest.bStillPending = true;
                    }

                }
            }

            _TrackList.AddTrack( tempTrack );  

            return ReturnValue;
        }
        public List<Track> TrackData()
        {
            return _TrackList.List();
        }
        public void AddRequest(string _sNewSimvarRequest, string _sNewUnitRequest, bool _bIsString)
        {
            if( Validate( _sNewSimvarRequest ) == false )
            {
                throw new Exception( string.Format( "Request {0} is not valid", _sNewSimvarRequest ));
            }
            
            SimvarRequest oSimvarRequest = new()
            {
                eDef = (DEFINITION)m_iCurrentDefinition,
                eRequest = (REQUEST)m_iCurrentRequest,
                Parameter = _sNewSimvarRequest,
                bIsString = _bIsString,
                Measure = _sNewUnitRequest 
            };

            if (lSimvarRequests.Contains<SimvarRequest>(oSimvarRequest)) return ;

            oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
            oSimvarRequest.bStillPending = oSimvarRequest.bPending;

            if( lSimvarRequests != null) lSimvarRequests.Add(oSimvarRequest);

            ++m_iCurrentDefinition;
            ++m_iCurrentRequest;
        }

        private static bool Validate( string request )
        {
            if (request == null) return false;

            if (SimVars.Names.Contains(request)) return true;

            return false;
        }
        #endregion

        #region Constructor
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SimvarsViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            lObjectIDs = new ObservableCollection<uint>
            {
                1
            };

            lSimvarRequests = new ObservableCollection<SimvarRequest>();
        }

        #endregion
    }
}