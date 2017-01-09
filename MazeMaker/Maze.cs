namespace MazeMaker
{
	using System;
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
			for (int i = 0; i < 15; i++)
			{
				this.GenerateMaze(true);
			}
		}

		public Maze(string filename)
		{
			using (var image = new Bitmap(filename))
			{
				this.Height = image.Height;
				this.Width = image.Width;

				this._maze = new State[this.Width, this.Height];

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

		private void GenerateMaze(bool altPath = false)
		{
			Coordinates c;

			if (!altPath)
			{
				this.Start = this.FindStart();
				this._maze[this.Start.X, this.Start.Y] = State.Start;
				c = this.Start;
			}
			else
			{
				c = new Coordinates(this._random.Next(0, this.Width), this._random.Next(0, this.Height));
			}

			int size = (int)(this.Width * this.Height);
			int lenInDir = 0;
			int currDir = 0;

			int i = 0;
			do
			{
				int d = 0;
				if (i == 0 || this._random.NextDouble() <= 0.2f)
				{
					d = this._random.Next(0, 4);
					if (d != currDir)
					{
						lenInDir = 0;
					}

					currDir = d;
				}

				if (lenInDir > 5)
				{
					d = (d + 1) % 4;
					currDir = d;
					lenInDir = 0;
				}

				lenInDir++;
				var prev = c;
				switch (d)
				{
					case 0:
						c = this.MoveUp(c);
						break;
					case 1:
						c = this.MoveLeft(c);
						break;
					case 2:
						c = this.MoveDown(c);
						break;
					default:
						c = this.MoveRight(c);
						break;
				}

				if (this._maze[prev.X, prev.Y] == this._maze[c.X, c.Y] && this._maze[c.X, c.Y] == State.Open)
				{
					if (this._random.NextDouble() < 0.5f) continue;
				}

				if (this._maze[c.X, c.Y] != State.Start && this._maze[c.X, c.Y] != State.End) this._maze[c.X, c.Y] = State.Open;

				if (i + 1 == size)
				{
					if (!altPath)
					{
						this._maze[c.X, c.Y] = State.End;
						this.End = new Coordinates(c.X, c.Y);
					}
				}

				i++;
			}
			while (i < size);

			// end tends to be toward the edge, so let's randomly swap with some other open coordinate
			if (!altPath)
			{
				for (int x = this._random.Next(0, this.Width); x < this.Width; x++)
				{
					for (int y = this._random.Next(0, this.Height); y < this.Height; y++)
					{
						if (this[x, y] == State.Open)
						{
							if (this._random.NextDouble() < 0.5)
							{
								this[x, y] = State.End;
								this[this.End.X, this.End.Y] = State.Open;
								this.End = new Coordinates(x, y);
								goto End;
							}
						}
					}
				}

				End:
				;
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