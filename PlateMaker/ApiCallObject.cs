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
        public string EncodedUrl { get; set; }
        public dynamic ApiResponse { get; set; }

        public ApiCallObject(string plateNumber)
        {
            BaseUrl = "http://skyserver.sdss.org/dr17/en/tools/search/x_results.aspx?searchtool=SQL&TaskName=Skyserver.Search.SQL&syntax=NoSyntax&ReturnHtml=true&cmd=";
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
            EncodedQueryString = HttpUtility.UrlEncode(QueryString);
            ContentFormat = "&format=jsonx";
            ApiResponse = null;
        }

        public Dictionary<int, string[]> MakeTheApiCall()
        {

            var client = new RestClient();
            var request = new RestRequest(BaseUrl + EncodedQueryString + ContentFormat);

            var response = client.ExecuteAsync(request, Method.GET).GetAwaiter().GetResult();

            if (!response.IsSuccessful)
            {
                string message = "Error retrieving API response: " + response.ErrorMessage;
                Console.WriteLine(message);
                var exception = new Exception(message, response.ErrorException);
                //throw exception;
            }
            else
            {
                Console.WriteLine("SDSS API response status code: " + response.StatusCode);

                //Console.WriteLine(response.Content);
                // deserializes json response
                ApiResponse = JsonConvert.DeserializeObject(response.Content);
            }

            //Console.WriteLine("EncodedQueryString" + this.EncodedQueryString);
            //Console.WriteLine(this.BaseUrl + this.EncodedQueryString + this.ContentFormat);
            //Console.WriteLine(this.ApiResponse);

            Dictionary<int, string[]> stellarObjectData = new Dictionary<int, string[]> { };

            //foreach (var stellarObject in this.ApiResponse[0]["TableName"] == "Table1") 
            //{
            //    stellarObjectData.Add(stellarObject[0] Key["ObjId"], [stellarObject["xFocal"], sellarObject["yFocal"]]);
            //}



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
                    //might try the image thing later
                    //ApiResponse[0]["Rows"][i]["img"].ToString(),
                };

                stellarObjectData.Add(i, objectDataString);
            }

            //foreach(var item in this.ApiResponse[0]["Rows"])
            //{
            //    string objectIdAsString = item["ObjId"].ToString();
            //    string[] xYArray = new string[] { item["xFocal"].ToString(), item["yFocal"].ToString() };

            //    stellarObjectData.Add(objectIdAsString, xYArray);
            //}

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