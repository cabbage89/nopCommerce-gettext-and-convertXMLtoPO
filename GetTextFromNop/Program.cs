using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GetTextFromNop
{
    class Program
    {
        static void Main(string[] args)
        {
            var patterns = new[]
            {
                new Regex("T\\(\"(?<token>[^\"]*)\"\\)", RegexOptions.Compiled | RegexOptions.Multiline),
                new Regex("NopResourceDisplayName\\(\"(?<token>[^\"]*)\"\\)", RegexOptions.Compiled | RegexOptions.Multiline)
            };
            Console.WriteLine("本程序可获取nopCommerce中的所有待翻译的词条");
            Console.WriteLine("请输入列表文件路径,(使用: dir *.cs /s/b 命令)");
            var path = Console.ReadLine();

            var filePathList = new List<string>();
            using (var fs = new FileStream(path, FileMode.Open))
            using (StreamReader sr = new StreamReader(fs))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                    filePathList.Add(line);
            }
            Console.WriteLine($"共{filePathList.Count}个文件");

            var dict = new System.Collections.Concurrent.ConcurrentDictionary<string, List<string>>();

            //用PLINQ可能更快,不过同步问题不好处理
            filePathList.ForEach(x =>
           {
               Console.WriteLine($"[线程{System.Threading.Thread.CurrentThread.ManagedThreadId}]正在处理:{x}");
               using (var fs = new FileStream(x, FileMode.Open))
               using (StreamReader sr = new StreamReader(fs))
               {
                   var result = sr.ReadToEnd();
                   foreach (var pattern in patterns)
                       foreach (Match m in pattern.Matches(result))
                       {
                           var token = m.Groups["token"].Value;
                           dict.AddOrUpdate(token, new List<string> { x }, (key, list) =>
                           {
                               list.Add(x);
                               return list;
                           });
                       }
               }

           });

            Console.WriteLine($"字典数量:{dict.Keys.Count}");

            var query = from kv in dict
                        select new { token = kv.Key, context = kv.Value };

            using (var fs = new FileStream($"nop_{DateTime.Now.Ticks}.pot", FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                query.ToList().ForEach(x =>
                {
                    x.context.ForEach(y =>
                    {
                        sw.WriteLine($"#: {y}");
                    });
                    sw.WriteLine($"msgid \"{x.token}\"");
                    sw.WriteLine($"msgstr \"\"");
                    sw.WriteLine();
                });
            }
            Console.WriteLine("PO文件生成完毕!");
            System.Diagnostics.Process.Start("explorer.exe", Environment.CurrentDirectory);

        }
    }
}
