using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class PayPalService : IPaypalService
    {
        private readonly PayPalHttpClient _payPalHttpClient;
        private readonly ILogger<PayPalService> _logger;
        private readonly string _baseUrl;
        private readonly string _returnUrl;
        private readonly string _cancelUrl;
        private readonly IConfiguration _IConfiguration;
        public PayPalService(IConfiguration configuration, ILogger<PayPalService> logger)
        {
            _logger = logger;
            var clientId = configuration["PayPal:ClientId"];
            var clientSecret = configuration["PayPal:ClientSecret"];
            var isSandbox = configuration["Paypal:Mode"].Equals("Sandbox", StringComparison.OrdinalIgnoreCase);
            _IConfiguration = configuration;
            _baseUrl = _IConfiguration["PayPal:BaseUrl"];
            _returnUrl = _IConfiguration["PayPal:ReturnUrl"];
            _cancelUrl = _IConfiguration["PayPal:CancelUrl"];

            PayPalEnvironment environment = isSandbox
                ? new SandboxEnvironment(clientId, clientSecret)
                : new LiveEnvironment(clientId, clientSecret);

            _payPalHttpClient = new PayPalHttpClient(environment);
        }
        public async Task<(string Status, string OrderId)> CapturePaymentAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token is required", nameof(token));
            }

            try
            {
                // Kiểm tra trạng thái của đơn hàng
                var orderStatus = await CheckOrderStatusAsync(token);
                if (orderStatus == "COMPLETED")
                {
                    _logger.LogInformation("Order already captured for token: {token}", token);
                    return ("COMPLETED", token);
                }

                // Thực hiện capture nếu đơn hàng chưa hoàn tất
                var request = new OrdersCaptureRequest(token);
                request.RequestBody(new OrderActionRequest());
                var response = await _payPalHttpClient.Execute(request);
                var result = response.Result<Order>();

                return (result.Status, result.Id);
            }
            catch (PayPalHttp.HttpException ex)
            {
                _logger.LogError(ex, "Error capturing PayPal payment for token: {token}", token);
                throw;
            }
        }

        private async Task<string> CheckOrderStatusAsync(string token)
        {
            var request = new OrdersGetRequest(token);
            var response = await _payPalHttpClient.Execute(request);
            var result = response.Result<Order>();
            return result.Status;
        }

        public async Task<string> CreatePaymentAsync(decimal amount, int orderNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_returnUrl) || string.IsNullOrEmpty(_cancelUrl))
                {
                    throw new InvalidOperationException("Base URL or return/cancel URL configuration is missing.");
                }

                var fullReturnUrl = $"{_baseUrl.TrimEnd('/')}{_returnUrl}?orderNumber={orderNumber}";
                var fullCancelUrl = $"{_baseUrl.TrimEnd('/')}{_cancelUrl}?orderNumber={orderNumber}";

                var order = new OrderRequest
                {
                    CheckoutPaymentIntent = "CAPTURE",
                    PurchaseUnits = new List<PurchaseUnitRequest>
            {
                new PurchaseUnitRequest
                {
                    AmountWithBreakdown = new AmountWithBreakdown
                    {
                        CurrencyCode = "USD",
                        Value = amount.ToString("0") // Đảm bảo định dạng chính xác cho số tiền
                    },
                    ReferenceId = orderNumber.ToString(),
                }
            },
                    ApplicationContext = new ApplicationContext
                    {
                        ReturnUrl = fullReturnUrl,
                        CancelUrl = fullCancelUrl,
                        UserAction = "PAY_NOW",
                        ShippingPreference = "NO_SHIPPING"
                    }
                };

                var request = new OrdersCreateRequest();
                request.Prefer("return=representation");
                request.RequestBody(order);

                var response = await _payPalHttpClient.Execute(request);
                var result = response.Result<Order>();

                var approveUrl = result.Links?.FirstOrDefault(x => x.Rel.Equals("approve", StringComparison.OrdinalIgnoreCase))?.Href;

                if (string.IsNullOrEmpty(approveUrl))
                {
                    throw new System.Exception("PayPal approve URL not found in response.");
                }

                return approveUrl;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal payment");
                throw;
            }
        }
    }
}
