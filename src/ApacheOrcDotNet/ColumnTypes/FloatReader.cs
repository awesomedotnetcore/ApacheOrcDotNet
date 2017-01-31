﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApacheOrcDotNet.ColumnTypes
{
	public class FloatReader : ColumnReader
	{
		public FloatReader(StripeStreamCollection stripeStreams, uint columnId) : base(stripeStreams, columnId)
		{
		}

		public IEnumerable<float?> Read()
		{
			var present = ReadBooleanStream(Protocol.StreamKind.Present);
			var data = ReadBinaryStream(Protocol.StreamKind.Data);
			int dataIndex = 0;
			if (present == null)
			{
				var value = BitConverter.ToSingle(data, dataIndex);
				dataIndex += 4;
				yield return value;
			}
			else
			{
				foreach (var isPresent in present)
				{
					if (isPresent)
					{
						var value = BitConverter.ToSingle(data, dataIndex);
						dataIndex += 4;
						yield return value;
					}
					else
						yield return null;
				}
			}
		}
	}
}
