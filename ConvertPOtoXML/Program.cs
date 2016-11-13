using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ConvertPOtoXML
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("此工具将po文件转化成nop的语言文件。");
            Console.WriteLine("请输入PO文件路径。");

            var outputFile = $"{DateTime.Now.Ticks}.nopres.xml";
            var path = Console.ReadLine();

            var kv = new Dictionary<string, string>();

            using (var fs = new FileStream(path, FileMode.Open))
            using (StreamReader sr = new StreamReader(fs))
            {
                string line;

                string key = null, value = null;
                while (fs.CanRead && (line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;
                    var words = line.Split(new[] { ' ' });
                    var field = words[0].Trim(new[] { '"' });
                    var word = words[1].Trim(new[] { '"' });
                    if (field == "msgid")
                    {
                        key = word;
                    }
                    else if (field == "msgstr")
                    {
                        value = word;
                        if (!string.IsNullOrWhiteSpace(value))
                            kv.Add(key, value);
                    }
                }
            }

            var query = from x in kv
                        select x;

            var doc = new XmlDocument();
            var root = doc.CreateElement("Language");
            root.SetAttribute("Name", "中文");

            query.ToList().ForEach(x =>
            {

                var node = doc.CreateElement("LocaleResource");
                node.SetAttribute("Name", x.Key);
                var valueNode = doc.CreateElement("Value");
                valueNode.InnerText = x.Value;
                node.AppendChild(valueNode);
                root.AppendChild(node);
            });
            doc.AppendChild(root);
            doc.Save(outputFile);

            Console.WriteLine("xml文件生成完毕!");
            System.Diagnostics.Process.Start("explorer.exe", Environment.CurrentDirectory);

        }
    }
}
