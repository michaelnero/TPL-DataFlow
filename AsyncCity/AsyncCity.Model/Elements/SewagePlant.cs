using AsyncCity.Model.Resources;
using System;
using System.Threading.Tasks.Dataflow;

namespace AsyncCity.Model.Elements {
    public class SewagePlant : ICityElement {
        public SewagePlant(Guid id, int size, ITargetBlock<ConsumptionData> underservedBlock) {
            this.ID = id;
            this.SewageConsumer = new ResourceConsumer<Sewage>(id, size, ElementType.Sewage, underservedBlock);
        }

        public Guid ID { get; private set; }

        public ResourceConsumer<Sewage> SewageConsumer { get; private set; }

        public ElementType ElementType {
            get { return ElementType.Sewage; }
        }

        public void Start() {
            this.SewageConsumer.Start(TimeSpan.FromSeconds(0.5));
        }

        public void Stop() {
            this.SewageConsumer.Stop();
        }

        public void ChangeSize(int size) {
            this.SewageConsumer.LoadFactor = size;
        }
    }
}
