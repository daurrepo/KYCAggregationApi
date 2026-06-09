namespace Carnegie.KycAggregationApi.Application.Exceptions;

public class CustomerNotFoundException : Exception
{
    public CustomerNotFoundException()
        : base("Customer data not found for the provided SSN") { }

    public CustomerNotFoundException(string ssn)
        : base($"Customer data not found for the provided SSN: {ssn}") { }


}
