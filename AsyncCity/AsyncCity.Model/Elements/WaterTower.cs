using AsyncCity.Model.Resources;
using System;

namespace AsyncCity.Model.Elements {
    public class WaterTower : ICityElement {
        public WaterTower(Guid id, int size) {
            this.ID = id;
            this.WaterProducer = new ResourceProducer<Water>(id, size);
        }

        public Guid ID { get; private set; }

        public ResourceProducer<Water> WaterProducer { get; private set; }

        public ElementType ElementType {
            get { return ElementType.Water; }
        }

        public void Start() {
            this.WaterProducer.Start(TimeSpan.FromSeconds(0.5));
        }

        public void Stop() {
            this.WaterProducer.Stop();
        }

        public void ChangeSize(int size) {
            this.WaterProducer.UnitsToProduce = size;
        }
    }
}
