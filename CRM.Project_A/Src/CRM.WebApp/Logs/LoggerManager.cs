using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CRM.WebApp
{
    public class LoggerManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public LoggerManager()
        {
            FileTarget loggerTarget = (FileTarget)LogManager.Configuration.FindTargetByName("file");
            loggerTarget.DeleteOldFileOnStartup = false;
        }
        public void LogInfo(HttpMethod request, Uri uri)
        {
            logger.Info($"Request: [ {request} ] | URL [ {uri} ]");
        }
        public void LogError(Exception exception, HttpMethod request, Uri uri)
        {
            logger.Error($"\nRequest: [ {request} ] | URL [ {uri} ]\nErr: [ {exception.Message} ] Inner: [ {exception.InnerException?.Message} ]\n" + new string('-', 120));
        }
        public void LogException(Exception ex)
        {
            logger.Log(LogLevel.Fatal, ex, $"\nErr: {ex.Message}\nInner: {ex.InnerException?.Message}\n");
        }
        public string ReadLogErrorData()
        {
            var fileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("file");
            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            string fileName = fileTarget.FileName.Render(logEventInfo);
            if (!File.Exists(fileName))
                File.Create($"{logEventInfo.TimeStamp}.log");
            var data = File.ReadAllLines(fileName);
            string path = System.Web.HttpContext.Current?.Request.MapPath("~//Templates//LogErrors.html");
            var html = File.ReadAllText(path);
            string res = "";
            foreach (string s in data)
                res += s + "</br>";
            var t = html.Replace("{data}", res).Replace("{filename}", fileName);
            return t;
        }
    }
}