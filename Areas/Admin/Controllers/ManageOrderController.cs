using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Stripe;
using System.Threading.Tasks;

namespace pharmacy.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ManageOrderController : Controller
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly EmailService _emailService;
        

        public ManageOrderController(IOrderItemRepository orderItemRepository, EmailService _emailService, IOrderRepository orderRepository)
        {
            this._orderItemRepository = orderItemRepository;
            this._emailService = _emailService;
            _orderRepository = orderRepository;
            
        }

        public IActionResult GetAll()
        {
            var orders = _orderRepository.Get(
             null,
             query => query.Include(o => o.OrderItems).ThenInclude(oi => oi.Product),
             query => query.Include(o => o.OrderItems).ThenInclude(oi => oi.Device),
             query => query.Include(o => o.ApplicationUser)
         );

            return View(orders);
        }

       // Confirm Order
        public IActionResult Confirm(int id)
        {

            var order = _orderRepository.Get(
            e=>e.OrderId==id ,
            query => query.Include(o => o.OrderItems).ThenInclude(oi => oi.Product),
            query => query.Include(o => o.OrderItems).ThenInclude(oi => oi.Device),
            query => query.Include(o => o.ApplicationUser)
           ).FirstOrDefault();

            if (order != null)
            {
                
                string toEmail = order.ApplicationUser?.Email;
                string subject = $"Order Confirmation - Order #{order.OrderId}";

                
                var totalCost = order.OrderItems.Sum(item => item.cost * item.count);
                var itemList = string.Join("<br/>", order.OrderItems.Select(item =>
                    $"{item.Product?.ProductName} - Quantity: {item.count} - Price: {item.cost * item.count} EGP"));

                string body = $"Dear {order.ApplicationUser?.UserName},<br/><br/>" +
                              $"Your order has been confirmed.<br/>" +
                              $"Order ID: {order.OrderId}<br/>" +
                              $"Total Price: {totalCost} EGP<br/>" +  
                              $"Items:<br/>{itemList}<br/>" +
                              $"Thank you for shopping with us!";

                _emailService.SendOrderConfirmationEmail(toEmail, subject, body);
                TempData["confirm"] = "Order confirmed and email sent successfully.";
                return RedirectToAction("GetAll");
            }

            return NotFound();
        }


        public IActionResult CancelOrder(int id)
        {
            var order = _orderRepository.GetOne(e => e.OrderId == id, e => e.ApplicationUser, e => e.OrderItems);

            if (order != null)
            {
                _orderRepository.Delete(order);
                _orderRepository.commit();
                var customerEmail = order.ApplicationUser?.Email; 
                string subject = $"Order #{order.OrderId} Cancellation";
                string body = $"Dear {order.ApplicationUser?.UserName},<br/><br/>" +
                                  $"Your order has been canceled<br/>" +
                                  $"Order ID: {order.OrderId}<br/><br/>" +
                                  "Thank you for using our services.";

                    _emailService.SendOrderConfirmationEmail(customerEmail, subject, body);
                    
                     TempData["confirm"] = "Order Canceled and email sent successfully.";
                   
                    return RedirectToAction("GetAll");
            }

            return NotFound();
        }
    }
}
