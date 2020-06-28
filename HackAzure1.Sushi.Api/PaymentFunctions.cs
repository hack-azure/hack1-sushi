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
            // ���̃T���v���ł� input validation �����Ă��܂���B


            // ������ Customer �ɕR�Â��邱�Ƃ��ł��܂����A����͖��� Customer ���쐬���܂��B
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
            // �V�K�쐬���� Customer �� PaymentMethod ��R�Â��܂��B
            // paymentMethodMethod �͊��ɕʂ� customer �ɕR�Â����Ă���ꍇ�G���[�ɂȂ�܂��B
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
            //�����ŁAStripe �� Dashboard > Billing > �C���{�C�X�ɃC���{�C�X���ǉ�����܂��B

            return invoiceResult;
        }
    }
}