namespace MyanvieBE.DTOs.Order
{
    public class CreateOrderResponseDto
    {
        public OrderDto Order { get; set; }
        public string? PaymentUrl { get; set; }
    }
}