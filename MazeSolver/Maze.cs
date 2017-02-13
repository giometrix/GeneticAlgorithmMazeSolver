namespace MazeSolver
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;

	public class Maze
	{
		private const int BUFFER = 2;

		private readonly State[,] _maze;

		private readonly Random _random = new Random();

		public Maze(int height, int width)
		{
			this.Height = height;
			this.Width = width;

			this._maze = new State[width, height];
			this.GenerateMaze();
		}

		public Maze(string filename)
		{
			using (var image = new Bitmap(filename))
			{
				this._maze = new State[image.Width, image.Height];
				this.FillMaze(image);
			}
		}

		public Maze(State[,] state, int height, int width, Point start, Point end)
		{
			this._maze = state;
			this.Height = height;
			this.Width = width;
			this.Start = start;
			this.End = end;
		}

		/// <summary>
		/// Maze coordinate state.
		/// </summary>
		public enum State : byte
		{
			Closed = 0,

			Open = 1,

			Start = 2,

			End = 3,

			Walked = 4,

			MultiWalked = 5
		}

		public Point End { get; private set; }

		public int Height { get; private set; }

		public Point Start { get; private set; }

		public int Width { get; private set; }

		public State this[int x, int y]
		{
			get
			{
				return this._maze[x, y];
			}

			set
			{
				this._maze[x, y] = value;
			}
		}

		public Maze Copy()
		{
			var array = new State[this.Width, this.Height];
			for (int x = 0; x < this.Width; x++)
			{
				for (int y = 0; y < this.Height; y++)
				{
					array[x, y] = this._maze[x, y];
				}
			}

			return new Maze(array, this.Height, this.Width, this.Start, this.End);
		}

		public Point MoveDown(Point currentPosition)
		{
			if (currentPosition.Y >= this.Height - BUFFER)
			{
				return currentPosition;
			}
			else
			{
				return new Point(currentPosition.X, currentPosition.Y + 1);
			}
		}

		public Point MoveLeft(Point currentPosition)
		{
			if (currentPosition.X <= BUFFER)
			{
				return currentPosition;
			}
			else
			{
				return new Point(currentPosition.X - 1, currentPosition.Y);
			}
		}

		public Point MoveRight(Point currentPosition)
		{
			if (currentPosition.X >= this.Width - BUFFER)
			{
				return currentPosition;
			}
			else
			{
				return new Point(currentPosition.X + 1, currentPosition.Y);
			}
		}

		public Point MoveUp(Point currentPosition)
		{
			if (currentPosition.Y <= BUFFER)
			{
				return currentPosition;
			}
			else
			{
				return new Point(currentPosition.X, currentPosition.Y - 1);
			}
		}

		private void FillMaze(Bitmap image)
		{
			this.Height = image.Height;
			this.Width = image.Width;

			for (int x = 0; x < this.Width; x++)
			{
				for (int y = 0; y < this.Height; y++)
				{
					var color = image.GetPixel(x, y);
					if (color == Color.FromArgb(0, 0, 0))
					{
						this._maze[x, y] = State.Closed;
					}
					else if (color == Color.FromArgb(255, 255, 255))
					{
						this._maze[x, y] = State.Open;
					}
					else if (color.R == 255)
					{
						this._maze[x, y] = State.End;
						this.End = new Point(x, y);
					}
					else if (color.B == 255)
					{
						this._maze[x, y] = State.Start;
						this.Start = new Point(x, y);
					}
				}
			}
		}

		private void GenerateMaze()
		{
			using (var image = new Bitmap(this.Width, this.Height))
			{
				using (var g = Graphics.FromImage(image))
				{
					g.Clear(Color.Black);
					var openPen = Pens.White;
					for (int i = 0; i < 15; i++)
					{
						var origin = new Point(this._random.Next(0, this.Width), this._random.Next(0, this.Height));
						var width = this._random.Next((int)(this.Width * 0.2f), (int)(this.Width * 0.9f));
						var height = this._random.Next((int)(this.Height * 0.2f), (int)(this.Height * 0.9f));
						g.DrawRectangle(openPen, origin.X, origin.Y, width, height);
					}

					// get all of the open coordinates
					var openCoord = new List<Point>(this.Width * this.Height);

					for (int x = 0; x < this.Width; x++)
					{
						for (int y = 0; y < this.Height; y++)
						{
							var c = image.GetPixel(x, y);
							if (c.R == Color.White.R && c.G == Color.White.G && c.B == Color.White.B)
							{
								openCoord.Add(new Point(x, y));
							}
						}
					}

					// randomly select a start and an end
					var start = openCoord[this._random.Next(0, openCoord.Count)];
					var end = openCoord[this._random.Next(0, openCoord.Count)];

					image.SetPixel(start.X, start.Y, Color.Blue);
					image.SetPixel(end.X, end.Y, Color.Red);
				}

				this.FillMaze(image);
			}
		}


	}
}