using AsyncCity.Model.Elements;
using System;

namespace AsyncCity.Model {
    public class ConsumptionData {
        public ConsumptionData(Guid elementID, int deficit, ElementType elementType) {
            this.ElementID = elementID;
            this.Deficit = deficit;
            this.ElementType = elementType;
        }

        public Guid ElementID { get; private set; }

        public int Deficit { get; private set; }

        public ElementType ElementType { get; private set; }
    }
}
