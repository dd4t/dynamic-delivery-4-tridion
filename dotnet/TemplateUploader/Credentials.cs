using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TemplateUploader
{
    public partial class Credentials : Form
    {
        Main mainForm = null;
        public Credentials(Main f)
        {
            mainForm = f;
            InitializeComponent();
        }


        private void btnOK_Click(object sender, EventArgs e)
        {
            mainForm.Username = tbUsername.Text;
            mainForm.Password = tbPassword.Text;
            this.Close();
            mainForm.ConnectToTridion();
        }

    }
}
