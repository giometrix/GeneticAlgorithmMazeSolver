namespace MazeMaker
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using GeneticSharp.Domain.Chromosomes;

	public class MazeWalker
	{

		private readonly Gene[] _instructions;

		private readonly Maze _maze;

		public MazeWalker(Maze maze, Gene[] instructions)
		{
			this._maze = maze;
			this._instructions = instructions;
		}

		public static Gene[] GenerateRandomInstructions(int length)
		{
			var instructions = new Gene[length];

			var rand = new Random();
			for (int i = 0; i < length; i++)
			{
				switch (rand.Next(0, 4))
				{
					case 0:
						instructions[i] = new Gene("00");
						break;
					case 1:
						instructions[i] = new Gene("01");
						break;
					case 2:
						instructions[i] = new Gene("10");
						break;
					case 3:
						instructions[i] = new Gene("11");
						break;
				}
			}

			return instructions;
		}


		public int Walk(out int repeatedSteps, out int closestDistanceToEnd)
		{
			int steps = 0;
			repeatedSteps = 0;
			var coord = this._maze.Start;
			var prevcoord = coord;
			var walked = new Dictionary<Maze.Coordinates,int>();
			closestDistanceToEnd = int.MaxValue;
			
			for (int i = 0; i < this._instructions.Length; i++)
			{
				steps++;

				var instruction = this._instructions[i].Value as string;

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

				var distance = (int)Math.Sqrt(Math.Pow(this._maze.End.X - coord.X, 2) + Math.Pow(this._maze.End.Y - coord.Y, 2));
				if (distance < closestDistanceToEnd)
				{
					closestDistanceToEnd = distance;
				}
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