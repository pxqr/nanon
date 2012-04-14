using System;

using Nanon.Math.Linear;
using Nanon.Math.Activator;

namespace Nanon.NeuralNetworks.Layer.Convolutional
{
	public class Convolutor<T> : SingleLayer<T, T> where T : IMatrix<T>
	{
		double bias;
		double biasGradient;
		protected T weights;
		protected T gradients;

		protected T predError;
		
		internal Convolutor(IActivator activatorA) : 
			base(activatorA)
		{
			bias = 0.0d;
			biasGradient = 0.0d;
		}
		
		#region implemented abstract members of Nanon.NeuralNetworks.Layer.SingleLayer[T,T]
		
		public override T FeedForward (T input)
		{
			input.Convolve(weights, signals);
			signals.Add(bias, signals);
			signals.Transform(activator.Activate,  outputs);
			return outputs;
		}

		public override T PropagateBackward (T input, T predSignal, T error)
		{
			throw new NotImplementedException ();
		}

		public override void Gradient (T inputs, T outputError)
		{
			inputs.Involve(outputError, gradients);
			biasGradient += outputError.Sum;
		}

		public override void Correct(double coeff)
		{
			gradients.Mul(coeff, gradients);
			weights.Sub(gradients, weights);
			gradients.SetToZero();
			bias -= coeff * biasGradient;
			biasGradient = 0.0d;
		}
		
		#endregion
	}
	
	public class MatrixConvolutor : Convolutor<Matrix>
	{
		public MatrixConvolutor(int inputWidth, int inputHeight, int outputWidth, int outputHeight, IActivator activator) : base(activator) 
		{
			var kernelWidth  = inputWidth  - outputWidth  + 1;
			var kernelHeight = inputHeight - outputHeight + 1;
				
			weights = new Matrix(kernelWidth, kernelHeight);
			gradients = new Matrix(kernelWidth, kernelHeight);
			predError = new Matrix(inputWidth, inputHeight);
			
			signals = new Matrix(outputWidth, outputHeight);
			outputs = new Matrix(outputWidth, outputHeight);
		}
	}
}
