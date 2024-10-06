using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Threading.Tasks;

namespace pharmacy.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ManageOrderController : Controller
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly EmailService _emailService;
        // private readonly IPaymentService _paymentService;

        public ManageOrderController(IOrderItemRepository orderItemRepository, EmailService _emailService) //IPaymentService _paymentService )
        {
            this._orderItemRepository = orderItemRepository;
            this._emailService = _emailService;
            //this._paymentService = _paymentService;
            

        }

        public IActionResult GetAll()
        {
            var orderItems = _orderItemRepository.Get(null, e => e.Product, e => e.Order, e => e.Device , e=> e.Order.ApplicationUser);
            return View(orderItems);
        }


        //Confirm Order
        public IActionResult Confirm(int id)
        {
            // الحصول على معلومات الاوردر
            var orderItem = _orderItemRepository.GetOne(e => e.Id == id, e => e.Product, e => e.Order, e =>e.Order.ApplicationUser);

            if (orderItem != null)
            {
               
                // إرسال رسالة التأكيد إلى البريد الإلكتروني
                string toEmail = orderItem.Order?.ApplicationUser?.Email; 
                string subject = $"Order Confirmation - Order #{orderItem.OrderId}";
                string body = $"Dear {orderItem.Order?.ApplicationUser?.UserName},<br/><br/>" +
                              $"Your order for {orderItem.Product?.ProductName} has been confirmed.<br/>" +
                              $"Order ID: {orderItem.OrderId}<br/>" +
                              $"Total Price: {orderItem.cost * orderItem.count}<br/>" +
                              $"Quantity : {orderItem.count}<br/>" +
                              $"Thank you for shopping with us!";

                _emailService.SendOrderConfirmationEmail(toEmail, subject, body);
                TempData["confirm"] = "Order confirmed and email sent successfully.";
                return RedirectToAction("GetAll");
            }

            return NotFound();
        }


        ////Cancel Order
        //public IActionResult CancelOrder(int orderitemId)
        //{
        //    // الحصول على معلومات الطلب
        //    var orderitem = _orderItemRepository.GetOne(e => e.Id == orderitemId, e => e.Order);

        //    if (orderitem != null && !string.IsNullOrEmpty(orderitem.Order.PaymentIntentId))
        //    {
        //        // استرداد الأموال باستخدام خدمة المدفوعات
        //        var refund = _paymentService.RefundPayment(orderitem.Order.PaymentIntentId);

        //        if (refund != null && refund.Status == "succeeded")
        //        {
        //            TempData["success"] = "Order has been cancelled and refund has been processed successfully.";
        //            return RedirectToAction("GetAll");
        //        }
        //        else
        //        {
        //            TempData["error"] = "Failed to process refund.";
        //            return RedirectToAction("GetAll");
        //        }
        //    }

        //    TempData["error"] = "Order not found or has no payment intent.";
        //    return RedirectToAction("GetAll");
        //}




    }
}