using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataEntry_Project
{
    public partial class frmAuthors : Form
    {
        
        
        public frmAuthors()
        {
            InitializeComponent();
        }

        SqlConnection booksConn;
        SqlCommand authorsComm;
        SqlDataAdapter authorsAdapter;
        DataTable authorsTable;
        CurrencyManager authorsManager;
        SqlCommandBuilder builderComm;
        bool dbError=false;
        public string AppState { get; set; }

        private void frmAuthors_Load(object sender, EventArgs e)
        {
            var connString = @"Server = DESKTOP-0JJ0PH8; Database = PRUEBA; Trusted_Connection = True;";
            booksConn = new SqlConnection(connString);
            booksConn.Open();
            authorsComm = new SqlCommand("SELECT * from Authors Order By Author", booksConn);
            authorsAdapter = new SqlDataAdapter();
            authorsTable = new DataTable();
            authorsAdapter.SelectCommand= authorsComm;
            authorsAdapter.Fill(authorsTable);

            txtAuthorID.DataBindings.Add("Text", authorsTable, "AU_ID");
            txtAuthorName.DataBindings.Add("Text", authorsTable, "Author");
            txtAuthorBorn.DataBindings.Add("Text", authorsTable, "Year_Born");
            authorsManager = (CurrencyManager)BindingContext[authorsTable];

            SetAppState("View");



        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            authorsManager.Position--;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            authorsManager.Position++;
        }

        private void frmClosing(object sender, FormClosingEventArgs e)
        {
            booksConn.Close();
            booksConn.Dispose();
            authorsComm.Dispose();
            authorsAdapter.Dispose();
            authorsTable.Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            try
            {
                var savedRecord = txtAuthorName.Text;
                authorsManager.EndCurrentEdit();
                builderComm = new SqlCommandBuilder(authorsAdapter);

                if (AppState == "Edit")
                {
                    var authRow = authorsTable.Select("Au_ID = " + txtAuthorID.Text);

                    if (String.IsNullOrEmpty(txtAuthorBorn.Text))
                        authRow[0]["Year_Born"] = DBNull.Value;
                    else
                        authRow[0]["Year_Born"] = txtAuthorBorn.Text;

                    authorsAdapter.Update(authorsTable);
                    txtAuthorBorn.DataBindings.Add("Text", authorsTable, "Year_Born");
                }
                else
                {
                    authorsTable.DefaultView.Sort = "Author";
                    authorsManager.Position = authorsTable.DefaultView.Find(savedRecord);
                    authorsAdapter.Update(authorsTable);
                }

                MessageBox.Show("Record saved", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetAppState("View");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Saving record", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult response;
            response = MessageBox.Show("Are you sure you want to delete this record", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (response == DialogResult.No) 
            {
                return;

            }
            try
            {
                authorsManager.RemoveAt(authorsManager.Position);
                builderComm = new SqlCommandBuilder(authorsAdapter);
                authorsAdapter.Update(authorsTable);
                AppState = "Delete";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error deleting record", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //Validacion Botones Visibles por default y al presionar un boton
        private void SetAppState(string appState)
        {
            switch (appState)
            {
                case "View":
                    txtAuthorName.ReadOnly= true;
                    txtAuthorBorn.ReadOnly= true;
                    btnFirst.Enabled= true;
                    btnLast.Enabled= true;
                    btnPrevious.Enabled= true;
                    btnNext.Enabled= true;
                    btnSave.Enabled= false;
                    btnCancel.Enabled= false;
                    btnAddNew.Enabled= true;
                    btnDelete.Enabled= true;
                    btnDone.Enabled= true;
                    btnCancel.Enabled= false;
                    txtAuthorName.TabStop=false;
                    txtAuthorBorn.TabStop=false;
                    break;
                default://add and edit states

                    txtAuthorName.ReadOnly = false;
                    txtAuthorBorn.ReadOnly = false;
                    btnFirst.Enabled = false;
                    btnLast.Enabled = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnSave.Enabled = true;
                    btnEdit.Enabled = true;
                    btnCancel.Enabled = true;
                    btnAddNew.Enabled = false;
                    btnDelete.Enabled = false;
                    btnDone.Enabled = false;
                    txtAuthorName.Focus();
                    break;
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            txtAuthorBorn.DataBindings.Clear();
            SetAppState("Edit");
            AppState = "Edit";
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            try
            {
                authorsManager.AddNew();
                SetAppState("Add");
                AppState= "Add";
            }
            catch
            {

            }
            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            SetAppState("View");
        }

        private void txtAuthorBorn_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9' ) || e.KeyChar == 8)
            {
                e.Handled = false;
                lblWrongInput.Visible= false;
            }
            else
            {
                e.Handled = true;
                lblWrongInput.Visible= true;
            }
        }
        //Validacion de datos vacios y solo numeros
        private bool ValidateInput()
        {
            string message = "";
            int inputYear, currentYear;
            bool allOK = true;

            if (txtAuthorName.Text.Trim().Equals(""))
            {
                message = "Author' s name is required" + "r\n";
                txtAuthorName.Focus();
                allOK = false;
            }
            if (!txtAuthorBorn.Text.Trim().Equals(""))
            {
                inputYear = Convert.ToInt32(txtAuthorBorn.Text);
                currentYear = DateTime.Now.Year;
                if (inputYear >= currentYear)
                {
                    message += "Invalid Year";
                    txtAuthorBorn.Focus();
                    allOK = false;
                }

            }
                
            if (!allOK)
            {
                MessageBox.Show(message, "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

                return allOK;
                
            
            
        }

        private void txtAuthorName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 13) 
            {
                txtAuthorBorn.Focus();
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            authorsManager.Position = 0;
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            authorsManager.Position = authorsManager.Count - 1;
        }
    }
}

