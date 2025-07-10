using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

using Microsoft.FlightSimulator.SimConnect;
using System.Collections.ObjectModel;
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
        private bool disposed = false;

        /// <summary>
        /// Releases all resources used by the Connect instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources and optionally releases managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (lSimvarRequests != null)
                    {
                        foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
                        {
                            oSimvarRequest.bPending = true;
                            oSimvarRequest.bStillPending = true;
                        }
                    }

                    try { m_oSimConnect?.Dispose(); } catch (Exception ex) { logger?.LogError(ex, "Error disposing SimConnect."); }
                    try { timer?.Dispose(); } catch (Exception ex) { logger?.LogError(ex, "Error disposing timer."); }
                }

                m_oSimConnect = null;
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Connect"/> class.
        /// Releases unmanaged resources before the object is reclaimed by garbage collection.
        /// </summary>
        ~Connect()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets a value indicating whether the connection to the simulator is established.
        /// </summary>
        public bool isConnected
        {
            get
            {
                return m_oSimConnect is not null;
            }
        }
        #endregion

        #region Private variables
        private static EventLogSettings myEventLogSettings = new EventLogSettings
        {
            SourceName = _sourceName,
            LogName = _logName
        };
        private const string _sourceName = "Simulator Service";
        private const string _logName = "Application";
        private SimConnect? m_oSimConnect = null;
        private ObservableCollection<SimvarRequest> lSimvarRequests = new();
        private ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.AddEventLog(myEventLogSettings);
        });
        private ILogger? logger = null;
        private System.Timers.Timer? timer;
        private string AircaftLoaded = UnknownAircraft;
        private uint m_iCurrentDefinition = 0;
        private uint m_iCurrentRequest = 0;
        private IntPtr hWnd = IntPtr.Zero;
        private readonly object simvarRequestsLock = new();
        #endregion

        #region Private Methods
        private void ConnectToSim()
        {
            if (m_oSimConnect is null)
            {
                try
                {
                    m_oSimConnect = new SimConnect("SimListener", hWnd, WM_USER_SIMCONNECT, null, 0);

                    if (m_oSimConnect is not null)
                    {
                        logger?.LogInformation("SimConnect connection established.");
                        m_oSimConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
                        m_oSimConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);
                        m_oSimConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);
                        m_oSimConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(SimConnect_OnRecvSimobjectDataBytype);
                        m_oSimConnect.OnRecvSystemState += new SimConnect.RecvSystemStateEventHandler(SimConnect_OnRecvEvent);

                        try
                        {
                            m_oSimConnect.SubscribeToSystemEvent(Event.RECUR_1SEC, "1sec");
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError(ex, "Failed to subscribe to system event.");
                        }
                        SimConnected?.Invoke(this, EventArgs.Empty);
                    }
                }
                catch (COMException)
                {
                    logger?.LogError("SimConnect connection failed. Is MSFS running?");
                    m_oSimConnect = null;
                    SimDisconnected?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                    logger?.LogError("Unexpected error during SimConnect connection.");
                    m_oSimConnect = null;
                    SimDisconnected?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        private void InternalAddRequest(string _sNewSimvarRequest, string _sNewUnitRequest, bool _bIsString)
        {
            logger?.LogInformation($"AddRequest {_sNewSimvarRequest} ");


            if (!ValidateRequest(_sNewSimvarRequest))
            {
                 logger?.LogError($"Invalid request: {_sNewSimvarRequest}. Skipping.");
                 throw new InvalidSimDataRequestException($"Invalid request: {_sNewSimvarRequest}. Skipping.");
            }

            if (m_oSimConnect is null)
            {
                logger?.LogDebug("SimConnect is not connected. Cannot add request.");
                throw new SimulatorNotConnectedException("SimConnect is not connected. Cannot add request.");
            }
            if (lSimvarRequests is null)
            {
                lSimvarRequests = new ObservableCollection<SimvarRequest>();
            }
            if (m_iCurrentDefinition >= (uint)DEFINITION.MAX_DEFINITIONS || m_iCurrentRequest >= (uint)REQUEST.MAX_REQUESTS)
            {
                logger?.LogError("Maximum definitions or requests reached. Cannot add more.");
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

            try
            {
                oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
                oSimvarRequest.bStillPending = oSimvarRequest.bPending;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Failed to register SimvarRequest: {_sNewSimvarRequest}");
                oSimvarRequest.bPending = true;
                oSimvarRequest.bStillPending = true;
            }

            lSimvarRequests.Add(oSimvarRequest);

            lSimvarRequests.Add(oSimvarRequest);

            ++m_iCurrentDefinition;
            ++m_iCurrentRequest;
            logger?.LogDebug($"Request {_sNewSimvarRequest} added with Definition ID: {oSimvarRequest.eDef} and Request ID: {oSimvarRequest.eRequest}");
        }
        private void ReceiveSimConnectMessage()
        {
            try
            {
                m_oSimConnect?.ReceiveMessage();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error receiving SimConnect message.");
                m_oSimConnect = null;
                SimDisconnected?.Invoke(this, EventArgs.Empty);
            }
        }
        private bool RegisterToSimConnect(SimvarRequest _oSimvarRequest)
        {
            logger?.LogDebug($"Registering Request {_oSimvarRequest.sName}");

            if (m_oSimConnect != null)
            {
                try
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
                catch (Exception ex)
                {
                    logger?.LogError(ex, $"Failed to register data definition for {_oSimvarRequest.sName}");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            logger?.LogDebug("SimConnect_OnRecvOpen");
            if (lSimvarRequests is null)
            {
                return;
            }

            foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            {
                if (oSimvarRequest.bPending)
                {
                    try
                    {
                        oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
                        oSimvarRequest.bStillPending = oSimvarRequest.bPending;
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, $"Failed to re-register SimvarRequest: {oSimvarRequest.sName}");
                        oSimvarRequest.bPending = true;
                        oSimvarRequest.bStillPending = true;
                    }
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
            try
            {
                if ((Requests)data.dwRequestID == Requests.AIRCRAFT_LOADED)
                {
                    AircaftLoaded = data.szString;
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error processing SimConnect_OnRecvEvent.");
            }
        }
        

        // Replace the method with thread-safe access
        private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            logger?.LogDebug($"Received SimObject Data for Request ID: {data.dwRequestID}");
            List<Dictionary<string, string>> AircraftData = new List<Dictionary<string, string>>();

            uint iRequest = data.dwRequestID;
            lock (simvarRequestsLock)
            {
                if (lSimvarRequests != null)
                {
                    foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
                    {
                        if (iRequest == (uint)oSimvarRequest.eRequest)
                        {
                            logger?.LogDebug($"Processing request: {oSimvarRequest.sName} with ID: {iRequest} == {(uint)oSimvarRequest.eRequest}");
                            if (string.IsNullOrEmpty(oSimvarRequest.sName))
                            {
                                logger?.LogError($"Request {iRequest} has no name. Skipping.");
                                continue;
                            }

                            try
                            {
                                if (oSimvarRequest.bIsString)
                                {
                                    ResultStructure result = (ResultStructure)data.dwData[0];
                                    oSimvarRequest.dValue = 0;
                                    oSimvarRequest.sValue = result.sValue;
                                    AircraftData.Add(new Dictionary<string, string> { { oSimvarRequest.sName, oSimvarRequest.sValue ?? string.Empty } });
                                    logger?.LogDebug($"Received string value: {oSimvarRequest.sValue} for request: {oSimvarRequest.sName}");
                                }
                                else
                                {
                                    double dValue = (double)data.dwData[0];
                                    oSimvarRequest.dValue = dValue;
                                    oSimvarRequest.sValue = dValue.ToString("F9");
                                    AircraftData.Add(new Dictionary<string, string> { { oSimvarRequest.sName, oSimvarRequest.dValue.ToString() } });
                                    logger?.LogDebug($"Received double value: {oSimvarRequest.dValue} for request: {oSimvarRequest.sName}");
                                }
                            }
                            catch (Exception ex)
                            {
                                logger?.LogError(ex, $"Error processing SimobjectData for {oSimvarRequest.sName}");
                            }

                            oSimvarRequest.bPending = false;
                            oSimvarRequest.bStillPending = false;
                        }
                    }
                }
            }
            try
            {
                SimDataRecieved?.Invoke(this, new SimulatorData() { TimeReached = DateTime.Now, AircraftData = AircraftData });
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error invoking SimDataRecieved event.");
            }
        }
        /// <summary>
        /// Validates a Simvar request string to ensure it is properly formatted and exists in the list of known SimVars.
        /// </summary>
        /// <param name="request">The Simvar request string to validate.</param>
        /// <returns>
        /// True if the request is valid and exists in the list of known SimVars; otherwise, false.
        /// </returns>
        public static bool ValidateRequest(string request)
        {
            Console.WriteLine($"Validating request: {request}");

            string trimmedRequest;
            string trimmedIndex;

            if ( request?.Split(":").Length < 2 )
            {
                // If no index is provided, default to "0"
                trimmedRequest = request ?? "";
                trimmedIndex = "1";
            }
            else
            {
                trimmedRequest = request?.Split(":")[0] ?? "";
                trimmedIndex = request?.Split(":")[1] ?? "1";
            }
            Console.WriteLine($"Checking index : {trimmedIndex} is an integer between 1 and 10");

            if (!int.TryParse(trimmedIndex, out int index))
            {
                Console.WriteLine($"Non integer validating request index: {trimmedIndex}");
                return false;
            }

            if (index < 1 || index > 10)
            {
                Console.WriteLine($"Integer value out of bounds in request index: {index}");
                return false;
            }

            Console.WriteLine($"Checking request: {trimmedRequest} is in SimVar List");
            return !string.IsNullOrWhiteSpace(request) && SimVars.Names.Contains(trimmedRequest);
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
                    { "isConnected", isConnected.ToString() },
                    { "AircaftLoaded", AircaftLoaded ?? UnknownAircraft }
                };
            try
            {
                m_oSimConnect?.RequestSystemState(Requests.AIRCRAFT_LOADED, "AircraftLoaded");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error requesting system state.");
            }

            if (lSimvarRequests != null)
            {
                foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
                {
                    if (!oSimvarRequest.bPending)
                    {
                        try
                        {
                            m_oSimConnect?.RequestDataOnSimObjectType(oSimvarRequest.eRequest, oSimvarRequest.eDef, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
                            oSimvarRequest.bPending = true;
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError(ex, $"Error requesting data for {oSimvarRequest.sName}");
                        }
                    }
                }
            }
            return ReturnValue;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the timer is enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return timer?.Enabled ?? false;
            }
            set
            {
                if (timer != null)
                {
                    timer.Enabled = value;
                }
            }
        }
        /// <summary>
        /// Adds a new Simvar request to the SimConnect connection.
        /// </summary>
        /// <param name="_sNewSimvarRequest">The name of the Simvar to request.</param>
        public void AddRequest(string _sNewSimvarRequest)
        {
            try
            {
                InternalAddRequest(_sNewSimvarRequest, "", false);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Error adding request: {_sNewSimvarRequest}");
                throw; // Fixes CA2200 by re-throwing the exception without altering the stack trace.
            }
        }
        /// <summary>
        /// Adds multiple Simvar requests to the SimConnect connection.
        /// </summary>
        /// <param name="Outputs">A list of Simvar names to request.</param>
        public void AddRequests(List<string> Outputs)
        {
            if (Outputs is not null && Outputs.Any())
            {
                foreach (string output in Outputs)
                {
                    try
                    {
                        AddRequest(output);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, $"Error adding request: {output}");
                        throw;
                    }
                }
            }
        }

        #endregion

        #region Constructor
        /// <summary>Initializes a new instance of the <see cref="T:SimListener.Connect" /> class.</summary>
        public Connect()
        {
            this.Initialise(IntPtr.Zero, DefaultTimerIntervalMs);
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
        /// <summary>
        /// Initializes a new instance of the <see cref="Connect"/> class with a specified window handle, timer interval, and logger factory.
        /// </summary>
        /// <param name="hWnd">The handle to the window that will receive SimConnect messages.</param>
        /// <param name="time">The interval, in milliseconds, for the timer used to attempt connection.</param>
        /// <param name="loggerFactory">The logger factory to create loggers.</param>
        public Connect(IntPtr hWnd, int time, ILoggerFactory loggerFactory)
        {
            this.factory = loggerFactory ?? LoggerFactory.Create(builder => builder.AddConsole());
            this.Initialise(hWnd, time);
        }
        private void Initialise(IntPtr hWnd, int time)
        {
            try
            {
                this.logger = factory.CreateLogger("Flight Simulator Listener");
                logger?.LogInformation("Connect initialized with hWnd: {hWnd} and timer interval: {time} ms", hWnd, time);

                if (hWnd != IntPtr.Zero)
                {
                    this.hWnd = hWnd;
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
                logger?.LogDebug($"Connect initialized with timer interval: {time} ms");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during Connect initialization.");
            }
        }
        #endregion

        #region Event Handlers
        private void TimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (!isConnected)
                {
                    logger?.LogDebug("Attempting connection...");
                    this.ConnectToSim();
                }
                else
                {

                    Dictionary<string, string> data = AircraftData();
                    if (data.Count > 0)
                    {
                        logger?.LogDebug("SimConnect is connected. Stopping timer");
                        //timer.Stop();
                        foreach (var item in data)
                        {
                            logger?.LogDebug($"Returned Data: {item.Key} = {item.Value}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error in TimerElapsed handler.");
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
        /// Invoked when the simulator is successfully connected.  
        /// Triggers the <see cref="SimConnected"/> event.  
        /// </summary>  
        protected virtual void OnSimConnected()
        {
            try
            {
                SimConnected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {

                logger?.LogError(ex, "Error invoking SimConnected event.");
            }
        }
        /// <summary>
        /// Invoked when the simulator is disconnected.
        /// Restarts the timer and triggers the SimDisconnected event.
        /// </summary>
        protected virtual void OnSimDisconnected()
        {
            try
            {
                if (timer != null)
                {
                    logger?.LogInformation("Simulator Disconnected Restarting Timer.");
                    timer.Enabled = true;
                    timer.Start();
                }
                SimDisconnected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error invoking SimDisconnected event.");
            }
        }
        /// <summary>  
        /// Invoked when simulator data is received.  
        /// Triggers the <see cref="SimDataRecieved"/> event with the provided simulator data.  
        /// </summary>  
        /// <param name="e">The simulator data received from the simulator.</param>  
        protected virtual void OnSimDataRecieved(SimulatorData e)
        {
            try
            {
                SimDataRecieved?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error invoking SimDataRecieved event.");
            }
        }
        /// <summary>
        /// Returns a list of all current SimvarRequests.
        /// </summary>
        /// <returns>A list of SimvarRequest objects.</returns>
        public List<string> listRequests()
        {
            lock (simvarRequestsLock)
            {
                return lSimvarRequests?
                    .Where(r => !string.IsNullOrEmpty(r.sName))
                    .Select(r => r.sName!)
                    .ToList() ?? new List<string>();
            }
        }

        #endregion


    }
}