using System;
using Nanon.Data;
using Nanon.Math.Linear;
using Nanon.Model;
using Nanon.Learning.Tools;
using Nanon.Model.Classifier;
using Nanon.Learning.Optimization;
using Nanon.NeuralNetworks;
using System.Linq;

namespace Nanon.Test
{
	public class CNNTest
	{
		static DataSet<Matrix, Vector> testDataSet;
		static DataSet<Matrix, Vector> trainDataSet;
		
		static DataSet<Matrix, Vector> Load(string trainImagesPath, string trainLabelsPath)
		{
			Console.WriteLine("Load data from {0} \n{1}", trainImagesPath, trainLabelsPath);
			return DataSet<Matrix, Matrix>.FromFile(trainImagesPath, trainLabelsPath)
				   				          .Convert(x => x, 
				                 				   x => Vector.FromIndex((int)x.Cells[0], 10, -1.0d, 1.0d));
		}
		
		static void LoadDataSet(string trainImagesPath, string trainLabelsPath, string testImagesPath, string testLabelsPath)
		{
			trainDataSet = Load(trainImagesPath, trainLabelsPath).Take(100);
			testDataSet  = Load(testImagesPath, testLabelsPath).Take(100);
			GC.Collect();
			
			Console.WriteLine("Normalize data");
			Vector[] vectors = trainDataSet.Inputs.Select(x => x.ToVector).ToArray();
			var inputsNormalizator = new Normalization(vectors);
			
			foreach (var input in trainDataSet.Inputs)
				inputsNormalizator.Normalize(input.ToVector);
			
			foreach (var input in testDataSet.Inputs)
				inputsNormalizator.Normalize(input.ToVector);
		}
		
		static double Test(IHypothesis<Matrix, Vector> network, IDataSet<Matrix, Vector> dataSet, double oldcost = Double.PositiveInfinity)
		{
			var rtester = new RegressionTester<Matrix>(network);
			var cost = rtester.Test(dataSet);
			    
			var classifier = new MaxFitClassifier<Matrix>(network);				
			var ctester = new ClassifierTester<Matrix, Vector, int>(classifier, x => x.IndexOfMax);
			var accuracy = ctester.Test(dataSet);
				
			Console.Write("C {0} | {1}%", cost, accuracy * 100);
			
			if (oldcost < cost)
			{
				Console.WriteLine();
				Console.WriteLine("Warning: probably weights will divergent!");
			}
			
			return cost;
		}
		
		public static void Test(string trainImagesPath, string trainLabelsPath, string testImagesPath, string testLabelsPath)
		{
			LoadDataSet(trainImagesPath, trainLabelsPath, testImagesPath, testLabelsPath);
			var network   = NetworkBuilder.Create(trainDataSet);
			
			var cost = Double.PositiveInfinity;
			var optimizer = new GradientDescent<Matrix, Vector>(10, 0.1, x => 1, 1000, 
			    x => { 
					Console.Write("trainSet: ");
					cost = Test(x, trainDataSet, cost);
					Console.Write(" || ");
					Console.Write("testSet:  ");
					Test(x, testDataSet);
					Console.WriteLine();
				});
			
			var trainer   = new Trainer<Matrix, Vector>(optimizer);
			
			Console.WriteLine("Initial");
			Test(network, testDataSet);
			Console.WriteLine("StartLearning");
			trainer.Train(network, trainDataSet);
			Console.WriteLine("EndLearning");
			Test(network, testDataSet);
		}
	}
}

