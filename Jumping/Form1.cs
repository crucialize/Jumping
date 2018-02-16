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


		private void Jump()
		{
			const double MoveFactor = 5.13;

			//calculate d
			double k, KFactor = 0.581;

			int x1 = StartPoint.X,
				y1 = StartPoint.Y,
				x2 = EndPoint.X,
				y2 = EndPoint.Y;

			k = +KFactor;
			var d1 = Math.Abs(k * (x2 - x1) + y1 - y2) / Math.Sqrt(k * k + 1);

			k = -KFactor;
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

		private void pictureBox1_Click(object sender, EventArgs e)
		{
			var EArg = (MouseEventArgs)e;
			if (EArg.Button == MouseButtons.Left)
			{
				if (state == 0)
				{
					//to select start point
					StartPoint = EArg.Location;
					state = 1;
				}
				else if (state == 1)
				{
					//to select end point
					EndPoint = EArg.Location;
					Jump();
					state = 0;
				}
			}
			else if (EArg.Button == MouseButtons.Right)
			{
				state = 0;//撤销
			}
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
