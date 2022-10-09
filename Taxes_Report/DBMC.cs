using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.CodeDom;
using System.Drawing;
using System.IO;


namespace Taxes_Report
{
    public class DBMC
    {

        public static void CreateDatabase()
        {
            SqlConnection con = new SqlConnection("Data Source=JAFAROV\\SQLEXPRESS;database=master;Integrated Security=true");
            con.Open();
            SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'WORK') BEGIN SELECT 'Verlenler bazasi bu adla artiq movcuddur' AS Message END ELSE BEGIN CREATE DATABASE [WORK] SELECT 'New Database is Created' END", con);

            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                con.Close();
                con.Dispose();
            }

        }
        public static void CreateTableProc()
        {
            SqlConnection con = new SqlConnection(DALC.GetConnectionString());
            con.Open();

            SqlCommand cmd1 = new SqlCommand("Create table VERGI_HESABATI(ID INT NOT NULL PRIMARY KEY IDENTITY(1000,1),SIRKET VARCHAR(50),TARIX DATE,MEBLEG NUMERIC(18,0),NAGD NUMERIC(18,0),XERCLER NUMERIC(18,0),MAAS NUMERIC(18,0))", con);
            SqlCommand cmd2 = new SqlCommand("CREATE FUNCTION CASH_CALCUL(@AMOUNT NUMERIC(18,0)) RETURNS NUMERIC(18,0) AS BEGIN DECLARE @RESULT NUMERIC(18,0) SET @RESULT = @AMOUNT * 0.015 RETURN @RESULT END", con);
            SqlCommand cmd3 = new SqlCommand("CREATE FUNCTION COST_CALCUL(@DATE DATE) RETURNS NUMERIC(18,0) AS BEGIN DECLARE @COST NUMERIC(18,0), @RESULT NUMERIC(18,0) SELECT @COST = XERCLER FROM VERGI_HESABATI WHERE MONTH(TARIX) = DATEPART(MONTH,@DATE) IF(@COST > 0) SET @RESULT = 0 ELSE SET @RESULT = 87 RETURN @RESULT END", con);
            SqlCommand cmd4 = new SqlCommand("CREATE FUNCTION TEAM_SALARY_CALCUL(@COMPANY_NAME VARCHAR(50),@AMOUNT NUMERIC(18,0)) RETURNS NUMERIC(18,0) AS BEGIN DECLARE @RESULT NUMERIC(18,0) IF(@COMPANY_NAME='ELGUN MMC') SET @RESULT = @AMOUNT * 0.773 ELSE SET @RESULT = @AMOUNT * 0.80 RETURN @RESULT END", con);
            SqlCommand cmd5 = new SqlCommand("CREATE PROC SP_PROFIT_TAXES_REPORT\r\n@START DATE,\r\n@END DATE \r\nAS \r\nBEGIN \r\nSELECT SUM(MEBLEG) AS 'MEDAXIL',\r\nSUM(XERCLER+NAGD) AS 'XERCLER',\r\nSUM(MEBLEG-NAGD-XERCLER) AS 'GELIR',\r\nSUM((MEBLEG-NAGD-XERCLER)*0.05) AS 'VERGI',\r\nSUM(MEBLEG-((MEBLEG-NAGD-XERCLER)*0.05)-XERCLER-NAGD-MAAS) AS 'QALIQ'\r\nFROM VERGI_HESABATI \r\nWHERE TARIX BETWEEN @START AND @END \r\nEND", con);
            SqlCommand cmd6 = new SqlCommand("CREATE PROC SP_INSERT_REPORT @SIRKET VARCHAR(50), @TARIX DATE, @MEBLEG MONEY AS BEGIN INSERT INTO VERGI_HESABATI VALUES (@SIRKET, @TARIX, @MEBLEG, DBO.CASH_CALCUL(@MEBLEG), DBO.COST_CALCUL(@TARIX),DBO.TEAM_SALARY_CALCUL(@SIRKET,@MEBLEG)) END", con);

            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                cmd1.ExecuteNonQuery();
                cmd2.ExecuteNonQuery();
                cmd3.ExecuteNonQuery();
                cmd4.ExecuteNonQuery();
                cmd5.ExecuteNonQuery();
                cmd6.ExecuteNonQuery();

                MessageBox.Show("table and prosedures create success");
            }
            catch (SqlException ex)
            {

                MessageBox.Show(ex.Message+"Xetani duzelt");
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }
        public static bool CheckTable()
        {
            bool exist = false;

            SqlConnection con = new SqlConnection(DALC.GetConnectionString());
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'VERGI_HESABATI'", con);

            exist = (int)cmd.ExecuteScalar() > 0;

            return exist;
        }
    }
}
