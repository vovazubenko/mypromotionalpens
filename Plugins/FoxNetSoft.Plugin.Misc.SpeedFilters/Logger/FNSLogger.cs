using System;
using System.IO;
using System.Web.Hosting;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Logger
{
    public class FNSLogger
    {
        #region Fields

        private readonly bool _showDebugInfo = false;
        private string filename;
        private string logPath;

        #endregion

        #region Ctor
        public FNSLogger(bool showDebugInfo = false)
        {
            this._showDebugInfo = showDebugInfo;
            filename = PluginLog.SystemName;
            filename.Replace(" ", "_");
            if (filename.Length == 0)
            {
                filename = "foxnetsoft_log";
            }
            this.logPath = HostingEnvironment.MapPath("~/App_Data/" + filename + "_log.txt");
        }
        #endregion

        public void LogMessage(string message)
        {
            try
            {
                if (!this._showDebugInfo)
                    return;

                message = string.Format("{0}*******{1}{2}", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:ffff"), Environment.NewLine, message);
                try
                {
                    if (File.Exists(this.logPath))
                    {
                        System.IO.FileInfo file = new System.IO.FileInfo(logPath);
                        if (file.Length > 1024 * 1024 * 50)
                            file.Delete();
                    }
                }
                catch
                {
                }
                using (var fs = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(message);
                }
            }
            catch
            {
                /*var logger = EngineContext.Current.Resolve<ILogger>();
                logger.Error(exc.Message, exc);*/
            }
        }

        public void ClearLogFile()
        {
            try
            {
                if (File.Exists(logPath))
                    File.Delete(logPath);
            }
            catch
            {
            }
        }

        public string GetLogFilePath()
        {
            return this.logPath;
        }
    }
}






