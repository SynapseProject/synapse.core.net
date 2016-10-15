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

namespace Synapse.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Plan plan = null;
            string planYaml = File.ReadAllText( @"C:\Devo\git\Synapse\synapse.net\Synapse.Tester\yaml\example.yml" );
            planYaml = File.ReadAllText( @"C:\Devo\git\Synapse\synapse.net\Synapse.cli\bin\Debug\plan0.result.yml" );
            using( StringReader reader = new StringReader( planYaml ) )
                plan = Plan.FromYaml( reader );
            planDlg.LoadPlan( plan );
        }
    }
}