namespace Core.Localization
{
    public readonly struct LocaleOption
    {
        public int Index { get; }
        public string Code { get; }
        public string DisplayName { get; }

        public LocaleOption(int index, string code, string displayName)
        {
            Index = index;
            Code = code;
            DisplayName = displayName;
        }
    }
}
