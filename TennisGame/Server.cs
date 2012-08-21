using System;
using System.Collections.Generic;
using System.Windows;

namespace TennisGame
{
	public class Server : MarshalByRefObject
	{
		public const int WinPoint = 3;
		public const double Size = 0.08;
		private double[] barPos = new double[2];
		public double BarPos(bool bFirst) { return barPos[bFirst ? 0 : 1]; }
		private int[] score = new int[2];
		public int Score(bool bFirst) { return score[bFirst ? 0 : 1]; }
		private Point discPos;
		public Point DiscPos { get { return discPos; } }
		private Vector dir;

		private int count;
		public bool IsWaiting;
		public bool IsRunning;
		public Server()
		{
			Reset();
			count = 24;
			IsWaiting = true;
			IsRunning = !false;
		}
		public void Reset()
		{
			barPos[0] = 0;
			barPos[1] = 1 - Window1.barRate;
			score[0] = score[1] = 0;
			Init(true);
			IsRunning = true;
		}
		private void Init(bool bFirst)
		{
			discPos = new Point(0.5, 0.5);
			dir = new Vector((new Random().NextDouble() * 0.05 - 0.025),
				0.05 * (bFirst ? -1 : 1));
			count = 8;
		}
		public void Do()
		{
			if (IsWaiting) return;
			if (count > 0)
			{
				--count;
				return;
			}
			discPos += dir;
			if (discPos.X < Size)
			{
				discPos.X = -discPos.X + 2 * Size;
				dir.X = -dir.X;
			}
			else if (discPos.X > 1 - Size)
			{
				discPos.X = -discPos.X + 2 * (1 - Size);
				dir.X = -dir.X;
			}
			for (int i = 0; i < 2; i++)
			{
				if (i == 0 && discPos.Y >= Size / 2) continue;
				else if (i == 1 && discPos.Y <= 1 - Size / 2) continue;
				double x = discPos.X - barPos[i];
				if (x < 0 || x > Window1.barRate)
				{
					if (++score[1 - i] >= WinPoint) IsRunning = false;
					else Init(i == 0);
				}
				else
				{
					discPos.Y = -discPos.Y + 2 * (i == 0 ? Size : (1 - Size));
					dir.Y = -dir.Y;
//					if (x < Window1.barRate * 0.3) dir.Y -= 0.016;
//					else if (x > Window1.barRate * 0.7) dir.Y += 0.016;
				}
			}
		}
		public void Move(bool bFirst, bool bLeft)
		{
			int i = bFirst ? 0 : 1;
			barPos[i] += (bLeft ? -1 : 1) * Size / 3;
			if (barPos[i] < 0)
				barPos[i] = 0;
			else if (barPos[i] > 1 - Window1.barRate)
				barPos[i] = 1 - Window1.barRate;
		}
	}
}
