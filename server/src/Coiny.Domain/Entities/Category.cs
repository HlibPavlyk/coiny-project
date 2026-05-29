using Coiny.Domain.Enums;

namespace Coiny.Domain.Entities;

public class Category
{
    public int Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int? ParentId { get; set; }

    public byte Level { get; set; }

    public int DisplaySort { get; set; }

    /// <summary>Only leaf categories accept lots.</summary>
    public bool IsLeaf { get; set; }

    /// <summary>Drives the JSONB attribute schema on the create-lot form. Non-null only for leaf categories.</summary>
    public SubcategoryKind? SubcategoryKind { get; set; }

    /// <summary>Denormalized active lot count; refreshed on lot publish/end.</summary>
    public int LotCountActive { get; set; }

    public virtual Category? Parent { get; set; }

    public virtual ICollection<Category> Children { get; set; } = [];

    public virtual ICollection<Lot> Lots { get; set; } = [];
}
