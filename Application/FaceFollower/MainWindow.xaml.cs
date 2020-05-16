using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
using AForge.Video;
using AForge.Video.DirectShow;

namespace FaceFollower
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly VideoCaptureDevice videoSource;

		public MainWindow()
		{
			InitializeComponent();

			FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
			videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
			videoSource.NewFrame += VideoSource_NewFrame;
			videoSource.Start();
		}

		private void VideoSource_NewFrame(object sender, NewFrameEventArgs args)
		{
			if (args.Frame.Clone() is Bitmap bitmap)
			{
				this.Dispatcher.Invoke(() =>
				{
					BitmapImage source = new BitmapImage();
					using (Stream stream = new MemoryStream())
					{
						bitmap.Save(stream, ImageFormat.Png);
						stream.Position = 0;
						source.BeginInit();
						source.CacheOption = BitmapCacheOption.OnLoad;
						source.StreamSource = stream;
						source.EndInit();
					}
					image.Source = source;
				});
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			videoSource.SignalToStop();

			base.OnClosed(e);
		}
	}
}
