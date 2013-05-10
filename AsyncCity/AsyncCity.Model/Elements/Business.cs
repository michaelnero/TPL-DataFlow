using AsyncCity.Model.Resources;
using System;
using System.Threading.Tasks.Dataflow;

namespace AsyncCity.Model.Elements {
    public class Business : ICityElement {
        public Business(Guid id, int size, ITargetBlock<ConsumptionData> underservedBlock) {
            this.ID = id;
            this.ElectricityConsumer = new ResourceConsumer<Electricity>(id, size, ElementType.Electricity, underservedBlock);
            this.WaterConsumer = new ResourceConsumer<Water>(id, size, ElementType.Water, underservedBlock);
            this.PeopleConsumer = new ResourceConsumer<People>(id, size, ElementType.People, underservedBlock);
            this.TrashProducer = new ResourceProducer<Trash>(id, size);
            this.SewageProducer = new ResourceProducer<Sewage>(id, size);
        }

        public Guid ID { get; private set; }

        public ResourceConsumer<Electricity> ElectricityConsumer { get; private set; }

        public ResourceConsumer<Water> WaterConsumer { get; private set; }

        public ResourceConsumer<People> PeopleConsumer { get; private set; }

        public ResourceProducer<Trash> TrashProducer { get; private set; }

        public ResourceProducer<Sewage> SewageProducer { get; private set; }

        public ElementType ElementType {
            get { return ElementType.Business; }
        }

        public void Start() {
            this.ElectricityConsumer.Start(TimeSpan.FromSeconds(0.5));
            this.WaterConsumer.Start(TimeSpan.FromSeconds(0.5));
            this.PeopleConsumer.Start(TimeSpan.FromSeconds(0.5));
            this.TrashProducer.Start(TimeSpan.FromSeconds(0.5));
            this.SewageProducer.Start(TimeSpan.FromSeconds(0.5));
        }

        public void Stop() {
            this.ElectricityConsumer.Stop();
            this.WaterConsumer.Stop();
            this.PeopleConsumer.Stop();
            this.TrashProducer.Stop();
            this.SewageProducer.Stop();
        }
        
        public void ChangeSize(int size) {
            this.ElectricityConsumer.LoadFactor = size;
            this.WaterConsumer.LoadFactor = size;
            this.PeopleConsumer.LoadFactor = size;
            this.TrashProducer.UnitsToProduce = size;
            this.SewageProducer.UnitsToProduce = size;
        }
    }
}
