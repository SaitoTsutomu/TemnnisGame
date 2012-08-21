using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TennisGame
{
	/// <summary>
	/// Window1.xaml の相互作用ロジック
	/// </summary>
	public partial class Window1 : Window
	{
		private const int portno = 8102;
		private const int mgn = 50;
		internal const float barRate = 0.2F;
		internal const float boardRate = 0.3F;
		private Server svr;
		private bool bFirst = false;
		private DispatcherTimer timer1 = new DispatcherTimer();
		public Window1()
		{
			InitializeComponent();
			gr.SizeChanged += new SizeChangedEventHandler(gr_SizeChanged);
			SetDiskPos(new Point(100, 150));
			SetPadPos(true, 11);
		}
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				if (Properties.Settings.Default.Server == Environment.MachineName
					|| Properties.Settings.Default.Server == "localhost")
				{
					bFirst = true;
					System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(
								new System.Runtime.Remoting.Channels.Tcp.TcpChannel(portno), false);
					BackgroundWorker bw = new BackgroundWorker();
					bw.DoWork += new DoWorkEventHandler(bw_DoWork);
					bw.RunWorkerAsync();
					System.Threading.Thread.Sleep(100);
				}
			}
			catch { bFirst = false; }
			svr = (Server)Activator.GetObject(typeof(Server), "tcp://" +
				Properties.Settings.Default.Server + ":" + portno + "/TennisGame");
			if (!bFirst)
			{
				ca.Position = new Point3D(-ca.Position.X, ca.Position.Y, -ca.Position.Z);
				ca.LookDirection = new Vector3D(-ca.LookDirection.X, ca.LookDirection.Y, -ca.LookDirection.Z);
				svr.IsWaiting = false;
			}
			timer1.Interval = TimeSpan.FromMilliseconds(50);
			timer1.Tick += new EventHandler(timer1_Tick);
			timer1.Start();
			MouseMove += new MouseEventHandler(Window1_MouseMove);
		}
		void Window1_MouseMove(object sender, MouseEventArgs e)
		{
			Point p = e.GetPosition(Viewport3D_test);
			VisualTreeHelper.HitTest(Viewport3D_test, null, HitTest, new PointHitTestParameters(p));
		}
		HitTestResultBehavior HitTest(HitTestResult ht)
		{
			var r = ht as RayMeshGeometry3DHitTestResult;
			if (r == null) return HitTestResultBehavior.Continue;
			svr.Move(bFirst, r.PointHit.X < (svr.BarPos(bFirst) - 0.5 + barRate / 2) * 300);
			return HitTestResultBehavior.Stop;
		}
		void timer1_Tick(object sender, EventArgs e)
		{
			timer1.Stop();
			try
			{
				bool bRun = svr.IsRunning;
				button1.Visibility = bRun ? Visibility.Collapsed : Visibility.Visible;
				if (bRun && bFirst) svr.Do();
				SetDiskPos(svr.DiscPos);
				SetPadPos(bFirst, svr.BarPos(bFirst));
				SetPadPos(!bFirst, svr.BarPos(!bFirst));
				label1.Content = string.Format("({0}-{1})",
					svr.Score(bFirst), svr.Score(!bFirst));
				label2.Visibility = Visibility.Collapsed; 
				if (!svr.IsRunning)
				{
					label2.Visibility = Visibility.Visible; 
					label2.Content = svr.Score(bFirst) == Server.WinPoint ? "You win !" : "You loss";
				}
				timer1.Start();
			}
			catch { }
		}
		void bw_DoWork(object sender, DoWorkEventArgs e)
		{
			System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownServiceType(
				typeof(Server), "TennisGame", System.Runtime.Remoting.WellKnownObjectMode.Singleton);
		}
		private void SetDiskPos(Point pos)
		{
			var tt = (Transform3DGroup)Model_test_disk.Transform;
			var tr = (TranslateTransform3D)tt.Children[0];
			tr.OffsetX = (pos.X - 0.5) * 300;
			tr.OffsetZ = (pos.Y - 0.5) * 400;
		}
		private void SetPadPos(bool bOne, double x)
		{
			var tt = (Transform3DGroup)(bOne ? Model_test_pad1 : Model_test_pad2).Transform;
			var tr = (TranslateTransform3D)tt.Children[0];
			tr.OffsetX = (x - 0.5 + barRate / 2) * 300;
		}
		void gr_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Viewport3D_test.Width = gr.ActualWidth;
			Viewport3D_test.Height = gr.ActualHeight;
		}
		private void button1_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				svr.Reset();
				button1.Visibility = Visibility.Collapsed;
			}
			catch { }
		}
	}
}
