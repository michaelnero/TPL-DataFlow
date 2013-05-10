using AsyncCity.Model.Resources;
using System;

namespace AsyncCity.Model.Elements {
    public class ElectricCompany : ICityElement {
        public ElectricCompany(Guid id, int size) {
            this.ID = id;
            this.ElectricityProducer = new ResourceProducer<Electricity>(id, size);
        }

        public Guid ID { get; private set; }

        public ResourceProducer<Electricity> ElectricityProducer { get; private set; }

        public ElementType ElementType {
            get { return ElementType.Electricity; }
        }

        public void Start() {
            this.ElectricityProducer.Start(TimeSpan.FromSeconds(0.5));
        }

        public void Stop() {
            this.ElectricityProducer.Stop();
        }

        public void ChangeSize(int size) {
            this.ElectricityProducer.UnitsToProduce = size;
        }
    }
}
