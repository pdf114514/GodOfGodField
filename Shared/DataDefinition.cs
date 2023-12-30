using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace GodOfGodField.Shared;

// Some of the data definition classes key `guardian` is ignored
public class DataDefinition(JsonElement json) {
    public JsonElement Json { get; init; } = json;
    public string ImageName { get; init; } = json.GetProperty("imageName").GetString()!;
    public string Category { get; init; } = json.GetProperty("category").GetString()!;
    public string Name { get; init; } = json.GetProperty("name").GetString()!;

    public int? Price { get; init; } = json.TryGetProperty("price", out var price) ? price.GetInt32() : null;
    public int? GiftRate { get; init; } = json.TryGetProperty("giftRate", out var giftRate) ? giftRate.GetInt32() : null;
    public int? Atk { get; init; } = json.TryGetProperty("atk", out var atk) ? atk.GetInt32() : null;
    public int? Def { get; init; } = json.TryGetProperty("def", out var def) ? def.GetInt32() : null;
    public int? HitRate { get; init; } = json.TryGetProperty("hitRate", out var hitRate) ? hitRate.GetInt32() : null;
    public bool? IsPlusAtk { get; init; } = json.TryGetProperty("isPlusAtk", out var isPlusAtk) ? isPlusAtk.GetBoolean() : null;
    public string? Element { get; init; } = json.TryGetProperty("element", out var element) ? element.GetString() : null;
    public string? Ability { get; init; } = json.TryGetProperty("ability", out var ability) ? ability.GetString() : null;
    public int? AbilityValue { get; init; } = json.TryGetProperty("abilityValue", out var abilityValue) ? abilityValue.GetInt32() : null;
    public string? Curse { get; init; } = json.TryGetProperty("curse", out var curse) ? curse.GetString() : null;
    public int? Cost { get; init; } = json.TryGetProperty("cost", out var cost) ? cost.GetInt32() : null;
    public string? Guardian { get; init; } = json.TryGetProperty("guardian", out var guardian) ? guardian.GetString() : null;
    public int? GuardianAttackRate { get; init; } = json.TryGetProperty("guardianAttackRate", out var guardianAttackRate) ? guardianAttackRate.GetInt32() : null;
    public int? AppearanceRate { get; init; } = json.TryGetProperty("appearanceRate", out var appearanceRate) ? appearanceRate.GetInt32() : null;

    public Stream GetImageStream() => Resources.GetResource($"images/items/{Category}/{ImageName}.png")!;

    public bool IsT<T>([NotNullWhen(true)] out T? data) where T : DataDefinition => (data = this as T) != null;

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

public class StringEnum<T> where T : StringEnum<T> {
    private static ReadOnlyDictionary<string, string>? _Dict;
    public static ReadOnlyDictionary<string, string> ToDictionary() => _Dict ??= typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(x => x.IsLiteral && !x.IsInitOnly && x.FieldType == typeof(string)).ToDictionary(x => x.Name, x => (string)x.GetRawConstantValue()!).AsReadOnly();
}

public class EAbility : StringEnum<EAbility> {
    public const string AbsorbHP = "absorbHP";
    public const string AddCurse = "addCurse";
    public const string AddCurseOnDamage = "addCurseOnDamage";
    public const string AddItem = "addItem";
    public const string AtkBy2xMP = "atkBy2xMP";
    public const string AttackDyingly = "attackDyingly";
    public const string AttackEveryEnemy = "attackEveryEnemy";
    public const string AttackSomebody = "attackSomebody";
    public const string AttackTwice = "attackTwice";
    public const string AttractDanger = "attractDanger";
    public const string BlockMiracle = "blockMiracle";
    public const string BlockWeapon = "blockWeapon";
    public const string BoostCP = "boostCP";
    public const string BoostCPOfEverybody = "boostCPOfEverybody";
    public const string BoostCPToEnemy = "boostCPToEnemy";
    public const string BoostHP = "boostHP";
    public const string BoostHPOrDealDamage = "boostHPOrDealDamage";
    public const string BoostMP = "boostMP";
    public const string BoostMPAndAddCurse = "boostMPAndAddCurse";
    public const string BoostSomething = "boostSomething";
    public const string BounceMiracle = "bounceMiracle";
    public const string BounceWeapon = "bounceWeapon";
    public const string Buy = "buy";
    public const string CallPhenomenon = "callPhenomenon";
    public const string CategoryWeapons = "categoryWeapons";
    public const string CollectCPOfEverybody = "collectCPOfEverybody";
    public const string ConfuseEverybody = "confuseEverybody";
    public const string ConsumeAllMP = "consumeAllMP"; // Magical stick
    public const string Counter2xAtk = "counter2xAtk";
    public const string CounterAtk = "counterAtk";
    public const string CounterBoost2xMP = "counterBoost2xMP";
    public const string CounterCurse = "counterCurse";
    public const string CounterTakeCP = "counterTakeCP";
    public const string CutCost = "cutCost"; // Ex. If you use this weapon with a miracle, the cost of the miracle will be reduced (zero)
    public const string Danger = "danger";
    public const string DealDamage = "dealDamage";
    public const string DealSameDamage = "dealSameDamage";
    public const string Discard = "discard";
    public const string DoubleAtk = "doubleAtk";
    public const string Exchange = "exchange";
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
    public const string Sacrifice = "sacrifice";
    public const string SelfCurse = "selfCurse";
    public const string SelfCurseAndRedraw = "selfCurseAndRedraw";
    public const string Sell = "sell";
    public const string SetCurseOfEverybody = "setCurseOfEverybody";
    public const string SetElement = "setElement";
    public const string SetGuardian = "setGuardian";
    public const string SetGuardianOfEverybody = "setGuardianOfEverybody";
    public const string SetHPOfEverybody = "setHPOfEverybody";
    public const string ShuffleItemsOfEverybody = "shuffleItemsOfEverybody";
    public const string TakeCP = "takeCP";
}

public class ECurse : StringEnum<ECurse> {
    public const string Cold = "cold";
    public const string Fever = "fever";
    public const string Hell = "hell";
    public const string Heaven = "heaven";
    public const string Fog = "fog";
    public const string Flash = "flash";
    public const string Dream = "dream";
    public const string Darkcloud = "darkcloud";
}

public class EElement : StringEnum<EElement> {
    public const string Fire = "fire";
    public const string Water = "water";
    public const string Wood = "wood";
    public const string Stone = "stone";
    public const string Light = "light";
    public const string Darkness = "darkness";
}

public class EGuardian : StringEnum<EGuardian> {
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

public class TradeDataDefinition(JsonElement json) : DataDefinition(json) {
    public new int Price { get => base.Price!.Value; init => base.Price = value; }
    public new int GiftRate { get => base.GiftRate!.Value; init => base.GiftRate = value; }
    public new string? Ability { get => base.Ability; init => base.Ability = value; }
}

public class WeaponDataDefinition(JsonElement json) : DataDefinition(json) {
    public new int Price { get => base.Price.GetValueOrDefault(); init => base.Price = value; }
    public new int GiftRate { get => base.GiftRate.GetValueOrDefault(); init => base.GiftRate = value; }
    public new int Atk { get => base.Atk.GetValueOrDefault(); init => base.Atk = value; } // Magical stick does not have Atk
    public new int Def { get => base.Def.GetValueOrDefault(); init => base.Def = value; }
    public new int HitRate { get => base.HitRate.GetValueOrDefault(); init => base.HitRate = value; }
    public new bool IsPlusAtk { get => base.IsPlusAtk.GetValueOrDefault(); init => base.IsPlusAtk = value; }
    public new string? Element { get => base.Element; init => base.Element = value; }
    public new string? Ability { get => base.Ability; init => base.Ability = value; }
    public new int AbilityValue { get => base.AbilityValue.GetValueOrDefault(); init => base.AbilityValue = value; } // Ex. When Ability is AttackDyingly, AbilityValue will be 30
    public new string? Curse { get => base.Curse; init => base.Curse = value; } // Not null when Ability is EAbility.AddCurseOnDamage
}

public class ArmorDataDefinition(JsonElement json) : DataDefinition(json) {
    public new int Price { get => base.Price.GetValueOrDefault(); init => base.Price = value; }
    public new int GiftRate { get => base.GiftRate.GetValueOrDefault(); init => base.GiftRate = value; }
    public new int Def { get => base.Def.GetValueOrDefault(); init => base.Def = value; } // Armor which has EAbility.CounterAtk does not have Def
    public new int Atk { get => base.Atk.GetValueOrDefault(); init => base.Atk = value; }
    public new int HitRate { get => base.HitRate.GetValueOrDefault(); init => base.HitRate = value; }
    public new bool IsPlusAtk { get => base.IsPlusAtk.GetValueOrDefault(); init => base.IsPlusAtk = value; }
    public new string? Element { get => base.Element; init => base.Element = value; }
    public new string? Ability { get => base.Ability; init => base.Ability = value; }
    public new int AbilityValue { get => base.AbilityValue.GetValueOrDefault(); init => base.AbilityValue = value; } // This will not be used
    public new string? Curse { get => base.Curse; init => base.Curse = value; }
}

public class SundryDataDefinition(JsonElement json) : DataDefinition(json) {
    public new int Price { get => base.Price.GetValueOrDefault(); init => base.Price = value; }
    public new int GiftRate { get => base.GiftRate.GetValueOrDefault(); init => base.GiftRate = value; }
    public new int Atk { get => base.Atk.GetValueOrDefault(); init => base.Atk = value; }
    public new bool IsPlusAtk { get => base.IsPlusAtk.GetValueOrDefault(); init => base.IsPlusAtk = value; }
    public new string? Ability { get => base.Ability; init => base.Ability = value; }
    public new int AbilityValue { get => base.AbilityValue.GetValueOrDefault(); init => base.AbilityValue = value; }
    public new string? Curse { get => base.Curse; init => base.Curse = value; }
}

public class MiracleDataDefinition(JsonElement json) : DataDefinition(json) {
    public new int Price { get => base.Price!.Value; init => base.Price = value; }
    public new int GiftRate { get => base.GiftRate.GetValueOrDefault(); init => base.GiftRate = value; }
    public new int Atk { get => base.Atk.GetValueOrDefault(); init => base.Atk = value; }
    public new int HitRate { get => base.HitRate.GetValueOrDefault(); init => base.HitRate = value; }
    public new bool IsPlusAtk { get => base.IsPlusAtk.GetValueOrDefault(); init => base.IsPlusAtk = value; }
    public new string? Element { get => base.Element; init => base.Element = value; }
    public new string? Ability { get => base.Ability; init => base.Ability = value; }
    public new int AbilityValue { get => base.AbilityValue.GetValueOrDefault(); init => base.AbilityValue = value; }
    public new string? Curse { get => base.Curse; init => base.Curse = value; }
    public new int Cost { get => base.Cost.GetValueOrDefault(); init => base.Cost = value; }
}

public class DevilDataDefitition(JsonElement json) : DataDefinition(json) {
    public new string Ability { get => base.Ability!; init => base.Ability = value; }
    public new int AbilityValue { get => base.AbilityValue.GetValueOrDefault(); init => base.AbilityValue = value; }
    public new int AppearanceRate { get => base.AppearanceRate.GetValueOrDefault(); init => base.AppearanceRate = value; }
}

public class GuardianDataDefinition(JsonElement json) : DataDefinition(json) {
    public new string Guardian { get => base.Guardian!; init => base.Guardian = value; }
    public new int GuardianAttackRate { get => base.GuardianAttackRate.GetValueOrDefault(); init => base.GuardianAttackRate = value; }
    public new int Atk { get => base.Atk.GetValueOrDefault(); init => base.Atk = value; }
    public new int HitRate { get => base.HitRate.GetValueOrDefault(); init => base.HitRate = value; }
    public new string? Element { get => base.Element; init => base.Element = value; }
    public new string? Ability { get => base.Ability; init => base.Ability = value; }
    public new int AbilityValue { get => base.AbilityValue.GetValueOrDefault(); init => base.AbilityValue = value; }
    public new string? Curse { get => base.Curse; init => base.Curse = value; }
}

public class PhenomenaDataDefinition(JsonElement json) : DataDefinition(json) {
    public new int Atk { get => base.Atk.GetValueOrDefault(); init => base.Atk = value; }
    public new int HitRate { get => base.HitRate.GetValueOrDefault(); init => base.HitRate = value; }
    public new string? Element { get => base.Element; init => base.Element = value; }
    public new string? Ability { get => base.Ability; init => base.Ability = value; }
    public new int AbilityValue { get => base.AbilityValue.GetValueOrDefault(); init => base.AbilityValue = value; }
    public new string? Curse { get => base.Curse; init => base.Curse = value; }
}