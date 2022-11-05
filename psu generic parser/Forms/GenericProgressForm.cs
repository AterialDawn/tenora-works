using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace psu_generic_parser.Forms
{
    public partial class GenericProgressForm : Form
    {
        public string DescriptionText { get { return descriptionLabel.Text; } set { descriptionLabel.Text = value; } }
        public double ProgressPercent { get { return (double)progressBar.Value / (double)progressBar.Maximum; } set { progressBar.Value = (int)Math.Floor((Math.Max(Math.Min(1.0, value), 0.0) * progressBar.Maximum)); } }
        public GenericProgressForm()
        {
            InitializeComponent();
        }
    }
}
