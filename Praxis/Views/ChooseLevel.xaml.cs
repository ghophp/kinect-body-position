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
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using Microsoft.Expression.Controls;

namespace Praxis.Views
{
	public partial class ChooseLevel
	{
        private MainWindow main = null;

		public ChooseLevel()
		{
			this.InitializeComponent();

            this.main = (App.Current.MainWindow as MainWindow);
            this.main.SetCalloutContent("Escolha o nível pressionando um dos botões ao lado!");
            this.main.CallMonsterAnimation("Shake");
		}

        private void OnFastClick(object sender, RoutedEventArgs e)
        {
            this.LevelChoosed("fast");
            e.Handled = true;
        }

        private void OnMediumClick(object sender, RoutedEventArgs e)
        {
            this.LevelChoosed("medium");
            e.Handled = true;
        }

        private void OnIntenseClick(object sender, RoutedEventArgs e)
        {
            this.LevelChoosed("intense");
            e.Handled = true;
        }

        private void LevelChoosed(string level)
        {
			MainWindow.level = level;
            this.main.SetBoardContent("Views/Track.xaml");
        }
	}
}