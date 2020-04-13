/*
File: Builder.cs
Project: Assignment2
Programmer: Doosan Beak
First Version: 2019-09-24
Descrption: This file contains Builder class and methods that support bulding soap messge
*/
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebServiceApplication.Pages
{
    /*
    Class : Builder
    DESCRIPTION : This class contains methods for building soap message body
    */
    public class Builder
    {
 
        string Name { get; set; }
        string[] Values { get; set; }
        Dictionary<string, string> Parameters { get; set; }
        string TargetNamespace { get; set; }
        public Builder(string name,string targetNamespace, string[] input, Dictionary<string, string> parameters)
        {
            Name = name;
            Values = input;
            Parameters = parameters;
            TargetNamespace = targetNamespace;
        }
        /*
        Function : BuildSoap
        DESCRIPTION : This method make a soap message
        PARAMETERS : no parameters
        RETURNS : string: soap message
        */
        public string BuildSoap()
        {
            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    $"<soap:Envelope {GetNamespace()}>" +
                    "<soap:Body>" +
                        BuildBody() +
                    "</soap:Body>" +
                    "</soap:Envelope>";
        }
        /*
        Function : BuildBody
        DESCRIPTION : This method appends name, namespaces, elements in a string
        PARAMETERS : no parameters
        RETURNS : string: soap body
        */
        public string BuildBody()
        {
            string body = "";
            StringBuilder value = new StringBuilder();
            foreach (var i in Parameters.Zip(Values, (a, b) => new { a, b }))
            {
                value.Append($"<{i.a.Key}>{i.b}</{i.a.Key}>");
            }
            if(Name.StartsWith("self:"))
            {

                body = $"<{Name.Substring(5)} xmlns=\"{TargetNamespace}\" />";
            }
            else
            {
                body= $"<{Name} xmlns=\"{TargetNamespace}\">{value.ToString()}</{Name}>";
            }

            return body;
        }
        /*
        Function : GetNamespace
        DESCRIPTION : This method returns namespaces
        PARAMETERS : no parameters
        RETURNS : no return
        */
        public string GetNamespace()
        {
            return "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" "+
                    "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" "+
                    "xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"";
        }
    }
}
