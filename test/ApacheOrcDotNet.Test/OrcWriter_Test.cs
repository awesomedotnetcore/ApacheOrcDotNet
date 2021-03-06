﻿using ApacheOrcDotNet.ColumnTypes;
using ApacheOrcDotNet.Test.TestHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ApacheOrcDotNet.Test
{
    public class OrcWriter_Test
    {
		[Fact]
		public void CompleteStream_IntColumn_SingleStripe_RoundTrip()
		{
			var testElements = new List<IntColumnTest>();
			var random = new Random(123);
			for (int i = 0; i < 70000; i++)
				testElements.Add(new IntColumnTest { Column1 = random.Next() });
			TestRoundTripLongColumn(testElements, 1, testElements.Select(e => (long?)e.Column1));
		}


        [Fact]
        public void CompleteStream_IntColumn_MultipleStripe_RoundTrip()
        {
            var testElements = new List<IntColumnTest>();
            var random = new Random(123);
            for (int i = 0; i < 32000000; i++)
                testElements.Add(new IntColumnTest { Column1 = random.Next() });
            TestRoundTripLongColumn(testElements, 1, testElements.Select(e => (long?)e.Column1));
        }

        void TestRoundTripLongColumn<T>(IEnumerable<T> testElements, int columnId, IEnumerable<long?> expectedResults)
		{
			var memStream = new MemoryStream();
			using (var writer = new OrcWriter<T>(memStream, new WriterConfiguration())) //Use the default configuration
			{
				writer.AddRows(testElements);
			}

			memStream.Seek(0, SeekOrigin.Begin);

			var dataFile = new DataFileHelper(memStream);
			var stream = dataFile.GetStream();
			var fileTail = new FileTail(stream);

			var expectedEnumerator = expectedResults.GetEnumerator();
			foreach(var stripe in fileTail.Stripes)
			{
				var stripeStreamCollection = stripe.GetStripeStreamCollection();
				var longReader = new LongReader(stripeStreamCollection, (uint)columnId);
				var resultEnumerator = longReader.Read().GetEnumerator();

                ulong count = 0;
				while(resultEnumerator.MoveNext())
				{
					Assert.True(expectedEnumerator.MoveNext());
					Assert.Equal(expectedEnumerator.Current, resultEnumerator.Current);
                    count++;
				}
                Assert.Equal(stripe.NumRows, count);
			}
			Assert.False(expectedEnumerator.MoveNext());		//We should have used all expected results
		}
	}

	class IntColumnTest
	{
		public int Column1 { get; set; }
	}
}
