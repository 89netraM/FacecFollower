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
using AForge.Video;
using AForge.Video.DirectShow;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace FaceFollower
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private static readonly string azureKey = Environment.GetEnvironmentVariable("AZURE_FACE_KEY");
		private static readonly string azureEndpoint = Environment.GetEnvironmentVariable("AZURE_FACE_ENDPOINT");

		private readonly VideoCaptureDevice videoSource;
		private readonly IFaceClient faceClient;

		private Bitmap latestImage = new Bitmap(1, 1);
		private bool faceDetectionRunning = false;

		public MainWindow()
		{
			InitializeComponent();

			FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
			videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
			videoSource.NewFrame += VideoSource_NewFrame;
			videoSource.Start();

			faceClient = new FaceClient(new ApiKeyServiceClientCredentials(azureKey), new DelegatingHandler[] { });
			faceClient.Endpoint = azureEndpoint;
		}

		private void VideoSource_NewFrame(object sender, NewFrameEventArgs args)
		{
			if (args.Frame.Clone() is Bitmap bitmap)
			{
				lock (latestImage)
				{
					latestImage = bitmap;
				}
				this.Dispatcher.Invoke(() =>
				{
					BitmapImage source = new BitmapImage();
					using (Stream stream = new MemoryStream())
					{
						bitmap.Save(stream, ImageFormat.Bmp);
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

		private void image_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			canvas.Width = e.NewSize.Width;
		}

		private void StartStop_Click(object sender, RoutedEventArgs e)
		{
			if (faceDetectionRunning)
			{
				faceDetectionRunning = false;
				faceRect.Visibility = Visibility.Hidden;
				SetStatus("Stopped");
			}
			else
			{
				faceDetectionRunning = true;
				Task.Run(Timer_Tick);
				SetStatus("Running...");
			}
		}

		private async void Timer_Tick()
		{
			while (faceDetectionRunning)
			{
				try
				{
					IList<DetectedFace> faces;
					using (Stream stream = new MemoryStream())
					{
						lock (latestImage)
						{
							latestImage.Save(stream, ImageFormat.Bmp);
						}
						stream.Position = 0;
						faces = await faceClient.Face.DetectWithStreamAsync(stream);
					}

					if (faceDetectionRunning)
					{
						this.Dispatcher.Invoke(() =>
						{
							if (faces.Count > 0 && faces[0] is DetectedFace face)
							{
								faceRect.Visibility = Visibility.Visible;
								faceRect.Width = face.FaceRectangle.Width / (double)latestImage.Width * image.ActualWidth;
								faceRect.Height = face.FaceRectangle.Height / (double)latestImage.Height * image.ActualHeight;
								Canvas.SetLeft(faceRect, face.FaceRectangle.Left / (double)latestImage.Width * image.ActualWidth);
								Canvas.SetTop(faceRect, face.FaceRectangle.Top / (double)latestImage.Height * image.ActualHeight);
								SetStatus($"Running: Found face [{face.FaceRectangle.Left}, {face.FaceRectangle.Top}, {face.FaceRectangle.Width}, {face.FaceRectangle.Height}]");
							}
							else
							{
								SetStatus("Running: No face found");
							}
						});
					}
				}
				// Catch and display Face API errors.
				catch (APIErrorException ex)
				{
					MessageBox.Show(ex.Message, "API Error");
				}

				await Task.Delay(3000);
			}
		}

		private void SetStatus(string msg)
		{
			status.Text = $"Status {DateTime.Now:hh:mm:ss}: {msg}";
		}

		protected override void OnClosed(EventArgs e)
		{
			videoSource.SignalToStop();

			base.OnClosed(e);
		}
	}
}
