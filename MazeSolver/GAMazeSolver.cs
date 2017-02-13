namespace MazeSolver
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
	using GeneticSharp.Infrastructure.Threading;

	using Gifed;

	public class GAMazeSolver
	{
		private readonly int maxRunTimeInSeconds;

		private readonly int stagnationThreshold;

		private readonly bool generateUpdateImages;

		private readonly int updateImageFrequency;

		private readonly AnimatedGif gif;

		private bool animate;

		public GAMazeSolver(int maxRunTimeInSeconds = 10*60, int stagnationThreshold = 300, bool generateUpdateImages = true, int updateImageFrequency = 500, bool animate=true)
		{
			this.maxRunTimeInSeconds = maxRunTimeInSeconds;
			this.stagnationThreshold = stagnationThreshold;
			this.generateUpdateImages = generateUpdateImages;
			this.updateImageFrequency = updateImageFrequency;
			this.animate = animate;
			if (animate)
			{
				this.gif = new AnimatedGif();
			}

		}
		public void Solve(Maze maze)
		{
			const int NumberOfGenes = 5000;
			const float MutationRate = 0.02f;
			const float CrossOverRate = 0.6f;
			var selection = new EliteSelection();
			var crossover = new UniformCrossover();
			var mutation = new UniformMutation(true);
			var chromosome = new MazeSolverChromosome(NumberOfGenes);
			var reinsertion = new ElitistReinsertion();
			var population = new Population(50, 100, chromosome);
			var fitness = new MazeWalkerFitness(maze);
			IChromosome best = chromosome;
			best.Fitness = fitness.Evaluate(best);

			var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
			ga.MutationProbability = MutationRate;
			ga.Reinsertion = reinsertion;
			ga.Termination = new OrTermination(
				                 new FitnessStagnationTermination(this.stagnationThreshold),
				                 new TimeEvolvingTermination(TimeSpan.FromSeconds(this.maxRunTimeInSeconds)));
			ga.CrossoverProbability = CrossOverRate;
			ga.TaskExecutor = new SmartThreadPoolTaskExecutor()
				                  {
					                  MinThreads = Environment.ProcessorCount,
					                  MaxThreads = Environment.ProcessorCount
				                  };
			ga.GenerationRan += (sender, eventargs) =>
				{
					if (this.generateUpdateImages)
					{
						if (ga.GenerationsNumber == 1 || ga.GenerationsNumber % this.updateImageFrequency == 0)
						{
							var winnerSteps = FillMazeStepsWalked(maze, ga.BestChromosome);
							ImageDraw(winnerSteps, $"gen{ga.GenerationsNumber}.png");
						}
					}
					if (ga.GenerationsNumber % 10 == 0)
					{
						Console.WriteLine(
							$"{ga.GenerationsNumber} generations completed. Best fitness: {ga.BestChromosome.Fitness}.  Best so far: {best.Fitness}. Time evolving: {ga.TimeEvolving.TotalMinutes} min.");
					}

					if (ga.BestChromosome.Fitness > best.Fitness)
					{
						best = ga.BestChromosome.Clone();

						// ga.Population.CurrentGeneration.Chromosomes.Add(best);
					}
				};
			ga.TerminationReached += (sender, eventargs) =>
				{
					Console.WriteLine($"Termination Reached");
					var winnerSteps = FillMazeStepsWalked(maze, best);
					ImageDraw(winnerSteps, "best.png");

					var serializer = new XmlSerializer(typeof(string[]));
					using (var sw = new StreamWriter("bestchromosome.xml"))
					{
						serializer.Serialize(sw, best.GetGenes().Select(g => g.Value as string).ToArray());
					}

					if (this.animate)
					{
						File.Delete("output.gif");
						this.gif.Save("output.gif");
					}
				};

			if (this.generateUpdateImages)
			{
				this.ImageDraw(maze, "gen0.png");
			}
			
			ga.Start();
		}

		private static Maze FillMazeStepsWalked(Maze m, IChromosome chromosome)
		{
			var winnerSteps = m.Copy();
			var w = new MazeWalker(winnerSteps, chromosome.GetGenes());
			int repeatedSteps;
			int closest;
			w.Walk(out repeatedSteps, out closest);
			return winnerSteps;
		}

		private void ImageDraw(Maze maze, string filename)
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
							if (maze[x, y] == Maze.State.Open) g.FillRectangle(openBrush, x * SCALE, y * SCALE, SCALE, SCALE);
							if (maze[x, y] == Maze.State.Walked) g.FillRectangle(walkedBrush, x * SCALE, y * SCALE, SCALE, SCALE);
							if (maze[x, y] == Maze.State.MultiWalked) g.FillRectangle(multiWalkedBrush, x * SCALE, y * SCALE, SCALE, SCALE);
									 
							if (maze[x, y] == Maze.State.Start) g.FillRectangle(new SolidBrush(Color.Blue), x * SCALE, y * SCALE, SCALE, SCALE);
							if (maze[x, y] == Maze.State.End) g.FillRectangle(new SolidBrush(Color.Red), x * SCALE, y * SCALE, SCALE, SCALE);
						}
					}

					g.Save();
					File.Delete(filename);
					image.Save(filename, ImageFormat.Png);

					if (this.animate)
					{
						this.gif.AddFrame(new GifFrame(Image.FromFile(filename), 50));
					}
				}
			}
		}
	}
}