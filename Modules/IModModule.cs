namespace ArfTattleHook.Modules
{
    public interface IModModule
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        string Category { get; }
        bool Enabled { get; set; }
        bool IsExpanded { get; set; }

        void OnInitialize();
        void OnUpdate();
        void OnFixedUpdate();
        void OnDrawSettings();
    }
}
