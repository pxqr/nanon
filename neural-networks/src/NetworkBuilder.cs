using System;
using System.Collections.Generic;

using Nanon.Math.Activator;
using Nanon.Math.Linear;
using Nanon.Data;
using Nanon.NeuralNetworks.Layer;
using Nanon.NeuralNetworks.Layer.Composition;
using Nanon.NeuralNetworks.Layer.Convolutional;

namespace Nanon.NeuralNetworks
{
	public class NetworkBuilder
	{
		// single layer network
		public static NeuralNetwork<Vector> Create(IDataSet<Vector, Vector> dataSet, IActivator activator)
		{
			var workLayer = new FullyConnectedLayer(dataSet.FirstInput.Size, dataSet.FirstOutput.Size, activator);
			var outputLayer = new OutputLayer<Vector>();
			var layers = new CompositeLayer<Vector, Vector, Vector>(workLayer, outputLayer);
			return new NeuralNetwork<Vector>(layers);
		}
		
		public static NeuralNetwork<Vector> Create(IDataSet<Vector, Vector> dataSet, IActivator activator, List<int> hiddenSizes)
		{
			if (hiddenSizes.Count == 0)
				return Create(dataSet, activator);
			
			var inputSize  = dataSet.FirstInput.Size;
			var outputSize = dataSet.FirstOutput.Size;
			var sizes = new List<int>{inputSize};
			sizes.AddRange(hiddenSizes);
			sizes.Add(outputSize);
			
			var layerCount = sizes.Count - 1;
			var layers = new ISingleLayer<Vector, Vector>[layerCount];
			
			for (var i = 0; i < layerCount; ++i)
				layers[i] = new FullyConnectedLayer(sizes[i], sizes[i + 1], activator);
			
			var compositeLayer = LayerCompositor.ComposeGeteroneneous(layers);
			
			return new NeuralNetwork<Vector>(compositeLayer);
		}
		
		public static NeuralNetwork<Matrix> Create(IDataSet<Matrix, Vector> dataSet)
		{
			var count  = 10;
			
			var a = new ISingleLayer<Matrix, Matrix>[count];
			for (var i = 0; i < count; ++i)
				a[i] = new MatrixConvolutor(dataSet.FirstInput.Width, dataSet.FirstInput.Height, 2, 2, new Tanh());
			
			var b = new ISingleLayer<Matrix, Matrix>[count];
			for (var i = 0; i < count; ++i)
				b[i] = new MatrixSubsampler(2, 2, 1, 1, new Tanh());
			
			var splitter = new Splitter<Matrix, Matrix>(a);
			var merger   = new MatrixMerger<Matrix>(b);
			//var classif  = new FullyConnectedLayer(1 * count, 10, new Tanh());
			
			var comp = CompositeLayer<Vector, Vector[], Vector>.Compose(splitter, merger
			//                                                             , classif
			                                                             );
			
			return new NeuralNetwork<Matrix>(comp);
		}
	}
}

