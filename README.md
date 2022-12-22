# Flight Simulator Listener Library
## Purpose
This library provides a wrapper around the Microsoft.FlightSimulator.SimConnect library.

It provides the following methods:

- ConnecttoSim - This method will connect to Microsoft Flight Simulator (MFS).
- AddRequest - Adding data requests to MFS
- AircaftData - Returning requested data from MFS
- TrackData - Returning historic data from MFS
- Disconnect - Disconnecting from MFS
- Validate - Validating a data request value is valid

## Installation 

```C
dotnet add package SimListener --version 1.0.0
```