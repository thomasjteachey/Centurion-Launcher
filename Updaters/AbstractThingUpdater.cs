using System; 
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Ionic.Zip;
using System.Net.Cache;
using IniParser;
using IniParser.Model;
using System.Diagnostics.Eventing.Reader;

public abstract class AbstractThingUpdater
{
    protected string versionFile;
    protected string localVersionFilePath;
    protected string serverUrl;
    protected string latestZipName;
    protected string zipToDownload;
    protected string root;
    protected string extractDirectory;
    protected decimal serverVersion;
    protected WebClient webClient;
    protected BackgroundWorker backgroundWorker1;

    protected IniData parsedData;

    public AbstractThingUpdater(BackgroundWorker backgroundWorker1)
    {
        this.backgroundWorker1 = backgroundWorker1;
    }

    public virtual void initialize()
    {
        parsedData = new FileIniDataParser().LoadFile("launcher.ini");
        serverUrl = parsedData["CENTURION"]["patch.url"];
        root = AppDomain.CurrentDomain.BaseDirectory;
    }


    public bool execute()
    {
        if (checkForUpdates())
        {
            preUpdate();
            UpdateThing();
            postUpdate();
            return true;
        }
        return false;
    }

    public void UpdateThing()
    {
        string sUrlToReadFileFrom = serverUrl + latestZipName;
        string sFilePathToWriteFileTo = root + latestZipName;
        FileInfo prevFile = new FileInfo(sFilePathToWriteFileTo);
        if (prevFile.Exists)
        {
            prevFile.Delete();
        }
        Uri url = new Uri(sUrlToReadFileFrom);
        System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
        System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
        response.Close();

        Int64 iSize = response.ContentLength;

        Int64 iRunningByteTotal = 0;

        using (System.Net.WebClient client = new System.Net.WebClient())
        {
            using (System.IO.Stream streamRemote = client.OpenRead(new Uri(sUrlToReadFileFrom)))
            {
                using (Stream streamLocal = new FileStream(sFilePathToWriteFileTo, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    int iByteSize = 0;
                    byte[] byteBuffer = new byte[iSize];
                    while ((iByteSize = streamRemote.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
                    {
                        streamLocal.Write(byteBuffer, 0, iByteSize);
                        iRunningByteTotal += iByteSize;

                        double dIndex = (double)(iRunningByteTotal);
                        double dTotal = (double)byteBuffer.Length;
                        double dProgressPercentage = (dIndex / dTotal);
                        int iProgressPercentage = (int)(dProgressPercentage * 100);

                        backgroundWorker1.ReportProgress(iProgressPercentage);
                    }

                    streamLocal.Close();
                }

                streamRemote.Close();
            }
        }
        string path = root;

        //unzip
        if (!String.IsNullOrEmpty(extractDirectory))
        {
            path = extractDirectory;
        }
        using (ZipFile zip = ZipFile.Read(latestZipName))
        {
            foreach (ZipEntry zipFiles in zip)
            {
                zipFiles.Extract(path, true);
            }
        }
        //Delete Zip File
        deleteFile(latestZipName);

        using (StreamWriter sw = new StreamWriter(localVersionFilePath))
        {
            sw.Write("" + serverVersion);
        }
    }

    public bool checkForUpdates()
    {
        FileStream fs = null;
        
        if (!File.Exists(localVersionFilePath))
        {
            using (fs = File.Create(localVersionFilePath))
            {

            }

            using (StreamWriter sw = new StreamWriter(localVersionFilePath))
            {
                sw.Write("0.0");
            }
        }

        //checks client version
        string lclVersion;
        using (StreamReader reader = new StreamReader(localVersionFilePath))
        {
            lclVersion = reader.ReadLine();
        }
        decimal localVersion = 0;
        try
        {
            localVersion = decimal.Parse(lclVersion);
        }
        catch (Exception e)
        {

        }

        webClient = new WebClient();
        string serverVersionString = webClient.DownloadString(serverUrl + versionFile);

        try
        {
            serverVersion = decimal.Parse(serverVersionString);
        }
        catch (Exception e)
        {

            serverVersion = 0.1m;
        }
        LogHelper.Log(LogTarget.File, "localVersion: " + localVersion + " serverVersion: " + serverVersion);

        return localVersion != serverVersion;
    }

    public abstract void preUpdate();
    public abstract void postUpdate();

    static bool deleteFile(string f)
    {
        try
        {
            File.Delete(f);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }
}

