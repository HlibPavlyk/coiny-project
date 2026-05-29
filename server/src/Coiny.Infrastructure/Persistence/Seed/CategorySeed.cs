using Coiny.Domain.Entities;
using Coiny.Domain.Enums;

namespace Coiny.Infrastructure.Persistence.Seed;

/// <summary>Hardcoded 3-level category tree seeded via HasData. IDs are stable integers.</summary>
public static class CategorySeed
{
    // Root categories (Level 0)
    private static readonly Category Coins = new()
    {
        Id = 1, Slug = "coins", Name = "Coins",
        ParentId = null, Level = 0, DisplaySort = 1, IsLeaf = false, SubcategoryKind = null
    };

    private static readonly Category Banknotes = new()
    {
        Id = 2, Slug = "banknotes", Name = "Banknotes",
        ParentId = null, Level = 0, DisplaySort = 2, IsLeaf = false, SubcategoryKind = null
    };

    private static readonly Category MedalsOrders = new()
    {
        Id = 3, Slug = "medals-orders", Name = "Medals & Orders",
        ParentId = null, Level = 0, DisplaySort = 3, IsLeaf = false, SubcategoryKind = null
    };

    // Mid-level categories (Level 1) — Coins
    private static readonly Category CoinsWorld = new()
    {
        Id = 10, Slug = "coins-world", Name = "World Coins",
        ParentId = 1, Level = 1, DisplaySort = 1, IsLeaf = false, SubcategoryKind = null
    };

    private static readonly Category CoinsUkraine = new()
    {
        Id = 11, Slug = "coins-ukraine", Name = "Ukrainian Coins",
        ParentId = 1, Level = 1, DisplaySort = 2, IsLeaf = false, SubcategoryKind = null
    };

    // Mid-level categories (Level 1) — Banknotes
    private static readonly Category BanknotesWorld = new()
    {
        Id = 20, Slug = "banknotes-world", Name = "World Banknotes",
        ParentId = 2, Level = 1, DisplaySort = 1, IsLeaf = false, SubcategoryKind = null
    };

    private static readonly Category BanknotesUkraine = new()
    {
        Id = 21, Slug = "banknotes-ukraine", Name = "Ukrainian Banknotes",
        ParentId = 2, Level = 1, DisplaySort = 2, IsLeaf = false, SubcategoryKind = null
    };

    // Mid-level categories (Level 1) — Medals & Orders
    private static readonly Category MedalsMilitary = new()
    {
        Id = 30, Slug = "medals-military", Name = "Military Medals",
        ParentId = 3, Level = 1, DisplaySort = 1, IsLeaf = false, SubcategoryKind = null
    };

    private static readonly Category MedalsCivilian = new()
    {
        Id = 31, Slug = "medals-civilian", Name = "Civilian Awards",
        ParentId = 3, Level = 1, DisplaySort = 2, IsLeaf = false, SubcategoryKind = null
    };

    // Leaf categories (Level 2) — SubcategoryKind assigned here
    private static readonly Category CoinsWorldLeaf = new()
    {
        Id = 100, Slug = "coins-world-general", Name = "General World Coins",
        ParentId = 10, Level = 2, DisplaySort = 1, IsLeaf = true, SubcategoryKind = SubcategoryKind.Coin
    };

    private static readonly Category CoinsUkraineLeaf = new()
    {
        Id = 101, Slug = "coins-ukraine-general", Name = "General Ukrainian Coins",
        ParentId = 11, Level = 2, DisplaySort = 1, IsLeaf = true, SubcategoryKind = SubcategoryKind.Coin
    };

    private static readonly Category BanknotesWorldLeaf = new()
    {
        Id = 110, Slug = "banknotes-world-general", Name = "General World Banknotes",
        ParentId = 20, Level = 2, DisplaySort = 1, IsLeaf = true, SubcategoryKind = SubcategoryKind.Banknote
    };

    private static readonly Category BanknotesUkraineLeaf = new()
    {
        Id = 111, Slug = "banknotes-ukraine-general", Name = "General Ukrainian Banknotes",
        ParentId = 21, Level = 2, DisplaySort = 1, IsLeaf = true, SubcategoryKind = SubcategoryKind.Banknote
    };

    private static readonly Category MedalsMilitaryLeaf = new()
    {
        Id = 120, Slug = "medals-military-general", Name = "General Military Medals",
        ParentId = 30, Level = 2, DisplaySort = 1, IsLeaf = true, SubcategoryKind = SubcategoryKind.Medal
    };

    private static readonly Category MedalsCivilianLeaf = new()
    {
        Id = 121, Slug = "medals-civilian-general", Name = "General Civilian Awards",
        ParentId = 31, Level = 2, DisplaySort = 1, IsLeaf = true, SubcategoryKind = SubcategoryKind.Medal
    };

    public static readonly Category[] All =
    [
        Coins, Banknotes, MedalsOrders,
        CoinsWorld, CoinsUkraine,
        BanknotesWorld, BanknotesUkraine,
        MedalsMilitary, MedalsCivilian,
        CoinsWorldLeaf, CoinsUkraineLeaf,
        BanknotesWorldLeaf, BanknotesUkraineLeaf,
        MedalsMilitaryLeaf, MedalsCivilianLeaf,
    ];
}
