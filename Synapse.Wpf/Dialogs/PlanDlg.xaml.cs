using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Synapse.Core;

namespace Synapse.Wpf.Dialogs
{
    public partial class PlanDlg : UserControl
    {
        public PlanDlg()
        {
            InitializeComponent();

            txtName.Text = @"C:\Devo\synapse\synapse.core.net\Synapse.UnitTests\Plans\Plans\.yaml";
        }

        public void LoadPlan(Plan plan)
        {
            this.DataContext = plan;
        }

        private void cmdLoadPlan_Click(object sender, RoutedEventArgs e)
        {
            string planYaml = File.ReadAllText( txtName.Text );
            Plan plan = null;
            using( StringReader reader = new StringReader( planYaml ) )
                plan = Plan.FromYaml( reader );
            this.DataContext = plan;
        }
    }
}