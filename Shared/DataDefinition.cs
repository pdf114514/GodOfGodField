using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace GodOfGodField.Shared;

// Some of the data definition classes key `guardian` is ignored
public class DataDefinition {
    public JsonElement Json { get; init; }
    public string ImageName { get; init; }
    public string Category { get; init; }
    public string Name { get; init; }

    public DataDefinition(JsonElement json) {
        Json = json;
        ImageName = json.GetProperty("imageName").GetString()!;
        Category = json.GetProperty("category").GetString()!;
        Name = json.GetProperty("name").GetString()!;
    }

    public Stream GetImageStream() => Resources.GetResource($"images/items/{Category}/{ImageName}.png")!;

    public bool IsT<T>([NotNullWhen(true)] out T? data) where T : DataDefinition {
        if (this is T t) {
            data = t;
            return true;
        } else {
            data = null;
            return false;
        }
    }

    public bool IsArmor([NotNullWhen(true)] out ArmorDataDefinition? data) => IsT(out data);
    public bool IsDevil([NotNullWhen(true)] out DevilDataDefitition? data) => IsT(out data);
    public bool IsGuardian([NotNullWhen(true)] out GuardianDataDefinition? data) => IsT(out data);
    public bool IsMiracle([NotNullWhen(true)] out MiracleDataDefinition? data) => IsT(out data);
    public bool IsPhenomena([NotNullWhen(true)] out PhenomenaDataDefinition? data) => IsT(out data);
    public bool IsSundry([NotNullWhen(true)] out SundryDataDefinition? data) => IsT(out data);
    public bool IsTrade([NotNullWhen(true)] out TradeDataDefinition? data) => IsT(out data);
    public bool IsWeapon([NotNullWhen(true)] out WeaponDataDefinition? data) => IsT(out data);

    public static DataDefinition Deserialize(JsonElement json) {
        var category = json.GetProperty("category").GetString();
        return category switch {
            "armor" => new ArmorDataDefinition(json),
            "devils" => new DevilDataDefitition(json),
            "guardians" => new GuardianDataDefinition(json),
            "miracles" => new MiracleDataDefinition(json),
            "phenomena" => new PhenomenaDataDefinition(json),
            "sundries" => new SundryDataDefinition(json),
            "trade" => new TradeDataDefinition(json),
            "weapons" => new WeaponDataDefinition(json),
            _ => throw new($"Unknown category: {category}"),
        };
    }
}

public class EAbility {
    public const string AbsorbHP = "absorbHP";
    public const string AddCurseOnDamage = "addCurseOnDamage";
    public const string AddItem = "addItem";
    public const string AtkBy2xMP = "atkBy2xMP";
    public const string AttackDyingly = "attackDyingly";
    public const string AttackEveryEnemy = "attackEveryEnemy";
    public const string AttackSomebody = "attackSomebody";
    public const string AttractDanger = "attractDanger";
    public const string BlockMiracle = "blockMiracle";
    public const string BlockWeapon = "blockWeapon";
    public const string BoostCP = "boostCP";
    public const string BoostHP = "boostHP";
    public const string BoostHPOrDealDamage = "boostHPOrDealDamage";
    public const string BoostMP = "boostMP";
    public const string BoostMPAndAddCurse = "boostMPAndAddCurse";
    public const string BoostSomething = "boostSomething";
    public const string BounceMiracle = "bounceMiracle";
    public const string BounceWeapon = "bounceWeapon";
    public const string CallPhenomenon = "callPhenomenon";
    public const string CollectCPOfEverybody = "collectCPOfEverybody";
    public const string ConfuseEverybody = "confuseEverybody";
    public const string Counter2xAtk = "counter2xAtk";
    public const string CounterAtk = "counterAtk";
    public const string CounterBoost2xMP = "counterBoost2xMP";
    public const string CounterCurse = "counterCurse";
    public const string CounterTakeCP = "counterTakeCP";
    public const string CutCost = "cutCost"; // Ex. If you use this weapon with a miracle, the cost of the miracle will be reduced (zero)
    public const string Danger = "danger";
    public const string DealDamage = "dealDamage";
    public const string DealSameDamage = "dealSameDamage";
    public const string DoubleAtk = "doubleAtk";
    public const string FilterAtkElement = "filterAtkElement";
    public const string ReflectAnything = "reflectAnything";
    public const string ReflectMiracle = "reflectMiracle";
    public const string ReflectWeapon = "reflectWeapon";
    public const string RemoveAllCurses = "removeAllCurses";
    public const string RemoveItems = "removeItems";
    public const string RemoveMildCurses = "removeMildCurses";
    public const string RemoveSomething = "removeSomething";
    public const string RemoveUsedMiracles = "removeUsedMiracles";
    public const string Revive = "revive";
    public const string SelfCurse = "selfCurse";
    public const string SelfCurseAndRedraw = "selfCurseAndRedraw";
    public const string SetCurseOfEverybody = "setCurseOfEverybody";
    public const string SetElement = "setElement";
    public const string SetGuardian = "setGuardian";
    public const string SetGuardianOfEverybody = "setGuardianOfEverybody";
    public const string SetHPOfEverybody = "setHPOfEverybody";
    public const string ShuffleItemsOfEverybody = "shuffleItemsOfEverybody";
}

public class ECurse {
    public const string Fog = "fog";
}

public class EElement {
    public const string Fire = "fire";
    public const string Water = "water";
    public const string Wood = "wood";
    public const string Stone = "stone";
    public const string Light = "light";
    public const string Darkness = "darkness";
}

public class EGuardian {
    public const string Mars = "mars";
    public const string Mercury = "mercury";
    public const string Jupiter = "jupiter";
    public const string Saturn = "saturn";
    public const string Uranus = "uranus";
    public const string Pluto = "pluto";
    public const string Neptune = "neptune";
    public const string Venus = "venus";
    public const string Earth = "earth";
    public const string Moon = "moon";
}

public class TradeDataDefinition : DataDefinition {
    public int Price { get; init; } = 0;
    public int GiftRate { get; init; } = 0;

    public TradeDataDefinition(JsonElement json) : base(json) {
        Price = json.TryGetProperty("price", out var price) ? price.GetInt32() : 0;
        GiftRate = json.TryGetProperty("giftRate", out var giftRate) ? giftRate.GetInt32() : 0;
    }
}

public class WeaponDataDefinition : DataDefinition {
    public int Price { get; init; }
    public int GiftRate { get; init; }
    public int Atk { get; init; } = 0; // Magical stick does not have Atk
    public int Def { get; init; } = 0;
    public int HitRate { get; init; } = 0;
    public bool IsPlusAtk { get; init; } = false;
    public string? Element { get; init; } = null;
    public string? Ability { get; init; } = null;
    public int AbilityValue { get; init; } = 0; // Ex. When Ability is AttackDyingly, AbilityValue will be 30
    public string? Curse { get; init; } = null; // Not null when Ability is EAbility.AddCurseOnDamage

    public WeaponDataDefinition(JsonElement json) : base(json) {
        Price = json.TryGetProperty("price", out var price) ? price.GetInt32() : 0;
        GiftRate = json.TryGetProperty("giftRate", out var giftRate) ? giftRate.GetInt32() : 0;
        Atk = json.TryGetProperty("atk", out var atk) ? atk.GetInt32() : 0;
        Def = json.TryGetProperty("def", out var def) ? def.GetInt32() : 0;
        HitRate = json.TryGetProperty("hitRate", out var hitRate) ? hitRate.GetInt32() : 0;
        IsPlusAtk = json.TryGetProperty("isPlusAtk", out var isPlusAtk) ? isPlusAtk.GetBoolean() : false;
        Element = json.TryGetProperty("element", out var element) ? element.GetString() : null;
        Ability = json.TryGetProperty("ability", out var ability) ? ability.GetString() : null;
        AbilityValue = json.TryGetProperty("abilityValue", out var abilityValue) ? abilityValue.GetInt32() : 0;
        Curse = json.TryGetProperty("curse", out var curse) ? curse.GetString() : null;
    }
}

public class ArmorDataDefinition : DataDefinition {
    public int Price { get; init; }
    public int GiftRate { get; init; }
    public int Def { get; init; } = 0; // Armor which has EAbility.CounterAtk does not have Def
    public int Atk { get; init; } = 0;
    public int HitRate { get; init; } = 0;
    public bool IsPlusAtk { get; init; } = false;
    public string? Element { get; init; } = null;
    public string? Ability { get; init; } = null;
    public int AbilityValue { get; init; } = 0; // This will not be used
    public string? Curse { get; init; } = null;

    public ArmorDataDefinition(JsonElement json) : base(json) {
        Price = json.TryGetProperty("price", out var price) ? price.GetInt32() : 0;
        GiftRate = json.TryGetProperty("giftRate", out var giftRate) ? giftRate.GetInt32() : 0;
        Def = json.TryGetProperty("def", out var def) ? def.GetInt32() : 0;
        Atk = json.TryGetProperty("atk", out var atk) ? atk.GetInt32() : 0;
        HitRate = json.TryGetProperty("hitRate", out var hitRate) ? hitRate.GetInt32() : 0;
        IsPlusAtk = json.TryGetProperty("isPlusAtk", out var isPlusAtk) ? isPlusAtk.GetBoolean() : false;
        Element = json.TryGetProperty("element", out var element) ? element.GetString() : null;
        Ability = json.TryGetProperty("ability", out var ability) ? ability.GetString() : null;
        AbilityValue = json.TryGetProperty("abilityValue", out var abilityValue) ? abilityValue.GetInt32() : 0;
        Curse = json.TryGetProperty("curse", out var curse) ? curse.GetString() : null;
    }
}

public class SundryDataDefinition : DataDefinition {
    public int Price { get; init; }
    public int GiftRate { get; init; }
    public int Atk { get; init; } = 0;
    public bool IsPlusAtk { get; init; } = false;
    public string? Ability { get; init; } = null;
    public int AbilityValue { get; init; } = 0;
    public string? Curse { get; init; } = null;

    public SundryDataDefinition(JsonElement json) : base(json) {
        Price = json.TryGetProperty("price", out var price) ? price.GetInt32() : 0;
        GiftRate = json.TryGetProperty("giftRate", out var giftRate) ? giftRate.GetInt32() : 0;
        Atk = json.TryGetProperty("atk", out var atk) ? atk.GetInt32() : 0;
        IsPlusAtk = json.TryGetProperty("isPlusAtk", out var isPlusAtk) ? isPlusAtk.GetBoolean() : false;
        Ability = json.TryGetProperty("ability", out var ability) ? ability.GetString() : null;
        AbilityValue = json.TryGetProperty("abilityValue", out var abilityValue) ? abilityValue.GetInt32() : 0;
        Curse = json.TryGetProperty("curse", out var curse) ? curse.GetString() : null;
    }
}

public class MiracleDataDefinition : DataDefinition {
    public int Price { get; init; }
    public int GiftRate { get; init; }
    public int Atk { get; init; } = 0;
    public int HitRate { get; init; } = 0;
    public bool IsPlusAtk { get; init; } = false;
    public string? Element { get; init; } = null;
    public string? Ability { get; init; } = null;
    public int AbilityValue { get; init; } = 0;
    public string? Curse { get; init; } = null;
    public int Cost { get; init; }

    public MiracleDataDefinition(JsonElement json) : base(json) {
        Price = json.TryGetProperty("price", out var price) ? price.GetInt32() : 0;
        GiftRate = json.TryGetProperty("giftRate", out var giftRate) ? giftRate.GetInt32() : 0;
        Atk = json.TryGetProperty("atk", out var atk) ? atk.GetInt32() : 0;
        IsPlusAtk = json.TryGetProperty("isPlusAtk", out var isPlusAtk) ? isPlusAtk.GetBoolean() : false;
        Element = json.TryGetProperty("element", out var element) ? element.GetString() : null;
        Ability = json.TryGetProperty("ability", out var ability) ? ability.GetString() : null;
        AbilityValue = json.TryGetProperty("abilityValue", out var abilityValue) ? abilityValue.GetInt32() : 0;
        Curse = json.TryGetProperty("curse", out var curse) ? curse.GetString() : null;
        Cost = json.GetProperty("cost").GetInt32();
    }
}

public class DevilDataDefitition : DataDefinition {
    public string Ability { get; init; }
    public int AbilityValue { get; init; } = 0;
    public int AppearanceRate { get; init; }

    public DevilDataDefitition(JsonElement json) : base(json) {
        Ability = json.GetProperty("ability").GetString()!;
        AbilityValue = json.TryGetProperty("abilityValue", out var abilityValue) ? abilityValue.GetInt32() : 0;
        AppearanceRate = json.GetProperty("appearanceRate").GetInt32();
    }
}

public class GuardianDataDefinition : DataDefinition {
    public string Guardian { get; init; }
    public int GuardianAttackRate { get; init; } = 0; // EGuardian.Earth and EGuardian.Moon do not have GuardianAttackRate WHAT
    public int Atk { get; init; } = 0;
    public int HitRate { get; init; } = 0;
    public string? Element { get; init; } = null;
    public string? Ability { get; init; } = null;
    public int AbilityValue { get; init; } = 0;
    public string? Curse { get; init; } = null;

    public GuardianDataDefinition(JsonElement json) : base(json) {
        Guardian = json.GetProperty("guardian").GetString()!;
        GuardianAttackRate = json.TryGetProperty("guardianAttackRate", out var guardianAttackRate) ? guardianAttackRate.GetInt32() : 0;
        Atk = json.TryGetProperty("atk", out var atk) ? atk.GetInt32() : 0;
        HitRate = json.TryGetProperty("hitRate", out var hitRate) ? hitRate.GetInt32() : 0;
        Element = json.TryGetProperty("element", out var element) ? element.GetString() : null;
        Ability = json.TryGetProperty("ability", out var ability) ? ability.GetString() : null;
        AbilityValue = json.TryGetProperty("abilityValue", out var abilityValue) ? abilityValue.GetInt32() : 0;
        Curse = json.TryGetProperty("curse", out var curse) ? curse.GetString() : null;
    }
}

public class PhenomenaDataDefinition : DataDefinition {
    public int Atk { get; init; } = 0;
    public int HitRate { get; init; } = 0;
    public string? Element { get; init; } = null;
    public string? Ability { get; init; } = null;
    public int AbilityValue { get; init; } = 0;
    public string? Curse { get; init; } = null;

    public PhenomenaDataDefinition(JsonElement json) : base(json) {
        Atk = json.TryGetProperty("atk", out var atk) ? atk.GetInt32() : 0;
        HitRate = json.TryGetProperty("hitRate", out var hitRate) ? hitRate.GetInt32() : 0;
        Element = json.TryGetProperty("element", out var element) ? element.GetString() : null;
        Ability = json.TryGetProperty("ability", out var ability) ? ability.GetString() : null;
        AbilityValue = json.TryGetProperty("abilityValue", out var abilityValue) ? abilityValue.GetInt32() : 0;
        Curse = json.TryGetProperty("curse", out var curse) ? curse.GetString() : null;
    }
}