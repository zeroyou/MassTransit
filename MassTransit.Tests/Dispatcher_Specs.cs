// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Tests
{
	using System.Collections;
	using Magnum.DateTimeExtensions;
	using Messages;
	using NUnit.Framework;
	using NUnit.Framework.SyntaxHelpers;
	using Rhino.Mocks;
	using TextFixtures;

	[TestFixture]
	public class When_a_message_is_delivered_to_the_service_bus :
		LoopbackLocalAndRemoteTestFixture
	{
		internal class PingHandler : Consumes<PingMessage>.All
		{
			private static int _pinged;
			private readonly FutureMessage<PingMessage> _fm;

			public PingHandler(FutureMessage<PingMessage> fm)
			{
				_fm = fm;
			}

			public static int Pinged
			{
				get { return _pinged; }
			}

			public void Consume(PingMessage message)
			{
				_pinged++;
				_fm.Set(message);
			}
		}

		[Test]
		public void A_consumer_object_should_receive_the_message()
		{
			FutureMessage<PingMessage> fm = new FutureMessage<PingMessage>();
			PingHandler handler = new PingHandler(fm);

			LocalBus.Subscribe(handler);

			int old = PingHandler.Pinged;

			RemoteBus.Publish(new PingMessage());
			fm.IsAvailable(1.Seconds());
			Assert.That(PingHandler.Pinged, Is.GreaterThan(old));
		}

		[Test]
		public void A_consumer_type_should_be_created_to_receive_the_message()
		{
			FutureMessage<PingMessage> fm = new FutureMessage<PingMessage>();
			PingHandler ph = new PingHandler(fm);

			ObjectBuilder.Stub(x => x.GetInstance<PingHandler>()).Return(ph);
			ObjectBuilder.Stub(x => x.GetInstance<PingHandler>(new Hashtable())).IgnoreArguments().Return(ph);


			LocalBus.Subscribe<PingHandler>();

			int old = PingHandler.Pinged;

			RemoteBus.Publish(new PingMessage());
			fm.IsAvailable(1.Seconds());
			Assert.That(PingHandler.Pinged, Is.GreaterThan(old));
		}
	}
}