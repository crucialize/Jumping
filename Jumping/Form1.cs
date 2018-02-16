using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpAdbClient;

namespace Jumping
{
	public partial class Form1 : Form
	{
		AdbServer server;
		AdbClient client;
		DeviceData TargetDevice;
		bool GettingScreenshot = false;

		void ExecuteADBShell(string command)
		{
			client.ExecuteRemoteCommand(command, TargetDevice, new Receiver());
		}

		async Task ExecuteADBShellAsync(string command)
		{
			await client.ExecuteRemoteCommandAsync(command, TargetDevice, new Receiver(),System.Threading.CancellationToken.None,10000);

		}

		Bitmap GetOneScreenshot()
		{
			ExecuteADBShell("/system/bin/screencap -p /sdcard/screenshot.png");
			var service = new SyncService(TargetDevice);
			var stream = new MemoryStream();
			service.Pull("/sdcard/screenshot.png", stream, null, System.Threading.CancellationToken.None);

			Bitmap img = new Bitmap(stream);
			stream.Dispose();

			return img;
		}

		async Task<Bitmap> GetOneScreenshotAsync()
		{
			await ExecuteADBShellAsync("/system/bin/screencap -p /sdcard/screenshot.png");
			var service = new SyncService(TargetDevice);
			var stream = new MemoryStream();
			service.Pull("/sdcard/screenshot.png", stream, null, System.Threading.CancellationToken.None);

			Bitmap img = new Bitmap(stream);
			stream.Dispose();
			return img;
		}

		async void GetAndShowContinously()
		{
			var img = await GetOneScreenshotAsync();
			pictureBox1.Image = img;
			Thread.Sleep(50);//in case adb.IO very fast
			GetAndShowContinously();
		}

		public Form1()
		{
			server = new AdbServer();
			var result = server.StartServer(@"C:\adb\adb.exe", restartServerIfNewer: false);
			Console.WriteLine("Adb server connection state: " + result.ToString());

			client = new AdbClient();
			var devices = client.GetDevices();

			TargetDevice = devices[0];//to-do: query user; 0 devices;

			InitializeComponent();
			GetAndShowContinously();
		}

		Point StartPoint, EndPoint, SelectedPoint;
		int state = 0;

		private void button4_Click(object sender, EventArgs e)
		{
			var img = GetOneScreenshot();
			pictureBox1.Image = img;
		}

		private void button1_Click_1(object sender, EventArgs e)
		{
			StartPoint = SelectedPoint;
		}

		private void button3_Click(object sender, EventArgs e)
		{
			const double MoveFactor = 4.55;

			//calculate d
			double k;

			int x1 = StartPoint.X,
				y1 = StartPoint.Y,
				x2 = EndPoint.X,
				y2 = EndPoint.Y;

			k = 1;
			var d1 = Math.Abs(k * (x2 - x1) + y1 - y2) / Math.Sqrt(k * k + 1);

			k = -1;
			var d2 = Math.Abs(k * (x2 - x1) + y1 - y2) / Math.Sqrt(k * k + 1);

			if (d1 > d2)
				Console.WriteLine("/");
			else if (d1 < d2)
				Console.WriteLine("\\");
			else
				Console.WriteLine("=");

			var D = Math.Max(d1, d2);

			var time = (int)(D * MoveFactor);

			ExecuteADBShell("input swipe 500 500 500 500 " + time.ToString());

		}

		private void button2_Click(object sender, EventArgs e)
		{
			EndPoint = SelectedPoint;
		}


		private void pictureBox1_Click(object sender, EventArgs e)
		{
			var CurrentPoint = ((MouseEventArgs)e).Location;
			SelectedPoint = CurrentPoint;
			Console.WriteLine("Selected.");
		}
	}

	class Receiver : IShellOutputReceiver
	{
		bool IShellOutputReceiver.ParsesErrors => throw new NotImplementedException();

		void IShellOutputReceiver.AddOutput(string line)
		{
			Console.WriteLine("adb reply: " + line);
		}

		void IShellOutputReceiver.Flush()
		{

		}
	}

}
