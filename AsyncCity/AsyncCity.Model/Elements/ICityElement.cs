using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCity.Model.Elements {
    public interface ICityElement {
        Guid ID { get; }

        ElementType ElementType { get; }

        void Start();

        void Stop();

        void ChangeSize(int size);
    }
}
