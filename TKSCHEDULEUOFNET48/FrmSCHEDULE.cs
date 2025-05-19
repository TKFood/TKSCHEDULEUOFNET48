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
        string APPROVEAL_NAME = "";
        string APPROVEAL_PS = "";

        public FrmSCHEDULE()
        {
            InitializeComponent();

            timer1.Enabled = true;
            timer1.Interval = 1000 * 60;
            //timer1.Interval = 1000 ;
            timer1.Start();
        }

        private void FrmSCHEDULE_Load(object sender, EventArgs e)
        {
            //APPROVEAL_NAME = "iteng";
            //APPROVEAL_PS = "PP@ssw0rd0";

            DataTable DT = SEARCHUOF_TB_APPROVAL_ACCOUNT();

            if (DT != null && DT.Rows.Count >= 1)
            {
                APPROVEAL_NAME = DT.Rows[0]["APPROVEAL_NAME"].ToString();
                APPROVEAL_PS = DT.Rows[0]["APPROVEAL_PS"].ToString();
            }


        }
        #region FUNCTION
        private void timer1_Tick(object sender, EventArgs e)
        {
            //檢查當前時間是否在星期一到星期五
            //早上 8 點到下午 6 點之間，如果是，則執行您的程式碼
            //獲取當前時間
            DateTime currentTime = DateTime.Now;

            // 檢查是否為星期一到星期五
            if (currentTime.DayOfWeek >= DayOfWeek.Monday && currentTime.DayOfWeek <= DayOfWeek.Saturday)
            {
                // 檢查是否在早上8:00到下午18:00之間
                if (currentTime.Hour >= 8 && currentTime.Hour <= 18)
                {
                    DAILY_RUN();
                }
            }
        }

        public void DAILY_RUN()
        {     
            try
            {

            }
            catch { }
            finally{ }
            try
            {
                //指定自動簽核
                DataTable DT = SEARCHUOFTB_WKF_TASK();
                FORM_AUTO_APPROVAL(DT);
            }
            catch { }
            finally { }
            try
            {
                //自動簽核-指定「100A.客戶基本資料表」的財務+總經理代簽
                DataTable DT = SEARCHUOFTB_WKF_TASK_100A();
                FORM_AUTO_APPROVAL(DT);
            }
            catch { }
            finally { }
        }

        private string RSAEncrypt(string publicKey, string crText)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] base64PublicKey = Convert.FromBase64String(publicKey);
            rsa.FromXmlString(System.Text.Encoding.UTF8.GetString(base64PublicKey));
            byte[] ctTextArray = Encoding.UTF8.GetBytes(crText);
            byte[] decodeBs = rsa.Encrypt(ctTextArray, false);
            return Convert.ToBase64String(decodeBs);
        }

        public void FORM_AUTO_APPROVAL(DataTable DT)
        {


            //SignNext(string token, string taskId, string siteId, int nodeSeq, string signerGuid) 
            //public string SignNext2(string token, string taskId, string siteId, int nodeSeq, string signerGuid);
            //wkf.SignNext2(token, taskId, siteId, nodeSeq, signerGuid);

            try
            {
                //DataTable DT_TASK = SEARCHUOFTB_WKF_TASK();
                DataTable DT_TASK = DT;

                //UserUCO useruco = new UserUCO();
                //EBUser ebuser = useruco.GetEBUser("b6f50a95-17ec-47f2-b842-4ad12512b431");

                string publicKey = "PFJTQUtleVZhbHVlPjxNb2R1bHVzPitNdXJpamQxZ3YzMmZkVzlZUXdBNVNPa3g3bHR4cFUxYlM2UjZaRGU3Y2hXWFpJQVBXMitiRkVacTRUMEIrR3VTVUFkNDl5QnBkVUtFek1Sa1RwcGtaVFlkVGNOeTBJcVc4UVluWWRXNWdNQjRyNitjZGpobTRPamEyaGJybDVYQzdsY3N6cGVDSUg4TzZ1REQ5N0kxdjBUYUlHZkphejFiM2l6Y3h5R1R6VT08L01vZHVsdXM+PEV4cG9uZW50PkFRQUI8L0V4cG9uZW50PjwvUlNBS2V5VmFsdWU+";
                string NAME = RSAEncrypt(publicKey, APPROVEAL_NAME);
                string PS = RSAEncrypt(publicKey, APPROVEAL_PS);

                Auth.Authentication auth = new Auth.Authentication();
                auth.Url = "https://eip.tkfood.com.tw/UOF/PublicAPI/System/Authentication.asmx";
                string TKID = auth.GetToken("IT", NAME, PS);
                //TKID = "AB536ED0CA9438AD6AB714880A37BF0BC68B866A652467F7";
                //TKID = "AB536ED0CA9438AD6AB714880A37BF0B2648F42CEE71B039";


                BPM.Wkf wkf = new BPM.Wkf();
                wkf.Url = "https://eip.tkfood.com.tw/UOF/publicapi/wkf/wkf.asmx";

                string token = TKID;
                string signerGuid = "";
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
                        signerGuid = DR["ORIGINAL_SIGNER"].ToString();

                        wkf.SignNext(token, taskId, siteId, nodeSeq, signerGuid);
                    }

                    MessageBox.Show("完成");

                }

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            { }
            

        }
        public DataTable SEARCHUOF_TB_APPROVAL_ACCOUNT()
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
                SqlConnectionStringBuilder sqlsb = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString);

                //資料庫使用者密碼解密
                sqlsb.Password = TKID.Decryption(sqlsb.Password);
                sqlsb.UserID = TKID.Decryption(sqlsb.UserID);

                String connectionString;
                sqlConn = new SqlConnection(sqlsb.ConnectionString);

                sbSql.Clear();
                sbSqlQuery.Clear();

                //庫存數量看LA009 IN ('20004','20006','20008','20019','20020'

                sbSql.AppendFormat(@"                                     
                                   SELECT 
                                    [APPROVEAL_NAME]
                                    ,[APPROVEAL_PS]
                                    FROM [TKSCHEDULEUOFNET48].[dbo].[TB_APPROVAL_ACCOUNT]
                                                                 
                                    ");


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
        public DataTable SEARCHUOFTB_WKF_TASK()
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
                                    SELECT TB_WKF_TASK.TASK_ID,SITE_ID,NODE_SEQ,ORIGINAL_SIGNER,TB_WKF_FORM.FORM_NAME,TB_WKF_TASK.DOC_NBR,TB_WKF_TASK.TASK_RESULT,TB_WKF_TASK.TASK_STATUS
                                    FROM [UOF].[dbo].TB_WKF_TASK,[UOF].dbo.TB_WKF_TASK_NODE,[UOF].[dbo].TB_EB_USER,[UOF].[dbo].TB_WKF_FORM,[UOF].[dbo].TB_WKF_FORM_VERSION
                                    WHERE 1=1
                                    AND TB_WKF_TASK.TASK_ID=TB_WKF_TASK_NODE.TASK_ID
                                    AND TB_WKF_TASK_NODE.ORIGINAL_SIGNER=TB_EB_USER.USER_GUID
                                    AND TB_WKF_FORM.FORM_ID=TB_WKF_FORM_VERSION.FORM_ID
                                    AND TB_WKF_TASK.FORM_VERSION_ID=TB_WKF_FORM_VERSION.FORM_VERSION_ID
                                    AND TB_WKF_TASK.TASK_STATUS NOT IN ('2')
                                    AND ISNULL(TB_WKF_TASK_NODE.ACTUAL_SIGNER,'')=''
                                    AND TB_EB_USER.ACCOUNT+TB_WKF_FORM.FORM_NAME COLLATE Chinese_Taiwan_Stroke_BIN IN (
                                    SELECT 
                                    [ACCOUNT]+[FORM_NAME]
                                    FROM [192.168.1.105].[TKSCHEDULEUOFNET48].[dbo].[TB_UOF_FORM_APPROVALS]
                                    )
                              
                                    ");


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

        public DataTable SEARCHUOFTB_WKF_TASK_100A()
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
                                    SELECT TB_WKF_TASK.TASK_ID,SITE_ID,NODE_SEQ,ORIGINAL_SIGNER,TB_WKF_FORM.FORM_NAME,TB_WKF_TASK.DOC_NBR,TB_WKF_TASK.TASK_RESULT,TB_WKF_TASK.TASK_STATUS
                                    FROM [UOF].[dbo].TB_WKF_TASK,[UOF].dbo.TB_WKF_TASK_NODE,[UOF].[dbo].TB_EB_USER,[UOF].[dbo].TB_WKF_FORM,[UOF].[dbo].TB_WKF_FORM_VERSION
                                    WHERE 1=1
                                    AND TB_WKF_TASK.TASK_ID=TB_WKF_TASK_NODE.TASK_ID
                                    AND TB_WKF_TASK_NODE.ORIGINAL_SIGNER=TB_EB_USER.USER_GUID
                                    AND TB_WKF_FORM.FORM_ID=TB_WKF_FORM_VERSION.FORM_ID
                                    AND TB_WKF_TASK.FORM_VERSION_ID=TB_WKF_FORM_VERSION.FORM_VERSION_ID
                                    AND TB_WKF_TASK.TASK_STATUS NOT IN ('2')
                                    AND ISNULL(TB_WKF_TASK_NODE.ACTUAL_SIGNER,'')=''
                                    AND TB_EB_USER.ACCOUNT+TB_WKF_FORM.FORM_NAME COLLATE Chinese_Taiwan_Stroke_BIN IN (
                                    SELECT 
                                    [ACCOUNT]+[FORM_NAME]
                                    FROM [192.168.1.105].[TKSCHEDULEUOFNET48].[dbo].[TB_UOF_FORM_APPROVALS_100A]
                                    )
                              
                                    ");


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
            //SEARCHUOFTB_WKF_TASK，先找自動簽的工號+表單
            DataTable DT= SEARCHUOFTB_WKF_TASK();

            FORM_AUTO_APPROVAL(DT);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            //自動簽核-指定「100A.客戶基本資料表」的財務+總經理代簽
            DataTable DT = SEARCHUOFTB_WKF_TASK_100A();

            FORM_AUTO_APPROVAL(DT);
        }



        #endregion

      
    }
}
