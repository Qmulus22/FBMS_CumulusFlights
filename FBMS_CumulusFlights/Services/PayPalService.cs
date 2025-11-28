using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalCheckoutSdk.Payments;
using System.Threading.Tasks;

public class PayPalService
{
    private readonly PayPalHttpClient _client;

    public PayPalService(PayPalHttpClient client)
    {
        _client = client;
    }

    public async Task<string> CreateOrder(decimal amount)
    {
        var request = new OrdersCreateRequest();
        request.Prefer("return=representation");
        request.RequestBody(new OrderRequest()
        {
            CheckoutPaymentIntent = "CAPTURE",
            PurchaseUnits = new List<PurchaseUnitRequest>()
            {
                new PurchaseUnitRequest()
                {
                    AmountWithBreakdown = new AmountWithBreakdown()
                    {
                        CurrencyCode = "PHP", // Change to your currency
                        Value = amount.ToString("F2")
                    }
                }
            }
        });

        var response = await _client.Execute(request);
        var statusCode = response.StatusCode;

        if (statusCode == System.Net.HttpStatusCode.Created)
        {
            var order = response.Result<Order>();
            return order.Id; // Return the order ID
        }
        else
        {
            throw new Exception("Error creating PayPal order: " + response.StatusCode);
        }
    }

    //public async Task<Order> CaptureOrder(string orderId)
    //{
    //    var request = new OrdersCaptureRequest(orderId);
    //    request.RequestBody(new OrderActionRequest());

    //    var response = await _client.Execute(request);
    //    var statusCode = response.StatusCode;

    //    // Log the response for debugging
    //    Console.WriteLine($"Capture Order Response Status: {statusCode}");
    //    Console.WriteLine($"Response Body: {response}");

    //    if (statusCode == System.Net.HttpStatusCode.OK || statusCode == System.Net.HttpStatusCode.Ambiguous)
    //    {
    //        return response.Result<Order>();
    //    }
    //    else
    //    {
    //        throw new Exception("Error capturing PayPal order: " + response.StatusCode);
    //    }
    //}
}