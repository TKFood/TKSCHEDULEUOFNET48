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
using System.Security.Cryptography;

namespace TKSCHEDULEUOFNET48
{
    public partial class FrmSCHEDULE : Form
    {
        public FrmSCHEDULE()
        {
            InitializeComponent();
        }


        #region FUNCTION
        private string RSAEncrypt(string publicKey, string crText)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] base64PublicKey = Convert.FromBase64String(publicKey);
            rsa.FromXmlString(System.Text.Encoding.UTF8.GetString(base64PublicKey));
            byte[] ctTextArray = Encoding.UTF8.GetBytes(crText);
            byte[] decodeBs = rsa.Encrypt(ctTextArray, false);
            return Convert.ToBase64String(decodeBs);
        }
        #endregion

        #region BUTTON
        private void button1_Click(object sender, EventArgs e)
        {
            UserUCO useruco = new UserUCO();
            EBUser ebuser = useruco.GetEBUser("b6f50a95-17ec-47f2-b842-4ad12512b431");

            string publicKey = "PFJTQUtleVZhbHVlPjxNb2R1bHVzPitNdXJpamQxZ3YzMmZkVzlZUXdBNVNPa3g3bHR4cFUxYlM2UjZaRGU3Y2hXWFpJQVBXMitiRkVacTRUMEIrR3VTVUFkNDl5QnBkVUtFek1Sa1RwcGtaVFlkVGNOeTBJcVc4UVluWWRXNWdNQjRyNitjZGpobTRPamEyaGJybDVYQzdsY3N6cGVDSUg4TzZ1REQ5N0kxdjBUYUlHZkphejFiM2l6Y3h5R1R6VT08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjwvUlNBS2V5VmFsdWU+";
            string NAME = RSAEncrypt(publicKey, "iteng");
            string PS= RSAEncrypt(publicKey, "P@ssw0rd0");

            Auth.Authentication auth = new Auth.Authentication();
            string TKID=auth.GetToken("IT", NAME, PS);

            // public string SignNext2(string token, string taskId, string siteId, int nodeSeq, string signerGuid);
            Wkf wkf = new Wkf();
           
            wkf.SignNext2("", "", "", 0, "");

        }
        #endregion
    }
}
