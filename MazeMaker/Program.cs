namespace MazeMaker
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq;
	using System.Xml.Serialization;

	using GeneticSharp.Domain;
	using GeneticSharp.Domain.Chromosomes;
	using GeneticSharp.Domain.Crossovers;
	using GeneticSharp.Domain.Mutations;
	using GeneticSharp.Domain.Populations;
	using GeneticSharp.Domain.Reinsertions;
	using GeneticSharp.Domain.Selections;
	using GeneticSharp.Domain.Terminations;
	using GeneticSharp.Extensions.AutoConfig;
	using GeneticSharp.Infrastructure.Threading;

	class Program
	{
		private static void ConsoleDraw(Maze maze)
		{
			for (int x = 0; x < maze.Width; x++)
			{
				for (int y = 0; y < maze.Height; y++)
				{
					if (maze[x, y] == Maze.State.Open) Console.Write(" ");
					if (maze[x, y] == Maze.State.Closed) Console.Write("X");
					if (maze[x, y] == Maze.State.Start) Console.Write("S");
					if (maze[x, y] == Maze.State.Walked || maze[x, y] == Maze.State.MultiWalked) Console.Write("W");
					if (maze[x, y] == Maze.State.End) Console.Write("E");
				}

				Console.WriteLine();
			}
		}

		private static Maze FillMazeStepsWalked(Maze m, IChromosome chromosome)
		{
			var winnerSteps = m.Copy();
			var w = new MazeWalker(winnerSteps, string.Join(String.Empty, chromosome.GetGenes()));
			int repeatedSteps;
			w.Walk(out repeatedSteps);
			return winnerSteps;
		}

		private static void ImageDraw(Maze maze, string filename)
		{
			const int SCALE = 5;

			using (var image = new Bitmap(100 * SCALE, 100 * SCALE))
			{
				using (var g = Graphics.FromImage(image))
				{
					g.Clear(Color.Black);

					var openBrush = new SolidBrush(Color.White);
					var walkedBrush = new SolidBrush(Color.Green);
					var multiWalkedBrush = new SolidBrush(Color.Orange);
					for (int x = 0; x < maze.Width; x++)
					{
						for (int y = 0; y < maze.Height; y++)
						{
							if (maze[y, x] == Maze.State.Open) g.FillRectangle(openBrush, x * SCALE, y * SCALE, SCALE, SCALE);
							if (maze[y, x] == Maze.State.Walked) g.FillRectangle(walkedBrush, x * SCALE, y * SCALE, SCALE, SCALE);
							if (maze[y, x] == Maze.State.MultiWalked) g.FillRectangle(multiWalkedBrush, x * SCALE, y * SCALE, SCALE, SCALE);

							if (maze[y, x] == Maze.State.Start) g.FillRectangle(new SolidBrush(Color.Blue), x * SCALE, y * SCALE, SCALE, SCALE);
							if (maze[y, x] == Maze.State.End) g.FillRectangle(new SolidBrush(Color.Red), x * SCALE, y * SCALE, SCALE, SCALE);
						}
					}

					g.Save();
					File.Delete(filename);
					image.Save(filename, ImageFormat.Png);
				}
			}
		}

		static void Main(string[] args)
		{
			Maze m;
			if (args.Length == 0) m = new Maze(100, 100);
			else
			{
				m = new Maze(args[0]);
			}

			const int numberOfGenes = 5000;
			const float mutationRate = 0.02f;
			const float crossOverRate = 0.6f;
			var selection = new EliteSelection();
			var crossover = new UniformCrossover();
			var mutation = new UniformMutation(true);
			var chromosome = new MazeSolverChromosome(numberOfGenes);
			var reinsertion = new ElitistReinsertion();
			var population = new Population(50, 100, chromosome);
			var fitness = new MazeWalkerFitness(m);
			IChromosome best = chromosome;
			best.Fitness = fitness.Evaluate(best);


			var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
			ga.MutationProbability = mutationRate;
			ga.Reinsertion = reinsertion;
			ga.Termination = new OrTermination(
				                 new FitnessStagnationTermination(300),
				                 new TimeEvolvingTermination(TimeSpan.FromMinutes(10)));
			ga.CrossoverProbability = crossOverRate;
			ga.TaskExecutor = new SmartThreadPoolTaskExecutor() {MinThreads = Environment.ProcessorCount, MaxThreads = Environment.ProcessorCount};
			ga.GenerationRan += (sender, eventargs) =>
				{
					if (ga.GenerationsNumber == 1 || ga.GenerationsNumber % 500 == 0)
					{
						var winnerSteps = FillMazeStepsWalked(m, ga.BestChromosome);
						ImageDraw(winnerSteps, $"gen{ga.GenerationsNumber}.png");
					}

					if (ga.GenerationsNumber % 10 == 0)
					{
						Console.WriteLine($"{ga.GenerationsNumber} generations completed. Best fitness: {ga.BestChromosome.Fitness}.  Best so far: {best.Fitness}. Time evolving: {ga.TimeEvolving.TotalMinutes} min.");
					}

					if (ga.BestChromosome.Fitness > best.Fitness)
					{
						
						best = ga.BestChromosome.Clone();
						//ga.Population.CurrentGeneration.Chromosomes.Add(best);
					}
				};
				ga.TerminationReached += (sender, eventargs) =>
				{
					Console.WriteLine($"Termination Reached");
					var winnerSteps = FillMazeStepsWalked(m, best);
					ImageDraw(winnerSteps, "best.png");
					

					var serializer = new XmlSerializer(typeof(string[]));
					using (var sw = new StreamWriter("bestchromosome.xml"))
					{
						serializer.Serialize(sw, best.GetGenes().Select(g => g.Value as string).ToArray());
					}
				};

			ga.Start();
		}
	}
}