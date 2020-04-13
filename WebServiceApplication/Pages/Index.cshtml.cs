/*
File: Index.cshtml.cs
Project: Assignment2
Programmer: Doosan Beak 
First Version: 2019-09-24
Descrption: This file contains methods that support sending request and receiving response. 
            And also has methods that support read info from a file and setting the services and methods
*/
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;

namespace WebServiceApplication.Pages
{
    public class IndexModel : PageModel
    {

        //[BindProperty]
        public Method Method { get; set; }
        [BindProperty]
        public Service WebService { get; set; }
        public List<Service> ServiceList { get; set; }
        public WSDL wsdl { get; set; }
        
        
        //default consturctor
        public IndexModel()
        {
            ServiceList = ReadFromConfigFile();
        }

        /*
        Function : Serialize
        DESCRIPTION : This method serialize the list of service into json string
        PARAMETERS : List<Service>: service list
        RETURNS : string
        */
        private string Serialize(List<Service> value)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return System.Text.Json.JsonSerializer.Serialize<List<Service>>(value, options);
        }

        /*
        Function : OnGetMethods
        DESCRIPTION : This method gets method list and return to client
        PARAMETERS : int?: service id
        RETURNS : JsonResult: values that from the service
        */
        public JsonResult OnGetMethods(int? serviceId)
        {
            //send methods list in the service
            int index = 0;
            // if serivce id is null just return empty method list
            if (!serviceId.HasValue)
            {
                return new JsonResult(new { Status = "Success", Method = "" });
            }
            else
            {
                index = serviceId.Value;
            }
            
            List<string> methods = new List<string>();
            WebService = ServiceList[index];
            foreach (var method in ReadFromConfigFile()[index].Methods)
            {
                var name = "";
                if(method.Name.StartsWith("self:"))
                {
                    name = method.Name.Substring(5);
                }
                else
                {
                    name = method.Name;
                }

                methods.Add(name);
            }
            return new JsonResult(new { Status = "Success", Methods = ServiceList[index].Methods });

        }
        /*
        Function : OnGetResult
        DESCRIPTION : This method makes soap message and send request
        PARAMETERS : int WebService
                     int method
                     string[] input value
        RETURNS : JsonResult: values that from the service
        */
        public JsonResult OnGetResult(int webService, int method, string[] arr)
        {

            // convert json string input to string array
            List<string> input = new List<string>();
            var array = JsonDocument.Parse(arr[0], new JsonDocumentOptions { AllowTrailingCommas = true });
            var root = array.RootElement;
            foreach(var item in root.EnumerateArray())
            {
                input.Add(item.ToString().Replace('"',' '));
            }

            // get result from service
            var result = GetResponse(CreateWebRequest(webService, method, input.ToArray()));
            var methodName = ServiceList[webService].Methods[method].Name + "Respons";

            // split the response and extract the result from the response
            //var stt = result.IndexOf($"{methodName}") - 1;
            //stt = result.IndexOf("Body>")+5;
            //if(result.IndexOf("Fault>")!=-1)
            //{
            //    stt = result.IndexOf("Fault>") + 5;
            //}
            //var end = result.IndexOf("</soap:Body") ;
            //if (result.IndexOf("</soap:Fault") != -1)
            //{
            //    end = result.IndexOf("</soap:Fault");
            //}
            //var stt = result.IndexOf("Fault>") == -1 ? result.IndexOf("Body>") + 5 : result.IndexOf("Fault>") + 6;
            //var end= result.IndexOf("</soap:Fault") == -1? result.IndexOf("</soap:Body") : result.IndexOf("</soap:Fault");
            //var final = result.Substring(stt, end - stt);

            // convert xml into xdocument object for parsing
            XDocument xdoc = XDocument.Parse(result);
            XNamespace env = "http://schemas.xmlsoap.org/soap/envelope/";
            var rooot=xdoc.Root.Element(env + "Body");

            var schema = rooot.Descendants();
            List<Dictionary<string, string>> respones = new List<Dictionary<string, string>>();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (var element in schema)
            {
                var name = element.Name.LocalName;
                var value = element.Value;
                if (dic.ContainsKey(name))
                {
                    respones.Add(dic);
                    dic = new Dictionary<string, string>();
                    dic.Add(name, value);
                }
                else
                {
                    dic.Add(name, value);
                    if (name=="faultstring")
                    {
                        var log = Logging.GetLogger();
                        log.WriteLog(value);
                    }
                }
            }
            respones.Add(dic);
            
            return new JsonResult(new { Status = "Success", table = respones });
        }

        /*
        Function : CreateWebRequest
        DESCRIPTION : This method makes soap message and send request
        PARAMETERS : int WebService
                     int method
                     string[] input
        RETURNS : HttpWebRequest: request object which will get response from serivce
        */
        public HttpWebRequest CreateWebRequest(int WebService, int method, string[] input)
        {
            //which serivce?
            var service = ServiceList[WebService];
            string methodName = service.Methods[method].Name;
            string soapAction = service.Methods[method].SoapAction;
            var agent = service.Agent;
            Dictionary<string,string> parameters = service.Methods[method].Parameters;
            
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(service.URL);
            webRequest.Headers.Add("SOAPAction", soapAction);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            webRequest.UserAgent = agent;

            Builder requestBuilder = new Builder(methodName, service.TargetNamespace, input, parameters);
            string data = requestBuilder.BuildSoap();
            
            byte[] bytes = Encoding.ASCII.GetBytes(data);
            webRequest.ContentLength = bytes.Length;
            using (Stream reqStream = webRequest.GetRequestStream())
            {
                reqStream.Write(bytes, 0, bytes.Length);
            }
            return webRequest;

        }
        /*
        Function : GetResponse
        DESCRIPTION : This method reads soap response file from the http stream and return
        PARAMETERS : HttpWebRequest: request object
        RETURNS : string
        */
        public string GetResponse(HttpWebRequest webRequest)
        {
            string responseText = string.Empty;
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse())
                {
                    HttpStatusCode status = resp.StatusCode;
                    Stream respStream = resp.GetResponseStream();
                    using (StreamReader sr = new StreamReader(respStream))
                    {
                        responseText = sr.ReadToEnd();
                    }
                }
            }
            
            catch (WebException we)
            {
                HttpStatusCode status=((HttpWebResponse)we.Response).StatusCode;
                Stream respStream = ((HttpWebResponse)we.Response).GetResponseStream();
                using (StreamReader sr = new StreamReader(respStream))
                {
                    responseText = sr.ReadToEnd();
                }

            }
            catch (InvalidOperationException ioe)
            {

            }
            catch (Exception e)
            {

            }

            return responseText;
        }


        /*
        Function : ReadFromConfigFile
        DESCRIPTION : This method reads json string from the file and parse the json string 
                      and extracts methods, parameters, types
        PARAMETERS : no parameters
        RETURNS : List<Service>
        */
        public List<Service> ReadFromConfigFile()
        {
            List<Method> methods = new List<Method>();
            List<Service>  myService = new List<Service>();
            Dictionary<string,string> dic = new Dictionary<string, string>();
            FileControl fc = new FileControl();
            // gets json string from the file
            var jsonString = fc.GetJsonString();
            try
            {
                using (JsonDocument document = JsonDocument.Parse(jsonString,
                                           new JsonDocumentOptions { AllowTrailingCommas = true }))
                {
                    JsonElement root = document.RootElement;
                    JsonElement services = root.GetProperty("Services");
                    
                    //JsonElement urls = services.GetProperty("Url");
                    foreach (JsonElement service in services.EnumerateArray())
                    {
                        var id = service.GetProperty("Id").ToString();
                        var name = service.GetProperty("Name").ToString();
                        var url = service.GetProperty("URL").ToString();
                        var target = service.GetProperty("TargetNamespace").ToString();
                        var agent= service.GetProperty("Agent").ToString();
                        methods = new List<Method>();

                        //get method/ parameters/ parameter names/ parameter types
                        foreach (JsonElement method in service.GetProperty("Methods").EnumerateArray())
                        {
                            dic = new Dictionary<string, string>();
                            
                            foreach (var x in method.GetProperty("Parameters").EnumerateObject())
                            {
                                var n=x.Name;
                                var type=x.Value.ToString();
                                dic.Add(n, type);
                            }
                            var methodName = method.GetProperty("Name");
                            var soapAction = method.GetProperty("SoapAction");
                            var dependency= method.GetProperty("Dependency");
                            
                            methods.Add(new Method(methodName.ToString(), soapAction.ToString(), dic, dependency.ToString()) );
                        }
                        myService.Add(new Service { Id = id, Name = name, URL = url, TargetNamespace= target, Methods = methods, Agent= agent });
                    }
                }

            }
            catch (JsonReaderException)
            {
                throw new JsonReaderException("is invalid without a matching open");
            }

            return myService;
        }


        
    }

}
