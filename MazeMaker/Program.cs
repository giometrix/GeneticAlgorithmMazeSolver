namespace MazeMaker
{
	public class Program
	{
		static void Main(string[] args)
		{
			Maze m;
			if (args.Length == 0)
			{
				m = new Maze(100, 100);
			}
			else
			{
				m = new Maze(args[0]);
			}

			var solver = new GAMazeSolver();
			solver.Solve(m);
		}
	}
}