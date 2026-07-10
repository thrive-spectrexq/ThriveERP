using ThriveERP.Domain.Common;

namespace ThriveERP.Domain.Entities;

/// <summary>
/// Represents a product category with optional hierarchical nesting.
/// </summary>
public class Category : BaseEntity
{
    /// <summary>Gets or sets the category name. Max 100 characters.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional parent category identifier for hierarchical nesting.</summary>
    public Guid? ParentCategoryId { get; set; }

    // Navigation properties

    /// <summary>Gets or sets the parent category (self-referencing).</summary>
    public Category? ParentCategory { get; set; }

    /// <summary>Gets the child sub-categories.</summary>
    public ICollection<Category> SubCategories { get; set; } = [];

    /// <summary>Gets the products in this category.</summary>
    public ICollection<Product> Products { get; set; } = [];
}
