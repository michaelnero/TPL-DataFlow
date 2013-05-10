using AsyncCity.Model.Resources;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace AsyncCity.Model.Elements {
    public class ResourceConsumer<T> where T : Resource, new() {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ResourceConsumer<T>));

        private readonly object sync = new object();
        private readonly ConcurrentDictionary<ISourceBlock<T>, IDisposable> sourceLinkMappings = new ConcurrentDictionary<ISourceBlock<T>, IDisposable>();
        private readonly LinkedList<int> deficitHistory = new LinkedList<int>();
        private readonly TransformBlock<T, T> consumeBlock;
        private readonly Timer reportConsumptionTimer;
        private readonly ITargetBlock<ConsumptionData> consumptionBlock;        

        private long state; // 0 = stopped, 1 = start requested, 2 = running, 3 = stop requested

        private int? currentLoad;
        private IDisposable targetLink;

        public ResourceConsumer(Guid id, int loadFactor, ElementType elementType, ITargetBlock<ConsumptionData> consumptionBlock) {
            this.ID = id;
            this.LoadFactor = loadFactor;
            this.ElementType = elementType;
            this.consumptionBlock = consumptionBlock;
            this.consumeBlock = new TransformBlock<T, T>(new Func<T, T>(this.ConsumeResource));
            this.reportConsumptionTimer = new Timer(this.OnReduceLoadTimerTick, null, Timeout.Infinite, Timeout.Infinite);
        }

        public Guid ID { get; private set; }

        public ElementType ElementType { get; private set; }

        public int LoadFactor { get; set; }

        public ITargetBlock<T> Target {
            get { return this.consumeBlock; }
        }

        public void Start(TimeSpan reduceLoadTimeSpan) {
            if (0 != Interlocked.CompareExchange(ref this.state, 1, 0)) {
                // Wasn't stopped
                return;
            }

            // Move to running state
            Interlocked.Exchange(ref this.state, 2);

            this.reportConsumptionTimer.Change(reduceLoadTimeSpan, reduceLoadTimeSpan);
        }

        public void Stop() {
            if (Interlocked.CompareExchange(ref this.state, 3, 2) != 2) {
                // Wasn't running
                return;
            }

            Interlocked.Exchange(ref this.state, 0);

            this.consumeBlock.Complete();
            this.ExchangeNext(null);

            this.reportConsumptionTimer.Dispose();
        }

        public IDisposable AddSource(ISourceBlock<T> source) {
            var link = source.LinkTo(this.consumeBlock);
            this.sourceLinkMappings.TryAdd(source, link);
            return link;
        }

        public void RemoveSource(ISourceBlock<T> source) {
            IDisposable sourceLink;
            if (this.sourceLinkMappings.TryRemove(source, out sourceLink)) {
                sourceLink.Dispose();
            }
        }

        public void ExchangeNext(ITargetBlock<T> next) {
            lock (this.sync) {
                if (null != this.targetLink) {
                    this.targetLink.Dispose();
                }

                if (null != next) {
                    this.targetLink = this.consumeBlock.LinkTo(next);
                }
            }
        }

        private T ConsumeResource(T resource) {
            var copy = new T {
                OriginatingElementID = resource.OriginatingElementID,
                Units = resource.Units
            };

            int loadFactor = this.LoadFactor;

            lock (this.sync) {
                while ((0 != copy.Units) && (this.currentLoad.GetValueOrDefault(-1) < loadFactor)) {
                    // The resource consumer will greedily consume as many units as it needs to function
                    this.currentLoad = this.currentLoad.GetValueOrDefault() + 1;
                    copy.Units--;
                }
            }

            return copy;
        }

        private void OnReduceLoadTimerTick(object ignore) {
            int currentDeficit;
            lock (this.sync) {
                currentDeficit = (this.currentLoad.GetValueOrDefault(-1) < this.LoadFactor) ? 1 : 0;

                if (null != this.currentLoad) {
                    this.currentLoad = this.currentLoad.GetValueOrDefault() - 1;
                    if (0 > this.currentLoad.GetValueOrDefault()) {
                        this.currentLoad = null;
                    }
                }
            }

            if (1 == currentDeficit) {
                logger.InfoFormat("Element {0} of type {1} has deficit {2} with load {3}", this.ID, this.ElementType, currentDeficit, this.currentLoad);
            }

            ConsumptionData data;
            lock (this.deficitHistory) {
                this.deficitHistory.AddLast(currentDeficit);
                if (10 < this.deficitHistory.Count) {
                    this.deficitHistory.RemoveFirst();
                }

                var deficit = this.deficitHistory.Sum();

                data = new ConsumptionData(this.ID, deficit, this.ElementType);
            }
            
            this.consumptionBlock.SendAsync(data);
        }
    }
}
