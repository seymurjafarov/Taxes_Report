using Taxes_Report;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Taxes_Report
{
    public partial class Taxes_App : Form
    {
        public Taxes_App()
        {
            InitializeComponent();


            DBMC.CreateDatabase();

            if (DBMC.CheckTable() == false)
            {
                DBMC.CreateTableProc();
            }

            Doldur();

            DataTable dt = GetCompany();
            comboBox1.DataSource = dt;
            comboBox1.DisplayMember = "SIRKET";
        }
        public void Doldur()
        {
            SqlConnection con = new SqlConnection(DALC.GetConnectionString());
            con.Open();
            SqlCommand cmd = new SqlCommand("Select * from VERGI_HESABATI", con);

            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            SqlDataReader dr = cmd.ExecuteReader();

            DataTable dt = new DataTable();

            dt.Load(dr);

            dataGridView1.DataSource = dt;

            con.Close();
        }
        public DataTable GetCompany()
        {
            SqlConnection con = new SqlConnection(DALC.GetConnectionString());
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT SIRKET FROM VERGI_HESABATI GROUP BY SIRKET", con);

            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            con.Close();
            return dt;
        }
        public void GetProfitTaxes(string start, string end)
        {
            SqlConnection con = new SqlConnection(DALC.GetConnectionString());
            con.Open();
            SqlCommand cmd = new SqlCommand("SP_PROFIT_TAXES_REPORT", con);

            cmd.CommandType = CommandType.StoredProcedure;

            SqlParameter START = new SqlParameter("@START", SqlDbType.Date);
            START.Direction = ParameterDirection.Input;
            START.Value = start;
            cmd.Parameters.Add(START);

            SqlParameter END = new SqlParameter("@END", SqlDbType.Date);
            END.Direction = ParameterDirection.Input;
            END.Value = end;
            cmd.Parameters.Add(END);

            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            SqlDataReader dr = cmd.ExecuteReader();

            DataTable dt = new DataTable();

            dt.Load(dr);

            dataGridView1.DataSource = dt;

            con.Close();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            GetProfitTaxes(dateTimePicker1.Text, dateTimePicker2.Text);

        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            GetProfitTaxes(dateTimePicker1.Text, dateTimePicker2.Text);

            
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            

            try
            {
                DataGridViewRow Row = dataGridView1.CurrentRow;
                textBox1.Tag = Row.Cells[1].Value.ToString();
                DateTime dt = Convert.ToDateTime(Row.Cells[2].Value.ToString());
                DateTime dt1 = Convert.ToDateTime(dt.ToString("dd.MM.yyyy".Replace("dd", null)));
                dateTimePicker1.Value = dt1;
                dateTimePicker2.Value = dateTimePicker1.Value.AddMonths(1);

                GetProfitTaxes(dateTimePicker1.Text, dateTimePicker2.Text);

               

            }
            catch (FormatException)
            {

                //MessageBox.Show("Bos xana secile bilmez");
                Doldur();
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(DALC.GetConnectionString());
            con.Open();
            SqlCommand cmd = new SqlCommand("SP_INSERT_REPORT", con);

            cmd.CommandType = CommandType.StoredProcedure;
            string name = comboBox1.Text;
            string money = textBox1.Text;
            string date = dateTimePicker1.Text;

            SqlParameter NAME = new SqlParameter("@SIRKET", SqlDbType.VarChar, 50);
            NAME.Direction = ParameterDirection.Input;
            NAME.Value = name;
            cmd.Parameters.Add(NAME);

            SqlParameter DATE = new SqlParameter("@TARIX", SqlDbType.Date);
            DATE.Direction = ParameterDirection.Input;
            DATE.Value = date;
            cmd.Parameters.Add(DATE);

            SqlParameter MONEY = new SqlParameter("@MEBLEG", SqlDbType.Money);
            MONEY.Direction = ParameterDirection.Input;
            MONEY.Value = money;
            cmd.Parameters.Add(MONEY);

            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            try
            {
                cmd.ExecuteNonQuery();
                MessageBox.Show("Ugurla elave edildi");
            }
            catch (FormatException)
            {

                MessageBox.Show("mebleg bos qala bilmez");
            }
            finally
            {
                con.Close();
                con.Dispose();
            }

            Doldur();
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(DALC.GetConnectionString());
            con.Open();
            SqlCommand cmd = new SqlCommand("Delete VERGI_HESABATI where ID='" + textBox1.Tag + "'", con);
            int count = 0;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            try
            {
                count = cmd.ExecuteNonQuery();
                if (count != 0)
                {
                    MessageBox.Show("silindi");
                }
                else
                {
                    MessageBox.Show("lutfen bir xana secin");
                }

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
            Doldur();
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Doldur();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            DataTable dt = DALC.GetReport();

            DALC.ExportToExcel(dt, null);
        }

    }
}
