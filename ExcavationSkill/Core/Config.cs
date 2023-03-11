using MoonShared.Config;

namespace ExcavationSkill
{
    [ConfigClass(I18NNameSuffix = "")]
    public class Config
    {
        [ConfigOption]
        public bool AlternativeSkillPageIcon { get; set; } = false;


        [ConfigOption]
        public bool EnablePrestige{ get; set; } = false;
    }
}
