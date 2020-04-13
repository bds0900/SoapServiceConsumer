/*
File: FileControl.cs
Project: Assignment2
Programmer: Doosan Beak
First Version: 2019-09-24
Descrption: This file contains file control class and file input output mothods
*/
using System;
using System.Linq;
using System.IO;
namespace WebServiceApplication.Pages
{
    /*
    Class : FileControl
    DESCRIPTION : This class controls the file open, close, read and write
    */
    public class FileControl
    {
        public StreamReader sr = null;
        public StreamWriter sw = null;
        private const char FILE_READONLY = 'r';
        private const char FILE_WRITE = 'W';
        private const char FILE_APPEND = '+';

        private const string DB_DIRECTORY = "DB";
        public FileControl()
        {
            Directory.CreateDirectory(DB_DIRECTORY);
        }
        /*
        Function : OpenFile
        DESCRIPTION : This method open file stream 
        PARAMETERS : string: file name 
                     char: open type
        RETURNS : no return
        */
        public void OpenFile(string fileName, char type)
        {
            string filePath = $"{DB_DIRECTORY}/{fileName}";
            try
            {
                if (type == FILE_READONLY)
                {
                    sw = null;
                    sr = new StreamReader(filePath);
                }
                else if (type == FILE_WRITE)
                {
                    sw = new StreamWriter(filePath);
                    sr = null;
                }
                else if (type == FILE_APPEND)
                {
                    sw = new StreamWriter(filePath, true);
                    sr = null;
                }
            }
            catch
            {
                //MethodBase m = MethodBase.GetCurrentMethod();
                //logger.WriteLog(m.ReflectedType.Name, m.Name, $"Can not open {filePath} file - FILE OPEN ERROR");
            }
        }
        /*
        Function : CloseFile
        DESCRIPTION : This method closes file stream
        PARAMETERS : no parameters
        RETURNS : no return
        */
        public void CloseFile()
        {
            if (sr != null)
            {
                sr.Close();
                sr = null;
            }
            if (sw != null)
            {
                sw.Close();
                sw = null;
            }
        }
        
        /*
        Function : GetFileList
        DESCRIPTION : This method gets file list from DB folder
        PARAMETERS : no parameters
        RETURNS : string[]: list of file names
        */
        public string[] GetFileList()
        {
            return Directory.GetFiles(Environment.CurrentDirectory + "/DB").Select(Path.GetFileName).ToArray(); ;
            
        }
        /*
        Function : GetJsonString
        DESCRIPTION : This method reads json string from a file
        PARAMETERS : no parameters
        RETURNS : string: json string
        */
        public string GetJsonString()
        {
            return File.ReadAllText(Environment.CurrentDirectory + "/DB/config.json");
        }
        /*
        Function : SetJsonString
        DESCRIPTION : This method writes json string in a json format file
        PARAMETERS : serialized: json string
        RETURNS : no return
        */
        public void SetJsonString(string serialized)
        {
            File.WriteAllText(Environment.CurrentDirectory + "/DB/config.json", serialized);
        }

    }
}
