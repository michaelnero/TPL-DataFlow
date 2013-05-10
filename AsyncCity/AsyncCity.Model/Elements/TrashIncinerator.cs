using AsyncCity.Model.Resources;
using System;
using System.Threading.Tasks.Dataflow;

namespace AsyncCity.Model.Elements {
    public class TrashIncinerator : ICityElement {
        public TrashIncinerator(Guid id, int size, ITargetBlock<ConsumptionData> underservedBlock) {
            this.ID = id;
            this.TrashConsumer = new ResourceConsumer<Trash>(id, size, ElementType.Trash, underservedBlock);
        }

        public Guid ID { get; private set; }

        public ResourceConsumer<Trash> TrashConsumer { get; private set; }

        public ElementType ElementType {
            get { return ElementType.Trash; }
        }

        public void Start() {
            this.TrashConsumer.Start(TimeSpan.FromSeconds(0.5));
        }

        public void Stop() {
            this.TrashConsumer.Stop();
        }

        public void ChangeSize(int size) {
            this.TrashConsumer.LoadFactor = size;
        }
    }
}
