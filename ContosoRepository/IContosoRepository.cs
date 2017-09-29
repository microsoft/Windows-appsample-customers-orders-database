using ContosoModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContosoRepository
{
    public class IContosoRepository
    {
        ICustomerRepository Customers { get; }

        IOrderRepository Orders { get; }

        IProductRepository Products { get;  }
    }
}
