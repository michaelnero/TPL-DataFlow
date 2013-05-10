using AsyncCity.Model.Resources;
using System;
using System.Threading.Tasks.Dataflow;

namespace AsyncCity.Model.Elements {
    public class Home : ICityElement {
        public Home(Guid id, int residents, ITargetBlock<ConsumptionData> underservedBlock) {
            this.ID = id;
            this.ElectricityConsumer = new ResourceConsumer<Electricity>(id, residents, ElementType.Electricity, underservedBlock);
            this.WaterConsumer = new ResourceConsumer<Water>(id, residents, ElementType.Water, underservedBlock);
            this.PeopleProducer = new ResourceProducer<People>(id, residents);
            this.TrashProducer = new ResourceProducer<Trash>(id, residents);
            this.SewageProducer = new ResourceProducer<Sewage>(id, residents);
        }

        public Guid ID { get; private set; }

        public ResourceConsumer<Electricity> ElectricityConsumer { get; private set; }

        public ResourceConsumer<Water> WaterConsumer { get; private set; }

        public ResourceProducer<People> PeopleProducer { get; private set; }

        public ResourceProducer<Trash> TrashProducer { get; private set; }

        public ResourceProducer<Sewage> SewageProducer { get; private set; }

        public ElementType ElementType {
            get { return ElementType.House; }
        }

        public void Start() {
            this.ElectricityConsumer.Start(TimeSpan.FromSeconds(0.5));
            this.WaterConsumer.Start(TimeSpan.FromSeconds(0.5));
            this.PeopleProducer.Start(TimeSpan.FromSeconds(0.5));
            this.TrashProducer.Start(TimeSpan.FromSeconds(0.5));
            this.SewageProducer.Start(TimeSpan.FromSeconds(0.5));
        }

        public void Stop() {
            this.ElectricityConsumer.Stop();
            this.WaterConsumer.Stop();
            this.PeopleProducer.Stop();
            this.TrashProducer.Stop();
            this.SewageProducer.Stop();
        }

        public void ChangeSize(int size) {
            this.ElectricityConsumer.LoadFactor = size;
            this.WaterConsumer.LoadFactor = size;
            this.PeopleProducer.UnitsToProduce = size;
            this.TrashProducer.UnitsToProduce = size;
            this.SewageProducer.UnitsToProduce = size;
        }
    }
}
