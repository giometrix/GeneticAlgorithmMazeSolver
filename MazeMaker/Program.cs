namespace MazeMaker
{
	using System.Linq;

	using CommandLine;

	public class Program
	{
		public static void Main(string[] args)
		{
			var commandLineParser = Parser.Default.ParseArguments<Options>(args);
			bool exit = false;
			int stagnation = default(int);
			int runtime = default(int);
			string mazePath = default(string);
			bool generateUpdateImages = default(bool);
			int imageUpdateFrequency = default(int);
			bool animate = default(bool);
			commandLineParser.WithParsed(
				p =>
					{
						stagnation = p.StagnationThreshold;
						mazePath = p.MazeMath;
						runtime = p.MaxRunTimeInSeconds;
						generateUpdateImages = p.GenerateUpdateImages;
						imageUpdateFrequency = p.ImageUpdateFrequency;
						animate = p.Animate;
					});

			commandLineParser.WithNotParsed(
				n =>
					{
						if (n.Any())
						{
							exit = true;
						}
					});

			if (exit)
			{
				return;
			}

			Maze m;
			m = mazePath.Length == 0 ? new Maze(100, 100) : new Maze(mazePath);

			var solver = new GAMazeSolver(stagnationThreshold: stagnation, maxRunTimeInSeconds: runtime, animate:animate);
			solver.Solve(m);
		}

		class Options
		{
			[Option('u', "generate-update-images", HelpText = "Generate update images", Required = false)]
			public bool GenerateUpdateImages { get; set; }

			[Option('q', "image-update-frequency",
				 HelpText = "How often update images are generated (default every 500 generations)", Required = false)]
			public int ImageUpdateFrequency { get; set; } = 500;

			[Option('a', "animate", HelpText = "Creates an animated gif", Required = false)]
			public bool Animate { get; set; }

			[Option('r', "max-runtime",
				 HelpText = "Max Runtime (In Seconds) - Stop running after this amount of time elapses (default 600)",
				 Required = false)]
			public int MaxRunTimeInSeconds { get; set; } = 600;

			[Option('f', "maze-file-path",
				 HelpText =
					 "Maze file path - Load a maze bmp image.  Use R:0,G:0,B:255 for start,  R:255,G:0,B:0 for end, R:255,G:255,B:255 for path and R:0,G:0,B:0 for closed",
				 Required = false)]
			public string MazeMath { get; set; } = string.Empty;

			[Option('s', "stagnation-threshold",
				 HelpText = "Stagnation threshold - stop when this many generations pass without a new winner (default 300)",
				 Required = false)]
			public int StagnationThreshold { get; set; } = 300;
		}
	}
}