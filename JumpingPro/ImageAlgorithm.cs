using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpingPro
{
	static class ImageAlgorithm
	{

		public static int ColorDiff(Color a, Color b)
		{
			var Rdiff = Math.Abs(a.R - b.R);
			var Bdiff = Math.Abs(a.B - b.B);
			var Gdiff = Math.Abs(a.G - b.G);

			return Rdiff + Bdiff + Gdiff;
		}

		public static void CrossMark(this Bitmap img, int px, int py, Color CrossColor)
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

		public static Point CalculateStartPoint(this Bitmap img)
		{
			const int DiffThreshold = 30;
			var StartColor = Color.FromArgb(56, 56, 98);

			List<Point> PointColl = new List<Point>();

			for (int x = 0; x < img.Size.Width; x += 5)
				for (int y = 0; y < img.Size.Height; y += 5)
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

			StartY += 66;

			return new Point(StartX, StartY);
		}

		public static Point CalculateEndPoint(this Bitmap img, Point StartP)
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

			int TargetX = -1, TargetY = -1;//edge of end block

			int TargetX1 = -1, TargetX2 = -1;

			{
				var LastColor = img.GetPixel(25, f1(25));

				for (int x = 25; x < 1080; x++)
				{
					var NowColor = img.GetPixel(x, f1(x));
					if (ColorDiff(NowColor, LastColor) <= 20)
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

		public delegate void PointCallback(Point point);

		public static void BFS(this Bitmap img, int x, int y, PointCallback callback)
		{
			//DFS会爆栈
			var q = new Queue<Point>();
			var Visited = new HashSet<Point>();

			q.Enqueue(new Point(x, y));

			while (q.Count > 0)
			{
				var NowPoint = q.Dequeue();
				callback(NowPoint);

				int NowX = NowPoint.X, NowY = NowPoint.Y;

				TryVisit(NowX - 1, NowY);
				TryVisit(NowX, NowY - 1);
				TryVisit(NowX + 1, NowY);
				TryVisit(NowX, NowY + 1);


				bool _CanVisit(int px, int py)
				{
					bool IsVisited = Visited.Contains(new Point(px, py));

					bool IsColorOk = ColorDiff(img.GetPixel(NowX, NowY), img.GetPixel(px, py)) <= 15;

					bool IsInRange = px > 0 && px < img.Width
						&& py > 0 && py < img.Height;
					;

					return !IsVisited && IsInRange && IsColorOk;
				}

				void TryVisit(int px, int py)
				{
					if (_CanVisit(px, py))
					{
						q.Enqueue(new Point(px, py));
						Visited.Add(new Point(px, py));
					}
				}

			}

		}
	}
}
