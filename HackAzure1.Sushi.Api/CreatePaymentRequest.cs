namespace HackAzure1.Sushi.Api
{
    public class CreatePaymentRequest
    {
        public string Email { get; set; }
        public string PaymentMethodId { get; set; }
        public string PriceId { get; set; }
    }
}