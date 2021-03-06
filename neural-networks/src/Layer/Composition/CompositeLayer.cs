using System.Collections.Generic;
using System.Linq;

using Nanon.Math.Linear;
using Nanon.NeuralNetworks.Layer;

namespace Nanon.NeuralNetworks.Layer.Composition
{
	public class CompositeLayer<InputT, HiddenT, OutputT>: ICompositeLayer<InputT, OutputT>
	{
		ISingleLayer<InputT,  HiddenT> first;
		ICompositeLayer<HiddenT, OutputT> second;
		
		public CompositeLayer(ISingleLayer<InputT, HiddenT> fst, ICompositeLayer<HiddenT, OutputT> snd)
		{
			first  = fst;
			second = snd;
		}
		
		public static ICompositeLayer<A, B> Singleton<A, B>(ISingleLayer<A, B> fst) 
			where B : IMatrix<B>
		{
			return new CompositeLayer<A, B, B>(fst, new OutputLayer<B>());
		}
		
		public static ICompositeLayer<A, C> Compose<A, B, C>(ISingleLayer<A, B> fst, ISingleLayer<B, C> snd)
			where C : IMatrix<C>
		{
			return new CompositeLayer<A, B, C>(fst, Singleton<B, C>(snd));
		}
		
		public static ICompositeLayer<A, D> Compose<A, B, C, D>(ISingleLayer<A, B> fst, ISingleLayer<B, C> snd, ISingleLayer<C, D> trd)
			where D : IMatrix<D>
		{
			var tail = Compose<B, C, D>(snd, trd);
			return new CompositeLayer<A, B, D>(fst, tail);
		}
		
		public static ICompositeLayer<A, E> Compose<A, B, C, D, E>(ISingleLayer<A, B> fst, 
		                                                           ISingleLayer<B, C> snd, 
		                                                           ISingleLayer<C, D> trd,
		                                                           ISingleLayer<D, E> fth)
			where E : IMatrix<E>
		{
			var tail = Compose<B, C, D, E>(snd, trd, fth);
			return new CompositeLayer<A, B, E>(fst, tail);
		}
		
		public static ICompositeLayer<A, F> Compose<A, B, C, D, E, F>(ISingleLayer<A, B> fst, 
		                                                           ISingleLayer<B, C> snd, 
		                                                           ISingleLayer<C, D> trd,
		                                                           ISingleLayer<D, E> fth,
		                                                           ISingleLayer<E, F> ffh)
			where F : IMatrix<F>
		{
			var tail = Compose<B, C, D, E, F>(snd, trd, fth, ffh);
			return new CompositeLayer<A, B, F>(fst, tail);
		}
		
		public static ICompositeLayer<A, G> Compose<A, B, C, D, E, F, G>(ISingleLayer<A, B> fst, 
		                                                           ISingleLayer<B, C> snd, 
		                                                           ISingleLayer<C, D> trd,
		                                                           ISingleLayer<D, E> fth,
		                                                           ISingleLayer<E, F> ffh,
		                                                           ISingleLayer<F, G> sth)
			where G : IMatrix<G>
		{
			var tail = Compose<B, C, D, E, F, G>(snd, trd, fth, ffh, sth);
			return new CompositeLayer<A, B, G>(fst, tail);
		}
		
		public static ICompositeLayer<A, H> Compose<A, B, C, D, E, F, G, H>(
																   ISingleLayer<A, B> fst, 
		                                                           ISingleLayer<B, C> snd, 
		                                                           ISingleLayer<C, D> trd,
		                                                           ISingleLayer<D, E> fth,
		                                                           ISingleLayer<E, F> ffh,
		                                                           ISingleLayer<F, G> sth,
																   ISingleLayer<G, H> svth)
			where H : IMatrix<H>
		{
			var tail = Compose<B, C, D, E, F, G, H>(snd, trd, fth, ffh, sth, svth);
			return new CompositeLayer<A, B, H>(fst, tail);
		}
		
		#region ICompositeLayer[InputT,OutputT] implementation
		
		public OutputT FeedForward(InputT input)
		{
			var hidden = first.FeedForward(input);
			var output = second.FeedForward(hidden);
			return output;
		}

		public InputT PropagateBackward(InputT input, OutputT error)
		{
			var errorInHiddenLayer = second.PropagateBackward(first.Output, error);
			first.Gradient(input, errorInHiddenLayer);
			return first.PropagateBackward(input, errorInHiddenLayer);
		}
		
		public void Backprop(InputT input, OutputT error)
		{
			var errorInHiddenLayer = second.PropagateBackward(first.Output, error);
			first.Gradient(input, errorInHiddenLayer);
		}

		public void Correct(double coeff)
		{
			first.Correct(coeff);
			second.Correct(coeff);
		}
		
		#endregion
	}
}