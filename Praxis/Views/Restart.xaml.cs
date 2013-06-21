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

namespace Praxis.Views
{
    /// <summary>
    /// Interaction logic for Restart.xaml
    /// </summary>
    public partial class Restart : Page
    {
        private MainWindow main = null;

        public Restart()
        {
            InitializeComponent();
            this.main = (App.Current.MainWindow as MainWindow);
        }

        private void OnYesClick(object sender, RoutedEventArgs e)
        {
            this.main.SetBoardContent("Views/ChooseLevel.xaml");
        }

        private void OnNoClick(object sender, RoutedEventArgs e)
        {
            this.main.ResetState();
        }
    }
}
