using System;

namespace Nanon.NeuralNetworks.Layer.Composition
{
	public class Splitter<InputT, OutputT> : ISingleLayer<InputT, OutputT[]>
	{
		ISingleLayer<InputT, OutputT>[] layers;
		OutputT[] outputs;
		int size;
		
		public Splitter (ISingleLayer<InputT, OutputT>[] parLayers)
		{
			layers = parLayers;
			size   = layers.Length;
			outputs = new OutputT[size];
		}

		
		#region ISingleLayer[InputT,OutputT[]] implementation
		
		public OutputT[] FeedForward (InputT input)
		{
			for (var i = 0; i < size; ++i)
			{
				outputs[i] = layers[i].FeedForward(input);
			}
			return outputs;
		}

		public InputT PropagateBackward (InputT input, OutputT[] error)
		{
			throw new NotImplementedException ();
		}

		public void Gradient (InputT input, OutputT[] outputError)
		{
			for (var i = 0; i < size; ++i)
				layers[i].Gradient(input, outputError[i]);
		}

		public void Correct (double coeff)
		{
			foreach(var layer in layers)
				layer.Correct(coeff);
		}

		public OutputT[] Output {
			get {
				return outputs;
			}
		}
		#endregion
	}
}

