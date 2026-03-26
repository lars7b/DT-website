namespace Backend.Repositories;

public sealed class PaymentRepository : RepositoryBase<Payment>
{
    public PaymentRepository(IDataContext dataContext)
        : base(dataContext) { }

    public IEnumerable<Payment> GetByOrderId(long orderId)
    {
        query = "";
        return;
    }

    public IEnumerable<Payment> GetById(long Id)
    {
        query = "SELECT * FROM payments WHERE Id = @Id";
        return;
    }

    //
    public IEnumerable<Payment> GetAll()
    {
        query = "SELECT * FROM payments";
        return;
    }

    public IEnumerable<Payment> GetByUser(long userId)
    {
        //  payment -> order -> customers
        query = "";
        return;
    }

    public bool Add(Payment payment)
    {
        query = "";
        return; //rows changed
    }
    public bool Update(Payment payment)
    {
        query = "";
        return; //rows changed
    }
    public bool Delete(Payment payment)
    {
        query = "";
        return; //rows changed
    }
}
