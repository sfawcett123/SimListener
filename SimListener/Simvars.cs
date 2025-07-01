using Microsoft.FlightSimulator.SimConnect;
using System.Diagnostics;
using YamlDotNet.Serialization;

namespace SimListener
{
    /// <summary>  
    /// Represents the simulation variables loaded from a YAML file.  
    /// </summary>  
    public class SimVars
    {
        /// <summary>  
        /// Initializes a new instance of the <see cref="SimVars"/> class.  
        /// Loads simulation variables from a YAML file and validates the data.  
        /// </summary>  
        /// <exception cref="ArgumentException">Thrown when the file path is null or empty.</exception>  
        /// <exception cref="FileNotFoundException">Thrown when the specified YAML file does not exist.</exception>  
        /// <exception cref="InvalidOperationException">Thrown when no measures are found in the YAML file.</exception>  
        public SimVars()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "measures.yaml");
            Debug.WriteLine($"Loading SimVars from: {filePath}");


            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified YAML file does not exist.", filePath);
            }
            // Load the YAML file  
            string yaml = File.ReadAllText(filePath);

            Data = Deserializer(yaml);

            if (Data == null || Data.Measures == null || Data.Measures.Count == 0)
            {
                throw new InvalidOperationException("No measures found in the YAML file.");
            }

        }

        /// <summary>  
        /// Gets the data definition containing simulation measures.  
        /// </summary>  
        public DataDefinition Data { get; set; } = new DataDefinition();

        /// <summary>
        /// Retrieves the name of the measure at the specified index.
        /// </summary>
        /// <param name="index">The index of the measure to retrieve.</param>
        /// <returns>The name of the measure at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
        public string GetMeasureName(int index)
        {
            if (index < 0 || index >= Data.Measures.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            return Data.Measures[index].Name;
        }


        /// <summary>  
        /// Finds the index of a measure by its name.  
        /// </summary>  
        /// <param name="measureName">The name of the measure to find.</param>  
        /// <returns>The index of the measure if found; otherwise, -1.</returns>  
        /// <exception cref="ArgumentException">Thrown when the measure name is null or empty.</exception>  
        public int FindMeasureIndex(string measureName)
        {
            if (string.IsNullOrEmpty(measureName))
            {
                throw new ArgumentException("Measure name cannot be null or empty.", nameof(measureName));
            }

            for (int i = 0; i < Data.Measures.Count; i++)
            {
                if (Data.Measures[i].Name.Equals(measureName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1; // Not found
        }

        /// <summary>  
        /// Represents the data definition containing a list of measures.  
        /// </summary>  
        public class DataDefinition
        {
            /// <summary>  
            /// Gets or sets the list of measures.  
            /// </summary>  
            public List<Measures> Measures { get; set; } = new List<Measures>();
        }

        /// <summary>  
        /// Represents a measure with simulation variables.  
        /// </summary>  
        public class Measures
        {
            /// <summary>  
            /// Gets or sets the simulation variable name.  
            /// </summary>  
            public string Name { get; set; } = "";
            /// <summary>  
            /// Gets or sets the simulation variable data type.  
            /// </summary>  
            public SIMCONNECT_DATATYPE DataType { get; set; } = SIMCONNECT_DATATYPE.FLOAT32;
        }

        /// <summary>  
        /// Deserializes the YAML data into a <see cref="DataDefinition"/> object.  
        /// </summary>  
        /// <param name="Data">The YAML data as a string.</param>  
        /// <returns>A <see cref="DataDefinition"/> object containing the deserialized data.</returns>  
        public DataDefinition Deserializer(string Data)
        {
            var deserializer = new DeserializerBuilder()
                 .WithCaseInsensitivePropertyMatching()
                 .Build();

            return deserializer.Deserialize<DataDefinition>(Data.ToString());
        }
    }

}
