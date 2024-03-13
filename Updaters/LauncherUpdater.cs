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

public class LauncherUpdater : AbstractThingUpdater
{
    string[] skipFiles = new string[0];
    string newDirectory;
    string oldDirectory;
    string launcherName;

    public LauncherUpdater(BackgroundWorker backgroundWorker) : base(backgroundWorker)
    {
    }

    public override void initialize()
    {
        base.initialize();
        versionFile = parsedData["CENTURION"]["launcher.version.file"];
        localVersionFilePath = parsedData["CENTURION"]["version.directory"] + Path.DirectorySeparatorChar + versionFile;
        latestZipName = parsedData["CENTURION"]["latest.launcher"];
        newDirectory = root + parsedData["CENTURION"]["new.launcher.directory"];
        oldDirectory = root + parsedData["CENTURION"]["old.launcher.directory"];
        extractDirectory = newDirectory;
        launcherName = parsedData["CENTURION"]["launcher.name"];
        string skipFilesString = parsedData["CENTURION"]["skippable.files"];
        skipFiles = skipFilesString.Split(new char[] { ',' });
    }

    public override void postUpdate()
    {
        moveDirectories();

        var startInfo = new ProcessStartInfo();
        startInfo.WorkingDirectory = root;
        startInfo.FileName = launcherName;
        Process p = Process.Start(startInfo);

        System.Environment.Exit(0);
    }


    // we start this method with new directory in "newdirectory" and olddirectory in root
    // we need olddirectory in "olddirectory" and newdirectory in root
    private void moveDirectories()
    {
        System.IO.DirectoryInfo di = new DirectoryInfo(root);
        System.IO.DirectoryInfo newDir = new DirectoryInfo(newDirectory);

        Util.clearDirectory(oldDirectory);

        foreach (FileInfo file in di.GetFiles())
        {
            if (skipFiles.Contains(file.Name))
            {
                continue;
            }
            file.MoveTo(oldDirectory + Path.DirectorySeparatorChar + file.Name);
        }
        foreach (DirectoryInfo dir in di.GetDirectories())
        {
            if (skipFiles.Contains(dir.Name) || dir.FullName.Equals(oldDirectory) || dir.FullName.Equals(newDirectory))
            {
                continue;
            }
            dir.MoveTo(oldDirectory + Path.DirectorySeparatorChar + dir.Name);
        }

        foreach (FileInfo file in newDir.GetFiles())
        {
            file.MoveTo(root + file.Name);
        }
        foreach (DirectoryInfo dir in newDir.GetDirectories())
        {
            if (dir.Name != "settings")
            {
                //don't overwrite settings
                dir.MoveTo(root + dir.Name);
            }
        }
        DirectoryInfo oldDir = new DirectoryInfo(oldDirectory);
    }

    public override void preUpdate()
    {
        System.IO.DirectoryInfo dir1 = new DirectoryInfo(newDirectory);
        System.IO.DirectoryInfo dir2 = new DirectoryInfo(oldDirectory);
        System.IO.DirectoryInfo dir3 = new DirectoryInfo(parsedData["CENTURION"]["version.directory"]);

        dir1.Create();
        dir2.Create();
        dir3.Create();
        Util.clearDirectory(oldDirectory);
        Util.clearDirectory(newDirectory);
    }
}
