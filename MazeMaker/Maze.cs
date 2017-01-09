namespace MazeMaker
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Threading;

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
			this._maze = new State[this.Width, this.Height];
			using (var image = new Bitmap(filename))
			{
				this.FillMaze(image);
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
					var color = image.GetPixel(y, x);
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
						this.End = new Coordinates(x, y);
					}
					else if (color.B == 255)
					{
						this._maze[x, y] = State.Start;
						this.Start = new Coordinates(x, y);
					}
				}
			}
		}

		public Maze(State[,] state, int height, int width, Coordinates start, Coordinates end)
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

		public Coordinates End { get; private set; }

		public int Height { get; private set; }

		public Coordinates Start { get; private set; }

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

		public Coordinates MoveDown(Coordinates currentPosition)
		{
			if (currentPosition.Y >= this.Height - BUFFER)
			{
				return currentPosition;
			}
			else
			{
				return new Coordinates(currentPosition.X, currentPosition.Y + 1);
			}
		}

		public Coordinates MoveLeft(Coordinates currentPosition)
		{
			if (currentPosition.X <= BUFFER)
			{
				return currentPosition;
			}
			else
			{
				return new Coordinates(currentPosition.X - 1, currentPosition.Y);
			}
		}

		public Coordinates MoveRight(Coordinates currentPosition)
		{
			if (currentPosition.X >= this.Width - BUFFER)
			{
				return currentPosition;
			}
			else
			{
				return new Coordinates(currentPosition.X + 1, currentPosition.Y);
			}
		}

		public Coordinates MoveUp(Coordinates currentPosition)
		{
			if (currentPosition.Y <= BUFFER)
			{
				return currentPosition;
			}
			else
			{
				return new Coordinates(currentPosition.X, currentPosition.Y - 1);
			}
		}

		private Coordinates FindStart()
		{
			int x, y;
			int side = this._random.Next(0, 4); // top,left,bottom,right
			if (side == 0)
			{
				x = this._random.Next(0, this.Width);
				y = 0;
			}
			else if (side == 1)
			{
				x = 0;
				y = this._random.Next(0, this.Height);
			}
			else if (side == 2)
			{
				x = this._random.Next(0, this.Width);
				y = this.Height - 1;
			}
			else
			{
				x = this.Width - 1;
				y = this._random.Next(0, this.Height);
			}

			return new Coordinates(x, y);
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
						var origin = new Coordinates(this._random.Next(0, this.Width), this._random.Next(0, this.Height));
						var width = this._random.Next((int)(this.Width * 0.2f), (int)(this.Width * 0.9f));
						var height = this._random.Next((int)(this.Height * 0.2f), (int)(this.Height * 0.9f));
						g.DrawRectangle(openPen,origin.X, origin.Y, width, height);
					}
					image.Save("test.png", ImageFormat.Png);
					//get all of the open coordinates
					var openCoord = new List<Coordinates>(this.Width * this.Height);
					
					for (int x = 0; x < this.Width; x++)
					{
						for (int y = 0; y < this.Height; y++)
						{
							var c = image.GetPixel(x, y);
							if (c.R == Color.White.R && c.G == Color.White.G && c.B == Color.White.B)
							{
								openCoord.Add(new Coordinates(x,y));
							}
						}
					}

					//randomly select a start and an end
					var start = openCoord[this._random.Next(0, openCoord.Count)];
					var end = openCoord[this._random.Next(0, openCoord.Count)];
					
					image.SetPixel(start.X, start.Y, Color.Blue);
					image.SetPixel(end.X, end.Y, Color.Red);
				}
				this.FillMaze(image);
			}
		}

		/// <summary>
		/// Maze coordinates
		/// </summary>
		public struct Coordinates
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="Coordinates"/> struct.
			/// </summary>
			/// <param name="x">
			/// The x coordinate.
			/// </param>
			/// <param name="y">
			/// The y coordinate.
			/// </param>
			public Coordinates(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}

			/// <summary>
			/// Gets the x coordinate.
			/// </summary>
			public int X { get; private set; }

			/// <summary>
			/// Gets the y coordinate.
			/// </summary>
			public int Y { get; private set; }

			public static bool operator ==(Coordinates lhs, Coordinates rhs)
			{
				return lhs.X == rhs.X && lhs.Y == rhs.Y;
			}

			public static bool operator !=(Coordinates lhs, Coordinates rhs)
			{
				return !(lhs == rhs);
			}

			public bool Equals(Coordinates other)
			{
				return this.X == other.X && this.Y == other.Y;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}

				return obj is Coordinates && this.Equals((Coordinates)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (this.X * 397) ^ this.Y;
				}
			}
		}
	}
}