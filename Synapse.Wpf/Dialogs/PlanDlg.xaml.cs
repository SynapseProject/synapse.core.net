using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Synapse.Core;

namespace Synapse.Wpf.Dialogs
{
    public partial class PlanDlg : UserControl
    {
        public PlanDlg()
        {
            InitializeComponent();

            txtName.Text = @"C:\Devo\synapse\synapse.core.net\Synapse.UnitTests\Plans\Plans\";

            IEnumerable<string> files = Directory.EnumerateFiles( txtName.Text , "*.yaml" );
            txtName.ItemsSource = files;
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