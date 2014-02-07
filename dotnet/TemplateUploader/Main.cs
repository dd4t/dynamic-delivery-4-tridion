using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using TemplateUploader.Properties;
using System.Diagnostics;

namespace TemplateUploader
{
    public partial class Main : Form
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public Main()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            Form credentials = new Credentials(this);
            credentials.ShowDialog();
        }

        private void tbUrl_TextKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && tbUrl.Text != "" && tbFolder.Text != "")
            {
                btnConnect_Click(sender, e);
            }
        }

        public void ConnectToTridion()
        {
            UploadTemplatesToTridion();
        }

        protected void UploadTemplatesToTridion()
        {
            const string TemplatesDllName = "DD4T.Templates.merged.dll";
            const string relPathTcmUploadAssembly = @"TcmUploadAssembly.exe";

            string cwd = Path.GetDirectoryName(Application.ExecutablePath);

            var tcmUploadAssembly = string.Format(@"{0}\{1}", cwd, relPathTcmUploadAssembly);
            //If we cannot find TcmUploadAssembly: exit
            if (!File.Exists(tcmUploadAssembly))
            {
                throw new Exception(Resources.CanNotFindTcmUploadAssembly);
            }

            string cmeUrl = tbUrl.Text;
            string folderUri = tbFolder.Text;
            

            var templatesDllPath = string.Format(@"{0}\{1}", cwd, TemplatesDllName);

            //Debugger.Launch();
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = string.Format(" \"{0}\" /targeturl:{1} /folder:{2} /username:{3} /password:{4} /verbose", templatesDllPath, cmeUrl, folderUri, Username, Password);
            start.FileName = tcmUploadAssembly;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.UseShellExecute = false;
            int exitCode = 0;
            StringBuilder output = new StringBuilder();
            using (Process proc = Process.Start(start))
            {
                using (StreamReader sr = proc.StandardOutput)
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        output.AppendLine(line);
                    }
                }
                proc.WaitForExit();
                exitCode = proc.ExitCode;
            }

            if (exitCode != 0)
            {
                throw new Exception(string.Format(Resources.ErrorMessage, exitCode, output.ToString()));
            }
        }   
    }
}
