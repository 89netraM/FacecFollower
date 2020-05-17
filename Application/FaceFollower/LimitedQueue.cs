using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceFollower
{
	class LimitedQueue<T> : IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
	{
		private readonly Queue<T> internalQueue;
		private int limit;
		public int Limit
		{
			get => limit;
			set
			{
				limit = value;
				DequeueOverflow();
			}
		}

		public int Count => internalQueue.Count;
		public object SyncRoot => (internalQueue as ICollection).SyncRoot;
		public bool IsSynchronized => (internalQueue as ICollection).IsSynchronized;

		public LimitedQueue(int limit)
		{
			internalQueue = new Queue<T>();
			Limit = limit;
		}

		public void Enqueue(T obj)
		{
			internalQueue.Enqueue(obj);
			DequeueOverflow();
		}

		public T Dequeue()
		{
			return internalQueue.Dequeue();
		}

		private void DequeueOverflow()
		{
			while (internalQueue.Count > Limit)
			{
				internalQueue.Dequeue();
			}
		}

		public void CopyTo(Array array, int index) => (internalQueue as ICollection).CopyTo(array, index);
		public IEnumerator<T> GetEnumerator() => internalQueue.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
