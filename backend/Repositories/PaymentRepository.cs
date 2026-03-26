using Backend.Models;

namespace Backend.Repositories;

public sealed class PaymentRepository : RepositoryBase<Payment>
{
    private readonly string _table = "payments";

    public PaymentRepository(IConfiguration configuration)
        : base(configuration, "payments") { }

    public List<Payment> GetByOrderId(long orderId)
    {
        string query = "";
        return new List<Payment>();
    }

    public List<Payment> GetByUser(long userId)
    {
        //  payment -> order -> customers
        string query = "";
        return new List<Payment>();
    }
}
