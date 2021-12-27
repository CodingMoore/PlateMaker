﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Extensions;
using System.Web;

namespace PlateScanner
{
    class ApiCallObject
    {
        public string BaseUrl { get; set; }
        public string QueryString { get; set; }
        public string EncodedQueryString { get; set; }
        public string ContentFormat { get; set; }
        public string EncodedUrl { get; set; }
        public string ApiResponse { get; set; }

        public ApiCallObject(string plateNumber)
        {
            BaseUrl = "http://skyserver.sdss.org/dr17/en/tools/search/x_results.aspx?searchtool=SQL&TaskName=Skyserver.Search.SQL&syntax=NoSyntax&ReturnHtml=true&cmd=";
            QueryString = "" +
                     "SELECT" + " " +
                     "plate, ObjId, s.cx, s.cy" + " " +
                     "FROM" + " " +
                     "PhotoObj AS p" + " " +
                     "JOIN" + " " +
                     "SpecObj AS s ON s.bestobjid = p.objid" + " " +
                     "WHERE" + " " +
                    $"s.plate = {plateNumber}";
            EncodedQueryString = HttpUtility.UrlEncode(this.QueryString);
            ContentFormat = "&format=jsonx";
            ApiResponse = String.Empty;
        }

        public void MakeTheApiCall()
        {

            var client = new RestClient();
            var request = new RestRequest(this.BaseUrl + this.EncodedQueryString + this.ContentFormat);

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
                Console.WriteLine("Response status code: " + response.StatusCode);
                //Console.WriteLine(response.StatusCode);
                this.ApiResponse = response.Content;
            }

            Console.WriteLine("EncodedQueryString" + this.EncodedQueryString);
            Console.WriteLine(this.BaseUrl + this.EncodedQueryString + this.ContentFormat);
            Console.WriteLine(this.ApiResponse);

        }
        
       
    }
}

// Example Search - https://skyserver.sdss.org/dr16/SkyServerWS/SearchTools/SqlSearch?cmd=select%20top%2010%20ra,dec%20from%20Frame&format=json

// URL encoding

// '+' or '%02' is ' ' (space)
// '%3D' is '=' (equals)
// '%2C' is ',' (equals)
// '<' or '%3c' is '<' (less than)
// '>' or '%3e' is '>' (greater than)
// '%0D%0A' is (enter/return/newline)

// Base URL: http://skyserver.sdss.org/dr17/en/tools/search/x_results.aspx?searchtool=SQL&TaskName=Skyserver.Search.SQL&syntax=NoSyntax&ReturnHtml=true&cmd=
// SQL Select statement = SELECT plate, ObjId, s.cx, s.cy FROM PhotoObj AS p JOIN SpecObj AS s ON s.bestobjid = p.objid WHERE s.plate <= 2534 AND s.plate >= 2533
// URL query string = SELECT+plate%2C+ObjId%2C+s.cx%2C+s.cy+FROM+PhotoObj+AS+p+JOIN+SpecObj+AS+s+ON+s.bestobjid+%3D+p.objid+WHERE+s.plate+<%3D+2534+AND+s.plate+>%3D+2533
// return data format = &format=html OR &format=jsonx
// full URL = http://skyserver.sdss.org/dr17/en/tools/search/x_results.aspx?searchtool=SQL&TaskName=Skyserver.Search.SQL&syntax=NoSyntax&ReturnHtml=true&cmd=SELECT+plate%2C+ObjId%2C+s.cx%2C+s.cy+FROM+PhotoObj+AS+p+JOIN+SpecObj+AS+s+ON+s.bestobjid+%3D+p.objid+WHERE+s.plate+<%3D+2534+AND+s.plate+>%3D+2533&format=html

// Example API Calls


//Finds all objects on a plate?!?!
//SELECT
//plate, specObjID
//FROM 
//SpecObjAll
//WHERE
//plate = 2534

//SELECT
//*
//FROM 
//SpecObjAll
//WHERE
//specObjID = 9262920495199967232
//Order by xFocal asc



//SELECT
//plate, ObjId, s.cx, s.cy
//FROM PhotoObj AS p
//JOIN SpecObj AS s ON s.bestobjid = p.objid
//WHERE 
//s.plate = 2534


//SELECT
//plate, p.ObjId, s.cx, s.cy
//FROM 
//PhotoObjAll AS p
//JOIN 
//SpecObjAll AS s ON s.specObjID = p.specObjID
//WHERE 
//s.plate = 2534


//Primary Keys
//SpecObjAll - primary   specObjID
//PhotoObjAll - primary - objID
//Plate2Target - Primary - Plate3TargetID