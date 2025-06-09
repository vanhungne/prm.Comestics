using Microsoft.Extensions.DependencyInjection;
using Service.Interface;
using Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection service)
        {
            service.AddScoped<IAuthenService, AuthenService>(); 
            service.AddScoped<IProductService, ProductService>();
            service.AddScoped<ICloundinaryService, CloundinaryService>();
           service.AddScoped<ICheckoutService, CheckoutService>();
            service.AddScoped<ICartService,CartService>();
            service.AddScoped<IPaypalService, PayPalService>();
            service.AddScoped<IOrderService, OrderService>();
            return service;
        }
        }
}
