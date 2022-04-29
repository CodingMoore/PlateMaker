using Newtonsoft.Json;
using RestSharp;
using System.Web;

namespace PlateMaker
{
    class ApiCallObject
    {
        public string BaseUrl { get; set; }
        public string QueryString { get; set; }
        public string EncodedQueryString { get; set; }
        public string ContentFormat { get; set; }
        public string? EncodedUrl { get; set; }
        public dynamic? ApiResponse { get; set; }

        public ApiCallObject(string plateNumber)
        {
            // The base URL to which API variables will inserted
            BaseUrl = "http://skyserver.sdss.org/dr17/en/tools/search/x_results.aspx?searchtool=SQL&TaskName=Skyserver.Search.SQL&syntax=NoSyntax&ReturnHtml=true&cmd=";
            // The SQL query that will be added onto the Base URL as query strings.  Note that the space after each line are required.
            QueryString = "" +
                "SELECT" + " " +
                "plate, ObjId, xFocal, yFocal, s.ra, s.dec, class" + " " +
                "FROM" + " " +
                "PhotoObj AS p" + " " +
                "JOIN" + " " +
                "SpecObj AS s ON s.bestobjid = p.objid" + " " +
                "WHERE" + " " +
               $"s.plate = {plateNumber}" + " " +
                "Order by xFocal asc";
            // Converts the Query String (SQL query) into a URL encoded format.
            EncodedQueryString = HttpUtility.UrlEncode(QueryString);
            // Sets the return type of the API call
            ContentFormat = "&format=jsonx";
            ApiResponse = null;
        }

        public Dictionary<int, string[]> MakeTheApiCall()
        {
            // Creates Api request client
            var client = new RestClient();
            // Creates the API request
            var request = new RestRequest(BaseUrl + ContentFormat);
            // Makes the API request and saves the response
            var response = client.ExecuteAsync(request, Method.GET).GetAwaiter().GetResult();

            // The If statement to check if the EncodedQueryString is null is here because EncodedQueryString was made to be a nullable value. This should never be hit, but it is here just in case.
            if (EncodedQueryString == null)
            {
                Console.WriteLine("The Encoded query string is null.");
                Console.WriteLine("Aborting API call.");
            }
            else if (!response.IsSuccessful)
            {
                Console.WriteLine("SDSS API response status code: " + response.StatusCode);
                string message = "Error retrieving API response: " + response.ErrorMessage;
                var exception = new Exception(message, response.ErrorException);
                Console.WriteLine(exception);
            }
            else if (response == null || string.IsNullOrEmpty(response.Content))
            {
                Console.WriteLine("The Api response was null or empty");
            }
            else
            { 
                Console.WriteLine("SDSS API response status code: " + response.StatusCode);
                // Deserializes json response from api call.
                ApiResponse = JsonConvert.DeserializeObject(response.Content);
            }

            Dictionary<int, string[]> stellarObjectData = new Dictionary<int, string[]> { };

            if (ApiResponse != null)
            {
                for (int i = 0; i < ApiResponse[0]["Rows"].Count; i++)
                {
                    string[] objectDataString = new string[] {
                        ApiResponse[0]["Rows"][i]["xFocal"].ToString(),
                        ApiResponse[0]["Rows"][i]["yFocal"].ToString(),
                        ApiResponse[0]["Rows"][i]["ObjId"].ToString(),
                        ApiResponse[0]["Rows"][i]["plate"].ToString(),
                        ApiResponse[0]["Rows"][i]["ra"].ToString(),
                        ApiResponse[0]["Rows"][i]["dec"].ToString(),
                        ApiResponse[0]["Rows"][i]["class"].ToString(),
                        // Might experiment with retrieving an image with the API at a later time.  Reminder to add the image to the SQL Select Statement.
                        //ApiResponse[0]["Rows"][i]["img"].ToString(),
                    };

                    stellarObjectData.Add(i, objectDataString);
                }
            }

            return stellarObjectData;
        }
    }
}

///////////////
/////Notes/////
///////////////

// Example of a general SkyServer database Search via URL - https://skyserver.sdss.org/dr17/SkyServerWS/SearchTools/SqlSearch?cmd=select%20top%2010%20ra,dec%20from%20Frame&format=json

// URL Encoding
// '+' or '%02' is ' ' (space)
// '%3D' is '=' (equals)
// '%2C' is ',' (comma)
// '<' or '%3c' is '<' (less than)
// '>' or '%3e' is '>' (greater than)
// '%0D%0A' is (enter/return/newline)

// Base URL: http://skyserver.sdss.org/dr17/en/tools/search/x_results.aspx?searchtool=SQL&TaskName=Skyserver.Search.SQL&syntax=NoSyntax&ReturnHtml=true&cmd=
// SQL Select statement = SELECT plate, ObjId, s.cx, s.cy FROM PhotoObj AS p JOIN SpecObj AS s ON s.bestobjid = p.objid WHERE s.plate <= 2534 AND s.plate >= 2533
// URL query string = SELECT+plate%2C+ObjId%2C+s.cx%2C+s.cy+FROM+PhotoObj+AS+p+JOIN+SpecObj+AS+s+ON+s.bestobjid+%3D+p.objid+WHERE+s.plate+<%3D+2534+AND+s.plate+>%3D+2533
// return data format = &format=html OR &format=jsonx
// full URL = http://skyserver.sdss.org/dr17/en/tools/search/x_results.aspx?searchtool=SQL&TaskName=Skyserver.Search.SQL&syntax=NoSyntax&ReturnHtml=true&cmd=SELECT+plate%2C+ObjId%2C+s.cx%2C+s.cy+FROM+PhotoObj+AS+p+JOIN+SpecObj+AS+s+ON+s.bestobjid+%3D+p.objid+WHERE+s.plate+<%3D+2534+AND+s.plate+>%3D+2533&format=html

//Primary Database Keys
//SpecObjAll - primary   specObjID
//PhotoObjAll - primary - objID
//Plate2Target - Primary - Plate3TargetID