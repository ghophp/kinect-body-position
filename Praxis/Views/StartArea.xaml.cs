using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Expression.Controls;
using Praxis.Monster;

namespace Praxis.Views
{
	public partial class StartArea : Page
	{
        private MainWindow main;

		public StartArea()
		{
			this.InitializeComponent();
            this.main = (App.Current.MainWindow as MainWindow);
		}
		
		private void OnStartAreaClick(object sender, RoutedEventArgs e)
        {
            this.main.SetBoardContent("Views/ChooseLevel.xaml");
			e.Handled = true;
        }
	}
}