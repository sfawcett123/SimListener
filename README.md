# Flight Simulator Listener Library

## Purpose
This library provides a wrapper around the Microsoft.FlightSimulator.SimConnect library.

To use this library, you need to have the Microsoft Flight Simulator installed and the SimConnect SDK available.

You can find the SimConnect SDK in the Microsoft Flight Simulator SDK installation directory, typically located at `C:\Program Files (x86)\Microsoft Flight Simulator SDK\SimConnect SDK`.

This library simplifies the process of connecting to the simulator, subscribing to events, and receiving data updates.

The library is designed to be used in .NET applications, and it provides a simple and intuitive API for interacting with the simulator.

## Features

- Connect to Microsoft Flight Simulator using SimConnect

- Subscribe to simulator events and receive data updates
- Handle simulator events in a straightforward manner

- Access simulator data in a structured way
- supports both synchronous and asynchronous operations

## Installation 

```C
dotnet add package SimListener --version 2.0.0
```
see [NuGet](https://www.nuget.org/packages/SimListener/) for more details.

## Initialization
To use the library, you need to initialize the `SimListener` class with the appropriate parameters. Here's an example of how to do this:
```csharp
using SimListener;
using System.Threading.Tasks;

class Program
{
	static async Task Main(string[] args)
	{
		// Initialize the SimListener with the default parameters
		var simListener = new SimListener.SimListener();
		// Connect to the simulator
		await simListener.ConnectAsync();
		// Subscribe to events or data updates as needed
		simListener.OnDataReceived += (sender, data) =>
		{
			// Handle received data
			Console.WriteLine($"Received data: {data}");
		};
		// Keep the application running to receive updates
		Console.WriteLine("Press any key to exit...");
		Console.ReadKey();
		// Disconnect when done
		await simListener.DisconnectAsync();
	}
}
```
### Usage
You can use the `SimListener` class to subscribe to various simulator events and receive data updates. The library provides a simple API for accessing simulator data, such as aircraft position, speed, altitude, and more.


### Example of Subscribing to Events

```csharp

using SimListener;
using System.Threading.Tasks;

class Program
{
	static async Task Main(string[] args)
	{
		// Initialize the SimListener with the default parameters
		var simListener = new SimListener.SimListener();
		// Connect to the simulator
		await simListener.ConnectAsync();
		// Subscribe to the OnDataReceived event
		simListener.OnDataReceived += (sender, data) =>
		{
			// Handle received data
			Console.WriteLine($"Received data: {data}");
		};
		// Keep the application running to receive updates
		Console.WriteLine("Press any key to exit...");
		Console.ReadKey();
		// Disconnect when done
		await simListener.DisconnectAsync();
	}
}
```

## Logging

The library supports logging using the `Microsoft.Extensions.Logging` framework. You can configure the logging provider to log messages to various outputs, such as console, file, or other logging systems.

## Contributing

If you would like to contribute to this library, please feel free to submit a pull request or open an issue on the GitHub repository. Contributions are welcome and appreciated!

## License

This library is licensed under the MIT License. See the LICENSE file for more details.

## Contact

If you have any questions or need assistance, please feel free to reach out via the GitHub repository or contact the author directly.
