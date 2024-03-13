using System; using System.Diagnostics;
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

namespace Launcher_v2
{
    public partial class Form1 : Form
    {

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        //Defines application root
        string Root = AppDomain.CurrentDomain.BaseDirectory;

        string serverUrl;
        string latestZipName;
        string latestLauncherZipName;
        string gameName;
        string versionDirectory;
        string skipFiles;

        IniData parsedData;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public Form1()
        {
            InitializeComponent();
            strtGameBtn.Enabled = false;

            parsedData = new FileIniDataParser().LoadFile("launcher.ini");
            string oldLauncher = parsedData["CENTURION"]["old.launcher.directory"];
            string newLauncher = parsedData["CENTURION"]["new.launcher.directory"];
            string gameDir = parsedData["CENTURION"]["game.directory"];

            DirectoryInfo oldDir = new DirectoryInfo(oldLauncher);
            DirectoryInfo newDir = new DirectoryInfo(newLauncher);

            if (oldDir.Exists)
            {
                Util.clearDirectory(oldLauncher);
                oldDir.Delete();
            }
            if (newDir.Exists)
            { 
                Util.clearDirectory(newLauncher);
                newDir.Delete();
            }

            string cachePath = gameDir + "/Cache";
            DirectoryInfo cache = new DirectoryInfo(cachePath);
            if (cache.Exists) 
            {
                Util.clearDirectory(cachePath);
                cache.Delete();
            }

            //Download progress
            backgroundWorker1.RunWorkerAsync();
        }

        //Makes the form dragable
        private void Form1_MouseDown(object sender,
        System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        //Close Button
        private void closeBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void closeBtn_MouseEnter(object sender, EventArgs e)
        {
            closeBtn.BackgroundImage = Properties.Resources.close2;
        }

        private void closeBtn_MouseLeave(object sender, EventArgs e)
        {
            closeBtn.BackgroundImage = Properties.Resources.close1;
        }

        //Minimize Button
        private void minimizeBtn_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void minimizeBtn_MouseEnter(object sender, EventArgs e)
        {
            minimizeBtn.BackgroundImage = Properties.Resources.minimize2;
        }

        private void minimizeBtn_MouseLeave(object sender, EventArgs e)
        {
            minimizeBtn.BackgroundImage = Properties.Resources.minimize1;
        }

        //Delete File
        protected static bool deleteFile(string f)
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

        private bool checkAndUpdateLauncher()
        {
            LauncherUpdater lu = new LauncherUpdater(backgroundWorker1);
            //downloadLbl.Text = "Updating launcher";
            lu.initialize();
            return lu.execute();
        }

        private bool checkAndUpdateGame()
        {
            GameUpdater gu = new GameUpdater(backgroundWorker1);
            //downloadLbl.Text = "Updating patch7";
            gu.initialize();
            return gu.execute();
        }

        private bool checkAndUpdateAddons()
        {
            AddonUpdater au = new AddonUpdater(backgroundWorker1);
            //downloadLbl.Text = "Updating addons";
            au.initialize();
            return au.execute();
        }


        //background Worker: Handles downloading the updates
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (checkAndUpdateLauncher())
            {
                Application.Exit();
            }
            checkAndUpdateGame();
            checkAndUpdateAddons();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            downloadLbl.ForeColor = System.Drawing.Color.Silver;
            downloadLbl.Text = "Downloading Updates";
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            strtGameBtn.Enabled = true;
            this.downloadLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(121)))), ((int)(((byte)(203)))));
            downloadLbl.Text = "Client is up to date";
        }


        //Starts the game
        private void strtGameBtn_Click(object sender, EventArgs e)
        {
            gameName = parsedData["CENTURION"]["game.name"];
            string workingDirectory = parsedData["CENTURION"]["game.directory"];
            var startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = workingDirectory;
            startInfo.FileName = gameName;
            Process p = Process.Start(startInfo);
            
            this.Close();
        }

        private void patchNotes_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

    }
}
