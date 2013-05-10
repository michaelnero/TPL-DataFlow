using AsyncCity.Model.Elements;
using AsyncCity.Model.Resources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace AsyncCity.Model {
    public class ConsumingNode<T> where T : Resource, new() {
        private readonly LinkedList<ResourceConsumer<T>> consumers = new LinkedList<ResourceConsumer<T>>();
        private readonly ConcurrentDictionary<Guid, IDisposable> producerLinks = new ConcurrentDictionary<Guid, IDisposable>();
        private readonly ConcurrentDictionary<Guid, IDisposable> consumerLinks = new ConcurrentDictionary<Guid, IDisposable>();
        private readonly BufferBlock<Resource> wasteBlock;

        public ConsumingNode(BufferBlock<Resource> wasteBlock) {
            this.Buffer = new BufferBlock<T>();
            this.wasteBlock = wasteBlock;
        }

        public BufferBlock<T> Buffer { get; private set; }

        public void Link(ResourceProducer<T> producer) {
            this.producerLinks[producer.ID] = producer.Source.LinkTo(this.Buffer);
        }

        public void Link(ResourceConsumer<T> consumer) {
            // A consuming node is basically a pipeline

            lock (this.consumers) {
                var firstNode = this.consumers.First;
                if (null == firstNode) {
                    // If we don't have a first node, set the buffer as the source for this consumer
                    consumer.AddSource(this.Buffer);
                    this.consumers.AddFirst(consumer);
                } else {
                    // If we do have nodes in the list, set the provided consumer as the next consumer in the pipeline
                    this.consumers.Last.Value.ExchangeNext(consumer.Target);
                    this.consumers.AddLast(consumer);
                }

                // Set the waste block as the next consumer of the provided consumer, to keep the waste block last in the pipeline
                consumer.ExchangeNext(this.wasteBlock);
            }
        }

        public void UnLink(Guid id) {
            IDisposable producerLink;
            if (this.producerLinks.TryRemove(id, out producerLink)) {
                producerLink.Dispose();
            }

            lock (this.consumers) {
                var consumer = this.consumers.FirstOrDefault(c => c.ID == id);
                if (null != consumer) {
                    consumer.RemoveSource(this.Buffer);
                    consumer.ExchangeNext(null);

                    var node = this.consumers.Find(consumer);
                    
                    if ((null != node.Previous) && (null != node.Next)) { // If we're removing a node in the middle
                        var previous = node.Previous.Value;
                        var next = node.Next.Value;

                        previous.ExchangeNext(next.Target);
                    } else if ((node == this.consumers.Last) && (node != this.consumers.First)) { // If we're removing the last node, but not the only node
                        var previous = node.Previous.Value;
                        
                        previous.ExchangeNext(this.wasteBlock);
                    } else if ((node == this.consumers.First) && (null != node.Next)) { // If we're removing the first node, but not the only node
                        var next = node.Next.Value;
                        
                        next.AddSource(this.Buffer);
                    }

                    this.consumers.Remove(node);
                }
            }
        }
    }
}
