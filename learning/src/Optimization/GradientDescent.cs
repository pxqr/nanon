using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Nanon.Math.Linear;
using Nanon.Math.Series;
using Nanon.Model;
using Nanon.Learning.Tools;

namespace Nanon.Learning.Optimization
{
	//
	//  Straightforward stohastic gradient descent. 
	//  It can be used in either online or batch learning.
	//  
	public class GradientDescent<InputT, OutputT> : IOptimizer<InputT, OutputT>
	{
		const double DoNotCheckCost = -1.0d;
			
		int iterationCount  = 100;
		int initialStepSize = 1;
		double learningRate = 1;
		bool showInfo = true;
		Action<IHypothesis<InputT, OutputT>> callback;
	
		//  learningProgression series should ever divergent!
		Func<int, double> learningProgression =  Series.HarmonicSeries;
		
		public GradientDescent(int iterationCountP, 
		                       double learningRateP, 
		                       Func<int, double> learningProgressionA, 
		                       int initialStepSizeP,
		                       Action<IHypothesis<InputT, OutputT>> callbackA)
		{
			iterationCount  = iterationCountP;
			learningRate    = learningRateP;
			initialStepSize = initialStepSizeP;
			learningProgression = learningProgressionA;
			callback = callbackA;
		}
		
		public bool ShowInfo 
		{
			get
			{
				return showInfo;
			}
			set
			{
				showInfo = value;
			}
		}
		
		public int IterationCount {
			get {
				return this.iterationCount;
			}
			set {
				iterationCount = value;
			}
		}
		
		public int InitialStepSize {
			get {
				return this.initialStepSize;
			}
			set {
				initialStepSize = value;
			}
		}
		
		void DoGradientStep(IHypothesis<InputT, OutputT> hypothesis, IEnumerable<Tuple<InputT, OutputT> > exsamples, double coeff, int stepSize)
		{
			var batchSize = 0;
			
			foreach(var ex in exsamples)
			{
				hypothesis.Gradient(ex.Item1, ex.Item2);
				
				++batchSize;
				
				if (batchSize == stepSize)
				{
					var factor  = (coeff / (double)batchSize) * learningRate;
					hypothesis.Correct(factor);
					batchSize = 0;
				}
			}
			
			if (batchSize != 0)
			{
				var factor  = (coeff / (double)batchSize) * learningRate;
				hypothesis.Correct(factor);
			}
		}
		
		public void Optimize(IHypothesis<InputT, OutputT> hypothesis, IEnumerable<Tuple<InputT, OutputT> > exsamples)
		{
			if (exsamples.Count() == 0)
				return;
			
			var stepSize = initialStepSize;
			
			for (var iteration = 1; iteration <= iterationCount; ++iteration)
			{
				var coeff =  learningProgression(iteration);
				DoGradientStep(hypothesis, exsamples, coeff, stepSize);
				stepSize *= 2;
				
				if (showInfo)
					callback(hypothesis);
				
			}
		}
	}
}