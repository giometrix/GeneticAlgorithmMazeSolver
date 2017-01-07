namespace MazeMaker
{
	using System;

	using GeneticSharp.Domain.Chromosomes;

	[Serializable]
	public class MazeSolverChromosome : ChromosomeBase
	{
		static Random _random = new Random();

		public MazeSolverChromosome(int length)
			: base(length)
		{
			for (int i = 0; i < length; i++)
			{
				this.ReplaceGene(i, this.GenerateGene(i));
			}
		}

		public MazeSolverChromosome(string chromosome)
			: base(chromosome.Length / 2)
		{
			for (int i = 0; i < chromosome.Length / 2; i += 2)
			{
				this.ReplaceGene(i / 2, new Gene(chromosome.Substring(i, 2)));
			}
		}

		public override IChromosome CreateNew()
		{
			return new MazeSolverChromosome(5000);
		}

		public override Gene GenerateGene(int geneIndex)
		{
			return new Gene(_random.Next(0, 2).ToString() + _random.Next(0, 2).ToString());
		}
	}
}