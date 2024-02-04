using System;
using System.Drawing;
using System.Windows.Forms;
using TestProject3.Classes;

namespace TestProject3.Forms
{
    public partial class fParcelProperties : Form
    {
        private cParcelObject m_CurrentParcelObject = null;

        public fParcelProperties(cParcelObject po)
        {
            InitializeComponent();

            m_CurrentParcelObject = po;
            FillForm();
            ShowDialog();
        }

        private void FillForm()
        {
            txtNumber.Text = m_CurrentParcelObject.Number.ToString();
            cmbSold.SelectedIndex = m_CurrentParcelObject.IsSold;
            txtOwner.Text = m_CurrentParcelObject.Name;
            txtPrice.Text = m_CurrentParcelObject.TotalPriceAsString;
            txtArea.Text = m_CurrentParcelObject.Area.ToString("N2");
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            m_CurrentParcelObject.IsSold = cmbSold.SelectedIndex;
            m_CurrentParcelObject.Name = txtOwner.Text;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
