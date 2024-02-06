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
using Ede.Uof.EIP.Organization.Util;
using System.Security.Cryptography;

namespace TKSCHEDULEUOFNET48
{
    public partial class FrmSCHEDULE : Form
    {
        SqlConnection sqlConn = new SqlConnection();
        SqlCommand sqlComm = new SqlCommand();
        string connectionString;

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

        public void FORM_AUTO_APPROVAL()
        {

            DataTable DT_TASK = SEARCHUOFTB_WKF_TASK("COP240200051");

            UserUCO useruco = new UserUCO();
            EBUser ebuser = useruco.GetEBUser("b6f50a95-17ec-47f2-b842-4ad12512b431");

            string publicKey = "PFJTQUtleVZhbHVlPjxNb2R1bHVzPitNdXJpamQxZ3YzMmZkVzlZUXdBNVNPa3g3bHR4cFUxYlM2UjZaRGU3Y2hXWFpJQVBXMitiRkVacTRUMEIrR3VTVUFkNDl5QnBkVUtFek1Sa1RwcGtaVFlkVGNOeTBJcVc4UVluWWRXNWdNQjRyNitjZGpobTRPamEyaGJybDVYQzdsY3N6cGVDSUg4TzZ1REQ5N0kxdjBUYUlHZkphejFiM2l6Y3h5R1R6VT08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjwvUlNBS2V5VmFsdWU+";
            string NAME = RSAEncrypt(publicKey, "iteng");
            string PS = RSAEncrypt(publicKey, "P@ssw0rd0");

            Auth.Authentication auth = new Auth.Authentication();
            auth.Url = "https://eip.tkfood.com.tw/UOF/PublicAPI/System/Authentication.asmx";
            string TKID = auth.GetToken("IT", NAME, PS);
            //TKID = "AB536ED0CA9438AD6AB714880A37BF0BC68B866A652467F7";
            //TKID = "AB536ED0CA9438AD6AB714880A37BF0B2648F42CEE71B039";


            BPM.Wkf wkf = new BPM.Wkf();
            wkf.Url = "https://eip.tkfood.com.tw/UOF/publicapi/wkf/wkf.asmx";

            string token = TKID;
            string signerGuid = "b6f50a95-17ec-47f2-b842-4ad12512b431";
            string taskId = "";
            string siteId = "";
            int nodeSeq = 0;
            if (DT_TASK != null && DT_TASK.Rows.Count >= 1)
            {
                foreach (DataRow DR in DT_TASK.Rows)
                {
                    taskId = DR["TASK_ID"].ToString();
                    siteId = DR["SITE_ID"].ToString();
                    nodeSeq = Convert.ToInt32(DR["NODE_SEQ"].ToString());
                }

            }



            //SignNext(string token, string taskId, string siteId, int nodeSeq, string signerGuid) 
            //public string SignNext2(string token, string taskId, string siteId, int nodeSeq, string signerGuid);

            wkf.SignNext(token, taskId, siteId, nodeSeq, signerGuid);
            //wkf.SignNext2(token, taskId, siteId, nodeSeq, signerGuid);

        }
        public DataTable SEARCHUOFTB_WKF_TASK(string DOC_NBR)
        {
            SqlDataAdapter adapter1 = new SqlDataAdapter();
            SqlCommandBuilder sqlCmdBuilder1 = new SqlCommandBuilder();
            DataSet ds1 = new DataSet();
            StringBuilder sbSql = new StringBuilder();
            StringBuilder sbSqlQuery = new StringBuilder();

            try
            {
                //connectionString = ConfigurationManager.ConnectionStrings["dberp"].ConnectionString;
                //sqlConn = new SqlConnection(connectionString);

                //20210902密
                Class1 TKID = new Class1();//用new 建立類別實體
                SqlConnectionStringBuilder sqlsb = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["dbUOF"].ConnectionString);

                //資料庫使用者密碼解密
                sqlsb.Password = TKID.Decryption(sqlsb.Password);
                sqlsb.UserID = TKID.Decryption(sqlsb.UserID);

                String connectionString;
                sqlConn = new SqlConnection(sqlsb.ConnectionString);

                sbSql.Clear();
                sbSqlQuery.Clear();

                //庫存數量看LA009 IN ('20004','20006','20008','20019','20020'

                sbSql.AppendFormat(@"  
                                    SELECT TB_WKF_TASK.TASK_ID,SITE_ID,NODE_SEQ,ORIGINAL_SIGNER
                                    FROM [UOF].[dbo].TB_WKF_TASK,[UOF].dbo.TB_WKF_TASK_NODE
                                    WHERE 1=1
                                    AND TB_WKF_TASK.TASK_ID=TB_WKF_TASK_NODE.TASK_ID
                                    AND ISNULL(TB_WKF_TASK_NODE.ACTUAL_SIGNER,'')=''
                                    AND ORIGINAL_SIGNER='b6f50a95-17ec-47f2-b842-4ad12512b431'
                                    AND DOC_NBR = '{0}'
                              
                                    ", DOC_NBR);


                adapter1 = new SqlDataAdapter(@"" + sbSql, sqlConn);

                sqlCmdBuilder1 = new SqlCommandBuilder(adapter1);
                sqlConn.Open();
                ds1.Clear();
                adapter1.Fill(ds1, "ds1");
                sqlConn.Close();

                if (ds1.Tables["ds1"].Rows.Count >= 1)
                {
                    return ds1.Tables["ds1"];
                }
                else
                {
                    return null;
                }

            }
            catch
            {
                return null;
            }
            finally
            {
                sqlConn.Close();
            }
        }

        #endregion


        #region BUTTON
        private void button1_Click(object sender, EventArgs e)
        {
            FORM_AUTO_APPROVAL();
        }
        #endregion
    }
}
