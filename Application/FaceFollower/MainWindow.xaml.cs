using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.Structure;

namespace FaceFollower
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly VideoCapture videoSource;
		private readonly CascadeClassifier[] detectors;
		private readonly Mat frame = new Mat();

		private Rectangle? previousFace = null;

		private bool faceDetectionRunning = false;

		public MainWindow()
		{
			InitializeComponent();

			detectors = new[]
			{
				new CascadeClassifier(Path.GetFullPath(@".\Algo\haarcascade_frontalface_default.xml")),
				new CascadeClassifier(Path.GetFullPath(@".\Algo\haarcascade_frontalface_alt.xml")),
				new CascadeClassifier(Path.GetFullPath(@".\Algo\haarcascade_frontalface_alt2.xml")),
				new CascadeClassifier(Path.GetFullPath(@".\Algo\haarcascade_profileface.xml"))
			};

			videoSource = new VideoCapture();
			videoSource.ImageGrabbed += VideoSource_NewFrame;
			videoSource.Start();
		}

		private void VideoSource_NewFrame(object sender, EventArgs args)
		{
			if (videoSource.Retrieve(frame) && frame.ToImage<Bgr, byte>() is Image<Bgr, byte> imageFrame)
			{
				if (faceDetectionRunning)
				{
					Image<Gray, byte> grayFrame = imageFrame.Convert<Gray, byte>();
					List<Rectangle> faces = new List<Rectangle>();
					for (int i = 0; i < detectors.Length; i++)
					{
						faces.AddRange(detectors[i].DetectMultiScale(grayFrame, 1.3, 5));
					}

					if (faces.Count > 0)
					{
						previousFace = MeanFace(faces);
					}

					if (previousFace is Rectangle face)
					{
						imageFrame.Draw(face, new Bgr(faces.Count > 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red), 2);
					}
				}

				this.Dispatcher.Invoke(() =>
				{
					BitmapImage source = new BitmapImage();
					using (Stream stream = new MemoryStream())
					{
						byte[] imageData = imageFrame.ToJpegData();
						stream.Write(imageData, 0, imageData.Length);
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

		private void StartStop_Click(object sender, RoutedEventArgs e)
		{
			if (faceDetectionRunning)
			{
				faceDetectionRunning = false;
				SetStatus("Stopped");
			}
			else
			{
				faceDetectionRunning = true;
				SetStatus("Running...");
			}
		}

		private void SetStatus(string msg)
		{
			status.Text = $"Status {DateTime.Now:hh:mm:ss}: {msg}";
		}

		protected override void OnClosed(EventArgs e)
		{
			videoSource.Stop();

			base.OnClosed(e);
		}

		private Rectangle MeanFace(IEnumerable<Rectangle> faces)
		{
			int top = faces.Min(f => f.Top);
			int left = faces.Min(f => f.Left);
			int right = faces.Max(f => f.Right);
			int bottom = faces.Max(f => f.Bottom);

			double x = (left + right) / 2.0d;
			double y = (top + bottom) / 2.0d;

			List<Rectangle> sorted = faces.OrderBy(f => Math.Sqrt(Math.Pow((f.Left + f.Width / 2.0d) - x, 2) + Math.Pow((f.Top + f.Height / 2.0d) - y, 2))).ToList();
			return sorted[sorted.Count / 2];
		}
	}
}
