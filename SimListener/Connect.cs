using Microsoft.FlightSimulator.SimConnect;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SimListener 
{
    /// <summary>
    /// Represents a connection to the Microsoft Flight Simulator using SimConnect.
    /// Provides methods to manage simulator data requests and handle connection events.
    /// </summary>
    public partial class Connect : IDisposable
    {
        #region Dispose and Connected Properties
        /// <summary>
        /// Releases all resources used by the Connect instance.
        /// </summary>
        public void Dispose()
        {
            if (lSimvarRequests != null)
            {
                foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
                {
                    oSimvarRequest.bPending = true;
                    oSimvarRequest.bStillPending = true;
                }
            }

            m_oSimConnect?.Dispose();
            m_oSimConnect = null;
        }
        /// <summary>
        /// Gets a value indicating whether the connection to the simulator is established.
        /// </summary>
        public bool Connected
        {
            get
            {
                if (m_oSimConnect is not null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region Private variables
        private SimConnect? m_oSimConnect = null;
        private ObservableCollection<SimvarRequest>? lSimvarRequests;
        System.Timers.Timer? timer;
        private string AircaftLoaded = "Unknown";
        private uint m_iCurrentDefinition = 0;
        private uint m_iCurrentRequest = 0;
        private IntPtr hWnd = IntPtr.Zero;
        #endregion

        #region Private Methods
        private void ConnectToSim()
        {
            if (m_oSimConnect is null)
            {
                try
                {
                    // The constructor is similar to SimConnect_Open in the native API  
                    m_oSimConnect = new SimConnect("SimListener", hWnd, WM_USER_SIMCONNECT, null, 0);

                    if (m_oSimConnect is not null)
                    {
                        Debug.WriteLine("SimConnect connection established.");
                        // Listen to connect and quit msgs  
                        m_oSimConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
                        m_oSimConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);
                        m_oSimConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);
                        m_oSimConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(SimConnect_OnRecvSimobjectDataBytype);
                        m_oSimConnect.OnRecvSystemState += new SimConnect.RecvSystemStateEventHandler(SimConnect_OnRecvEvent);

                        m_oSimConnect.SubscribeToSystemEvent(Event.RECUR_1SEC, "1sec");
                        SimConnected?.Invoke(this, EventArgs.Empty);
                    }
                }
                catch (COMException)
                {
                    Debug.WriteLine("SimConnect connection failed. Is MSFS running?");
                    m_oSimConnect = null;
                    SimDisconnected?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
        }
        private void InternalAddRequest(string _sNewSimvarRequest, string _sNewUnitRequest, bool _bIsString)
        {
            Debug.WriteLine($"AddRequest {_sNewSimvarRequest} ");

            if (!ValidateRequest(_sNewSimvarRequest))
            {
                Debug.WriteLine($"Invalid request: {_sNewSimvarRequest}. Skipping.");
                return;
            }
            if (m_oSimConnect is null)
            {
                Debug.WriteLine("SimConnect is not connected. Cannot add request.");
                return;
            }
            if (lSimvarRequests is null)
            {
                lSimvarRequests = new ObservableCollection<SimvarRequest>();
            }
            if (m_iCurrentDefinition >= (uint)DEFINITION.MAX_DEFINITIONS || m_iCurrentRequest >= (uint)REQUEST.MAX_REQUESTS)
            {
                Debug.WriteLine("Maximum definitions or requests reached. Cannot add more.");
                return;
            }

            SimvarRequest oSimvarRequest = new SimvarRequest
            {
                eDef = (DEFINITION)m_iCurrentDefinition,
                eRequest = (REQUEST)m_iCurrentRequest,
                sName = _sNewSimvarRequest,
                bIsString = _bIsString,
                sUnits = _bIsString ? null : _sNewUnitRequest
            };

            oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
            oSimvarRequest.bStillPending = oSimvarRequest.bPending;

            if (lSimvarRequests is not null)
            {
                lSimvarRequests.Add(oSimvarRequest);
            }

            ++m_iCurrentDefinition;
            ++m_iCurrentRequest;
            Debug.WriteLine($"Request {_sNewSimvarRequest} added with Definition ID: {oSimvarRequest.eDef} and Request ID: {oSimvarRequest.eRequest}");
        }
        private void ReceiveSimConnectMessage()
        {
            try
            {
                m_oSimConnect?.ReceiveMessage();
            }
            catch { 
                m_oSimConnect= null;
                SimDisconnected?.Invoke(this, EventArgs.Empty);
            }
            
        }
        private bool RegisterToSimConnect(SimvarRequest _oSimvarRequest)
        {
            Debug.WriteLine($"Registering Request {_oSimvarRequest.sName}");

            if (m_oSimConnect != null)
            {
                if (_oSimvarRequest.bIsString)
                {
                    m_oSimConnect.AddToDataDefinition(_oSimvarRequest.eDef, _oSimvarRequest.sName, "", SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    m_oSimConnect.RegisterDataDefineStruct<SimvarRequest>(_oSimvarRequest.eDef);
                }
                else
                {
                    m_oSimConnect.AddToDataDefinition(_oSimvarRequest.eDef, _oSimvarRequest.sName, _oSimvarRequest.sUnits, SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
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
            Debug.WriteLine("SimConnect_OnRecvOpen");
            if( lSimvarRequests is null)
            {
                return;
            }
              
            // Register pending requests
            foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            {
                if (oSimvarRequest.bPending)
                {
                    oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
                    oSimvarRequest.bStillPending = oSimvarRequest.bPending;
                }
            }
        }
        private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            SimDisconnected?.Invoke(this, EventArgs.Empty);
            m_oSimConnect = null;
        }
        private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SimDisconnected?.Invoke(this, EventArgs.Empty);
            m_oSimConnect = null;
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
            Debug.WriteLine($"Received SimObject Data for Request ID: {data.dwRequestID}");
            List<Dictionary<string, string>> AircraftData = new List<Dictionary<string, string>>(); 

            uint iRequest = data.dwRequestID;
            if (lSimvarRequests != null)
            {
                foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
                {           
                    if (iRequest == (uint)oSimvarRequest.eRequest )
                    {
                        Debug.WriteLine($"Processing request: {oSimvarRequest.sName} with ID: {iRequest} == {(uint)oSimvarRequest.eRequest}");
                        if(oSimvarRequest.sName is null)
                        {
                            Debug.WriteLine($"Request {iRequest} has no name. Skipping.");
                            continue;
                        }

                        if (oSimvarRequest.bIsString)
                        {
                            ResultStructure result = (ResultStructure)data.dwData[0];
                            oSimvarRequest.dValue = 0;
                            oSimvarRequest.sValue = result.sValue;
                            AircraftData.Add(new Dictionary<string, string> { { oSimvarRequest.sName, oSimvarRequest.sValue } });
                            Debug.WriteLine($"Received string value: {oSimvarRequest.sValue} for request: {oSimvarRequest.sName}");
                        }
                        else
                        {
                            double dValue = (double)data.dwData[0];           
                            oSimvarRequest.dValue = dValue;
                            oSimvarRequest.sValue = dValue.ToString("F9");
                            AircraftData.Add(new Dictionary<string, string> { { oSimvarRequest.sName, oSimvarRequest.dValue.ToString() } });
                            Debug.WriteLine($"Received double value: {oSimvarRequest.dValue} for request: {oSimvarRequest.sName}");
                        }

                        oSimvarRequest.bPending = false;
                        oSimvarRequest.bStillPending = false;
                    }
                }

            }
            SimDataRecieved?.Invoke(this, new SimulatorData() { TimeReached = DateTime.Now, AircraftData = AircraftData });
        }
        private static bool ValidateRequest(string request)
        {
            return request != null && SimVars.Names.Contains(request);
        }

        #endregion

        #region Public Methods
        /// <summary>  
        /// Retrieves the current aircraft data and connection status from the simulator.  
        /// </summary>  
        /// <returns>A dictionary containing the connection status and aircraft data.</returns>  
        public Dictionary<string, string> AircraftData()
        {
            ReceiveSimConnectMessage();

            Dictionary<string, string> ReturnValue = new()
            { 
                { "Connected"     , Connected.ToString() },
                { "AircaftLoaded" , AircaftLoaded }
            };
            m_oSimConnect?.RequestSystemState(Requests.AIRCRAFT_LOADED, "AircraftLoaded");

            if (lSimvarRequests != null)
            {
                foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
                {
                    if (!oSimvarRequest.bPending)
                    {
                        m_oSimConnect?.RequestDataOnSimObjectType(oSimvarRequest.eRequest, oSimvarRequest.eDef, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
                        oSimvarRequest.bPending = true;
                    }
                }
            }
            return ReturnValue;
        }

        /// <summary>
        /// Adds a new Simvar request to the SimConnect connection.
        /// </summary>
        /// <param name="_sNewSimvarRequest">The name of the Simvar to request.</param>
        public void AddRequest(string _sNewSimvarRequest)
        {
            InternalAddRequest(_sNewSimvarRequest, "", false);
        }
        /// <summary>
        /// Adds multiple Simvar requests to the SimConnect connection.
        /// </summary>
        /// <param name="Outputs">A list of Simvar names to request.</param>
        public void AddRequests(List<string> Outputs)
        {
            if (Outputs is not null)
            {
                if (Outputs.Any())
                {
                    foreach (string output in Outputs)
                    {
                        AddRequest(output);
                    }
                }
            }
        }

        #endregion

        #region Constructor
        /// <summary>Initializes a new instance of the <see cref="T:SimListener.Connect" /> class.</summary>
        public Connect()
        {
            this.Initialise(IntPtr.Zero , 1000);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Connect"/> class with a specified window handle and timer interval.
        /// </summary>
        /// <param name="hWnd">The handle to the window that will receive SimConnect messages.</param>
        /// <param name="time">The interval, in milliseconds, for the timer used to attempt connection.</param>
        public Connect(IntPtr hWnd, int time)
        {
            this.Initialise(hWnd, time);
        }
        private void Initialise(IntPtr hWnd , int time )
        {
            if (hWnd != IntPtr.Zero)
            {
                this.hWnd = hWnd ;
                Debug.WriteLine($"Connect initialized with hWnd: {hWnd}");
            }

            lSimvarRequests = new ObservableCollection<SimvarRequest>();
            timer = new System.Timers.Timer()
            {
                Interval = time, 
                Enabled = true,     
                AutoReset = true
            };
  
            timer.Elapsed += TimerElapsed;
            timer.Start();
            Debug.WriteLine($"Connect initialized with timer interval: {time} ms");
        }
        #endregion

        #region Event Handlers
        private void TimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if( !Connected)
            {
                Debug.WriteLine("Attempting connection...");
                this.ConnectToSim();

            }
            else
            {      
                Dictionary<string, string> data = AircraftData();
                if (data.Count > 0)
                {
                    Debug.WriteLine("SimConnect is connected. Stopping timer");
                    //timer.Stop();
                    foreach (var item in data)
                    {
                        Debug.WriteLine($"Returned Data: {item.Key} = {item.Value}");
                    }
                }               
            }
        }

        /// <summary>
        /// Event triggered when the simulator is successfully connected.
        /// </summary>
        public event EventHandler? SimConnected;
        /// <summary>
        /// Event triggered when the simulator is disconnected.
        /// </summary>
        public event EventHandler? SimDisconnected;
        /// <summary>
        /// Event triggered when simulator data is received.
        /// </summary>
        public event EventHandler<SimulatorData>? SimDataRecieved;
        /// <summary>
        /// Event triggered when the simulator is successfully connected.
        /// </summary>

        protected virtual void OnSimConnected()
        {
            EventHandler? handler = SimConnected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// Event triggered when the simulator is disconnected.
        /// </summary>
        protected virtual void OnSimDisconnected()
        {
            if (timer != null)
            {
                Debug.WriteLine("Simulator Disconnected Restarting Timer.");
                timer.Enabled = true;
                timer.Start();
            }

            EventHandler? handler = SimDisconnected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// Event triggered when simulator data is received.
        /// </summary>
        /// <param name="e">The simulator data containing aircraft information and timestamp.</param>
        protected virtual void OnSimDataRecieved(SimulatorData e)
        {
            EventHandler<SimulatorData>? handler = SimDataRecieved;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion
    }
}