using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClinicalTemplateReader.Helpers
{
    public class Logger
    {
        public static void LogError(string message)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filename = $"CTR-{DateTime.Now.ToString("yyyy-MM-dd")}-errors.txt";
            File.AppendAllText(Path.Combine(path, filename),
                $"{message}{Environment.NewLine}{Environment.NewLine}");
        }
    }
}
