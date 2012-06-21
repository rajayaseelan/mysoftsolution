using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace SetupNBearVsPlugin
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string myDocDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string visionName = ConfigurationManager.AppSettings["AddInVersionName"] ?? "2008";
            string visionNo = ConfigurationManager.AppSettings["AddInVersionNo"] ?? "9.0";
            string vsNetAddInDir = myDocDir + string.Format("\\Visual Studio {0}\\Addins", visionName);
            string addInFile = vsNetAddInDir + "\\MySoft.Tools.EntityDesignVsPlugin.AddIn";

            if (Directory.Exists(myDocDir + string.Format("\\Visual Studio {0}", visionName)) && (!Directory.Exists(vsNetAddInDir)))
            {
                Directory.CreateDirectory(vsNetAddInDir);
            }

            if (Directory.Exists(vsNetAddInDir))
            {
                if (args != null && args.Length > 0)
                {
                    if (args[0].ToLower() == "-u")
                    {
                        File.Delete(addInFile);
                        MessageBox.Show("卸载MySoft实体生成插件成功！", "卸载插件", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    try
                    {
                        string[] existingAddIns = Directory.GetFiles(vsNetAddInDir);
                        if (existingAddIns != null)
                        {
                            foreach (string addin in existingAddIns)
                            {
                                if (addin.Contains("MySoft.Tools.EntityDesignVsPlugin - For Testing.AddIn"))
                                {
                                    File.SetAttributes(addin, File.GetAttributes(addin) ^ FileAttributes.ReadOnly);
                                    File.Delete(addin);
                                    break;
                                }
                            }
                        }
                        string content = File.ReadAllText("MySoft.Tools.EntityDesignVsPlugin.AddIn");
                        content = string.Format(content, visionNo, AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'));
                        File.WriteAllText(addInFile, content);

                        MessageBox.Show("安装MySoft实体生成插件成功！", "安装插件", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(string.Format("本插件只适应于Visual Studio {0}！", visionName), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}