namespace Carnegie.KycAggregationApi.Infrastructure.ApiModels;

/// <summary>
/// Model for personal details returned from external API
/// </summary>
/// <param name="FirstName"></param>
/// <param name="SurName"></param>
/// <param name="Address"></param>
internal record PersonalDetailsApiModel(string FirstName, string SurName, string Address);