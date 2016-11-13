using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConvertNopXmlToPo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("本程序将nopCommerce的多语言文件转换为po文件.");
            string inputPath, outputFilePath;
            do
            {
                Console.WriteLine("请输入xml格式的语言文件路径");
                inputPath = Console.ReadLine();
            } while (!File.Exists(inputPath));




            XElement root = XElement.Load(inputPath);
            var res = root.Elements("LocaleResource");

            var query = from node in res
                        select new { msgid = node.Attribute("Name").Value, msgstr = node.Element("Value").Value };

            Console.WriteLine($"共{query.Count()}个词条");

            using (var fs = new FileStream($"nop_{DateTime.Now.Ticks}.po", FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                query.ToList().ForEach(x =>
                {

                    sw.WriteLine($"msgid \"{GetSafeTxt(x.msgid)}\"");
                    sw.WriteLine($"msgstr \"{GetSafeTxt(x.msgstr)}\"");
                    sw.WriteLine();
                });
            }
            Console.WriteLine("PO文件生成完毕!");
            System.Diagnostics.Process.Start("explorer.exe", Environment.CurrentDirectory);
        }

        static string GetSafeTxt(string str)
        {
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "").Replace("\r", "").Trim();
        }
    }
}
