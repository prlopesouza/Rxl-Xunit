using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Rxl_Xunit
{
    class RxlToXunit
    {
        static int Main(string[] args)
        {
            string xslFile = "ranorex-to-xunit.xsl";

            if (args.Length < 1)
            {
                Console.WriteLine("rxlog.data file name or parameter -d <directory> required.");
                return -1;
            }

            if (args[0].Equals("-d"))
            {
                string directory = ".\\";
                if (args.Length > 1)
                {
                    directory = args[1];
                    if (directory.Last<char>().Equals("\\"))
                    {
                        directory = directory.Substring(0, directory.Length - 1);
                    }
                    if (directory[0]=='\\')
                    {
                        directory = directory.Remove(0, 1);
                    }
                }

                string[] files = Directory.GetFiles(directory, "*.rxlog.data");
                foreach (string file in files)
                {
                    TransformFile(file, xslFile);
                }
            }
            else
            {
                string rxlogFile = args[0];
                if (!File.Exists(rxlogFile))
                {
                    Console.WriteLine("File " + rxlogFile + " not found.");
                    return -2;
                }

                return TransformFile(rxlogFile, xslFile);
            }

            return 1;
        }

        private static int TransformFile(string rxlogFile, string xslFile)
        {
            string path = Path.GetDirectoryName(rxlogFile);
            if (!path.Equals("")) path = path + "\\";
            string xUnitFile =  path + Path.GetFileNameWithoutExtension(rxlogFile) + "_xunit.xml";
            Console.WriteLine("Arquivo = " + xUnitFile);
            if (File.Exists(xslFile))
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(rxlogFile);

                    // Save report to stream
                    using (MemoryStream stream = new MemoryStream())
                    {
                        xmlDoc.Save(stream);

                        // Load the style sheet
                        System.Xml.Xsl.XslCompiledTransform xslTrans = new System.Xml.Xsl.XslCompiledTransform();
                        xslTrans.Load(xslFile);

                        // Perform Transformation
                        stream.Position = 0;
                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            XmlWriter writer = new XmlTextWriter(xUnitFile, new UTF8Encoding(false));
                            writer.WriteStartDocument();
                            xslTrans.Transform(reader, null, writer);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("A valid .rxlog.data file must be used.");
                    return -4;
                }
            }
            else
            {
                Console.WriteLine("File " + xslFile + " not found.");
                return -3;
            }

            Console.WriteLine("File " + rxlogFile + " transformed to " + xUnitFile);

            return 1;
        }
    }
}
