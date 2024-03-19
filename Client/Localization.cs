using System.Text.Json;
using GodOfGodField.Shared;

namespace GodOfGodField.Client;

public class Localization {
    private Dictionary<string, string> _Localizations = new();
    public string Language { get; private set; } = "ja";

    public Localization(string language = "ja") => SwitchLanguage(language);

    public string this[string key] => _Localizations.TryGetValue(key, out var value) ? value : $"MISSING: {key}";

    public void SwitchLanguage(string language) {
        Language = language;
        if (Resources.GetResource($"i18n/{language}.json") is not Stream stream) throw new($"The language `{language}` does not exist!" + (language == "ja" ? " You need to install resources on your server first." : string.Empty));
        _Localizations = Flatten(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(stream)!);
    }

    private Dictionary<string, string> Flatten(Dictionary<string, JsonElement> dict) {
        var result = new Dictionary<string, string>();
        foreach (var item in dict["items"].EnumerateArray()) result[$"items.{item.GetProperty("imageName").GetString()}"] = item.GetProperty("name").GetString()!;
        foreach (var (key, value) in dict["texts"].Deserialize<Dictionary<string, JsonElement>>()!) foreach (var kv in value.EnumerateObject()) result[$"texts.{key}.{kv.Name}"] = kv.Value.GetString()!;
        return result;
    }

    private static string[] _SupportedLanguages = null!;
    public static string[] SupportedLanguages => _SupportedLanguages ??= GetSupportedLanguages();

    private static string[] GetSupportedLanguages() {
        var assembly = typeof(Resources).Assembly;
        return assembly.GetManifestResourceNames().Where(x => x.StartsWith($"{assembly.GetName().Name}.Resources.i18n") && x.EndsWith(".json")).Select(x => x.Split(".")[^2]).ToArray();
    }

    public string? GetItemElement(int modelId) => GetItemElement(Resources.GetDataDefinitionByModelId(modelId)!);
    public string? GetItemElement(DataDefinition dataDef) {
        if (dataDef is null) return null;
        if (dataDef.IsArmor(out var armor)) return armor.Element?[0..1].ToUpper() + armor.Element?[1..];
        else if (dataDef.IsWeapon(out var weapon)) return weapon.Element?[0..1].ToUpper() + weapon.Element?[1..];
        return null;
    }

    public string? GetItemDescription(int modelId) {
        var dataDef = Resources.GetDataDefinitionByModelId(modelId);
        if (dataDef is null) return null;
        if (dataDef.IsArmor(out var armor)) return this["texts.game.def"].Replace("{{def}}", armor.Def.ToString());
        else if (dataDef.IsWeapon(out var weapon)) {
            var text = this[$"texts.game.{(weapon.IsPlusAtk ? "plusAtk" : "atk")}"].Replace("{{atk}}", weapon.Atk.ToString());
            if (weapon.HitRate != 0) text = this["texts.game.hitRate"].Replace("{{hitRate}}", weapon.HitRate.ToString()) + text;
            return text;
        } else if (dataDef.IsMiracle(out var miracle)) {
            var text = this[$"texts.game.{(miracle.IsPlusAtk ? "plusAtk" : "atk")}"].Replace("{{atk}}", miracle.Atk.ToString());
            if (miracle.HitRate != 0) text = this["texts.game.hitRate"].Replace("{{hitRate}}", miracle.HitRate.ToString()) + text;
            return text;
        }
        return null;
    }

    public string? GetItemDescriptionCP(int modelId) {
        var dataDef = Resources.GetDataDefinitionByModelId(modelId);
        if (dataDef is null) return null;
        return dataDef.Price.HasValue ? this["texts.game.cp"].Replace("{{cp}}", dataDef.Price.ToString()) : null;
    }

    public string? GetItemShortInformation(DataDefinition? dataDef) {
        if (dataDef is null) return null;
        if (dataDef.IsArmor(out var armor)) {
            if (armor.Def == 0) return null;
            return armor.Ability switch {
                EAbility.CounterAtk
                    or EAbility.Counter2xAtk
                    or EAbility.CounterCurse
                    or EAbility.CounterBoost2xMP
                    or EAbility.CounterTakeCP => this["texts.game.counter"],
                _ => this["texts.game.def"].Replace("{{def}}", armor.Def.ToString())
            };
        } else if (dataDef.IsWeapon(out var weapon)) {
            if (weapon.Ability == EAbility.AtkBy2xMP) return this["texts.game.atkBy2xMP"];
            var text = this[$"texts.game.{(weapon.IsPlusAtk ? "plusAtk" : "atk")}"].Replace("{{atk}}", weapon.Atk.ToString());
            if (weapon.HitRate != 0) text = this["texts.game.hitRate"].Replace("{{hitRate}}", weapon.HitRate.ToString()) + text;
            return text;
        } else if (dataDef.IsSundry(out var sundry)) {
            return sundry.Ability switch {
                EAbility.BoostHP => this["texts.abilities.boostHP"].Replace("{{hp}}", sundry.AbilityValue.ToString()),
                EAbility.BoostHPOrDealDamage => this["texts.abilities.boostHP"].Replace("{{hp}}", sundry.AbilityValue.ToString()),
                EAbility.BoostMP => this["texts.abilities.boostMP"].Replace("{{mp}}", sundry.AbilityValue.ToString()),
                EAbility.BoostMPAndAddCurse => this["texts.abilities.boostMP"].Replace("{{mp}}", sundry.AbilityValue.ToString()),
                _ => null
            };
        } else if (dataDef.IsMiracle(out var miracle)) {
            if (miracle.Atk != 0) {
                var text = this[$"texts.game.{(miracle.IsPlusAtk ? "plusAtk" : "atk")}"].Replace("{{atk}}", miracle.Atk.ToString());
                if (miracle.HitRate != 0) text = this["texts.game.hitRate"].Replace("{{hitRate}}", miracle.HitRate.ToString()) + text;
                return text;
            }
            return miracle.Ability switch {
                EAbility.AddCurse => this[$"texts.curseNames.{miracle.Curse}"],
                EAbility.BoostCP => this["texts.abilities.boostCP"].Replace("{{cp}}", miracle.AbilityValue.ToString()),
                EAbility.BoostHP => this["texts.abilities.boostHP"].Replace("{{hp}}", miracle.AbilityValue.ToString()),
                _ => null
            };
        } else if (dataDef.IsGuardian(out var guardian)) {
            if (guardian.Atk != 0) {
                var text = this["texts.game.atk"].Replace("{{atk}}", guardian.Atk.ToString());
                if (guardian.HitRate != 0) text = this["texts.game.hitRate"].Replace("{{hitRate}}", guardian.HitRate.ToString()) + text;
                return text;
            }
            return guardian.Ability switch {
                EAbility.AddCurse => this[$"texts.curseNames.{guardian.Curse}"],
                EAbility.BoostCP
                    or EAbility.BoostCPOfEverybody
                    or EAbility.BoostCPToEnemy => this["texts.abilities.boostCP"].Replace("{{cp}}", guardian.AbilityValue.ToString()),
                EAbility.BoostHP => this["texts.abilities.boostHP"].Replace("{{hp}}", guardian.AbilityValue.ToString()),
                EAbility.BoostMP => this["texts.abilities.boostMP"].Replace("{{mp}}", guardian.AbilityValue.ToString()),
                EAbility.TakeCP => this["texts.abilities.takeCP"].Replace("{{cp}}", guardian.AbilityValue.ToString()),
                _ => null
            };
        } else if (dataDef.IsPhenomena(out var phenomena)) {
            if (phenomena.Atk != 0) {
                var text = this["texts.game.atk"].Replace("{{atk}}", phenomena.Atk.ToString());
                if (phenomena.HitRate != 0) text = this["texts.game.hitRate"].Replace("{{hitRate}}", phenomena.HitRate.ToString()) + text;
                return text;
            }
            return phenomena.Ability switch {
                EAbility.BoostHP => this["texts.abilities.boostHP"].Replace("{{hp}}", phenomena.AbilityValue.ToString()),
                _ => null
            };
        } else if (dataDef.IsDevil(out var devil)) {
            return devil.Ability switch {
                EAbility.DealDamage => this["texts.abilities.dealDamage"].Replace("{{damage}}", devil.AbilityValue.ToString()),
                _ => null
            };
        } else if (dataDef.IsTrade(out _)) return null;
        return null;
    }

    public string? GetItemInformation(DataDefinition? dataDef) {
        if (dataDef is null) return null;
        if (dataDef.IsArmor(out var armor)) {
            if (armor.Atk != 0) {
                var text = this[$"texts.game.{(armor.IsPlusAtk ? "plusAtk" : "atk")}"].Replace("{{atk}}", armor.Atk.ToString());
                if (armor.HitRate != 0) text = this["texts.game.hitRate"].Replace("{{hitRate}}", armor.HitRate.ToString()) + text;
                return text;
            }
            return armor.Ability switch {
                EAbility.BlockMiracle => this["texts.abilities.blockMiracle"],
                EAbility.BounceMiracle => this["texts.abilities.bounceMiracle"],
                EAbility.Counter2xAtk => armor.HitRate == 0 ? this["texts.abilities.counter2xAtk"] : this["texts.game.hitRate"].Replace("{{hitRate}}", armor.HitRate.ToString()) + this["texts.abilities.counter2xAtk"],
                EAbility.CounterAtk => armor.HitRate == 0 ? this["texts.abilities.counterAtk"] : this["texts.game.hitRate"].Replace("{{hitRate}}", armor.HitRate.ToString()) + this["texts.abilities.counterAtk"],
                EAbility.CounterBoost2xMP => this["texts.abilities.counterBoost2xMP"],
                EAbility.CounterCurse => this[$"texts.curseNames.{armor.Curse}"],
                EAbility.CounterTakeCP => this["texts.abilities.counterTakeCP"],
                EAbility.CutCost => this["texts.abilities.cutCost"],
                EAbility.FilterAtkElement => this["texts.abilities.filterAtkElement"],
                EAbility.ReflectAnything => this["texts.abilities.reflectAnything"],
                EAbility.ReflectMiracle => this["texts.abilities.reflectMiracle"],
                EAbility.SelfCurse => this["texts.abilities.selfCurse"].Replace("{{curse}}", this[$"texts.curseNames.{armor.Curse}"]),
                EAbility.SelfCurseAndRedraw => this["texts.abilities.selfCurseAndRedraw"].Replace("{{curse}}", this[$"texts.curseNames.{armor.Curse}"]),
                _ => null
            };
        } else if (dataDef.IsWeapon(out var weapon)) {
            if (weapon.Def != 0) return this["texts.game.def"].Replace("{{def}}", weapon.Def.ToString());
            return weapon.Ability switch {
                EAbility.AddCurseOnDamage => this["texts.abilities.addCurseOnDamage"].Replace("{{curse}}", this[$"texts.curseNames.{weapon.Curse}"]),
                EAbility.AtkBy2xMP => this["texts.abilities.consumeAllMP"],
                EAbility.Danger => this["texts.abilities.attackSomebody"],
                EAbility.SetElement => this["texts.abilities.setElement"].Replace("{{element}}", this[$"texts.elementNames.{weapon.Element}"]),
                _ => weapon.Ability is null ? null : this[$"texts.abilities.{weapon.Ability}"]
            };
        } else if (dataDef.IsSundry(out var sundry)) {
            if (sundry.Atk != 0) {
                var text = this[$"texts.game.{(sundry.IsPlusAtk ? "plusAtk" : "atk")}"].Replace("{{atk}}", sundry.Atk.ToString());
                // if (sundry.HitRate != 0) text = this["texts.game.hitRate"].Replace("{{hitRate}}", sundry.HitRate.ToString()) + text;
                return text;
            }
            return sundry.Ability switch {
                EAbility.BoostHPOrDealDamage => this["texts.abilities.orDealDamage"].Replace("{{damage}}", sundry.AbilityValue.ToString()),
                EAbility.BoostMPAndAddCurse => this[$"texts.curseNames.{sundry.Curse}"],
                EAbility.Revive => this["texts.abilities.revive"].Replace("{{hp}}", sundry.AbilityValue.ToString()),
                _ => sundry.Ability is null ? null : this[$"texts.abilities.{sundry.Ability}"]
            };
        } else if (dataDef.IsMiracle(out var miracle)) {
            if (miracle.Atk == 0) return null;
            return miracle.Ability switch {
                EAbility.AddCurseOnDamage => this["texts.abilities.addCurseOnDamage"].Replace("{{curse}}", this[$"texts.curseNames.{miracle.Curse}"]),
                _ => miracle.Ability is null ? null : this[$"texts.abilities.{miracle.Ability}"]
            };
        } else if (dataDef.IsGuardian(out var guardian)) {
            return guardian.Ability switch {
                EAbility.AddCurseOnDamage => this["texts.abilities.addCurseOnDamage"].Replace("{{curse}}", this[$"texts.curseNames.{guardian.Curse}"]),
                EAbility.CategoryWeapons => null,
                _ => guardian.Ability is null ? null : this[$"texts.abilities.{guardian.Ability}"]
            };
        } else if (dataDef.IsPhenomena(out var phenomena)) {
            return phenomena.Ability switch {
                EAbility.SetCurseOfEverybody => this["texts.abilities.setCurseOfEverybody"].Replace("{{curse}}", this[$"texts.curseNames.{phenomena.Curse}"]),
                EAbility.SetHPOfEverybody => this["texts.abilities.setHPOfEverybody"].Replace("{{hp}}", phenomena.AbilityValue.ToString()),
                _ => phenomena.Ability is null ? null : this[$"texts.abilities.{phenomena.Ability}"]
            };
        } else if (dataDef.IsDevil(out var devil)) {
            if (devil.Ability == EAbility.DealDamage) return null;
            return devil.Ability is null ? null : this[$"texts.abilities.{devil.Ability}"];
        } else if (dataDef.IsTrade(out var trade)) {
            return trade.Ability is null ? null : this[$"texts.abilities.{trade.Ability}"];
        }
        return null;
    }
}