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
using Praxis.Utilities;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using System.Windows.Media.Animation;
using System.Media;
using System.Windows.Resources;

namespace Praxis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int FACTOR = 10;

        private readonly KinectSensorChooser sensorChooser;
        private double ratio = 0.0;

        public SoundPlayer congratsSound;
        public SoundPlayer readySound;
        public SoundPlayer errorSound;
        private SoundPlayer switchSound;
		
		public static KinectSensor currentSensor;
        public static string level = "fast";

        public string isTracking = "no-track";
        public string trackSide = "right";

        private Storyboard calloutChange;
        private int correctionFactor = 0;
        
        public MainWindow()
        {
            InitializeComponent();

            this.switchSound = new SoundPlayer(System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                @"Assets\Sounds\callout.wav"));

            this.congratsSound = new SoundPlayer(System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"Assets\Sounds\congrats.wav"));

            this.readySound = new SoundPlayer(System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"Assets\Sounds\ready.wav"));

            this.errorSound = new SoundPlayer(System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"Assets\Sounds\error.wav"));

            this.calloutChange = (this.FindResource("CalloutChange") as Storyboard);

            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += this.SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            var sensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, sensorBinding);
        }

        private void OnRenderSizeChanged(object sender, SizeChangedEventArgs sizeInfo)
        {
            if (sizeInfo.WidthChanged) 
                this.Width = sizeInfo.NewSize.Height * this.ratio;
            else 
                this.Height = sizeInfo.NewSize.Width / this.ratio;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Width = 1024;
            this.Height = 576;

            this.ratio = this.ActualWidth / this.ActualHeight;
        }

        public void SetBoardContent(string content)
        {
            this.switchSound.Play();
            this.board.Source = new Uri(content, UriKind.Relative);
        }

        public void SetCalloutContent(string value)
        {
            if(!value.Equals(this.callout.Content.ToString())){
                this.callout.Content = value;
                this.calloutChange.Begin();
            }
        }

        public void CallMonsterAnimation(string story)
        {
            Storyboard animation = (this.monster.FindResource(story) as Storyboard);
            if (animation != null)
            {
                animation.Begin();
            }
        }

        /// <summary>
        /// Called when the KinectSensorChooser gets a new sensor
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event arguments</param>
        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {

                    TransformSmoothParameters smoothingParam = new TransformSmoothParameters();
                    smoothingParam.Smoothing = 0.5f;
                    smoothingParam.Correction = 0.5f;
                    smoothingParam.Prediction = 0.5f;
                    smoothingParam.JitterRadius = 0.05f;
                    smoothingParam.MaxDeviationRadius = 0.04f;

                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable(smoothingParam);

                    args.NewSensor.DepthStream.Range = DepthRange.Default;
                    args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;

                    args.NewSensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
                }
                catch (InvalidOperationException)
                {
                    // Abrupted removed
                }
				
				currentSensor = args.NewSensor;
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if(this.isTracking != "no-track"){
                
                Skeleton[] skeletons = new Skeleton[0];

                using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (skeletonFrame != null)
                    {
                        skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                        skeletonFrame.CopySkeletonDataTo(skeletons);

                        if (skeletons.Length > 0)
                        {

                            foreach (Skeleton skel in skeletons)
                            {
                                if (skel.TrackingState == SkeletonTrackingState.Tracked)
                                {
                                    Praxis.Views.Track trackView = (this.board.Content as Praxis.Views.Track);
                                    if (this.correctionFactor > FACTOR)
                                    {
                                        switch (this.isTracking)
                                        {
                                            case Praxis.Views.Track.POSITION1:
                                                this.TrackPositionOne(skel, trackView);
                                                break;
                                            case Praxis.Views.Track.POSITION2:
                                                this.TrackPositionTwo(skel, trackView);        
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        this.correctionFactor++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TrackPositionOne(Skeleton skel, Praxis.Views.Track trackView)
        {
            this.correctionFactor = 0;
            if (skel.Joints[JointType.Spine].TrackingState == JointTrackingState.Tracked &&
                skel.Joints[JointType.ShoulderCenter].TrackingState == JointTrackingState.Tracked)
            {
                Joint rightHand = skel.Joints[JointType.HandRight];
                Joint leftHand = skel.Joints[JointType.HandLeft];
                Joint spine = skel.Joints[JointType.Spine];

                if (rightHand.Position.Y > spine.Position.Y && 
                    leftHand.Position.Y < spine.Position.Y &&
                    rightHand.Position.X < spine.Position.X)
                {

                    double angleBody = Math.Floor(this.AngleBetweenJoints(
                        skel.Joints[JointType.HandRight],
                        skel.Joints[JointType.ShoulderCenter],
                        skel.Joints[JointType.Spine]));

                    if (angleBody > 100 && angleBody <= 135)
                    {
                        trackView.startCount();
                    }
                    else
                    {
                        // Body is not in the angle boundary
                        trackView.stopCount();
                    }
                }
                else
                {
                    // Wrong position detected
                    trackView.stopCount();
                }
            }
            else
            {
                trackView.stopCount();
            }
        }

        private void TrackPositionTwo(Skeleton skel, Praxis.Views.Track trackView)
        {
            this.correctionFactor = 0;
            if (skel.Joints[JointType.Spine].TrackingState == JointTrackingState.Tracked &&
                skel.Joints[JointType.Head].TrackingState == JointTrackingState.Tracked &&
                skel.Joints[JointType.ShoulderCenter].TrackingState == JointTrackingState.Tracked)
            {
                float rightHand = skel.Joints[JointType.HandRight].Position.Y;
                float leftHand = skel.Joints[JointType.HandLeft].Position.Y;
                float spine = skel.Joints[JointType.Spine].Position.Y;

                if (rightHand < spine && leftHand < spine)
                {

                    double angleHead = Math.Floor(this.AngleBetweenJoints(
                        skel.Joints[JointType.Head],
                        skel.Joints[JointType.ShoulderCenter],
                        skel.Joints[JointType.ShoulderRight]));

                    if (angleHead <= 103)
                    {
                        trackView.startCount();
                    }
                    else
                    {
                        // Head is not in the angle boundary
                        trackView.stopCount();
                    }
                }
                else
                {
                    // Wrong position detected
                    trackView.stopCount();
                }
            }
            else
            {
                trackView.stopCount();
            }
        }

        /// <summary>
        /// Return the angle between 3 Joints
        /// </summary>
        /// <param name="j1"></param>
        /// <param name="j2"></param>
        /// <param name="j3"></param>
        /// <returns></returns>
        private double AngleBetweenJoints(Joint j1, Joint j2, Joint j3)
        {
            double Angle = 0;
            double shrhX = j1.Position.X - j2.Position.X;
            double shrhY = j1.Position.Y - j2.Position.Y;
            double shrhZ = j1.Position.Z - j2.Position.Z;
            
            double hsl = vectorNorm(shrhX, shrhY, shrhZ);
            
            double unrhX = j3.Position.X - j2.Position.X;
            double unrhY = j3.Position.Y - j2.Position.Y;
            double unrhZ = j3.Position.Z - j2.Position.Z;
            
            double hul = vectorNorm(unrhX, unrhY, unrhZ);
            double mhshu = shrhX * unrhX + shrhY * unrhY + shrhZ * unrhZ;
            double x = mhshu / (hul * hsl);

            if (x != Double.NaN)
            {
                if (-1 <= x && x <= 1)
                {
                    double angleRad = Math.Acos(x);
                    Angle = angleRad * (180.0 / Math.PI);
                }
                else
                {
                    Angle = 0;
                }
            }
            else
            {
                Angle = 0;
            }

            return Angle;
        }


        /// <summary>
        /// Euclidean norm of 3-component Vector
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private double vectorNorm(double x, double y, double z)
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
        }
		
		public void ResetState()
		{
            this.SetCalloutContent("Para iniciar, mova sua mão até a área ao lado e pressione!");
            this.SetBoardContent("Views/StartArea.xaml");
    	}
	}
}
