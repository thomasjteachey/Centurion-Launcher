using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

public abstract class LogBase
{
    protected readonly object lockObj = new object();
    public abstract void Log(string message);
}

public class FileLogger : LogBase
{
    public string filePath = "launcher.log";
        public override void Log(string message)
    {
        lock (lockObj)
        {
            using (StreamWriter streamWriter = new StreamWriter(filePath))
            {
                streamWriter.WriteLine(message);
                streamWriter.Close();
            }
        }
    }
}

public class EventLogger : LogBase
{
    public override void Log(string message)
    {
        lock (lockObj)
        {
            EventLog m_EventLog = new EventLog("");
            m_EventLog.Source = "IDGEventLog";
            m_EventLog.WriteEntry(message);
        }
    }
}

public class DBLogger : LogBase
{
    string connectionString = string.Empty;
    public override void Log(string message)
    {
        lock (lockObj)
        {
            //Code to log data to the database
        }
    }
}