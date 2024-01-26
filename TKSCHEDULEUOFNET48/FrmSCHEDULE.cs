using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using FastReport;
using FastReport.Data;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Linq;
using TKITDLL;
using System.Text.RegularExpressions;
using Ede.Uof.Utility.Data;
using Ede.Uof.Utility.Page.Common;
using Ede.Uof.EIP.SystemInfo;
using Ede.Uof.Web.PublicAPI.WKF;
using Ede.Uof.EIP.Organization.Util;


namespace TKSCHEDULEUOFNET48
{
    public partial class FrmSCHEDULE : Form
    {
        public FrmSCHEDULE()
        {
            InitializeComponent();
        }


        #region FUNCTION

        #endregion

        #region BUTTON
        private void button1_Click(object sender, EventArgs e)
        {
            UserUCO useruco = new UserUCO();
            EBUser ebuser = useruco.GetEBUser("b6f50a95-17ec-47f2-b842-4ad12512b431");
            // public string SignNext2(string token, string taskId, string siteId, int nodeSeq, string signerGuid);
            //Wkf wkf = new Wkf();
            //wkf.SignNext2("","","",0,"");
        }
        #endregion
    }
}
