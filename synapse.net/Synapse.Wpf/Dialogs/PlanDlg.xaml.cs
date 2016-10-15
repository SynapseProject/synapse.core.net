using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Interaction logic for PlanDlg.xaml
    /// </summary>
    public partial class PlanDlg : UserControl
    {
        public PlanDlg()
        {
            InitializeComponent();
        }

        public void LoadPlan(Plan plan)
        {
            this.DataContext = plan;
        }
    }
}
