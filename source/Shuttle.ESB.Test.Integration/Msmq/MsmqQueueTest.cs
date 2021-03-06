using System;
using System.IO;
using System.Transactions;
using NUnit.Framework;
using Shuttle.ESB.Msmq;

namespace Shuttle.ESB.Test.Integration.Msmq
{
	[TestFixture]
	public class MsmqQueueTest : IntegrationFixture
	{
		protected override void TestTearDown()
		{
			inboxQueue.Drop();
			outboxQueue.Drop();
		}

		private MsmqQueue inboxQueue;
		private MsmqQueue outboxQueue;

		protected override void TestSetUp()
		{
			base.TestSetUp();

			inboxQueue = new MsmqQueue(new Uri("msmq://./sit_inbox"), true);
			outboxQueue = new MsmqQueue(new Uri("msmq://./sit_outbox"), true);

			inboxQueue.Create();
			outboxQueue.Create();
		}

		[Test]
		public void Should_be_able_to_remove_a_message()
		{
			var messageId = Guid.NewGuid();

			inboxQueue.Enqueue(messageId, new MemoryStream());

			inboxQueue.Remove(messageId);

			Assert.IsNull(inboxQueue.Dequeue());
		}

		[Test]
		public void Should_be_able_to_send_and_receive_a_message_to_a_queue()
		{
			var stream = new MemoryStream();

			stream.WriteByte(100);

			inboxQueue.Enqueue(Guid.NewGuid(), stream);

			var dequeued = inboxQueue.Dequeue();

			Assert.AreEqual(100, dequeued.ReadByte());
		}

		[Test]
		public void Should_be_able_to_rollback_message_sent_in_transaction()
		{
			using (new TransactionScope())
			{
				inboxQueue.Enqueue(Guid.NewGuid(), new MemoryStream());
			}

			Assert.AreEqual(0, inboxQueue.Count);

			inboxQueue.Enqueue(Guid.NewGuid(), new MemoryStream());

			Assert.AreEqual(1, inboxQueue.Count);
		}
	}
}