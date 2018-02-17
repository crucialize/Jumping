using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAdbClient;

namespace JumpingPro
{
	class MyADB
	{
		public AdbServer server;
		public AdbClient client;
		public DeviceData TargetDevice;

		public MyADB(string AdbShellFilename)
		{
			server = new AdbServer();
			var result = server.StartServer(AdbShellFilename, restartServerIfNewer: false);
			Console.WriteLine("Adb server connection state: " + result.ToString());
			client = new AdbClient();

			TargetDevice = client.GetDevices()[0];
		}

		public void ExecuteADBShell(string command)
		{
			client.ExecuteRemoteCommand(command, TargetDevice, new Receiver());
		}

		public async Task ExecuteADBShellAsync(string command)
		{
			await client.ExecuteRemoteCommandAsync(command, TargetDevice, new Receiver(), System.Threading.CancellationToken.None, 10000);

		}

		public Bitmap GetScreenshot()
		{
			ExecuteADBShell("/system/bin/screencap -p /sdcard/screenshot.png");
			var service = new SyncService(TargetDevice);
			var stream = new MemoryStream();
			service.Pull("/sdcard/screenshot.png", stream, null, System.Threading.CancellationToken.None);

			Bitmap img = new Bitmap(stream);
			stream.Dispose();

			return img;
		}

		public async Task<Bitmap> GetScreenshotAsync()
		{
			await ExecuteADBShellAsync("/system/bin/screencap -p /sdcard/screenshot.png");
			var service = new SyncService(TargetDevice);
			var stream = new MemoryStream();
			service.Pull("/sdcard/screenshot.png", stream, null, System.Threading.CancellationToken.None);

			Bitmap img = new Bitmap(stream);
			stream.Dispose();
			return img;
		}

	}

	class Receiver : IShellOutputReceiver
	{
		bool IShellOutputReceiver.ParsesErrors => throw new NotImplementedException();

		void IShellOutputReceiver.AddOutput(string line)
		{
			//Console.WriteLine("adb reply: " + line);
		}

		void IShellOutputReceiver.Flush()
		{

		}
	}
}
