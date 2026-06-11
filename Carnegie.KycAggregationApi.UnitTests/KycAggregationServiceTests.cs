using Carnegie.KycAggregationApi.Application.Exceptions;
using Carnegie.KycAggregationApi.Application.Interfaces;
using Carnegie.KycAggregationApi.Application.Models;
using Carnegie.KycAggregationApi.Application.Services;
using Carnegie.KycAggregationApi.Domain;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Carnegie.KycAggregationApi.UnitTests;

public class KycAggregationServiceTests
{
    private readonly ICustomerDataClient _client = Substitute.For<ICustomerDataClient>();
    private readonly IKycRepository _repository = Substitute.For<IKycRepository>();
    private readonly ILogger<KycAggregationService> _logger = Substitute.For<ILogger<KycAggregationService>>();
    private readonly KycAggregationService _sut;

    private const string Ssn = "19900101-1234";

    public KycAggregationServiceTests()
    {
        _sut = new KycAggregationService(_client, _repository, _logger);
    }

    [Fact]
    public async Task GetAsync_CacheHit_ReturnsCachedData()
    {
        var cached = BuildCachedData(taxCountry: "SE");
        _repository.GetBySsnAsync(Ssn).Returns(cached);

        var result = await _sut.GetAsync(Ssn);

        Assert.NotNull(result);
        Assert.Equal(Ssn, result.Ssn);
        Assert.Equal("Anna", result.FirstName);
        Assert.Equal("SE", result.TaxCountry);
        await _client.DidNotReceive().GetPersonalDetailsAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAsync_CacheHitWithEmptyTaxCountry_ThrowsTaxCountryNotPresentException()
    {
        var cached = BuildCachedData(taxCountry: "");
        _repository.GetBySsnAsync(Ssn).Returns(cached);

        await Assert.ThrowsAsync<TaxCountryNotPresentException>(() => _sut.GetAsync(Ssn));
    }

    [Fact]
    public async Task GetAsync_CacheMissPersonalDetailsNotFound_ThrowsCustomerNotFoundException()
    {
        _repository.GetBySsnAsync(Ssn).Returns((AggregatedKycData?)null);
        _client.GetPersonalDetailsAsync(Ssn).Returns((PersonalDetails?)null);

        await Assert.ThrowsAsync<CustomerNotFoundException>(() => _sut.GetAsync(Ssn));
    }

    [Fact]
    public async Task GetAsync_CacheMissAllDataPresent_SavesAndReturnsResponse()
    {
        SetupCacheMiss();

        var result = await _sut.GetAsync(Ssn);

        Assert.NotNull(result);
        Assert.Equal(Ssn, result.Ssn);
        Assert.Equal("Anna", result.FirstName);
        Assert.Equal("Svensson", result.LastName);
        Assert.Equal("SE", result.TaxCountry);
        Assert.Equal(500000, result.Income);
        await _repository.Received(1).SaveAsync(Arg.Any<AggregatedKycData>());
    }

    [Fact]
    public async Task GetAsync_CacheMissMissingTaxCountry_ThrowsTaxCountryNotPresentException()
    {
        _repository.GetBySsnAsync(Ssn).Returns((AggregatedKycData?)null);
        _client.GetPersonalDetailsAsync(Ssn).Returns(new PersonalDetails("Anna", "Svensson"));
        _client.GetContactDetailsAsync(Ssn).Returns(BuildContactDetails());
        _client.GetKycFormAsync(Ssn, Arg.Any<DateOnly>()).Returns(new KycForm([]));

        await Assert.ThrowsAsync<TaxCountryNotPresentException>(() => _sut.GetAsync(Ssn));
    }

    [Fact]
    public async Task GetAsync_CacheMissNonNumericIncome_IncomeIsNull()
    {
        _repository.GetBySsnAsync(Ssn).Returns((AggregatedKycData?)null);
        _client.GetPersonalDetailsAsync(Ssn).Returns(new PersonalDetails("Anna", "Svensson"));
        _client.GetContactDetailsAsync(Ssn).Returns(BuildContactDetails());
        _client.GetKycFormAsync(Ssn, Arg.Any<DateOnly>()).Returns(new KycForm([
            new KycFormItem("tax_country", "SE"),
            new KycFormItem("annual_income", "not-a-number")
        ]));

        var result = await _sut.GetAsync(Ssn);

        Assert.Null(result!.Income);
    }

    [Fact]
    public async Task GetAsync_CacheMissNullContactDetails_ReturnsEmptyAddressEmailPhone()
    {
        _repository.GetBySsnAsync(Ssn).Returns((AggregatedKycData?)null);
        _client.GetPersonalDetailsAsync(Ssn).Returns(new PersonalDetails("Anna", "Svensson"));
        _client.GetContactDetailsAsync(Ssn).Returns((ContactDetails?)null);
        _client.GetKycFormAsync(Ssn, Arg.Any<DateOnly>()).Returns(new KycForm([
            new KycFormItem("tax_country", "SE")
        ]));

        var result = await _sut.GetAsync(Ssn);

        Assert.NotNull(result);
        Assert.Equal("", result.Address);
        Assert.Equal("", result.Email);
        Assert.Equal("", result.PhoneNumber);
    }

    [Fact]
    public async Task GetAsync_CacheMissContactDetails_FormatsAddressCorrectly()
    {
        SetupCacheMiss();

        var result = await _sut.GetAsync(Ssn);

        Assert.Equal("Kungsgatan 1, 111 22 Stockholm", result!.Address);
    }

    [Fact]
    public async Task GetAsync_CacheMissContactDetails_ReturnsPreferredEmail()
    {
        SetupCacheMiss();

        var result = await _sut.GetAsync(Ssn);

        Assert.Equal("anna@example.com", result!.Email);
    }

    [Fact]
    public async Task GetAsync_CacheMissContactDetails_ReturnsPreferredPhone()
    {
        SetupCacheMiss();

        var result = await _sut.GetAsync(Ssn);

        Assert.Equal("0701234567", result!.PhoneNumber);
    }

    private void SetupCacheMiss()
    {
        _repository.GetBySsnAsync(Ssn).Returns((AggregatedKycData?)null);
        _client.GetPersonalDetailsAsync(Ssn).Returns(new PersonalDetails("Anna", "Svensson"));
        _client.GetContactDetailsAsync(Ssn).Returns(BuildContactDetails());
        _client.GetKycFormAsync(Ssn, Arg.Any<DateOnly>()).Returns(new KycForm([
            new KycFormItem("tax_country", "SE"),
            new KycFormItem("annual_income", "500000")
        ]));
    }

    private static AggregatedKycData BuildCachedData(string taxCountry) =>
        new()
        {
            Ssn = Ssn,
            FirstName = "Anna",
            LastName = "Svensson",
            Address = "Kungsgatan 1, 111 22 Stockholm",
            Email = "anna@example.com",
            PhoneNumber = "0701234567",
            TaxCountry = taxCountry,
            Income = 500000
        };

    private static ContactDetails BuildContactDetails() =>
        new(
            Addresses: [new AddressInfo("Kungsgatan 1", "Stockholm", "111 22", "SE")],
            Emails: [new EmailItem(Preferred: false, "other@example.com"), new EmailItem(Preferred: true, "anna@example.com")],
            PhoneNumbers: [new PhoneItem(Preferred: false, "0709999999"), new PhoneItem(Preferred: true, "0701234567")]
        );
}
