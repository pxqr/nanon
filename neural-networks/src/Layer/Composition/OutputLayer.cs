using System;

using Nanon.Math.Linear;

namespace Nanon.NeuralNetworks.Layer.Composition
{
	public class OutputLayer<T> : ICompositeLayer<T, T>
	{
		public OutputLayer(){	}

		#region ICompositeLayer[InputT,OutputT] implementation
		
		public T FeedForward(T input)
		{
			return input;
		}
		
		public void Backprop(T input, T error) {	}
		
		public T PropagateBackward(T input, T signal, T error)
		{
			return error;
		}

		public void Gradient(T a, T b) {	}
		public void Correct(double coeff) {		}
		
		#endregion
	}
}