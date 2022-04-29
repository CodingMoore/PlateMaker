
namespace PlateMaker
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Program Started \n");

            // Defines the name of the .csv file that will hold all of the plate numbers. 
            string PlateFileName = "PlateList.csv";
            string? fileSavingDirectory = null;

            try
            {
                // Suppresses "CS8602: Dereference of possibly null reference" warning for fileSavingDirectory.
                // Directory.GetParent() can return null if there is no parent directory.
                // As this application controls the directory structure, we can know that fileSavingDirectory will not be null.
                #pragma warning disable 8602
                // Defines a file path inside this application that we will use later
                fileSavingDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
                // Un-suppresses 8602 warnings
                #pragma warning restore 8602
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(0);
            }

            if (fileSavingDirectory == null)
            {
                // I'm not sure if this can even be hit, but its here just in case.
                Console.WriteLine("The application directory is null... somehow");
                Environment.Exit(0);
            } 
            else
            {
                // Defines the path (including file name) that we will look in to find the "PlateList.csv" file.
                string csvImportPath = $"{fileSavingDirectory}\\{PlateFileName}";
                
                string[]? plates = null;

                try
                {
                    // Reads the .csv file found at the path "csvImportDirectory", and makes each line an index of a string array.
                    plates = File.ReadAllLines(csvImportPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Environment.Exit(0);
                }

                if (plates == null)
                {
                    // I'm not sure if this can even be hit, but its here just in case.
                    Console.WriteLine("The PlateList.csv was null... somehow");
                    Environment.Exit(0);
                }
                // The length must be greater than 1 since the first entry is the .csv file column header
                else if (plates.Length <= 1)
                {
                    Console.WriteLine("No plate numbers were detected in the PlateList.csv file.");
                    Console.WriteLine("Please add one or more valid plate numbers to the Plate Number(s) column of this file, and try again.");
                    Console.WriteLine("Put the plate numbers below the column header in the first column, one plate number per row, starting on row 2.");
                    Environment.Exit(0);
                }
                else
                {
                    // Loops through each plate number. We skip index 0 as that is the .csv file column header
                    for (int i = 1; i < plates.Length; i++)
                    {
                        string plateNumber = plates[i];

                        Console.WriteLine($"Current plate: {plateNumber}");

                        // Checks to see if the this "plate number" entry in the .csv file is an integer or not.  The "ignoreMe" value is not used as we only need the returned bool.
                        if (!int.TryParse(plateNumber, out int ignoreMe) )
                        {
                            Console.WriteLine("Plate Number was not an intiger - Skipping");
                        }
                        else
                        {
                            // Creates a new plate object for this plate number in the .csv file.
                            Plate plate = new Plate(plateNumber, fileSavingDirectory);

                            // creates an apiCallObject instance using this plate number.
                            ApiCallObject apiCall = new ApiCallObject(plateNumber);

                            // Makes the API call to the SDSS "Sky Server" database
                            Dictionary<int, string[]> stellarObjectData = apiCall.MakeTheApiCall();

                            if (stellarObjectData != null && stellarObjectData.Count > 0)
                            {
                                // Creates an SVG from the data returned by the API call.
                                string svgStringFromApi = plate.CreateSvgFromApiCoordinates(stellarObjectData);

                                // Creates an HTML file using the SVG "svgStringFromApi"
                                plate.CreateHtmlFromSvgFromApi(svgStringFromApi);

                                Console.WriteLine($"Plate {plateNumber} has been completed");
                            }
                            else
                            {
                                Console.WriteLine("Skipping Plate");
                            }
                        }
                        // Adds blank line to console output for readability.
                        Console.WriteLine("");

                    }
                }
            }

            Console.WriteLine("Program Finished");

            Console.ReadKey();
        }

    }
}