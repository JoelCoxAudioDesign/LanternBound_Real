using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Complete skill tree system for light abilities
/// Manages upgrades, prerequisites, and modifiers
/// </summary>
[System.Serializable]
public class LightSkillTree
{
    [SerializeField] private List<LightSkill> _allSkills = new List<LightSkill>();
    [SerializeField] private List<LightSkill> _unlockedSkills = new List<LightSkill>();

    private EnhancedLanternController _lanternController;

    public void Initialize(EnhancedLanternController controller)
    {
        _lanternController = controller;
        CreateDefaultSkillTree();
    }

    public bool CanUnlock(LightSkill skill)
    {
        // Check if already unlocked
        if (_unlockedSkills.Contains(skill)) return false;

        // Check prerequisites
        foreach (var prerequisite in skill.Prerequisites)
        {
            if (!_unlockedSkills.Any(s => s.SkillId == prerequisite))
                return false;
        }

        // Check light type requirements
        if (skill.RequiredLightType != EnhancedLanternController.LightType.None)
        {
            if (!_lanternController.HasDiscovered(skill.RequiredLightType))
                return false;
        }

        return true;
    }

    public void UnlockSkill(LightSkill skill)
    {
        if (!_unlockedSkills.Contains(skill))
        {
            _unlockedSkills.Add(skill);
            Debug.Log($"Skill unlocked: {skill.DisplayName}");
        }
    }

    public float GetModifier(LightSkill.SkillType modifierType, EnhancedLanternController.LightType lightType)
    {
        float totalModifier = 0f;

        foreach (var skill in _unlockedSkills)
        {
            if (skill.ModifierType == modifierType)
            {
                // Check if skill applies to this light type
                if (skill.AppliesToLightType == EnhancedLanternController.LightType.None ||
                    skill.AppliesToLightType == lightType)
                {
                    totalModifier += skill.ModifierValue;
                }
            }
        }

        return totalModifier;
    }

    public List<LightSkill> GetAvailableSkills()
    {
        return _allSkills.Where(skill => CanUnlock(skill)).ToList();
    }

    public List<LightSkill> GetUnlockedSkills()
    {
        return new List<LightSkill>(_unlockedSkills);
    }

    public List<LightSkill> GetAllSkills()
    {
        return new List<LightSkill>(_allSkills);
    }

    private void CreateDefaultSkillTree()
    {
        _allSkills.Clear();

        // === CORE UPGRADES (Apply to all light types) ===

        // Mana Efficiency Tree
        _allSkills.Add(new LightSkill
        {
            SkillId = "mana_efficiency_1",
            DisplayName = "Inner Focus I",
            Description = "Reduce mana consumption by 10%",
            EssenceCost = 5,
            ModifierType = LightSkill.SkillType.ManaEfficiency,
            ModifierValue = 0.1f,
            AppliesToLightType = EnhancedLanternController.LightType.None,
            Prerequisites = new string[0],
            SkillCategory = LightSkill.Category.Core
        });

        _allSkills.Add(new LightSkill
        {
            SkillId = "mana_efficiency_2",
            DisplayName = "Inner Focus II",
            Description = "Further reduce mana consumption by 15%",
            EssenceCost = 10,
            ModifierType = LightSkill.SkillType.ManaEfficiency,
            ModifierValue = 0.15f,
            AppliesToLightType = EnhancedLanternController.LightType.None,
            Prerequisites = new string[] { "mana_efficiency_1" },
            SkillCategory = LightSkill.Category.Core
        });

        _allSkills.Add(new LightSkill
        {
            SkillId = "mana_efficiency_3",
            DisplayName = "Inner Mastery",
            Description = "Maximum mana efficiency - 25% reduction",
            EssenceCost = 20,
            ModifierType = LightSkill.SkillType.ManaEfficiency,
            ModifierValue = 0.25f,
            AppliesToLightType = EnhancedLanternController.LightType.None,
            Prerequisites = new string[] { "mana_efficiency_2" },
            SkillCategory = LightSkill.Category.Core
        });

        // Mana Regeneration Tree
        _allSkills.Add(new LightSkill
        {
            SkillId = "mana_regen_1",
            DisplayName = "Light Renewal I",
            Description = "Increase mana regeneration by 25%",
            EssenceCost = 8,
            ModifierType = LightSkill.SkillType.ManaRegeneration,
            ModifierValue = 0.25f,
            AppliesToLightType = EnhancedLanternController.LightType.None,
            Prerequisites = new string[0],
            SkillCategory = LightSkill.Category.Core
        });

        _allSkills.Add(new LightSkill
        {
            SkillId = "mana_regen_2",
            DisplayName = "Light Renewal II",
            Description = "Further increase mana regeneration by 50%",
            EssenceCost = 15,
            ModifierType = LightSkill.SkillType.ManaRegeneration,
            ModifierValue = 0.5f,
            AppliesToLightType = EnhancedLanternController.LightType.None,
            Prerequisites = new string[] { "mana_regen_1" },
            SkillCategory = LightSkill.Category.Core
        });

        // Beam Range Tree
        _allSkills.Add(new LightSkill
        {
            SkillId = "beam_range_1",
            DisplayName = "Extended Reach I",
            Description = "Increase beam range by 20%",
            EssenceCost = 7,
            ModifierType = LightSkill.SkillType.BeamRange,
            ModifierValue = 0.2f,
            AppliesToLightType = EnhancedLanternController.LightType.None,
            Prerequisites = new string[0],
            SkillCategory = LightSkill.Category.Core
        });

        _allSkills.Add(new LightSkill
        {
            SkillId = "beam_range_2",
            DisplayName = "Extended Reach II",
            Description = "Further increase beam range by 35%",
            EssenceCost = 12,
            ModifierType = LightSkill.SkillType.BeamRange,
            ModifierValue = 0.35f,
            AppliesToLightType = EnhancedLanternController.LightType.None,
            Prerequisites = new string[] { "beam_range_1" },
            SkillCategory = LightSkill.Category.Core
        });

        // Beam Width Tree
        _allSkills.Add(new LightSkill
        {
            SkillId = "beam_width_1",
            DisplayName = "Wider Light I",
            Description = "Increase beam width by 25%",
            EssenceCost = 6,
            ModifierType = LightSkill.SkillType.BeamWidth,
            ModifierValue = 0.25f,
            AppliesToLightType = EnhancedLanternController.LightType.None,
            Prerequisites = new string[0],
            SkillCategory = LightSkill.Category.Core
        });

        _allSkills.Add(new LightSkill
        {
            SkillId = "beam_width_2",
            DisplayName = "Wider Light II",
            Description = "Further increase beam width by 40%",
            EssenceCost = 11,
            ModifierType = LightSkill.SkillType.BeamWidth,
            ModifierValue = 0.4f,
            AppliesToLightType = EnhancedLanternController.LightType.None,
            Prerequisites = new string[] { "beam_width_1" },
            SkillCategory = LightSkill.Category.Core
        });

        // === EMBER LIGHT SPECIALIZATIONS ===

        _allSkills.Add(new LightSkill
        {
            SkillId = "ember_efficiency",
            DisplayName = "Ember Mastery",
            Description = "Ember light costs 50% less mana",
            EssenceCost = 8,
            ModifierType = LightSkill.SkillType.ManaEfficiency,
            ModifierValue = 0.5f,
            AppliesToLightType = EnhancedLanternController.LightType.Ember,
            RequiredLightType = EnhancedLanternController.LightType.Ember,
            Prerequisites = new string[0],
            SkillCategory = LightSkill.Category.EmberSpecialization
        });

        _allSkills.Add(new LightSkill
        {
            SkillId = "ember_intensity",
            DisplayName = "Ember Amplification",
            Description = "Ember light is 30% brighter",
            EssenceCost = 10,
            ModifierType = LightSkill.SkillType.LightIntensity,
            ModifierValue = 0.3f,
            AppliesToLightType = EnhancedLanternController.LightType.Ember,
            RequiredLightType = EnhancedLanternController.LightType.Ember,
            Prerequisites = new string[] { "ember_efficiency" },
            SkillCategory = LightSkill.Category.EmberSpecialization
        });

        _allSkills.Add(new LightSkill
        {
            SkillId = "ember_persistence",
            DisplayName = "Eternal Ember",
            Description = "Ember light no longer drains mana when held",
            EssenceCost = 25,
            ModifierType = LightSkill.SkillType.SpecialAbility,
            ModifierValue = 1f,
            AppliesToLightType = EnhancedLanternController.LightType.Ember,
            RequiredLightType = EnhancedLanternController.LightType.Ember,
            Prerequisites = new string[] { "ember_intensity", "mana_efficiency_2" },
            SkillCategory = LightSkill.Category.EmberSpecialization
        });

        // === RADIANCE LIGHT SPECIALIZATIONS ===

        _allSkills.Add(new LightSkill
        {
            SkillId = "radiance_power",
            DisplayName = "Radiant Power",
            Description = "Radiance light is 40% more intense",
            EssenceCost = 12,
            ModifierType = LightSkill.SkillType.LightIntensity,
            ModifierValue = 0.4f,
            AppliesToLightType = EnhancedLanternController.LightType.Radiance,
            RequiredLightType = EnhancedLanternController.LightType.Radiance,
            Prerequisites = new string[0],
            SkillCategory = LightSkill.Category.RadianceSpecialization
        });

        _allSkills.Add(new LightSkill
        {
            SkillId = "radiance_piercing",
            DisplayName = "Piercing Radiance",
            Description = "Radiance light can pass through barriers",
            EssenceCost = 18,
            ModifierType = LightSkill.SkillType.SpecialAbility,
            ModifierValue = 1f,
            AppliesToLightType = EnhancedLanternController.LightType.Radiance,
            RequiredLightType = EnhancedLanternController.LightType.Radiance,
            Prerequisites = new string[] { "radiance_power" },
            SkillCategory = LightSkill.Category.RadianceSpecialization
        });

        _allSkills.Add(new LightSkill
        {
            SkillId = "radiance_burst",
            DisplayName = "Radiant Burst",
            Description = "Activating Radiance creates an area blast",
            EssenceCost = 30,
            ModifierType = LightSkill.SkillType.SpecialAbility,
            ModifierValue = 1f,
            AppliesToLightType = EnhancedLanternController.LightType.Radiance,
            RequiredLightType = EnhancedLanternController.LightType.Radiance,
            Prerequisites = new string[] { "radiance_piercing", "beam_width_2" },
            SkillCategory = LightSkill.Category.RadianceSpecialization
        });

        // === SOLAR FLARE SPECIALIZATIONS ===

        _allSkills.Add(new LightSkill
        {
            SkillId = "solar_damage",
            DisplayName = "Solar Intensity",
            Description = "Solar Flare deals 50% more damage",
            EssenceCost = 15,
            ModifierType = LightSkill.SkillType.Damage,
            ModifierValue = 0.5f,
            AppliesToLightType = EnhancedLanternController.LightType.SolarFlare,
            RequiredLightType = EnhancedLanternController.LightType.SolarFlare,
            Prerequisites = new string[0],
            SkillCategory = LightSkill.Category.SolarFlareSpecialization
        });

        _allSkills.Add(new LightSkill
        {
            SkillId = "solar_burn",
            DisplayName = "Lingering Burn",
            Description = "Solar Flare applies burning effect",
            EssenceCost = 20,
            ModifierType = LightSkill.SkillType.SpecialAbility,
            ModifierValue = 1f,
            AppliesToLightType = EnhancedLanternController.LightType.SolarFlare,
            RequiredLightType = EnhancedLanternController.LightType.SolarFlare,
            Prerequisites = new string[] { "solar_damage" },
            SkillCategory = LightSkill.Category.SolarFlareSpecialization
        });

        // === MOONBEAM SPECIALIZATIONS ===

        _allSkills.Add(new LightSkill
        {
            SkillId = "moonbeam_revelation",
            DisplayName = "Deep Sight",
            Description = "Moonbeam reveals more hidden secrets",
            EssenceCost = 14,
            ModifierType = LightSkill.SkillType.SpecialAbility,
            ModifierValue = 1f,
            AppliesToLightType = EnhancedLanternController.LightType.MoonBeam,
            RequiredLightType = EnhancedLanternController.LightType.MoonBeam,
            Prerequisites = new string[0],
            SkillCategory = LightSkill.Category.MoonBeamSpecialization
        });

        _allSkills.Add(new LightSkill
        {
            SkillId = "moonbeam_serenity",
            DisplayName = "Lunar Serenity",
            Description = "Moonbeam restores mana while active",
            EssenceCost = 25,
            ModifierType = LightSkill.SkillType.SpecialAbility,
            ModifierValue = 1f,
            AppliesToLightType = EnhancedLanternController.LightType.MoonBeam,
            RequiredLightType = EnhancedLanternController.LightType.MoonBeam,
            Prerequisites = new string[] { "moonbeam_revelation", "mana_regen_2" },
            SkillCategory = LightSkill.Category.MoonBeamSpecialization
        });

        // === MASTER SKILLS (Require multiple light types) ===

        _allSkills.Add(new LightSkill
        {
            SkillId = "light_synthesis",
            DisplayName = "Light Synthesis",
            Description = "Can blend two light types simultaneously",
            EssenceCost = 50,
            ModifierType = LightSkill.SkillType.SpecialAbility,
            ModifierValue = 1f,
            AppliesToLightType = EnhancedLanternController.LightType.None,
            Prerequisites = new string[] { "ember_persistence", "radiance_burst" },
            SkillCategory = LightSkill.Category.Master
        });

        _allSkills.Add(new LightSkill
        {
            SkillId = "inner_light_mastery",
            DisplayName = "Inner Light Mastery",
            Description = "All light abilities cost 25% less mana",
            EssenceCost = 40,
            ModifierType = LightSkill.SkillType.ManaEfficiency,
            ModifierValue = 0.25f,
            AppliesToLightType = EnhancedLanternController.LightType.None,
            Prerequisites = new string[] { "mana_efficiency_3", "mana_regen_2" },
            SkillCategory = LightSkill.Category.Master
        });

        _allSkills.Add(new LightSkill
        {
            SkillId = "prism_heart",
            DisplayName = "Prism Heart",
            Description = "Unlocks Prismatic Light - affects all elements",
            EssenceCost = 75,
            ModifierType = LightSkill.SkillType.SpecialAbility,
            ModifierValue = 1f,
            AppliesToLightType = EnhancedLanternController.LightType.PrismaticLight,
            Prerequisites = new string[] { "light_synthesis", "inner_light_mastery" },
            SkillCategory = LightSkill.Category.Master
        });

        Debug.Log($"Skill tree initialized with {_allSkills.Count} skills");
    }
}

/// <summary>
/// Individual skill in the light skill tree
/// </summary>
[System.Serializable]
public class LightSkill
{
    public string SkillId;
    public string DisplayName;
    [TextArea(2, 3)]
    public string Description;
    public int EssenceCost;

    [Header("Requirements")]
    public string[] Prerequisites = new string[0];
    public EnhancedLanternController.LightType RequiredLightType = EnhancedLanternController.LightType.None;

    [Header("Effects")]
    public SkillType ModifierType;
    public float ModifierValue;
    public EnhancedLanternController.LightType AppliesToLightType = EnhancedLanternController.LightType.None;

    [Header("Organization")]
    public Category SkillCategory;
    public Vector2 UIPosition; // For skill tree display

    public enum SkillType
    {
        ManaEfficiency,     // Reduces mana costs
        ManaRegeneration,   // Increases mana regen
        BeamRange,          // Increases beam range
        BeamWidth,          // Increases beam width
        LightIntensity,     // Increases light intensity
        Damage,             // Increases damage (for combat lights)
        SpecialAbility      // Unlocks special abilities
    }

    public enum Category
    {
        Core,                      // Universal upgrades
        EmberSpecialization,       // Ember-specific skills
        RadianceSpecialization,    // Radiance-specific skills
        SolarFlareSpecialization,  // Solar Flare-specific skills
        MoonBeamSpecialization,    // Moon Beam-specific skills
        StarlightSpecialization,   // Starlight-specific skills
        PrismaticSpecialization,   // Prismatic-specific skills
        VoidLightSpecialization,   // Void Light-specific skills
        Master                     // High-tier skills requiring multiple trees
    }

    public bool HasPrerequisite(string skillId)
    {
        return Prerequisites.Contains(skillId);
    }

    public bool IsSpecializationSkill()
    {
        return SkillCategory != Category.Core && SkillCategory != Category.Master;
    }

    public bool IsMasterSkill()
    {
        return SkillCategory == Category.Master;
    }
}