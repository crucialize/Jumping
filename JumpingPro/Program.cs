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
					var StartP = CalculateStartPoint();
					var EndP = CalculateEndPoint();

					CrossMark(StartP.X, StartP.Y, Color.Gold);
					CrossMark(EndP.X, EndP.Y, Color.Red);
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

						int TargetX=-1, TargetY=-1;//edge of end block

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

						if (Math.Abs(TargetX1 - StartP.X) <= 42)
						{
							TargetX = TargetX2;
							TargetY = f2(TargetX);
						}
						else if (Math.Abs(TargetX2 - StartP.X) <= 42)
						{
							TargetX = TargetX1;
							TargetY = f1(TargetX);
						}
						else if (StartP.X < 1080 / 2)
						{
							TargetX = TargetX2;
							TargetY = f2(TargetX);
						}
						else
						{
							TargetX = TargetX1;
							TargetY = f1(TargetX);
						}

						return new Point(TargetX, TargetY);

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
