using AsyncCity.Model;
using AsyncCity.Model.Elements;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AsyncCity.Hubs {
    [HubName("city")]
    public class CityHub : Hub {
        private static readonly ConcurrentDictionary<string, City> cities = new ConcurrentDictionary<string, City>();

        public void StartCity() {
            string connectionID = this.Context.ConnectionId;
            var context = GlobalHost.ConnectionManager.GetHubContext<CityHub>();
            var city = new City(connectionID, context);

            cities[connectionID] = city;
        }

        public void AddElement(ElementType type) {
            City city;
            if (cities.TryGetValue(this.Context.ConnectionId, out city)) {
                switch (type) {
                    case ElementType.Business:
                        city.AddBusiness(1);
                        break;
                    case ElementType.Electricity:
                        city.AddElectricCompany(1);
                        break;
                    case ElementType.House:
                        city.AddHome(1);
                        break;
                    case ElementType.Sewage:
                        city.AddSewagePlant(1);
                        break;
                    case ElementType.Trash:
                        city.AddTrashIncinerator(1);
                        break;
                    case ElementType.Water:
                        city.AddWaterTower(1);
                        break;
                    default:
                        break;
                }
            }
        }

        public void RemoveElement(Guid id) {
            City city;
            if (cities.TryGetValue(this.Context.ConnectionId, out city)) {
                city.RemoveElement(id);
            }
        }

        public void ChangeElementSize(Guid id, int size) {
            City city;
            if (cities.TryGetValue(this.Context.ConnectionId, out city)) {
                city.ChangeElementSize(id, size);
            }
        }

        public override Task OnDisconnected() {
            City city;
            if (cities.TryRemove(this.Context.ConnectionId, out city)) {
                city.Stop();
            }

            return base.OnDisconnected();
        }
    }
}