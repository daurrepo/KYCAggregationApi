namespace Carnegie.KycAggregationApi.Infrastructure.ApiModels;

/// <summary>
/// Model for contact details returned from external API
/// </summary>
/// <param name="Addresses"></param>
/// <param name="Emails"></param>
/// <param name="PhoneNumbers"></param>
internal record ContactDetailsApiModel(
    IReadOnlyList<AddressInfoApiModel> Addresses,
    IReadOnlyList<EmailItemApiModel> Emails,
    IReadOnlyList<PhoneItemApiModel> PhoneNumbers);

internal record AddressInfoApiModel(string Street, string City, string PostalCode, string Country);
internal record EmailItemApiModel(bool Preferred, string EmailAddress);
internal record PhoneItemApiModel(bool Preferred, string Number);