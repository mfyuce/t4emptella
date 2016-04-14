using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scriptella.execution;

namespace T4tella
{
    public class ScriptellaRunner
    {
        private const string tableSplit = "---------------------------TableSplit-------------------";
        public static string[] ExecuteTemplate(string text)
        {
            var folder = "templategen";
            try
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to create \"templategen\" directory.", e);
            }
            var newFile = folder + "/" + Guid.NewGuid();
            try
            {
                File.WriteAllText(newFile, text);
            }
            catch (Exception)
            {

                throw new Exception("Unable to write to \"" + newFile + "\" file.");
            }

            var newOutputFile = newFile + "_out";

            //var map = new java.util.HashMap();
            //map.put("template.outputfile",newOutputFile);
            //map.putAll(java.lang.System.getProperties());
            java.lang.System.setProperty("template.outputfile", Path.GetFileName(newOutputFile));
            var exec = EtlExecutor.newExecutor(new java.io.File(newFile));
            var stats = exec.execute();
            if (File.Exists(newOutputFile))
            {
                var ret = File.ReadAllText(newOutputFile).Trim();
                List<string> lst = SplitOutput(ret);
                Delete(new string[] { newFile, newOutputFile });
                return lst.ToArray();
            }
            else
            {
                //LogHelper
                Delete(new string[] { newFile, newOutputFile });
                return null;
            }
        }

        public static List<string> SplitOutput(   string ret)
        {
            List<string> lst = new List<string>();
            var firstIndex = ret.IndexOf(tableSplit);
            if (firstIndex == -1)
            {
                lst.Add(ret);
            }
            else
            {
                while (true)
                {
                    lst.Add(ret.Substring(0, firstIndex).Trim());
                    ret = ret.Substring(firstIndex + tableSplit.Length).Trim();
                    firstIndex = ret.IndexOf(tableSplit);
                    if (firstIndex == -1)
                    {
                        lst.Add(ret);
                        break;
                    }
                }
            }
            return lst;
        }
        private static void Delete(string[] files)
        {
            if (files != null)
            {
                foreach (var item in files)
                {
                    try
                    {
                        if (File.Exists(item))
                        {
                            File.Delete(item);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
