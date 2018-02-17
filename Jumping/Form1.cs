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
			lock (pictureBox1)
			{
				pictureBox1.Image = img;
			}

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


		private void button1_Click(object sender, EventArgs e)
		{
			lock (pictureBox1)
			{
				//please release
				Bitmap img = new Bitmap(pictureBox1.Image);
				var StartP=CalculateStartPoint();
				var EndP = CalculateEndPoint();
				//CrossMark(StartP.X, StartP.Y, Color.Green);
				//CrossMark(EndP.X, EndP.Y, Color.Red);

				var Time = CalculateJumpTime(StartP.X, StartP.Y, EndP.X, EndP.Y);
				var r = new Random();
				int TapX = r.Next(100, 1000);
				int TapY = r.Next(600, 1600);
				ExecuteADBShell(string.Format("input swipe {0} {1} {2} {3} {4}",TapX,TapY,TapX,TapY,Time));
				Console.WriteLine(Time);
				//pictureBox1.Image = img;
				//Thread.Sleep(2000);

				/// <summary>
				/// 注意：xy对应1080p的坐标
				/// </summary>
				/// <param name="x1"></param>
				/// <param name="y1"></param>
				/// <param name="x2"></param>
				/// <param name="y2"></param>
				int CalculateJumpTime(int x1, int y1, int x2, int y2)
				{
					const double MoveFactor = 1.5;//3.8

					//calculate d
					double k, KFactor = 0.581;

					k = +KFactor;
					var d1 = Math.Abs(k * (x2 - x1) + y1 - y2) / Math.Sqrt(k * k + 1);

					k = -KFactor;
					var d2 = Math.Abs(k * (x2 - x1) + y1 - y2) / Math.Sqrt(k * k + 1);

					var D = Math.Max(d1, d2);

					var Jtime = (int)(D * MoveFactor);

					return Jtime;
				}
				int ColorDiff(Color a, Color b)
			{
				var Rdiff = Math.Abs(a.R - b.R);
				var Bdiff = Math.Abs(a.B - b.B);
				var Gdiff = Math.Abs(a.G - b.G);

				return Rdiff + Bdiff + Gdiff;
			}
				void CrossMark(int px, int py, Color CrossColor)
				{
					for (int x = px - 1; x <= px + 1; x++)
						for (int y = py - 50; y <= py + 50; y++)
						{
							try
							{
								img.SetPixel(x, y, CrossColor);
							}
							catch (Exception)
							{
							}
						}

					for (int y = py - 1; y <= py + 1; y++)
						for (int x = px - 50; x <= px + 50; x++)
						{
							try
							{
								img.SetPixel(x, y, CrossColor);
							}
							catch (Exception)
							{
							}
						}

				}
				Point CalculateStartPoint()
				{
					const int DiffThreshold = 30;
					var StartColor = Color.FromArgb(56, 56, 98);

					List<Point> PointColl = new List<Point>();

					for (int x = 0; x < img.Size.Width; x += 2)
						for (int y = 0; y < img.Size.Height; y += 2)
						{
							if (ColorDiff(StartColor, img.GetPixel(x, y)) <= DiffThreshold)
							{
								img.SetPixel(x, y, Color.Gold);
								PointColl.Add(new Point(x, y));
							}
						}

					//center of start point
					int StartX, StartY;
					StartX = (int)(from p in PointColl select p.X).Average();
					StartY = (int)(from p in PointColl select p.Y).Average();

					StartY += 67;

					return new Point(StartX, StartY);
				}
				Point CalculateEndPoint()
				{
					double KFactor = 0.581;

					int f1(int x)
					{
						double k = +KFactor;
						int x0 = 335, y0 = 842;
						int y = (int)(k * (x - x0) + y0);
						return y;
					}

					int f2(int x)
					{
						double k = -KFactor;
						int x0 = 838, y0 = 842;
						int y = (int)(k * (x - x0) + y0);
						return y;
					}

					int TargetX1 = -1, TargetX2 = -1;

					{
						var LastColor = img.GetPixel(25, f1(25));

						for (int x = 25; x < 1080; x++)
						{
							var NowColor = img.GetPixel(x, f1(x));
							if (ColorDiff(NowColor, LastColor) <= 10)
							{
								//similar
							}
							else
							{
								//sudden change
								TargetX1 = x;
								break;
							}
						}

					}

					{
						var LastColor = img.GetPixel(1070, f2(1070));

						for (int x = 1070; x >= 0; x--)
						{
							var NowColor = img.GetPixel(x, f2(x));
							if (ColorDiff(NowColor, LastColor) <= 20)
							{
								//similar
							}
							else
							{
								//sudden change
								TargetX2 = x;
								break;
							}
						}
					}

					int TargetX, TargetY;//edge of end block
					if (TargetX1 < (1079 - TargetX2))
					{
						// \
						TargetX = TargetX1;
						TargetY = f1(TargetX);
					}
					else
					{
						// /
						TargetX = TargetX2;
						TargetY = f2(TargetX);
					}

					return new Point(TargetX, TargetY);

				}

			}
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

