using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JumpingPro
{
	class Program
	{
		static void Main(string[] args)
		{
			Directory.CreateDirectory("log");
			var adb = new MyADB(@"C:\adb\adb.exe");
			while (true)
			{
				try
				{
					Console.WriteLine("Start to get screenshot.");
					var img = adb.GetScreenshot();
					Console.WriteLine("End.");
					int utick = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
					img.Save(string.Format("./log/{0}.png", utick));

					var StartPoint = img.CalculateStartPoint();
					var EndEdge = img.CalculateEndPoint(StartPoint);

					var plist = new List<Point>();

					img.BFS(EndEdge.X, EndEdge.Y, p => plist.Add(p));

					img.CrossMark(EndEdge.X, EndEdge.Y, Color.Black);

					Point EndPoint = new Point();

					EndPoint.X = (int)plist.Average(p => p.X);
					EndPoint.Y = (int)plist.Average(p => p.Y);

					img.CrossMark(StartPoint.X, StartPoint.Y, Color.Gold);
					img.CrossMark(EndPoint.X, EndPoint.Y, Color.Red);
					img.Save(string.Format("./log/{0}_modified.png",utick));

					int time;
					if (plist.Count <= 500)
					{
						Console.WriteLine("Edge.");
						time = CalculateJumpTime2(StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, true);
					}
					else
					{
						time = CalculateJumpTime2(StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, false);
					}

					var r = new Random();
					int TapX = r.Next(100, 1000);
					int TapY = r.Next(600, 1600);
					adb.ExecuteADBShell(string.Format("input swipe {0} {1} {2} {3} {4}", TapX, TapY, TapX, TapY, time));
					//Console.WriteLine("execute end");
					Thread.Sleep(1360);

					/// <summary>
					/// 注意：xy对应1080p的坐标
					/// </summary>
					/// <param name="x1"></param>
					/// <param name="y1"></param>
					/// <param name="x2"></param>
					/// <param name="y2"></param>
					int CalculateJumpTime(int x1, int y1, int x2, int y2, bool less)
					{
						double MoveFactor = 1.56;

						if (less)
							MoveFactor -= 0.123;

						//calculate d
						double k, KFactor = 0.5820;

						k = +0.5773;
						var d1 = Math.Abs(k * (x2 - x1) + y1 - y2) / Math.Sqrt(k * k + 1);

						k = -0.5773;
						var d2 = Math.Abs(k * (x2 - x1) + y1 - y2) / Math.Sqrt(k * k + 1);

						var D = Math.Max(d1, d2);

						var Jtime = (int)(D * MoveFactor);

						return Jtime;
					}


					int CalculateJumpTime2(int x1, int y1, int x2, int y2, bool less)
					{
						double JumpFactor = 1.38;

						if (less)
							JumpFactor -= 0.123;

						//calculate d
						double k, KFactor = 0.5820;

						var D = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));


						var Jtime = (int)(D * JumpFactor);

						return Jtime;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}

		}
	}
}
