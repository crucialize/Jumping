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


			string LogDir = @"C:\Users\chenj\Desktop\Jumping\JumpingPro\bin\Debug\log";
			var ImgFiles = new List<string>();

			foreach(var f in Directory.GetFiles(LogDir, "*.png"))
			{
				if (f.IndexOf("modified") < 0)
					ImgFiles.Add(f);
			}

			for(int i = 0; i < 5; i++)
			{
				var img = new Bitmap(ImgFiles[i]);
				var StartPoint=img.CalculateStartPoint();
				img.CrossMark(StartPoint.X, StartPoint.Y,Color.White);



				img.Save(OutputDir + new FileInfo(ImgFiles[i]).Name + ".png");
			}

		}
	}
}
