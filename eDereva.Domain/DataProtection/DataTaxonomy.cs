using Microsoft.Extensions.Compliance.Classification;

namespace eDereva.Domain.DataProtection;

public static class DataTaxonomy
{
    public static string TaxonomyName { get; } = typeof(DataTaxonomy).FullName!;

    public static DataClassification SensitiveData { get; } = new(TaxonomyName, nameof(SensitiveData));

    public static DataClassification PiiData { get; } = new(TaxonomyName, nameof(PiiData));
}

public class SensitiveDataAttribute : DataClassificationAttribute
{
    protected internal SensitiveDataAttribute() : base(DataTaxonomy.SensitiveData)
    {
    }
}

public class PiiDataAttribute : DataClassificationAttribute
{
    protected internal PiiDataAttribute() : base(DataTaxonomy.PiiData)
    {
    }
}