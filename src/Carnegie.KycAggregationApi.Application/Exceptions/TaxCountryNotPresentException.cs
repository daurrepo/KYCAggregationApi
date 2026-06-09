namespace Carnegie.KycAggregationApi.Application.Exceptions;

public class TaxCountryNotPresentException : Exception
{
    public TaxCountryNotPresentException() : base("Tax country information is not present for the provided customer") { }
}
