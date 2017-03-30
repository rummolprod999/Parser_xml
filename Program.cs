using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Parser_xml
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string NameAr = "/out/nsi/nsiOrganization/nsiOrganization_all_20160911_010000_001.xml.zip";
            string DestAr = "./nsiOrganization_all_20160911_010000_001.xml.zip";
            string extractPath = "./nsiOrganization_all_20160911_010000_001";
            string NameF = "./nsiOrganization_all_20160911_010000_001/nsiOrganization_all_20160911_010000_028.xml";
            Client ftpCl = new Client("ftp://ftp.zakupki.gov.ru", "fz223free", "fz223free");
            ftpCl.DownloadFile(NameAr, DestAr);
            FileInfo fileInf = new FileInfo(DestAr);
            if (fileInf.Exists)
            {
                ZipFile.ExtractToDirectory(DestAr, extractPath);
                fileInf.Delete();
                FileInfo fileInfa = new FileInfo(NameF);
                if (fileInfa.Exists)
                {
                    using (StreamReader sr = new StreamReader(NameF, System.Text.Encoding.Default))
                    {
                        string text = sr.ReadToEnd();
                        if (text == null) throw new ArgumentNullException(nameof(text));
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(text);
                        string jsons = JsonConvert.SerializeXmlNode(doc);
                        JObject json = JObject.Parse(jsons);
                        /*File.WriteAllText("json.txt", json.ToString());
                        Console.WriteLine(json);*/
                        var org = from p in json["ns2:nsiOrganization"]["ns2:body"]["ns2:item"]
                            select p["ns2:nsiOrganizationData"];
                        int ind = 0;
                        foreach (var o in org)
                        {
                            if (ind <= 0)
                            {
                                /*Console.WriteLine(o);*/
                                var okpd2code = o.SelectTokens("ns2:classification.ns2:activities.ns2:okved");
                                foreach (var e in okpd2code)
                                {
                                    foreach (var s in e)
                                    {
                                        Console.WriteLine(s.SelectToken("ns2:code"));
                                    }
                                }
                            }
                            ind++;
                            try
                            {
                                var inn = o.SelectToken("ns2:mainInfo.inn") ?? "";

                                Console.WriteLine(inn);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                    }
                }
            }
            DirectoryInfo dirInfo = new DirectoryInfo(extractPath);
            dirInfo.Delete(true);
        }
    }
}