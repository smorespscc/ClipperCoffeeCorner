using System.Threading.Tasks;

namespace Services
{
    public interface ISquareCheckoutService
    {
        /// <summary>
        /// Creates a Square payment link and returns the hosted checkout URL.
        /// Amount is expressed in the smallest currency unit (cents for USD).
        /// </summary>
        Task<string> CreatePaymentLinkAsync(string itemName, long amountCents, string currency = "USD", string? redirectUrl = null);
    }
}