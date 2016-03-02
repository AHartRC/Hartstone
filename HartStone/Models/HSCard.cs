using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using HartStone.Classes;

namespace HartStone.Models
{
    public class HSCard
    {
        public HSCard()
        {

        }
        public HSCard(Dbf.RecordRow rawCard, CardDefs.EntityRow rawCardDef)
        {
            ProcessRecord(rawCard);
            ProcessDefinition(rawCardDef);
            ProcessExtraData();
            Console.Write("\r{0}\t\t\t", Name);
        }

        public override string ToString()
        {
            return string.Format("{0} | Cost: {1} | Attack: {2} | Mana: {3} | Damage: {4}-{5} | Healing: {6}-{7} | Draw: {8}-{9}",
                                Name, ManaCost, Attack, ManaCrystalAmount, DamageMinAmount, DamageMaxAmount, HealMinAmount, HealMaxAmount, DrawMinAmount, DrawMaxAmount);
        }

        private void ProcessRecord(Dbf.RecordRow row)
        {
            Dbf.FieldRow[] fields = row.GetFieldRows();

            foreach (Dbf.FieldRow field in fields)
            {
                string key = field.column;
                string value = field.Field_Text;

                switch (key)
                {
                    case "ID":
                        ListId = int.Parse(value);
                        //Console.WriteLine(ListId);
                        break;
                    case "NOTE_MINI_GUID":
                        CardId = value;
                        //Console.WriteLine(CardId);
                        break;
                    case "IS_COLLECTIBLE":
                        IsCollectible = bool.Parse(value);
                        //Console.WriteLine(IsCollectible);
                        break;
                    case "HERO_POWER_ID":
                        HeroPowerId = int.Parse(value);
                        //Console.WriteLine(HeroPowerId);
                        break;
                    case "LONG_GUID":
                        LongId = Guid.Parse(value);
                        //Console.WriteLine(LongId);
                        break;
                    case "CRAFTING_EVENT":
                        CraftingEvent = value;
                        //Console.WriteLine(CraftingEvent);
                        break;
                    default:
                        Console.WriteLine("UNKNOWN FIELD: {0} | {1}", key, value);
                        break;
                }
            }
        }

        private void ProcessDefinition(CardDefs.EntityRow row)
        {
            CardId = row.IsCardIDNull() ? null : row.CardID;
            DefinitionId = row.Entity_Id;
            MasterPower = row.IsMasterPowerNull() ? null : (Guid?)Guid.Parse(row.MasterPower);
            Version = row.IsversionNull() ? null : row.version;
            Entourage = row.GetEntourageCardRows();
            Powers = row.GetPowerRows();
            ReferencedTags = row.GetReferencedTagRows();
            Tags = row.GetTagRows();
            TriggeredPowerHistoryInfo = row.GetTriggeredPowerHistoryInfoRows();

            if (Tags.Any())
            {
                ProcessTags(Tags);
            }
            //if (Entourage.Any())
            //{
            //    ProcessEntourage(Entourage);
            //}
            //if (Powers.Any())
            //{
            //    ProcessPowers(Powers);
            //}
            //if (ReferencedTags.Any())
            //{
            //    ProcessReferencedTags(ReferencedTags);
            //}
            //if (TriggeredPowerHistoryInfo.Any())
            //{
            //    ProcessTriggeredPowerHistoryInfo(TriggeredPowerHistoryInfo);
            //}
        }

        private void ProcessExtraData()
        {
            Target = SpellLocation.OPPOSING_HERO; // Default target to opposing hero

            if (string.IsNullOrWhiteSpace(TextInHand))
                return;

            string tagText = TextInHand;
            TextInHand = tagText.Replace("#", "").Replace("$", "");
            tagText = tagText.ToLower();

            if (!IsCollectible)
                return; // Don't worry about cards that aren't collectible

            if (tagText.Contains("mana") || tagText.Contains("crystal") || Summoned)
            {
                GainManaCrystal = true;
                ManaCrystalAmount = new Random().Next(1, 3);
            }

            string healPattern = @"(heal|restore) (\#|)(?<HealAmount>\d*) health";
            string dmgPattern = @" (\$|)(?<DamageAmount>\d*?) damage(\.| instead| to (?<Target>.*?)(;|\.| and))";
            string drawPattern = "draw((s|)( until they have|) (?<Quantity>.*?)(,| card| and)| cards|n| it| from)";

            //Console.WriteLine(tagText);

            var healMatches = new Regex(healPattern).Matches(tagText);
            var dmgMatches = new Regex(dmgPattern).Matches(tagText);
            var drawMatches = new Regex(drawPattern).Matches(tagText);

            if (healMatches.Count > 0)
            {
                HealTarget = true;
                List<int> matchCounts = new List<int>();
                foreach (Match match in healMatches)
                {
                    int amount = new Random().Next(1, 2);
                    var amountMatch = match.Groups["HealAmount"];
                    if (amountMatch.Success)
                    {
                        string amountString = amountMatch.Value;
                        bool isNumber = int.TryParse(amountString, out amount);

                        if (!isNumber)
                        {
                            amount = new Random().Next(1, 5);
                        }
                    }
                    matchCounts.Add(amount);
                }
                HealMinAmount = matchCounts.Min();
                HealMaxAmount = matchCounts.Min();
            }
            if (dmgMatches.Count > 0)
            {
                DamageTarget = true;
                List<int> matchCounts = new List<int>();
                foreach (Match match in dmgMatches)
                {
                    int amount = new Random().Next(1, 2);
                    var amountMatch = match.Groups["DamageAmount"];
                    if (amountMatch.Success)
                    {
                        string amountString = amountMatch.Value;
                        bool isNumber = int.TryParse(amountString, out amount);

                        if (!isNumber)
                        {
                            amount = new Random().Next(1, 5);
                        }
                    }
                    matchCounts.Add(amount);
                }
                DamageMinAmount = matchCounts.Min();
                DamageMaxAmount = matchCounts.Min();
            }
            if (drawMatches.Count > 0)
            {
                DrawCard = true;
                List<int> matchCounts = new List<int>();
                foreach (Match match in drawMatches)
                {
                    int amount = new Random().Next(1, 3);
                    var amountMatch = match.Groups["Quantity"];
                    if (amountMatch.Success)
                    {
                        string amountString = amountMatch.Value;
                        bool isNumber = int.TryParse(amountString, out amount);

                        if (!isNumber)
                        {
                            amount = new Random().Next(1, 5);
                        }
                    }
                    matchCounts.Add(amount);
                }
                DrawMinAmount = matchCounts.Min();
                DrawMaxAmount = matchCounts.Min();
            }

            //if (tagText.Contains("#"))
            //{
            //    HealTarget = true;
            //    int flagIndex = tagText.IndexOf("#", StringComparison.Ordinal);
            //    int spaceIndex = tagText.IndexOf(" ", flagIndex, StringComparison.Ordinal);
            //    int numberLength = spaceIndex - flagIndex;
            //    string result = tagText.Substring(flagIndex + 1, numberLength - 1).Replace("#", "");
            //    string[] results = result.Split('-');

            //    if (results.Length > 1)
            //    {
            //        HealMinAmount = int.Parse(results[0]);
            //        HealMaxAmount = int.Parse(results[1]);
            //    }
            //    else
            //    {
            //        HealMinAmount = HealMaxAmount = int.Parse(result);
            //    }
            //}
            //if (tagText.Contains("$"))
            //{
            //    DamageTarget = true;
            //    int flagIndex = tagText.IndexOf("$", StringComparison.Ordinal);
            //    int spaceIndex = tagText.IndexOf(" ", flagIndex, StringComparison.Ordinal);
            //    int numberLength = spaceIndex - flagIndex;
            //    string result = tagText.Substring(flagIndex + 1, numberLength - 1).Replace("$", "");
            //    string[] results = result.Split('-');

            //    if (results.Length > 1)
            //    {
            //        DamageMinAmount = int.Parse(results[0]);
            //        DamageMaxAmount = int.Parse(results[1]);
            //    }
            //    else
            //    {
            //        DamageMinAmount = DamageMaxAmount = int.Parse(result);
            //    }
            //}

            //if (tagText.Contains("draw") && !tagText.Contains("draw from") && !tagText.Contains("draw it") && !tagText.Contains("when") && !tagText.Contains("at the ") && !tagText.Contains("choose"))
            //{
            //    DrawCard = true;
            //    Target = tagText.Contains("each") || tagText.Contains("both")
            //        ? SpellLocation.BOTH_HEROES
            //        : tagText.Contains("opponent") && !tagText.Contains("as many") && !tagText.Contains("equal to")
            //            ? SpellLocation.OPPOSING_HERO
            //            : SpellLocation.YOUR_HERO;

            //    int flagIndex = tagText.IndexOf(" ", tagText.IndexOf("draw", StringComparison.Ordinal), StringComparison.Ordinal);
            //    int spaceIndex = tagText.IndexOf(" ", flagIndex + 1, StringComparison.Ordinal);
            //    int numberLength = spaceIndex - flagIndex;
            //    string result = tagText.Substring(flagIndex + 1, numberLength - 1).Replace("$", "");
            //    string[] results = result.Split('-');

            //    switch (result)
            //    {
            //        case "a":
            //        case "one":
            //            result = "1";
            //            break;
            //        case "two":
            //            result = "2";
            //            break;
            //        case "cards":
            //        case "some":
            //            result = "3";
            //            break;
            //    }

            //    if (results.Length > 1)
            //    {
            //        DrawMinAmount = int.Parse(results[0]);
            //        DrawMaxAmount = int.Parse(results[1]);
            //    }
            //    else
            //    {
            //        DrawMinAmount = DrawMaxAmount = int.Parse(result);
            //    }
            //}

            //if (tagText.Contains("mana"))
            //{
            //    Target = tagText.Contains("each") || tagText.Contains("both")
            //        ? SpellLocation.BOTH_HEROES
            //        : tagText.Contains("opponent") && !tagText.Contains("as many") && !tagText.Contains("equal to")
            //            ? SpellLocation.OPPOSING_HERO
            //            : SpellLocation.YOUR_HERO;

            //    int flagIndex = tagText.IndexOf("give", StringComparison.Ordinal);
            //    int spaceIndex = tagText.IndexOf(" ", flagIndex + 1, StringComparison.Ordinal);
            //    int numberLength = spaceIndex - flagIndex;
            //    string result = tagText.Substring(flagIndex + 1, numberLength - 1).Replace("$", "");
            //    string[] results = result.Split('-');

            //    switch (result)
            //    {
            //        case "a":
            //        case "one":
            //            result = "1";
            //            break;
            //        case "two":
            //            result = "2";
            //            break;
            //        case "cards":
            //        case "some":
            //            result = "3";
            //            break;
            //    }
            //    var manaCrystalAmount = ManaCrystalAmount;
            //    ManaCrystalAmount = !int.TryParse(result, out manaCrystalAmount) ? new Random().Next(1, 3) : manaCrystalAmount;
            //}
        }

        public void ProcessTags(CardDefs.TagRow[] tags)
        {
            foreach (CardDefs.TagRow tag in tags)
            {
                int? tagID = tag.IsEntity_IdNull() ? (int?)null : tag.Entity_Id;
                GAME_TAG enumID = tag.IsenumIDNull() ? (GAME_TAG)(-1) : (GAME_TAG)int.Parse(tag.enumID);
                string type = tag.IsvalueNull() ? null : tag.type;
                string value = tag.IsvalueNull() ? null : tag.value;
                string tagText = tag.Tag_Text;


                switch (enumID)
                {
                    case GAME_TAG.CARDNAME:
                        Name = tagText;
                        //Console.WriteLine(Name);
                        break;
                    case GAME_TAG.CARD_SET:
                        Set = (TAG_CARD_SET)int.Parse(value);
                        //Console.WriteLine(Set);
                        break;
                    case GAME_TAG.CARDTYPE:
                        Type = (TAG_CARDTYPE)int.Parse(value);
                        //Console.WriteLine(Type);
                        break;
                    case GAME_TAG.FACTION:
                        Faction = (TAG_FACTION)int.Parse(value);
                        //Console.WriteLine(Faction);
                        break;
                    case GAME_TAG.CLASS:
                        Class = (TAG_CLASS)int.Parse(value);
                        //Console.WriteLine(Class);
                        break;
                    case GAME_TAG.RARITY:
                        Rarity = (TAG_RARITY)int.Parse(value);
                        //Console.WriteLine(Rarity);
                        break;
                    case GAME_TAG.HEALTH:
                        Health = int.Parse(value);
                        //Console.WriteLine(Health);
                        break;
                    case GAME_TAG.COST:
                        ManaCost = int.Parse(value);
                        //Console.WriteLine(ManaCost);
                        break;
                    case GAME_TAG.CARDTEXT_INHAND:
                        TextInHand = tagText;
                        //Console.WriteLine(TextInHand);
                        break;
                    case GAME_TAG.ENCHANTMENT_BIRTH_VISUAL:
                        BirthVisual = (TAG_ENCHANTMENT_VISUAL)int.Parse(value);
                        //Console.WriteLine(BirthVisual);
                        break;
                    case GAME_TAG.ENCHANTMENT_IDLE_VISUAL:
                        IdleVisual = (TAG_ENCHANTMENT_VISUAL)int.Parse(value);
                        //Console.WriteLine(IdleVisual);
                        break;
                    case GAME_TAG.SHOWN_HERO_POWER:
                        ShownHeroPower = int.Parse(value);
                        //Console.WriteLine(ShownHeroPower);
                        break;
                    case GAME_TAG.TARGETING_ARROW_TEXT:
                        TargetingArrowText = tagText;
                        //Console.WriteLine(TargetingArrowText);
                        break;
                    case GAME_TAG.AI_MUST_PLAY:
                        AIMustPlay = value.ToBoolean();
                        //Console.WriteLine(AIMustPlay);
                        break;
                    case GAME_TAG.TRIGGER_VISUAL:
                        TriggerVisual = value.ToBoolean();
                        //Console.WriteLine(TriggerVisual);
                        break;
                    case GAME_TAG.FREEZE:
                        Freeze = value.ToBoolean();
                        //Console.WriteLine(Freeze);
                        break;
                    case GAME_TAG.ARTISTNAME:
                        ArtistName = tagText;
                        //Console.WriteLine(ArtistName);
                        break;
                    case GAME_TAG.FLAVORTEXT:
                        FlavorText = tagText;
                        //Console.WriteLine(FlavorText);
                        break;
                    case GAME_TAG.HOW_TO_EARN:
                        HowToEarn = tagText;
                        //Console.WriteLine();
                        break;
                    case GAME_TAG.HOW_TO_EARN_GOLDEN:
                        HowToEarnGolden = tagText;
                        //Console.WriteLine(HowToEarnGolden);
                        break;
                    case GAME_TAG.ATK:
                        Attack = int.Parse(value);
                        //Console.WriteLine(Attack);
                        break;
                    case GAME_TAG.ELITE:
                        Elite = value.ToBoolean();
                        //Console.WriteLine(Elite);
                        break;
                    case GAME_TAG.COMBO:
                        Combo = value.ToBoolean();
                        //Console.WriteLine(Combo);
                        break;
                    case GAME_TAG.BATTLECRY:
                        Battlecry = value.ToBoolean();
                        //Console.WriteLine(Battlecry);
                        break;
                    case GAME_TAG.WINDFURY:
                        Windfury = value.ToBoolean();
                        //Console.WriteLine(Windfury);
                        break;
                    case GAME_TAG.TAUNT:
                        Taunt = value.ToBoolean();
                        //Console.WriteLine(Taunt);
                        break;
                    case GAME_TAG.CARDRACE:
                        Race = (TAG_RACE)int.Parse(value);
                        //Console.WriteLine(Race);
                        break;
                    case GAME_TAG.DEATHRATTLE:
                        Deathrattle = value.ToBoolean();
                        //Console.WriteLine(Deathrattle);
                        break;
                    case GAME_TAG.INSPIRE:
                        Inspire = value.ToBoolean();
                        //Console.WriteLine(Inspire);
                        break;
                    case GAME_TAG.TAG_ONE_TURN_EFFECT:
                        OneTurnEffect = value.ToBoolean();
                        //Console.WriteLine(OneTurnEffect);
                        break;
                    case GAME_TAG.CHARGE:
                        Charge = value.ToBoolean();
                        //Console.WriteLine(Charge);
                        break;
                    case GAME_TAG.AURA:
                        Aura = value.ToBoolean();
                        //Console.WriteLine(Aura);
                        break;
                    case GAME_TAG.TOPDECK:
                        TopDeck = value.ToBoolean();
                        //Console.WriteLine(TopDeck);
                        break;
                    case GAME_TAG.STEALTH:
                        Stealth = value.ToBoolean();
                        //Console.WriteLine(Stealth);
                        break;
                    case GAME_TAG.DIVINE_SHIELD:
                        DivineShield = value.ToBoolean();
                        //Console.WriteLine(DivineShield);
                        break;
                    case GAME_TAG.ENRAGED:
                        Enraged = value.ToBoolean();
                        //Console.WriteLine(Enraged);
                        break;
                    case GAME_TAG.SECRET:
                        Secret = value.ToBoolean();
                        //Console.WriteLine(Secret);
                        break;
                    case GAME_TAG.MORPH:
                        Morph = value.ToBoolean();
                        //Console.WriteLine(Morph);
                        break;
                    case GAME_TAG.AFFECTED_BY_SPELL_POWER:
                        AffectedBySpellPower = value.ToBoolean();
                        //Console.WriteLine(AffectedBySpellPower);
                        break;
                    case GAME_TAG.SPELLPOWER:
                        SpellPower = value.ToBoolean();
                        //Console.WriteLine(SpellPower);
                        break;
                    case GAME_TAG.RECALL:
                        Recall = value.ToBoolean();
                        //Console.WriteLine(Recall);
                        break;
                    case GAME_TAG.DURABILITY:
                        Health = int.Parse(value);
                        Durability = int.Parse(value);
                        //Console.WriteLine(Durability);
                        break;
                    case GAME_TAG.SILENCE: // WILL FALL!
                        Silence = value.ToBoolean();
                        //Console.WriteLine(Silence);
                        break;
                    case GAME_TAG.ADJACENT_BUFF:
                        AdjacentBuff = value.ToBoolean();
                        //Console.WriteLine(AdjacentBuff);
                        break;
                    case GAME_TAG.POISONOUS:
                        Poisonous = value.ToBoolean();
                        //Console.WriteLine(Poisonous);
                        break;
                    case GAME_TAG.FORGETFUL:
                        Forgetful = value.ToBoolean();
                        //Console.WriteLine(Forgetful);
                        break;
                    case GAME_TAG.HIDE_COST:
                        HideCost = value.ToBoolean();
                        //Console.WriteLine(HideCost);
                        break;
                    case GAME_TAG.EVIL_GLOW:
                        EvilGlow = value.ToBoolean();
                        //Console.WriteLine(EvilGlow);
                        break;
                    case GAME_TAG.HEROPOWER_DAMAGE:
                        HeroPowerDamage = value.ToBoolean();
                        //Console.WriteLine(HeroPowerDamage);
                        break;
                    case GAME_TAG.RECEIVES_DOUBLE_SPELLDAMAGE_BONUS:
                        ReceivesDoubleSpellDamageBonus = value.ToBoolean();
                        //Console.WriteLine(ReceivesDoubleSpellDamageBonus);
                        break;
                    case GAME_TAG.SUMMONED:
                        Summoned = value.ToBoolean();
                        //Console.WriteLine(ReceivesDoubleSpellDamageBonus);
                        break;
                    case (GAME_TAG)251:
                    case (GAME_TAG)252:
                    case (GAME_TAG)268:
                    case (GAME_TAG)321:
                    case (GAME_TAG)335:
                    case (GAME_TAG)349:
                    case (GAME_TAG)361:
                        //Console.WriteLine("GAME_TAG VALUE UNDEFINED: {0} | {1} | {2} | {3} | {4} | {5}", Name, tagID, enumID, type, value, tagText);
                        break;
                    default:
                        Console.WriteLine("UNKNOWN GAME_TAG DETECTED: {0} | {1} | {2} | {3} | {4}", enumID, tagID, type, value, tagText);
                        break;
                }
            }
        }

        public HSCard ProcessEntourage(CardDefs.EntourageCardRow[] entourage)
        {
            foreach (CardDefs.EntourageCardRow member in entourage)
            {
                int? rowId = member.IsEntity_IdNull() ? (int?)null : member.Entity_Id;
                string cardID = member.IscardIDNull() ? null : member.cardID;

                //Console.WriteLine("{0} | {1}", rowId, cardID);
            }

            return this;
        }

        public HSCard ProcessPowers(CardDefs.PowerRow[] powers)
        {
            foreach (CardDefs.PowerRow power in powers)
            {
                int? rowID = power.IsEntity_IdNull() ? (int?)null : power.Entity_Id;
                int powerID = power.Power_Id;
                Guid? definition = power.IsdefinitionNull() ? (Guid?)null : Guid.Parse(power.definition);
                CardDefs.PlayRequirementRow[] playRequirements = power.GetPlayRequirementRows();

                //Console.WriteLine("Power: {0} | {1} | {2}", rowID, powerID, definition);

                foreach (var requirement in playRequirements)
                {
                    int? parentPowerID = requirement.IsPower_IdNull() ? (int?)null : requirement.Power_Id;
                    int? param = !requirement.IsparamNull() ? (int?)null : int.Parse(requirement.param);
                    int? reqID = requirement.IsreqIDNull() ? (int?)null : int.Parse(requirement.reqID);

                    //Console.WriteLine("Requirement: {0} | {1} | {2}", parentPowerID, param, reqID);
                }
            }

            return this;
        }

        public HSCard ProcessReferencedTags(CardDefs.ReferencedTagRow[] tags)
        {
            foreach (var tag in tags)
            {
                var rowID = tag.IsEntity_IdNull() ? (int?)null : tag.Entity_Id;
                var enumID = tag.IsenumIDNull() ? (GAME_TAG)(-1) : (GAME_TAG)int.Parse(tag.enumID);
                var name = tag.IsnameNull() ? null : tag.name;
                var type = tag.IstypeNull() ? null : tag.type;
                var value = tag.IsvalueNull() ? null : tag.value;

                //Console.WriteLine("{0} | {1} | {2} | {3} | {4}", rowID, enumID, name, type, value);
            }

            return this;
        }

        public HSCard ProcessTriggeredPowerHistoryInfo(CardDefs.TriggeredPowerHistoryInfoRow[] historyInfo)
        {
            foreach (var info in historyInfo)
            {
                int? rowID = info.IsEntity_IdNull() ? (int?)null : info.Entity_Id;
                int? effectIndex = info.IseffectIndexNull() ? (int?)null : int.Parse(info.effectIndex);
                bool? showInHistory = !info.IsshowInHistoryNull() && bool.Parse(info.showInHistory);

                //Console.WriteLine("{0} | {1} | {2}", rowID, effectIndex, showInHistory);
            }

            return this;
        }

        public int ListId { get; set; }
        public int DefinitionId { get; set; }
        public string CardId { get; set; }
        public Guid LongId { get; set; }
        public Guid? MasterPower { get; set; }
        public string Version { get; set; }
        public bool IsCollectible { get; set; }
        public int HeroPowerId { get; set; }
        public string CraftingEvent { get; set; }
        public string Name { get; set; }
        public TAG_CARD_SET Set { get; set; }
        public TAG_CARDTYPE Type { get; set; }
        public TAG_FACTION Faction { get; set; }
        public TAG_CLASS Class { get; set; }
        public TAG_RARITY Rarity { get; set; }
        public int Health { get; set; }
        public int ManaCost { get; set; }
        public string TextInHand { get; set; }
        public TAG_ENCHANTMENT_VISUAL BirthVisual { get; set; }
        public TAG_ENCHANTMENT_VISUAL IdleVisual { get; set; }
        public int ShownHeroPower { get; set; }
        public string TargetingArrowText { get; set; }
        public bool AIMustPlay { get; set; }
        public bool TriggerVisual { get; set; }
        public bool Freeze { get; set; }
        public string ArtistName { get; set; }
        public string FlavorText { get; set; }
        public string HowToEarn { get; set; }
        public string HowToEarnGolden { get; set; }
        public int Attack { get; set; }
        public bool Elite { get; set; }
        public bool Combo { get; set; }
        public bool Battlecry { get; set; }
        public bool Windfury { get; set; }
        public bool Taunt { get; set; }
        public TAG_RACE Race { get; set; }
        public bool Deathrattle { get; set; }
        public bool Inspire { get; set; }
        public bool OneTurnEffect { get; set; }
        public bool Charge { get; set; }
        public bool Aura { get; set; }
        public bool TopDeck { get; set; }
        public bool Stealth { get; set; }
        public bool DivineShield { get; set; }
        public bool Enraged { get; set; }
        public bool Secret { get; set; }
        public bool Morph { get; set; }
        public bool AffectedBySpellPower { get; set; }
        public bool SpellPower { get; set; }
        public bool Recall { get; set; }
        public int Durability { get; set; }
        public bool Silence { get; set; }
        public bool AdjacentBuff { get; set; }
        public bool Poisonous { get; set; }
        public bool Forgetful { get; set; }
        public bool HideCost { get; set; }
        public bool EvilGlow { get; set; }
        public bool HeroPowerDamage { get; set; }
        public bool ReceivesDoubleSpellDamageBonus { get; set; }
        public bool Summoned { get; set; }
        public bool HealTarget { get; set; }
        public int HealMinAmount { get; set; }
        public int HealMaxAmount { get; set; }
        public bool DamageTarget { get; set; }
        public int DamageMinAmount { get; set; }
        public int DamageMaxAmount { get; set; }
        public int NumberOfTargets { get; set; }
        public SpellLocation Target { get; set; }
        public bool GainMana { get; set; }
        public bool GainManaCrystal { get; set; }
        public int ManaAmount { get; set; }
        public int ManaCrystalAmount { get; set; }
        public bool DrawCard { get; set; }
        public int DrawMinAmount { get; set; }
        public int DrawMaxAmount { get; set; }

        public CardDefs.EntourageCardRow[] Entourage { get; set; }
        public CardDefs.PowerRow[] Powers { get; set; }
        public CardDefs.ReferencedTagRow[] ReferencedTags { get; set; }
        public CardDefs.TagRow[] Tags { get; set; }
        public CardDefs.TriggeredPowerHistoryInfoRow[] TriggeredPowerHistoryInfo { get; set; }
    }
}
