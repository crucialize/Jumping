using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JumpingPro;

namespace AlgorithmTest
{
	class Program
	{
		static void Main(string[] args)
		{
			string OutputDir = "output/";
			if(Directory.Exists(OutputDir))
				Directory.Delete(OutputDir, true);
			Directory.CreateDirectory(OutputDir);


			string LogDir = @"C:\Users\Crucial\Desktop\train";
			var ImgFiles = new List<string>();

			foreach(var f in Directory.GetFiles(LogDir, "*.png"))
			{
				if (f.IndexOf("modified") < 0)
					ImgFiles.Add(f);
			}

			for(int i = 0; i < ImgFiles.Count; i++)
			{
				try
				{
					var img = new Bitmap(ImgFiles[i]);
					var StartPoint = img.CalculateStartPoint();
					img.CrossMark(StartPoint.X, StartPoint.Y, Color.White);

					var EndEdge = img.CalculateEndPoint(StartPoint);

					var plist = new List<Point>();


					img.BFS(EndEdge.X, EndEdge.Y, p => plist.Add(p));

					img.CrossMark(EndEdge.X, EndEdge.Y, Color.Black);

					Point EndPoint = new Point();

					EndPoint.X = (int)plist.Average(p => p.X);
					EndPoint.Y= (int)plist.Average(p => p.Y);

					double MoveFactor = 2.00;

					if (plist.Count <= 500)
						//边缘 少跳一点
						MoveFactor -= 0.2;

					//img.Save(OutputDir + new FileInfo(ImgFiles[i]).Name + ".png");
					
					

					img.Dispose();
				}
				catch { }
			}

		}
	}
}
