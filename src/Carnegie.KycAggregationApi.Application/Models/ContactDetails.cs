namespace Carnegie.KycAggregationApi.Application.Models;

public record ContactDetails(
    IReadOnlyList<AddressInfo> Addresses,
    IReadOnlyList<EmailItem> Emails,
    IReadOnlyList<PhoneItem> PhoneNumbers);

public record AddressInfo(string Street, string City, string PostalCode, string Country);
public record EmailItem(bool Preferred, string EmailAddress);
public record PhoneItem(bool Preferred, string Number);