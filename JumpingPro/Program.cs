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
					var img = adb.GetScreenshot();

					int utick = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
					img.Save(string.Format("./log/{0}.png", utick));

					var StartP = img.CalculateStartPoint();
					var EndP = img.CalculateEndPoint();

					img.CrossMark(StartP.X, StartP.Y, Color.Gold);
					img.CrossMark(EndP.X, EndP.Y, Color.Red);
					img.Save(string.Format("./log/{0}_modified.png",utick));

					var Time = CalculateJumpTime(StartP.X, StartP.Y, EndP.X, EndP.Y);
					var r = new Random();
					int TapX = r.Next(100, 1000);
					int TapY = r.Next(600, 1600);
					adb.ExecuteADBShell(string.Format("input swipe {0} {1} {2} {3} {4}", TapX, TapY, TapX, TapY, Time));
					//Console.WriteLine("execute end");
					Thread.Sleep(1500);

					/// <summary>
					/// 注意：xy对应1080p的坐标
					/// </summary>
					/// <param name="x1"></param>
					/// <param name="y1"></param>
					/// <param name="x2"></param>
					/// <param name="y2"></param>
					int CalculateJumpTime(int x1, int y1, int x2, int y2)
					{
						const double MoveFactor = 1.45;

						//calculate d
						double k, KFactor = 0.5775;

						k = +KFactor;
						var d1 = Math.Abs(k * (x2 - x1) + y1 - y2) / Math.Sqrt(k * k + 1);

						k = -KFactor;
						var d2 = Math.Abs(k * (x2 - x1) + y1 - y2) / Math.Sqrt(k * k + 1);

						var D = Math.Max(d1, d2);

						var Jtime = (int)(D * MoveFactor);

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
