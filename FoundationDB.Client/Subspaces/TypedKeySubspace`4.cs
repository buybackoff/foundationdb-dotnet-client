#region BSD Licence
/* Copyright (c) 2013-2018, Doxense SAS
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
	* Redistributions of source code must retain the above copyright
	  notice, this list of conditions and the following disclaimer.
	* Redistributions in binary form must reproduce the above copyright
	  notice, this list of conditions and the following disclaimer in the
	  documentation and/or other materials provided with the distribution.
	* Neither the name of Doxense nor the
	  names of its contributors may be used to endorse or promote products
	  derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

namespace FoundationDB.Client
{
	using System;
	using System.Diagnostics;
	using System.Runtime.CompilerServices;
	using Doxense.Collections.Tuples;
	using Doxense.Diagnostics.Contracts;
	using Doxense.Serialization.Encoders;
	using JetBrains.Annotations;

	[PublicAPI]
	public interface ITypedKeySubspace<T1, T2, T3, T4> : IKeySubspace
	{
		/// <summary>Helper to encode/decode keys using this subspace's default encoding</summary>
		[NotNull]
		TypedKeys<T1, T2, T3, T4> Keys { get; }

		/// <summary>Encoding used to generate and parse the keys of this subspace</summary>
		[NotNull]
		ICompositeKeyEncoder<T1, T2, T3, T4> KeyEncoder { get; }

	}

	[PublicAPI]
	public sealed class TypedKeySubspace<T1, T2, T3, T4> : KeySubspace, ITypedKeySubspace<T1, T2, T3, T4>
	{
		public ICompositeKeyEncoder<T1, T2, T3, T4> KeyEncoder { get; }

		internal TypedKeySubspace(Slice prefix, [NotNull] ICompositeKeyEncoder<T1, T2, T3, T4> encoder)
			: base(prefix)
		{
			this.KeyEncoder = encoder;
			this.Keys = new TypedKeys<T1, T2, T3, T4>(this, this.KeyEncoder);
		}

		public TypedKeys<T1, T2, T3, T4> Keys { get; }

	}

	[DebuggerDisplay("{Parent.ToString(),nq)}")]
	[PublicAPI]
	public sealed class TypedKeys<T1, T2, T3, T4>
	{

		[NotNull]
		private readonly TypedKeySubspace<T1, T2, T3, T4> Parent;

		[NotNull]
		public ICompositeKeyEncoder<T1, T2, T3, T4> Encoder { get; }

		internal TypedKeys(
			[NotNull] TypedKeySubspace<T1, T2, T3, T4> parent,
			[NotNull] ICompositeKeyEncoder<T1, T2, T3, T4> encoder)
		{
			Contract.Requires(parent != null && encoder != null);
			this.Parent = parent;
			this.Encoder = encoder;
		}

		#region ToRange()

		/// <summary>Return the range of all legal keys in this subpsace</summary>
		/// <returns>A "legal" key is one that can be decoded into the original triple of values</returns>
		[Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
		public KeyRange ToRange()
		{
			return this.Parent.ToRange();
		}

		/// <summary>Return the range of all legal keys in this subpsace, that start with the specified triple of values</summary>
		/// <returns>Range that encompass all keys that start with (tuple.Item1, tuple.Item2, tuple.Item3)</returns>
		[Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
		public KeyRange ToRange(STuple<T1, T2, T3, T4> tuple)
		{
			return ToRange(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
		}

		/// <summary>Return the range of all legal keys in this subpsace, that start with the specified triple of values</summary>
		/// <returns>Range that encompass all keys that start with (tuple.Item1, tuple.Item2, tuple.Item3)</returns>
		[Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
		public KeyRange ToRange((T1, T2, T3, T4) tuple)
		{
			return ToRange(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
		}

		/// <summary>Return the range of all legal keys in this subpsace, that start with the specified triple of values</summary>
		/// <returns>Range that encompass all keys that start with (item1, item2, item3)</returns>
		public KeyRange ToRange(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			//HACKHACK: add concept of "range" on  IKeyEncoder ?
			return KeyRange.PrefixedBy(Encode(item1, item2, item3, item4));
		}

		#endregion

		#region ToRangePartial()

		/// <summary>Return the range of all legal keys in this subpsace, that start with the specified triple of values</summary>
		/// <returns>Range that encompass all keys that start with (item1, item2, item3)</returns>
		public KeyRange ToRangePartial(STuple<T1, T2, T3> tuple)
		{
			//HACKHACK: add concept of "range" on  IKeyEncoder ?
			return KeyRange.PrefixedBy(EncodePartial(tuple.Item1, tuple.Item2, tuple.Item3));
		}

		/// <summary>Return the range of all legal keys in this subpsace, that start with the specified triple of values</summary>
		/// <returns>Range that encompass all keys that start with (item1, item2, item3)</returns>
		public KeyRange ToRangePartial((T1, T2, T3) tuple)
		{
			//HACKHACK: add concept of "range" on  IKeyEncoder ?
			return KeyRange.PrefixedBy(EncodePartial(tuple.Item1, tuple.Item2, tuple.Item3));
		}

		/// <summary>Return the range of all legal keys in this subpsace, that start with the specified triple of values</summary>
		/// <returns>Range that encompass all keys that start with (item1, item2, item3)</returns>
		public KeyRange ToRangePartial(T1 item1, T2 item2, T3 item3)
		{
			//HACKHACK: add concept of "range" on  IKeyEncoder ?
			return KeyRange.PrefixedBy(EncodePartial(item1, item2, item3));
		}

		/// <summary>Return the range of all legal keys in this subpsace, that start with the specified pair of values</summary>
		/// <returns>Range that encompass all keys that start with (item1, item2)</returns>
		public KeyRange ToRangePartial(T1 item1, T2 item2)
		{
			//HACKHACK: add concept of "range" on  IKeyEncoder ?
			return KeyRange.PrefixedBy(EncodePartial(item1, item2));
		}

		/// <summary>Return the range of all legal keys in this subpsace, that start with the specified triple of values</summary>
		/// <returns>Range that encompass all keys that start with (item1, item2, item3)</returns>
		public KeyRange ToRangePartial(T1 item1)
		{
			//HACKHACK: add concept of "range" on  IKeyEncoder ?
			return KeyRange.PrefixedBy(EncodePartial(item1));
		}

		#endregion

		#region Pack()

		public Slice this[T1 item1, T2 item2, T3 item3, T4 item4]
		{
			[Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Encode(item1, item2, item3, item4);
		}

		public Slice this[(T1, T2, T3, T4) items]
		{
			[Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Encode(items.Item1, items.Item2, items.Item3, items.Item4);
		}

		[Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Slice Pack(STuple<T1, T2, T3, T4> tuple)
		{
			return Encode(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
		}

		[Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Slice Pack((T1, T2, T3, T4) tuple)
		{
			return Encode(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
		}

		[Pure]
		public Slice Pack<TTuple>(TTuple tuple)
			where TTuple : IVarTuple
		{
			tuple.OfSize(4);
			return Encode(tuple.Get<T1>(0), tuple.Get<T2>(1), tuple.Get<T3>(2), tuple.Get<T4>(3));
		}

		#endregion

		#region Encode()

		[Pure]
		public Slice Encode(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			var bytes = this.Encoder.EncodeKey(item1, item2, item3, item4);
			var sw = this.Parent.OpenWriter(bytes.Count);
			sw.WriteBytes(in bytes);
			return sw.ToSlice();
		}

		#endregion

		#region EncodePartial()

		[Pure]
		public Slice EncodePartial(T1 item1, T2 item2, T3 item3)
		{
			var sw = this.Parent.OpenWriter(24);
			var tuple = (item1, item2, item3, default(T4));
			this.Encoder.WriteKeyPartsTo(ref sw, 3, ref tuple);
			return sw.ToSlice();
		}

		[Pure]
		public Slice EncodePartial(T1 item1, T2 item2)
		{
			var sw = this.Parent.OpenWriter(16);
			var tuple = (item1, item2, default(T3), default(T4));
			this.Encoder.WriteKeyPartsTo(ref sw, 1, ref tuple);
			return sw.ToSlice();
		}

		[Pure]
		public Slice EncodePartial(T1 item1)
		{
			var sw = this.Parent.OpenWriter(16);
			var tuple = (item1, default(T2), default(T3), default(T4));
			this.Encoder.WriteKeyPartsTo(ref sw, 1, ref tuple);
			return sw.ToSlice();
		}

		#endregion

		#region Decode()

		[Pure]
		//REVIEW: => Unpack()?
		//REVIEW: return ValueTuple<..> instead? (C#7)
		public STuple<T1, T2, T3, T4> Decode(Slice packedKey)
		{
			return this.Encoder.DecodeKey(this.Parent.ExtractKey(packedKey));
		}

		public void Decode(Slice packedKey, out T1 item1, out T2 item2, out T3 item3, out T4 item4)
		{
			(item1, item2, item3, item4) = this.Encoder.DecodeKey(this.Parent.ExtractKey(packedKey));
		}

		#endregion

		#region Dump()

		/// <summary>Return a user-friendly string representation of a key of this subspace</summary>
		[Pure]
		public string Dump(Slice packedKey)
		{
			if (packedKey.IsNull) return String.Empty;
			//TODO: defer to the encoding itself?
			var key = this.Parent.ExtractKey(packedKey);
			try
			{
				//REVIEW: we need a TryUnpack!
				return this.Encoder.DecodeKey(key).ToString();
			}
			catch (Exception)
			{ // decoding failed, or some other non-trival
				return key.PrettyPrint();
			}
		}

		#endregion

	}

}
