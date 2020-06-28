using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Threading.Tasks;

namespace HackAzure1.Sushi.Api
{
    public class PaymentFunctions
    {
        private readonly CustomerService _customerService;
        private readonly PaymentMethodService _paymentMethodService;
        private readonly InvoiceItemService _invoiceItemService;
        private readonly InvoiceService _invoiceService;

        public PaymentFunctions(CustomerService customerService,
                                PaymentMethodService paymentMethodService,
                                InvoiceItemService invoiceItemService,
                                InvoiceService invoiceService)
        {
            _customerService = customerService;
            _paymentMethodService = paymentMethodService;
            _invoiceItemService = invoiceItemService;
            _invoiceService = invoiceService;
        }

        [FunctionName("CreateInvoice")]
        public async Task<Invoice> CreateInvoice(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoice/create")]
            CreatePaymentRequest request,
            ILogger log)
        {
            // このサンプルでは input validation をしていません。


            // 既存の Customer に紐づけることもできますが、今回は毎回 Customer を作成します。
            var customerCreateOptions = new CustomerCreateOptions
            {
                Email = request.Email,
                PaymentMethod = request.PaymentMethodId,
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = request.PaymentMethodId
                }
            };

            var customer = await _customerService.CreateAsync(customerCreateOptions);


            // Attach payment method:
            // 新規作成した Customer に PaymentMethod を紐づけます。
            // paymentMethodMethod は既に別の customer に紐づけられている場合エラーになります。
            var options = new PaymentMethodAttachOptions
            {
                Customer = customer.Id
            };

            var paymentMethod = await _paymentMethodService.AttachAsync(request.PaymentMethodId, options);

            // Update customer's default payment method.
            var customerOptions = new CustomerUpdateOptions
            {
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = paymentMethod.Id,
                }
            };

            await _customerService.UpdateAsync(customer.Id, customerOptions);


            // Create InvoiceItem.
            var createOptions = new InvoiceItemCreateOptions
            {
                Customer = customer.Id,
                Price = request.PriceId
            };

            await _invoiceItemService.CreateAsync(createOptions);


            // Create Invoice
            var invoiceOptions = new InvoiceCreateOptions
            {
                Customer = customer.Id,
                AutoAdvance = true,
            };

            var invoiceResult = await _invoiceService.CreateAsync(invoiceOptions);
            //ここで、Stripe の Dashboard > Billing > インボイスにインボイスが追加されます。

            return invoiceResult;
        }
    }
}