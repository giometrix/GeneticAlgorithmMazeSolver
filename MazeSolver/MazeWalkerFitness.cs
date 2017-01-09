using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
namespace MazeSolver
{
	public class MazeWalkerFitness : IFitness
	{
		private readonly Maze maze;

		public MazeWalkerFitness(Maze maze)
		{
			this.maze = maze;
		}

		public double Evaluate(IChromosome chromosome)
		{
			var m = this.maze.Copy();
			var genes = chromosome.GetGenes();
			var walker = new MazeWalker(m, genes);
			int repeatedSteps;
			int closest;
			var steps = walker.Walk(out repeatedSteps, out closest);

			if (steps < int.MaxValue)
			{
				steps = steps + repeatedSteps;
			}
			else
			{
				steps -= this.maze.Height * this.maze.Width * 50; // prevent overflow
				var open = 0;

				for (int x = 0; x < this.maze.Width; x++)
				{
					for (int y = 0; y < this.maze.Height; y++)
					{
						if (m[x, y] == Maze.State.Open)
						{
							open++;

						}	
						
					}
				}
				steps += (open * 50) + ((int)closest * 2);
			}

			var contradictingSteps = 0;
			for (int i = 1; i < genes.Length; i++)
			{
				var current = genes[i].Value as string;
				var previous = genes[i].Value as string;

				switch (previous)
				{
					case MazeWalker.MoveDownInstruction:
						if (current == MazeWalker.MoveUpInstruction)
						{
							contradictingSteps++;
						}
						break;
					case MazeWalker.MoveUpInstruction:
						if (current == MazeWalker.MoveDownInstruction)
						{
							contradictingSteps++;
						}
						break;
					case MazeWalker.MoveLeftInstruction:
						if (current == MazeWalker.MoveRightInstruction)
						{
							contradictingSteps++;
						}
						break;
					case MazeWalker.MoveRightInstruction:
						if (current == MazeWalker.MoveLeftInstruction)
						{
							contradictingSteps++;
						}
						break;
				}
			}
			steps += contradictingSteps * 5;

			return -(steps);
		}
	}
}
