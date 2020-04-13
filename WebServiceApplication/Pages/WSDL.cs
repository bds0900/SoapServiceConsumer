/*
File: WSDL.cs
Project: Assignment2
Programmer: Doosan Beak
First Version: 2019-09-24
Descrption: This file contains class and methods that support get wsdl and parsing wsdl
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;


namespace WebServiceApplication.Pages
{
    public class WSDL
    {
        
        private XDocument xdoc;
        private Dictionary<string, string> namespaceList;
        private XmlNamespaceManager nsmgr;
        public WSDL()
        {
            xdoc = null;
        }
        /*
        Function : WSDL
        DESCRIPTION : This constructor read url and sets all the data member in the object
        PARAMETERS : string: url for wsdl
        RETURNS : no return
        */
        public WSDL(string url)
        {
            xdoc = null;
            GetWSDL(url);
            namespaceList = new Dictionary<string, string>();
            ParseWSDL();
        }

        /*
        Function : GetWSDL
        DESCRIPTION : This method get wsdl file as a string from the url
        PARAMETERS : string: url for wsdl
        RETURNS : no return
        */
        private void GetWSDL(string url)
        {
            string responseText = string.Empty;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "GET";
            //webRequest.Credentials = new NetworkCredential("userName", "password");
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse())
                {
                    HttpStatusCode status = resp.StatusCode;
                    Stream respStream = resp.GetResponseStream();
                    using (StreamReader sr = new StreamReader(respStream))
                    {
                        xdoc = XDocument.Load(sr);
                    }
                }
            }
            catch(InvalidOperationException ioe)
            {

            }
            

        }
        /*
        Function : ParseWSDL
        DESCRIPTION : This method parses a wsdl string and save method, parameter, type and bulid a list of methos and return it
        PARAMETERS : no parameters
        RETURNS : List<Method>:lis of method
        */
        public List<Method> ParseWSDL()
        {
            List<Method> methods = new List<Method>();
            XNamespace wsdl = "http://schemas.xmlsoap.org/wsdl/";
            XNamespace s = "http://www.w3.org/2001/XMLSchema";
            XNamespace soap = "http://schemas.xmlsoap.org/wsdl/soap/";
            //find a element has name "type" in http://schemas.xmlsoap.org/wsdl/ namespace then find name "schema" in http://www.w3.org/2001/XMLSchema
            var schema = xdoc.Root
                    .Element("{http://schemas.xmlsoap.org/wsdl/}types")
                    .Element("{http://www.w3.org/2001/XMLSchema}schema");
            var targetNamespace = schema.Attribute("targetNamespace").Value;
            var elements = schema.Elements(XName.Get("element", "http://www.w3.org/2001/XMLSchema"));
            Func<XElement, string> getName = (el) => el.Attribute("name").Value;

            // these are all method names
            var names = from el in elements
                        let name = getName(el)
                        where !name.EndsWith("Response")
                        select name;
            //get operation
            var operations = from test in xdoc.Root.Elements(wsdl + "binding")
                       where (test.Attribute("name").Value.Contains("Soap")&& !test.Attribute("name").Value.Contains("Soap12"))//EndsWith("Soap") 
                       select test.Elements(wsdl + "operation");

            //get soapActionss for the methods
            var soapActions = from yy in operations
                              from xx in yy.Elements(soap + "operation")
                             select xx.Attribute("soapAction").Value;


            foreach (var item in names.Zip(soapActions, (name, soapAction) => new { name, soapAction }))
            {
                // get a method
                var method = elements.Single(el => getName(el) == item.name);

                // these are all parameters for a given method
                var parameters = from par in method.Descendants(s + "element")
                                 select getName(par);
                // method types
                var types = from type in method.Descendants(s + "element")
                            select type.Attribute("type").Value.Substring(2);

                //method names and types
                Dictionary<string, string> ht = new Dictionary<string, string>();
                foreach (var i in parameters.Zip(types, (a, b) => new { a, b }))
                {
                    ht.Add(i.a, i.b);
                }

                methods.Add(new Method(item.name, item.soapAction, ht, null));
            }

            return methods;
        }

 





    }
}
