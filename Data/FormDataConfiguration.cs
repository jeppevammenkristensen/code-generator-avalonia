using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data;

public class FormDataConfiguration : IEntityTypeConfiguration<FormData>
{
    public void Configure(EntityTypeBuilder<FormData> builder)
    {
        builder.OwnsMany(x => x.Properties, x => x.ToJson());
    }
}