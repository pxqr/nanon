using System;

using Nanon.Math.Linear;

namespace Nanon.NeuralNetworks.Layer.Composition
{
	public class VectorMerger<InputT> : ISingleLayer<InputT[], Vector> 
	{
		ISingleLayer<InputT, Vector>[] layers;
		
		int size;       // layer count
		int outputSize; // size of output of each layer
		
		Vector outputs;
		Vector signals;
		InputT[] predErrors;
		
		public VectorMerger(ISingleLayer<InputT, Vector>[] parLayers)
		{
			size = parLayers.Length;
			
			if (size == 0) 
				throw new ArgumentException("layer count should be more than zero");
			
			layers = parLayers;
			outputSize = parLayers[0].Output.ToVector.Size;
			
			predErrors = new InputT[size];
			
			outputs = new Vector(size * outputSize);
			signals = new Vector(size * outputSize);
		}

		#region ISingleLayer[InputT[],Vector] implementation
		public Vector FeedForward (InputT[] input)
		{
			for (var i = 0; i < size; ++i)
			{
				var iOut = layers[i].FeedForward(input[i]);
				
				var iFrom = i * outputSize;
				var iTo   = iFrom + outputSize;
				
				outputs.Pack(iFrom, iTo, iOut.ToVector);
			}
			
			return outputs;	
		}

		public InputT[] PropagateBackward (InputT[] input, Vector error)
		{
			for (var i = 0; i < size; ++i)
			{
				var iFrom = i * outputSize;
				var iTo   = iFrom + outputSize;
				var unwindedError = error.Cut(iFrom, iTo);
				
				predErrors[i] = layers[i].PropagateBackward(input[i], unwindedError);
			}
			return predErrors;
		}

		public void Gradient(InputT[] input, Vector outputError)
		{
			for (var i = 0; i < size; ++i)
			{
				// cut 
				var iFrom = i * outputSize;
				var iTo   = iFrom + outputSize;
				var unwindedError = outputError.Cut(iFrom, iTo);

				layers[i].Gradient(input[i], unwindedError);
			}
		}

		public void Correct(double coeff)
		{
			for (var i = 0; i < size; ++i)
				layers[i].Correct(coeff);
		}

		public Vector Signal {
			get {
				return signals;
			}
		}

		public Vector Output {
			get {
				return outputs;
			}
		}
		#endregion
	}
}

