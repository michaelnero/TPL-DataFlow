using AsyncCity.Model.Resources;
using log4net;
using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace AsyncCity.Model.Elements {
    public class ResourceProducer<T> where T : Resource, new() {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ResourceProducer<T>));

        private readonly BufferBlock<T> produceBlock = new BufferBlock<T>();
        private readonly Timer produceTimer;

        private long state; // 0 = stopped, 1 = start requested, 2 = running, 3 = stop requested

        public ResourceProducer(Guid id, int unitsToProduce) {
            this.ID = id;
            this.UnitsToProduce = unitsToProduce;
            this.produceTimer = new Timer(this.OnProduceTimerTick, null, Timeout.Infinite, Timeout.Infinite);
        }

        public int UnitsToProduce { get; set; }

        public Guid ID { get; private set; }

        public ISourceBlock<T> Source {
            get { return this.produceBlock; }
        }

        public void Start(TimeSpan produceTimeSpan) {
            if (0 != Interlocked.CompareExchange(ref this.state, 1, 0)) {
                // Wasn't stopped
                return;
            }

            // Move to running state;
            Interlocked.Exchange(ref this.state, 2);

            this.produceTimer.Change(produceTimeSpan, produceTimeSpan);
        }

        public void Stop() {
            if (Interlocked.CompareExchange(ref this.state, 3, 2) != 2) {
                // Wasn't running
                return;
            }

            Interlocked.Exchange(ref this.state, 0);

            this.produceBlock.Complete();
            this.produceTimer.Dispose();
        }

        private void OnProduceTimerTick(object ignore) {
            logger.InfoFormat("Element {0} of type {1} producing {2}", this.ID, typeof(T).Name, this.UnitsToProduce);

            this.produceBlock.SendAsync(new T {
                OriginatingElementID = this.ID,
                Units = this.UnitsToProduce
            });
        }
    }
}
