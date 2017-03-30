﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Common.DebugUtils;
using Common.Properties;

namespace Common.Message
{
    public class XmlValidation
    {
        //IMPORTANT
        //you have to add prebuild event in Common project and add project resource:
        //1. Properties -> Build Events -> Pre build event command line -> echo $(ProjectDir) > "$(ProjectDir)\Jesus.txt"
        //2. Create dummy file called Jesus.txt in Common\Jesus.txt
        //3. Properties -> Resources -> Add Existing file (down arrow near Add Resource) ->  add Jesus.txt
        //IMPORTANT

        /// <summary>
        /// Xml validation
        /// </summary>
        /// <param name="message">XML message</param>
        /// 
        /// <exception cref="XmlSchemaValidationException">Is thrown when wrong xml</exception>
        //public static void Validate(string message)
        //{
        //    try
        //    {
        //        //Sweet Windows Magic
        //        var dir = Resources.Jesus;
        //        dir = dir.Replace(@"\\", @"\");
        //        dir = dir.Substring(0, dir.Length - 3);
        //        //Console.WriteLine(@dir + @"TheProjectGameCommunication.xsd");
        //        //End of Sweet Windows Magic


        //        XmlReaderSettings settings = new XmlReaderSettings();
        //        settings.Schemas.Add("https://se2.mini.pw.edu.pl/17-results/", @dir + @"TheProjectGameCommunication.xsd");
        //        settings.ValidationType = ValidationType.Schema;


        //        XmlReader reader = XmlReader.Create(new StringReader(message), settings);
        //        XmlDocument document = new XmlDocument();
        //        document.Load(reader);

        //        ValidationEventHandler eventHandler =
        //            new ValidationEventHandler(ValidationEventHandler);
        //        document.Validate(eventHandler);


        //    }
        //    catch (Exception ex)
        //    {
        //        ConsoleDebug.Error(ex.Message);
        //        throw new XmlSchemaValidationException(ex.Message);
        //    }
        //}

        static XmlValidation instance;
        XmlReaderSettings settings;

        private XmlValidation()
        {
            settings = new XmlReaderSettings();
            settings.Schemas.Add("https://se2.mini.pw.edu.pl/17-results/", "TheProjectGameCommunication.xsd");
            settings.ValidationType = ValidationType.Schema;
        }

        public static XmlValidation Instance
        {
            get
            {
                if (instance == null)
                    instance = new XmlValidation();
                return instance;
            }
        }

        public void Validate(string message)
        {
            XmlReader reader = XmlReader.Create(new StringReader(message), settings);
            XmlDocument document = new XmlDocument();
            document.Load(reader);

            ValidationEventHandler eventHandler =
                new ValidationEventHandler(ValidationEventHandler);
            document.Validate(eventHandler);

        }


        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
           
            Console.WriteLine("\n ERROR IN VALIDATION\n ");

            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Console.WriteLine("Error: {0}", e.Message);
                    throw new  XmlException();
                    break;
                case XmlSeverityType.Warning:
                    Console.WriteLine("Warning {0}", e.Message);
                    throw new XmlException();
                    break;
            }

         
         
        }
    }
}
