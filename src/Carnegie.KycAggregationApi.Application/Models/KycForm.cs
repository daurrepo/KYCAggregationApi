namespace Carnegie.KycAggregationApi.Application.Models;

public record KycForm(IReadOnlyList<KycFormItem> Items);
public record KycFormItem(string Key, string Value);
