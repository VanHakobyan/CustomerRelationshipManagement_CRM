using System.Collections.Generic;
using System.Web.Http;

namespace CRM.WebApp.Controllers
{
    [ExceptionCustomFilter]
    [RoutePrefix("api/Orders")]
    public class OrdersController : ApiController
    {
        [Authorize]
        public IHttpActionResult Get()
        {
            return Ok(Order.CreateOrders());
        }

    }

    #region Helpers
    [ExceptionCustomFilter]
    public class Order
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; }
        public string ShipperCity { get; set; }
        public bool IsShipped { get; set; }

        public static List<Order> CreateOrders()
        {
            List<Order> orderList = new List<Order>
            {
                new Order {OrderID = 10248, CustomerName = "Tsovinar Ghazaryan", ShipperCity = "Madrid", IsShipped = true },
                new Order {OrderID = 10249, CustomerName = "Khachik Sukiasyan", ShipperCity = "London", IsShipped = false},
                new Order {OrderID = 10250,CustomerName = "Tatevik Begjanyan", ShipperCity = "Paris", IsShipped = false },
                new Order {OrderID = 10251,CustomerName = "Narine Boyakchyan", ShipperCity = "Abu Dhabi", IsShipped = false},
                new Order {OrderID = 10252,CustomerName = "Zara Muradyan", ShipperCity = "Kuwait", IsShipped = true}
            };

            return orderList;
        }
    }

    #endregion
}
