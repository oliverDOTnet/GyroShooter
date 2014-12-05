using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiMath
{
    abstract class MovingAverage<T>
    {
        private Queue<T> data;
        private int order;
        private T value;

        public MovingAverage(int order)
        {
            this.order = order;
            this.data = new Queue<T>();
        }

        public void Push(T d)
        {
            data.Enqueue(d);

            while (data.Count > order)
            {
                data.Dequeue();
            }

            value = Aggregate(data);
        }

        protected abstract T Aggregate(Queue<T> data);

        public T Average
        {
            get { return value; }
        }

        public IEnumerable<T> Values
        {
            get { return data; }
        }

        public bool IsValid
        {
            get { return data.Count == order; }
        }
    }

    class MovingAverageDouble : MovingAverage<double>
    {
        public MovingAverageDouble(int order) : base(order) { }
        protected override double Aggregate(Queue<double> data)
        {
            return data.Aggregate((double x, double y) => x + y) / data.Count;
        }
    }
    class MovingAverageVector3 : MovingAverage<Vector3>
    {
        public MovingAverageVector3(int order) : base(order) { }
        protected override Vector3 Aggregate(Queue<Vector3> data)
        {
            return data.Aggregate((Vector3 x, Vector3 y) => x + y) / data.Count;
        }
    }
}
