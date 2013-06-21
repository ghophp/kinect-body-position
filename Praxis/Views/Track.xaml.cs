using Microsoft.Expression.Controls;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Praxis.Views
{
    /// <summary>
    /// Interaction logic for Track.xaml
    /// </summary>
    public partial class Track : Page
    {
        public const string POSITION1 = "Position1";
        public const string POSITION1_DESC = "Desloque o quadril para o lado com o braço acima da cabeça.";

        public const string POSITION2 = "Position2";
        public const string POSITION2_DESC = "Incline a cabeça até sentir pressão no músculo do pescoço.";

        private const int MIN_POSITIONS = 2;
        private const int MIN_SECONDS = 5;

        private MainWindow main = null;
        private int currentCount = MIN_SECONDS;
        private DispatcherTimer timer;

        private bool upAndRunning = false;
        public int executedPositions = 0;
		
        public Track()
        {
            InitializeComponent();

            this.main = (App.Current.MainWindow as MainWindow);
            this.main.SetCalloutContent("Vamos começar! Preste atenção na posição à seguir!");
        }

        private void OnPageInitialized(object sender, System.EventArgs e)
        {
            showPosition(POSITION1, POSITION1_DESC);
        }

        private async Task showPosition(string animation, string description)
        {
            await Task.Delay(2000);

            this.currentCountLabel.Visibility = Visibility.Hidden;
            this.controlGrid.Visibility = Visibility.Hidden;

            this.mainTrackLabel.Content = "Atenção para as instruções ao lado!";

            this.main.SetCalloutContent(description);
            this.main.CallMonsterAnimation(animation);

            awaitForPosition(animation);
        }

        private async Task awaitForPosition(string position)
        {
            await Task.Delay(8000);

            this.main.readySound.Play();
            this.main.SetCalloutContent("Agora é sua vez, coloque-se na posição que indiquei!");

            this.currentCountLabel.Visibility = Visibility.Visible;
            this.controlGrid.Visibility = Visibility.Visible;

            this.currentPositionLabel.Content = executedPositions.ToString();
            this.currentCountLabel.Content = MIN_SECONDS.ToString();
            this.mainTrackLabel.Content = "Fique na posição apresentada pelo instrutor e aguarde a contagem começar!";

            this.main.isTracking = position;
        }

        private async Task moveToRestartScreen()
        {
            await Task.Delay(2500);
            this.main.SetBoardContent("Views/Restart.xaml");
        }

        public void startCount()
        {
            if (!this.upAndRunning)
            {
                
                this.mainTrackLabel.Content = "Permaneça na posição por:";
                this.currentCountLabel.Content = MIN_SECONDS.ToString();

                this.timer = new DispatcherTimer();
                this.timer.Tick += new EventHandler(timerTick);
                this.timer.Interval = new TimeSpan(0, 0, 1);
                this.timer.Start();

                this.upAndRunning = true;
            }
        }

        public void stopCount()
        {
            if(this.upAndRunning && this.timer.IsEnabled){

                this.main.errorSound.Play();

                this.upAndRunning = false;
                this.timer.Stop();

                this.currentCount = MIN_SECONDS;
                this.currentCountLabel.Content = this.currentCount.ToString();
            }
        }

        private void timerTick(object sender, EventArgs args)
        {
            this.currentCount--;
            if (this.currentCount <= 0)
            {
                this.main.isTracking = "no-track";
                this.stopCount();

                this.executedPositions++;
                this.currentPositionLabel.Content = executedPositions.ToString();

                this.main.congratsSound.Play();
                if (executedPositions < MIN_POSITIONS)
                {
                    showPosition(POSITION2, POSITION2_DESC);
                }
                else
                {
                    this.main.SetCalloutContent("Vamos continuar ou chega de alongamento?");
                    moveToRestartScreen();
                }
            }
            else
            {
                this.currentCountLabel.Content = currentCount.ToString();
            }
        }
    }
}
