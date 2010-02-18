using System;
using System.Reflection;
using log4net;
using log4net.Config;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]
namespace Plexus.Utils
{
    /// <summary>
    /// Logging object (use log4net ver1.2).
    /// </summary>
    public static class Logger
    {
        public static ILog logger;
        public static void Info(System.Type type, object message)
        {
            System.Diagnostics.Debug.WriteLine(message);

            logger = LogManager.GetLogger(type);
            if (!logger.Logger.Repository.Configured)
                XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(GetConfigFile() + ".config"));
            logger.Info(message);
        }
        public static void Debug(System.Type type, object message)
        {
            System.Diagnostics.Debug.WriteLine(message);

            logger = LogManager.GetLogger(type);
            if (!logger.Logger.Repository.Configured)
                XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(GetConfigFile() + ".config"));
            logger.Debug(message);
        }
        public static void Warn(System.Type type, object message)
        {
            logger = LogManager.GetLogger(type);
            if (!logger.Logger.Repository.Configured)
                XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(GetConfigFile() + ".config"));
            logger.Warn(message);
        }
        public static void Error(System.Type type, object message)
        {
            System.Diagnostics.Debug.WriteLine(message);
            logger = LogManager.GetLogger(type);
            if (!logger.Logger.Repository.Configured)
                XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(GetConfigFile() + ".config"));
            logger.Error(message);
        }
        public static void Fatal(System.Type type, object message)
        {
            logger = LogManager.GetLogger(type);
            if (!logger.Logger.Repository.Configured)
                XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(GetConfigFile() + ".config"));
            logger.Fatal(message);
        }
        /// <summary>
        /// Runtime get a correct config file.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <b>author</b>   yeang-shing.then
        /// <b>since</b>    2008-10-06
        /// </remarks>
        private static string GetConfigFile()
        {
            string output = string.Empty;
            //found entry executed only
            System.Reflection.Assembly[] lAssemblies = System.Threading.Thread.GetDomain().GetAssemblies();
            foreach (System.Reflection.Assembly ass in lAssemblies)
            {
                if (ass.GetType() == typeof(System.Reflection.Assembly)
                    && (ass.ManifestModule.Name.IndexOf('.') == ass.ManifestModule.Name.LastIndexOf('.'))
                    && ass.ManifestModule.Name.ToLower().LastIndexOf(".exe") > -1)
                {
                    output = ass.ManifestModule.Name;
                    break;
                }
            }//end loops

            return output;
        }
    }//end class Logger
}
