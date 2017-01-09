namespace MazeSolver
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.ExceptionServices;

	using GeneticSharp.Domain.Chromosomes;

	public class MazeWalker
	{
		public const string MoveDownInstruction = "10";

		public const string MoveLeftInstruction = "01";

		public const string MoveRightInstruction = "11";

		public const string MoveUpInstruction = "00";

		private readonly Gene[] instructions;

		private readonly Maze maze;

		public MazeWalker(Maze maze, Gene[] instructions)
		{
			this.maze = maze;
			this.instructions = instructions;
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
						instructions[i] = new Gene(MoveUpInstruction);
						break;
					case 1:
						instructions[i] = new Gene(MoveLeftInstruction);
						break;
					case 2:
						instructions[i] = new Gene(MoveDownInstruction);
						break;
					case 3:
						instructions[i] = new Gene(MoveRightInstruction);
						break;
				}
			}

			return instructions;
		}

		[HandleProcessCorruptedStateExceptions]
		public int Walk(out int repeatedSteps, out int closestDistanceToEnd)
		{
			int steps = 0;
			repeatedSteps = 0;
			var coord = this.maze.Start;
			var prevcoord = coord;
			var walked = new Dictionary<Maze.Coordinates, int>();
			closestDistanceToEnd = int.MaxValue;

			foreach (Gene gene in this.instructions)
			{
				steps++;
				string instruction;

				try
				{
					instruction = gene.Value as string;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);

					// I keep; random memory corruption errors.  I'm not sure where they are coming from.
					continue;
				}

				coord = this.ExecuteInstruction(instruction, coord);

				if (this.maze[coord.X, coord.Y] == Maze.State.Closed)
				{
					coord = prevcoord;
				}

				if (this.maze[coord.X, coord.Y] == Maze.State.Walked || this.maze[coord.X, coord.Y] == Maze.State.MultiWalked)
				{
					if (this.maze[coord.X, coord.Y] == Maze.State.Walked)
					{
						this.maze[coord.X, coord.Y] = Maze.State.MultiWalked;
					}

					walked[coord]++;
				}

				if (this.maze[coord.X, coord.Y] == Maze.State.Open)
				{
					this.maze[coord.X, coord.Y] = Maze.State.Walked;
					walked[coord] = 0;
				}

				if (coord == this.maze.End)
				{
					return steps;
				}

				prevcoord = coord;

				var distance = (int)Math.Sqrt(Math.Pow(this.maze.End.X - coord.X, 2) + Math.Pow(this.maze.End.Y - coord.Y, 2));
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
				case MoveUpInstruction:
					coord = this.maze.MoveUp(coord);
					break;
				case MoveLeftInstruction:
					coord = this.maze.MoveLeft(coord);
					break;
				case MoveDownInstruction:
					coord = this.maze.MoveDown(coord);
					break;
				case MoveRightInstruction:
					coord = this.maze.MoveRight(coord);
					break;
			}

			return coord;
		}
	}
}