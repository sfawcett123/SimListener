# Flight Simulator Listener Library

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=sfawcett123_SimListener&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=sfawcett123_SimListener)

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