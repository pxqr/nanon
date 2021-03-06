using System;
using Nanon.Data;
using Nanon.Math.Linear;
using Nanon.Model;
using Nanon.Learning.Tools;
using Nanon.Model.Classifier;
using Nanon.Learning.Optimization;
using Nanon.NeuralNetworks;
using System.Linq;
using System.Diagnostics;
using Nanon.Learning.Deformation;

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
				                 				   x => Vector.FromIndex((int)x.Cells[0], 10, -0.8d, 0.8d));
		}
		
		static void LoadDataSet(string trainImagesPath, string trainLabelsPath, string testImagesPath, string testLabelsPath)
		{
			trainDataSet = Load(trainImagesPath, trainLabelsPath).Take(60000);
			GC.Collect();
			
			testDataSet  = Load(testImagesPath, testLabelsPath).Take(10000);
			GC.Collect();
			
			Console.WriteLine("Normalize data");
			Vector[] vectors = trainDataSet.Inputs.Select(x => x.ToVector).ToArray();
			var inputsNormalizator = new Normalization(vectors);
			
			foreach (var input in trainDataSet.Inputs)
				inputsNormalizator.Normalize(input.ToVector);
			
			foreach (var input in testDataSet.Inputs)
				inputsNormalizator.Normalize(input.ToVector);
			
			//Func<Matrix, Matrix> shifter = x => Deformation.Shift(x, 1, 1);
			//Func<Matrix, Matrix> distorter = x => Deformation.Distortion(x, 19, 19, 2, 5); 
			
			//var expandedSet = trainDataSet.Inputs.Select(distorter);
			//var expandedSet = trainDataSet.Inputs;
			
			//trainDataSet = new DataSet<Matrix, Vector>(
			//	expandedSet.Concat(trainDataSet.Inputs).ToArray(), 
			//	trainDataSet.Outputs.Concat(trainDataSet.Outputs).ToArray());
				
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
			GC.Collect();

			var network   = NetworkBuilder.CreateMnist(trainDataSet);
			
			Console.WriteLine("Initial");
			Test(network, testDataSet);
			Console.WriteLine("StartLearning");
			
			var cost = Double.PositiveInfinity;
			var timer = new Stopwatch();
			timer.Start();				
			
			var optimizer = new GradientDescent<Matrix, Vector>(3, 0.0007, 1, 
			    x => { 
					timer.Stop();		
					Console.Write("Ignored {0}% of samples ", 100 * NeuralNetwork<Matrix>.counter / (double)trainDataSet.Inputs.Count());
					Console.WriteLine("and gradient descent step time: {0} ms", timer.ElapsedMilliseconds);	
					NeuralNetwork<Matrix>.counter = 0;
					Console.Write("trainSet: ");
					cost = Test(x, trainDataSet, cost);
					Console.Write(" || ");
					Console.Write("testSet:  ");
					Test(x, testDataSet);
					Console.WriteLine();
					timer.Reset();
					timer.Start();
				});
			
			var trainer = new Trainer<Matrix, Vector>(optimizer);
			
			for (var i = 0; i < 10; ++i)
			{
				Console.WriteLine("Generation {0}", i);	
				trainer.Train(network, trainDataSet.Set);
				
				optimizer.IterationCount  += 1;
				optimizer.InitialStepSize *= 2;
			}
			
			Console.WriteLine("EndLearning");
			Test(network, testDataSet);
		}
	}
}