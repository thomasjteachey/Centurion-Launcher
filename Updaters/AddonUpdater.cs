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

public class AddonUpdater : AbstractThingUpdater
{

    public AddonUpdater(BackgroundWorker backgroundWorker) : base(backgroundWorker)
    {
    }

    public override void initialize()
    {
        base.initialize();
        versionFile = parsedData["CENTURION"]["addon.version.file"];
        localVersionFilePath = parsedData["CENTURION"]["version.directory"] + Path.DirectorySeparatorChar + versionFile;
        latestZipName = parsedData["CENTURION"]["latest.addon"];
        extractDirectory = root + parsedData["CENTURION"]["game.directory"] + "/Interface/AddOns/";
    }

    public override void postUpdate()
    {
    }

    public override void preUpdate()
    {
        //System.IO.DirectoryInfo dir1 = new DirectoryInfo(extractDirectory);
        //dir1.Create();
        //clearDirectory(extractDirectory);
    }
}
