namespace Payment.Application.ShoppingCarts;

public sealed record CartDto(
    List<CartItemDto> Items,
    decimal Total);
