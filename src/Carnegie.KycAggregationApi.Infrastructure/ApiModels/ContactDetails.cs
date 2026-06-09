namespace Carnegie.KycAggregationApi.Infrastructure.ApiModels;

internal record ContactDetailsApiModel(
    IReadOnlyList<AddressInfoApiModel> Addresses,
    IReadOnlyList<EmailItemApiModel> Emails,
    IReadOnlyList<PhoneItemApiModel> PhoneNumbers);

internal record AddressInfoApiModel(string Street, string City, string PostalCode, string Country);
internal record EmailItemApiModel(bool Preferred, string EmailAddress);
internal record PhoneItemApiModel(bool Preferred, string Number);