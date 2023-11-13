﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PetShop.Models;
using PetShop.Service.Products;
using System.Net;

namespace PetShop.Controllers
{
    public class OrderController : Controller
    {
        public ProductService _productService;
        private readonly CodecampN3Context _context;
        public OrderController(ProductService productService, CodecampN3Context context)
        {
            _productService = productService;
            _context = context;
        }

        [Route("addcart/{productid:int}", Name = "addcart")]
        public IActionResult AddToCart([FromRoute] int productid)
        {
            //not ok
            var product = _productService.GetById(productid);

            if (product == null)
            {
                return NotFound("Cart emty");
            }

            var cart = GetCartItems();
            var cartitem = cart.Find(p => p.product.Id == productid);
            if (cartitem != null)
            {
                cartitem.quantity++;
            }
            else
            {
                cart.Add(new CartItem() { quantity = 1, product = product });
            }

            SaveCartSession(cart);
            return RedirectToAction(nameof(Cart));
        }

        [Route("/removecart/{productid:int}", Name = "removecart")]
        public IActionResult RemoveCart([FromRoute] int productid)
        {
            var cart = GetCartItems();
            var cartitem = cart.Find(p => p.product.Id == productid);
            if (cartitem != null)
            {
                cart.Remove(cartitem);
            }

            SaveCartSession(cart);
            return RedirectToAction(nameof(Cart));
        }

        [Route("/updatecart", Name = "updatecart")]
        [HttpPost]
        public IActionResult UpdateCart([FromForm] int productid, [FromForm] int quantity)
        {
            var cart = GetCartItems();
            var cartitem = cart.Find(p => p.product.Id == productid);
            if (cartitem != null)
            {
                cartitem.quantity = quantity;
            }
            SaveCartSession(cart);
            return Ok();
        }

        [Route("/cart", Name = "cart")]
        public IActionResult Cart()
        {
            return View(GetCartItems());
        }

        //[Route("/checkout")]
        //public IActionResult CheckOut()
        //{
        //    return View();
        //}

        public const string CARTKEY = "cart";

        List<CartItem> GetCartItems()
        {
            var session = HttpContext.Session;
            string jsoncart = session.GetString(CARTKEY);
            if (jsoncart != null)
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(jsoncart);
            }
            return new List<CartItem>();
        }

        void ClearCart()
        {
            var session = HttpContext.Session;
            session.Remove(CARTKEY);
        }

        void SaveCartSession(List<CartItem> ls)
        {
            var session = HttpContext.Session;
            string jsoncart = JsonConvert.SerializeObject(ls);
            session.SetString(CARTKEY, jsoncart);
        }

        public double TotalNumber()
        {
            List<CartItem> lCart = HttpContext.Session as List<CartItem>;
            if (lCart == null)
            {
                return 0;
            }
            return lCart.Sum(c => c.quantity);
        }

        //public ActionResult SubmitOrder()
        //{
        //    var cart = GetCartItems();
        //    if (cart == null)
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }
        //    Order order = new Order();
        //    order.OrderDate = DateTime.Now;
        //    order.OrderStatus = "processing";

        //    List<CartItem> ls = GetCartItems();
        //    foreach(var item in ls) 
        //    {
        //        OrderDetail orderDetail = new OrderDetail();
        //        orderDetail.OrderId = order.Id;
        //        orderDetail.ProductId = item.product.Id;
        //        orderDetail.Description= item.product.Description;

        //    }
        //    return RedirectToAction("cart");
        //}
        [HttpGet]
        [Route("Checkout", Name = "Checkout")]
        public ActionResult Checkout()
        {
            return View(GetCartItems());
        }


        [HttpPost]
        [Route("Checkout", Name = "Checkout")]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string Name, string Address, string PhoneNumber, string Comment)
        {
            var cartItems = GetCartItems();
            var orderDetails = cartItems.Select(cartItem => new OrderDetail
            {
                ProductId = cartItem.product.Id,
                Quantity = cartItem.quantity,
                Total = (Int32.Parse(cartItem.product.Price) * cartItem.quantity).ToString()
            }).ToList();
            var total = orderDetails.Sum(orderDetail => Int32.Parse(orderDetail.Total));
            var order = new Order()
            {
                OrderDate = DateTime.Now,
                OrderStatus = "Pending",
                Total = total,
                Fullname = Name,
                Address = Address,
                Telephone = PhoneNumber,
                Comment = Comment,
                OrderDetails = orderDetails
            };
            //_productService.Orders.Add(order);
            //dbContext.SaveChanges();
            _context.Orders.Add(order);
            _context.SaveChanges();

            return RedirectToAction("Order_Submitted", "Order");
        }

        public ActionResult Order_Submitted()
        {
            return View();
        }
    }
}