# Tools used
- Visual Studio Community
- VS Code
- Insomnia
- Git and GitHub
- Claude Code: used as a teammate for scaffolding and boilerplate, discussing architecture, technical choices and creating tests


# Customer Data API
- According to specification all endpoints should be under /api/ but they are in root insead
- Open API Specification does not reflect actual API, see details below. Maybe someone manually edited this specification?
- Example data for Customer Data API does not reflect actual implementation
- Maybe I have missed some more things but there are some issues regarding specifcation vs implementation detailed below

## Three endpoints to get data
Customer Data API have three endpoints to get data, personal-details, contact-details, kyc-form. How to handle if one or two return 404? Maybe customer does not exist or only kyc-form does not exist. I see personal-details as preference as "master". If 404 from personal-details, then do not call contact-details or kyc-form.

## No validation of paths
Path traversal is valid, wonder why, should not be like that:
`https://customerdataapi-codetest-bughceejfwbwbkb9.swedencentral-01.azurewebsites.net/personal-details/../kyc-form/19800115-1234/2027-02-28`

## personal-details
Spec differ from actual implementation

### Specification
```json
{
	"first_name":"",
	"sur_name":"Johansson"
}
```

### Actual API differ from included API-specification
Property names not matching, plus one extra property compared to specification
```json
{
	"firstName": "Erik",
	"surName": "Johansson",
	"address": "Storgatan 1, 111 22 Stockholm"
}
```
Property `address` seems to be some kind of concatenated address, skipping this since there exists addresses in `/contact-details`

## contact-details
Spec for  Customer Data API differ from implementation in object names and property names
- Object name `address` => `addresses` 
- Object name `phone_numbers` => `phoneNumbers` 
- property `postal_code` => `postalCode`
- property `email_Address` => `emailAddress`
- What if there exists no preferred `emailAdress` or `number`, consider how to handle this.
- `addresses` is an array, consider including `preferred` for each address-item
  
## KYCForm
- Specification shows `kyc_registration_date`, this does not exist in Customer Data API implementation
- Specification shows `tax_country`, this does not exist in Customer Data API implementation
- Example has mixed casing for key-value-pairs, also includes two properties not included in actual implementation
- example data for `politically_exposed_person` is `No`, should be boolean instead

### endpoint "/kyc-form/{ssn}/{asOfDate}"
Includes a date parameter, calling this with date of today as default. Date parameter is not included in spec for "KYC Aggregation API Service". 
- Note 1: dateformat acceptable is `2026-05-29` and `2026-may-29`
- Note 2: future dates are acceptable and return data for example `/kyc-form/19800115-1234/2027-02-28`. If this is not handled in Customer Data API and not checked in KYCAggregated Data API then data from KYCAggregated Data API is not correct ie a customer may be seen as known customer for KYCAggregateAPI consumers but it is actually not.

# KYC Aggregation API Service

## Architecture and stack
- Clean Architecture, chosen because the separation is valid even for a simple API like this - changing persistence or consuming a changed API, another API or more APIs does not change application logic
- C#, .Net 10, EF with SQLite, xUnit, controllers (no minimal API), middleware for exception handling, typed HttpClient
- Had some thoughts about skipping Domain.Model since this is a code-assignment but in real world scenario there would be some business rules (check date for valid KYC-form, expiry depending on when latest saved and so on) so keeping the Domain layer is a good idea that does not force a structural rewrite in future.

## Validation of data
- Should implement business rule to invalidate cached data
- No future dates should be allowed
- Should check SSN for valid Luhn-format
- Should check for valid dates in birthdate
- If KYC-form is older than N-days (business rule) then GET new data from Customer Data API and update the existing record in KYC Aggregated Data database (and create historical record for audit and data-analysis)

## Endpoint "/kyc-data/{ssn}"
- Should add validation of SSNs
- API only returns latest data (today). Consider if should include a parameter to get historical records, all or for a specific date
- Consider if KYC Aggregation API should keep historical records to enable data-analysis
- Data returned (example-data) is missing data for property/key-value-pair `tax_country` since it does not exist in actual implementation of Customer Data API. It is included in specifikation but actual API does not return `tax_country`. Can not use property `nationality` since it may differ from `tax_country`. Maybe key-value-pair/property `tax_country` exists for another persons. Since property `tax_country` also is required the API will not be functional as intended regarding available test-data. 
  - Considering this I will 
  1. check if `tax_country` is available from `/kyc-form` 
  2. save data in KYC Aggregation API database
  3. log this customer with missing `tax_country`
  4. return 404 for customers with empty model `TaxCountry` and also include a error message like "Field tax_country is required and could not be found for this customer" (`TaxCountryNotPresentException`)

## Missing data
Specifikation for KYC Aggregation does not include returning KYC-data that is available from Customer Data API like risk_profile, politically_exposed_person and more that may seem to be important for a KYC-api.

## Possible data structure mismatch (Address)
KYCAggregation Data API returns a string with address, Customer Data API returns each address property in separate properties. 

## Customer SSN changes
Should include a endpoint for getting historical SSNs since a person may change SSN and name through Skatteverket

## Error logging
Now I use built-in logging, future development should implement a logger platform like Splunk, Application Insights/Azure Monitor or other logging provider for structured and centralized logging

## Caching
A persistent caching provider should be used to lighten the load (reads) of database since every request is first selected from  database and then if not exist calling the external API. Caching mechanism should as well include a cache invalidation mechanism if new data for a customer is retrieved. Also some business rules to determine when to check for new data if customer-data already exists in KYC-database.

## GDPR
Depending on logging used I would analyze PII and what is allowed to log and not because of sensitive PII data belonging to customer.
SSNs included in url for GET is in risk of being logged in serverlogs, browser history and so on. Consider using POST instead of GET.

## Security, authentication and authorization
Next iteration in this assignement I would focus on security like
- authentication and authorization with MSAL and OAuth 2.0/OpenID Connect, If Entra ID is used use app registrations and  also check if bearer tokens are valid.
 
## Containerisation
Containerisation with Docker or Kubernetes is out of scope but that should be considered.

## Tests
A lot more tests should be implemented:
- More Unit tests
- Integration Tests
- Performance Tests
- Security Tests (OWASP)

### Arrange-Act-Assert
Test should be written that uses pattern Arrange-Act-Assert

# Possible requirements
- Analyze what kind of consumers that are within the bank and what kind of requirements they may request for accessing data, like batch requests, GET for parameters like country and so on

# Final note
I enjoyed this assignment! It looked quite simple at first but looking deeper into Customer Data API, comparing attached specifications and then debugging and testing Customer Data API and validating data, constraints and so on it was not that straight forward, just as it may be in the real world. Writing great parts of this REFLECTION.md before starting to coding was not something I anticipated when I looked into this assignment at first.
