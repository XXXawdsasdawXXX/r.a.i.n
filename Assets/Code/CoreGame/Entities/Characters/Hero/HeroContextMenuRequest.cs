namespace CoreGame.Entities.Characters.Hero
{
    public readonly struct HeroContextMenuRequest
    {
        public HeroContextTarget Target { get; }
        public string HeroObjectId { get; }
        public string DisplayName { get; }

        public HeroContextMenuRequest(HeroContextTarget target)
        {
            Target = target;
            HeroObjectId = target != null ? target.HeroObjectId : string.Empty;
            DisplayName = target != null ? target.DisplayName : string.Empty;
        }
    }
}
