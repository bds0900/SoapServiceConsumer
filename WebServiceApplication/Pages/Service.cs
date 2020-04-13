/*
File: Service.cs
Project: Assignment2
Programmer: Doosan Beak
First Version: 2019-09-24
Descrption: This file contains Method class and Service Class as types
*/
using System.Collections.Generic;

namespace WebServiceApplication.Pages
{

    public class Method
    {
        public string Name { get; set; }
        public Dictionary<string,string> Parameters { get; set; }
        public string SoapAction { get; set; }
        public string Dependency { get; set; }
        public string Usage { get; set; }
        public Method(string name, string sapAction, Dictionary<string, string> param,string dependency)
        {
            Name = name;
            SoapAction = sapAction;
            Parameters = param;
            Dependency = dependency;
        }
    }
    public class Service
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string TargetNamespace { get; set; }
        public string Agent { get; set; }
        public List<Method> Methods { get; set; }
        

    }
}
