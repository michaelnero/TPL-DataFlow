using AsyncCity.Model.Elements;
using AsyncCity.Model.Resources;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks.Dataflow;

namespace AsyncCity.Model {
    public class City {
        private readonly string connectionID;
        private readonly IHubContext hubContext;
        private readonly BufferBlock<Resource> wasteBlock;
        private readonly BufferBlock<ConsumptionData> consumptionDataBlock;
        private readonly ConsumingNode<Electricity> electricityConsumer;
        private readonly ConsumingNode<Water> waterConsumer;
        private readonly ConsumingNode<Trash> trashConsumer;
        private readonly ConsumingNode<Sewage> sewageConsumer;
        private readonly ConsumingNode<People> peopleConsumer;
        private readonly List<ICityElement> elements = new List<ICityElement>();

        public City(string connectionID, IHubContext hubContext) {
            this.connectionID = connectionID;
            this.hubContext = hubContext;

            this.wasteBlock = new BufferBlock<Resource>();
            this.consumptionDataBlock = new BufferBlock<ConsumptionData>();
            this.electricityConsumer = new ConsumingNode<Electricity>(this.wasteBlock);
            this.waterConsumer = new ConsumingNode<Water>(this.wasteBlock);
            this.trashConsumer = new ConsumingNode<Trash>(this.wasteBlock);
            this.sewageConsumer = new ConsumingNode<Sewage>(this.wasteBlock);
            this.peopleConsumer = new ConsumingNode<People>(this.wasteBlock);

            this.consumptionDataBlock.LinkTo(new ActionBlock<ConsumptionData>(new Action<ConsumptionData>(this.OnConsumptionDataReported)));
            this.wasteBlock.LinkTo(new ActionBlock<Resource>(new Action<Resource>(this.OnWasteReported)));
        }

        public Business AddBusiness(int size) {
            var element = new Business(Guid.NewGuid(), size, this.consumptionDataBlock);

            this.electricityConsumer.Link(element.ElectricityConsumer);
            this.waterConsumer.Link(element.WaterConsumer);
            this.trashConsumer.Link(element.TrashProducer);
            this.sewageConsumer.Link(element.SewageProducer);
            this.peopleConsumer.Link(element.PeopleConsumer);

            element.Start();

            this.AddElement(element);

            return element;
        }

        public ElectricCompany AddElectricCompany(int size) {
            var element = new ElectricCompany(Guid.NewGuid(), size);

            this.electricityConsumer.Link(element.ElectricityProducer);

            element.Start();

            this.AddElement(element);

            return element;
        }

        public Home AddHome(int size) {
            var element = new Home(Guid.NewGuid(), size, this.consumptionDataBlock);

            this.electricityConsumer.Link(element.ElectricityConsumer);
            this.waterConsumer.Link(element.WaterConsumer);
            this.trashConsumer.Link(element.TrashProducer);
            this.sewageConsumer.Link(element.SewageProducer);
            this.peopleConsumer.Link(element.PeopleProducer);

            element.Start();

            this.AddElement(element);

            return element;
        }

        public SewagePlant AddSewagePlant(int size) {
            var element = new SewagePlant(Guid.NewGuid(), size, this.consumptionDataBlock);

            this.sewageConsumer.Link(element.SewageConsumer);

            element.Start();

            this.AddElement(element);

            return element;
        }

        public TrashIncinerator AddTrashIncinerator(int size) {
            var element = new TrashIncinerator(Guid.NewGuid(), size, this.consumptionDataBlock);

            this.trashConsumer.Link(element.TrashConsumer);

            element.Start();

            this.AddElement(element);

            return element;
        }

        public WaterTower AddWaterTower(int size) {
            var element = new WaterTower(Guid.NewGuid(), size);

            this.waterConsumer.Link(element.WaterProducer);

            element.Start();

            this.AddElement(element);

            return element;
        }

        public void ChangeElementSize(Guid id, int size) {
            ICityElement element = null;

            lock (this.elements) {
                element = this.elements.Find(e => e.ID == id);
            }

            if (null != element) {
                element.ChangeSize(size);
            }
        }

        public void RemoveElement(Guid id) {
            ICityElement element;
            lock (this.elements) {
                element = this.elements.Find(e => e.ID == id);
                if (null != element) {
                    this.elements.Remove(element);
                }
            }

            if (null != element) {
                this.electricityConsumer.UnLink(id);
                this.waterConsumer.UnLink(id);
                this.trashConsumer.UnLink(id);
                this.sewageConsumer.UnLink(id);
                this.peopleConsumer.UnLink(id);

                element.Stop();

                dynamic client = this.GetClient();
                client.onElementRemoved(id);
            }
        }

        public void Stop() {
            lock (this.elements) {
                var allElements = new List<ICityElement>(this.elements);
                foreach (var element in allElements) {
                    this.RemoveElement(element.ID);
                }
            }
        }

        private void AddElement(ICityElement element) {
            lock (this.elements) {
                this.elements.Add(element);
            }

            dynamic client = this.GetClient();
            client.onElementAdded(element.ID, element.ElementType.ToString());
        }

        private void OnConsumptionDataReported(ConsumptionData data) {
            dynamic client = this.GetClient();
            client.onConsumptionDataReported(data.ElementID, data.Deficit, data.ElementType);
        }

        private void OnWasteReported(Resource resource) {
            dynamic client = this.GetClient();
            client.onWasteReported(resource.OriginatingElementID, resource.Units);
        }

        private dynamic GetClient() {
            dynamic client = this.hubContext.Clients.Client(this.connectionID);
            return client;
        }
    }
}
