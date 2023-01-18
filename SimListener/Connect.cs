using Microsoft.FlightSimulator.SimConnect;

using System.Collections.ObjectModel;
using System.Runtime.InteropServices;


namespace SimListener
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ResultStructure
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string sValue;
    };

    public class Connect
    {
        #region Public
        public bool Connected => m_oSimConnect is not null;

        #endregion

        #region Private
        private const int WM_USER_SIMCONNECT = 0x0402;
        private SimConnect? m_oSimConnect;
        private readonly ObservableCollection<SimListener> lSimvarRequests;
        private readonly ObservableCollection<uint> lObjectIDs;
        private string AircaftLoaded = "Unknown";
        private uint m_iCurrentDefinition = 0;
        private uint m_iCurrentRequest = 0;
        private readonly TrackList _TrackList = new();
        #endregion Private

        #region Private Methods
        private void ReceiveSimConnectMessage()
        {
            m_oSimConnect?.ReceiveMessage();
        }

        private bool RegisterToSimConnect(SimListener _oSimvarRequest)
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
            if (sender is null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            _ = AddRequest("PLANE LATITUDE", "degrees", false);
            _ = AddRequest("PLANE LONGITUDE", "degrees", false);
            _ = AddRequest("AIRSPEED TRUE", "knots", false);
            _ = AddRequest("PLANE ALTITUDE", "feet", false);
            _ = AddRequest("PLANE HEADING DEGREES TRUE", "degrees", false);


            // Register pending requests
            if (lSimvarRequests != null)
            {
                foreach (SimListener oSimvarRequest in lSimvarRequests)
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
            m_oSimConnect?.Dispose();
        }
        private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            // throw new Exception( data.dwException.ToString() );
        }
        private void SimConnect_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_SYSTEM_STATE data)
        {
            if ((Requests)data.dwRequestID == Requests.AIRCRAFT_LOADED)
            {
                AircaftLoaded = data.szString;
            }
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
                    foreach (SimListener oSimvarRequest in lSimvarRequests)
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
            m_oSimConnect?.Dispose();

            // Set all requests as pending
            if (lSimvarRequests != null)
            {
                foreach (SimListener oSimvarRequest in lSimvarRequests)
                {
                    oSimvarRequest.bPending = true;
                    oSimvarRequest.bStillPending = true;
                }
            }
        }
        public void ConnectToSim()
        {
            if (m_oSimConnect is null)
            {
                try
                {
                    /// The constructor is similar to SimConnect_Open in the native API
                    m_oSimConnect = new SimConnect("SimListener", (IntPtr)null, WM_USER_SIMCONNECT, null, 0);

                    if (m_oSimConnect is not null)
                    {
                        /// Listen to connect and quit msgs
                        m_oSimConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
                        m_oSimConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);

                        /// Listen to exceptions
                        m_oSimConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);

                        /// Catch a simobject data request
                        m_oSimConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(SimConnect_OnRecvSimobjectDataBytype);

                        m_oSimConnect.OnRecvSystemState += new SimConnect.RecvSystemStateEventHandler(SimConnect_OnRecvEvent);
                        m_oSimConnect.SubscribeToSystemEvent(Event.RECUR_1SEC, "1sec");
                    }

                }
                catch (COMException)
                {

                }
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

            m_oSimConnect?.RequestSystemState(Requests.AIRCRAFT_LOADED, "AircraftLoaded");

            if (lSimvarRequests != null)
            {
                foreach (SimListener oSimvarRequest in lSimvarRequests)
                {
                    if (oSimvarRequest.Value is null)
                        continue;

                    if (oSimvarRequest.Parameter == "PLANE LONGITUDE")
                    {
                        tempTrack.Longitude = oSimvarRequest.Value;
                    }

                    if (oSimvarRequest.Parameter == "PLANE LATITUDE")
                    {
                        tempTrack.Latitude = oSimvarRequest.Value;
                    }

                    if (oSimvarRequest.Parameter == "AIRSPEED TRUE")
                    {
                        tempTrack.Airspeed = oSimvarRequest.Value;
                    }

                    if (oSimvarRequest.Parameter == "PLANE ALTITUDE")
                    {
                        tempTrack.Altitude = oSimvarRequest.Value;
                    }

                    if (oSimvarRequest.Parameter == "PLANE HEADING DEGREES TRUE")
                    {
                        tempTrack.Heading = oSimvarRequest.Value;
                    }

                    if (oSimvarRequest.Parameter is not null)
                    {
                        if (!ReturnValue.ContainsKey(oSimvarRequest.Parameter))
                        {
                            ReturnValue.Add(oSimvarRequest.Parameter, "");
                        }

                        if (oSimvarRequest.Value is not null)
                        {
                            ReturnValue[oSimvarRequest.Parameter] = oSimvarRequest.Value;
                        }
                    }

                    if (!oSimvarRequest.bPending)
                    {
                        m_oSimConnect?.RequestDataOnSimObjectType(oSimvarRequest.eRequest, oSimvarRequest.eDef, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);

                        oSimvarRequest.bPending = true;
                    }
                    else
                    {
                        oSimvarRequest.bStillPending = true;
                    }

                }
            }

            _TrackList.AddTrack(tempTrack);

            return ReturnValue;
        }
        public List<Track> TrackData()
        {
            return _TrackList.List();
        }
        public string AddRequests(List<string> Outputs)
        {
            if (Outputs is not null)
            {
                if ( Outputs.Any())
                {
                    foreach (string output in Outputs)
                    {
                        var rval = AddRequest(output);
                        if( rval != ErrorCodes.OK )
                        {
                            return output;
                        }
                    }
                }
            }
            return "OK";
        }
        public ErrorCodes AddRequest(string _sNewSimvarRequest)
        {
            return AddRequest(_sNewSimvarRequest, "", true);
        }

        public int Count()
        {
            return lSimvarRequests.Count;
        }
        public ErrorCodes AddRequest(string _sNewSimvarRequest, string _sNewUnitRequest, bool _bIsString)
        {
            
            if (Validate(_sNewSimvarRequest) == false)
            {
                return ErrorCodes.INVALID_DATA_REQUEST;
            }

            SimListener oSimvarRequest = new()
            {
                eDef = (Definition)m_iCurrentDefinition,
                eRequest = (Request)m_iCurrentRequest,
                Parameter = _sNewSimvarRequest,
                bIsString = _bIsString,
                Measure = _sNewUnitRequest
            };

            if (lSimvarRequests.Contains<SimListener>(oSimvarRequest))
            {
                return ErrorCodes.OK;
            }

            oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
            oSimvarRequest.bStillPending = oSimvarRequest.bPending;

            lSimvarRequests?.Add(oSimvarRequest);

            ++m_iCurrentDefinition;
            ++m_iCurrentRequest;

            return ErrorCodes.OK;
        }
        private static bool Validate(string request)
        {
            return request != null && SimVars.Names.Contains(request);
        }
        #endregion

        #region Constructor
        public Connect()
        {
            lObjectIDs = new ObservableCollection<uint>
            {
                1
            };

            lSimvarRequests = new ObservableCollection<SimListener>();
        }

        #endregion
    }
}