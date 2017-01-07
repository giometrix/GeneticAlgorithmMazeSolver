namespace MazeMaker
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class MazeWalker
	{
		private readonly string _instructions;

		private readonly Maze _maze;

		public MazeWalker(Maze maze, string instructions)
		{
			this._maze = maze;
			this._instructions = instructions;
		}

		public static string GenerateRandomInstructions(int length)
		{
			var instructions = new StringBuilder(length * 2);
			var rand = new Random();
			for (int i = 0; i < length; i++)
			{
				switch (rand.Next(0, 4))
				{
					case 0:
						instructions.Append("00");
						break;
					case 1:
						instructions.Append("01");
						break;
					case 2:
						instructions.Append("10");
						break;
					case 3:
						instructions.Append("11");
						break;
				}
			}

			return instructions.ToString();
		}


		public int Walk(out int repeatedSteps)
		{
			int steps = 0;
			repeatedSteps = 0;
			var coord = this._maze.Start;
			var prevcoord = coord;
			var walked = new Dictionary<Maze.Coordinates,int>();
			for (int i = 0; i < this._instructions.Length; i += 2)
			{
				steps++;

				var instruction = this._instructions.Substring(i, 2);

				coord = this.ExecuteInstruction(instruction, coord);

				if (this._maze[coord.X, coord.Y] == Maze.State.Closed)
				{
					coord = prevcoord;
				}

				if (this._maze[coord.X, coord.Y] == Maze.State.Walked || this._maze[coord.X, coord.Y] == Maze.State.MultiWalked)
				{
					if (this._maze[coord.X, coord.Y] == Maze.State.Walked)
					{
						this._maze[coord.X, coord.Y] = Maze.State.MultiWalked;
						
					}
					walked[coord]++;

				}

				if (this._maze[coord.X, coord.Y] == Maze.State.Open)
				{
					this._maze[coord.X, coord.Y] = Maze.State.Walked;
					walked[coord] = 0;
				}

				if (coord == this._maze.End)
				{
					return steps;
				}

				prevcoord = coord;
			}
			repeatedSteps = walked.ToList().Sum(x => x.Value);
			return int.MaxValue;
		}

		private Maze.Coordinates ExecuteInstruction(string instruction, Maze.Coordinates coord)
		{
			switch (instruction)
			{
				case "00":
					coord = this._maze.MoveUp(coord);
					break;
				case "01":
					coord = this._maze.MoveLeft(coord);
					break;
				case "10":
					coord = this._maze.MoveDown(coord);
					break;
				case "11":
					coord = this._maze.MoveRight(coord);
					break;
			}
			return coord;
		}
	}
}