using System.IO;

namespace DarknestDungeon.IC
{
    using JsonUtil = PurenailCore.SystemUtil.JsonUtil<DarknestDungeon>;

    public abstract class AbstractDataModule<D, T> : ItemChanger.Modules.Module where D : AbstractDataModule<D, T>, new()
    {
        private T? _data;
        public T Data
        {
            get
            {
                Load();
                return _data;
            }
        }

        private void Load() => _data ??= JsonUtil.DeserializeEmbedded<T>($"DarknestDungeon.Resources.Data.{JsonName()}.json");

        public static void UpdateJson(string root)
        {
            D mod = new();
            var path = Path.Combine(root, $"DarknestDungeon/Resources/Data/{mod.JsonName()}.json");
            File.Delete(path);
            JsonUtil.Serialize(mod.Data, path);
        }

        protected abstract string JsonName();

        public override void Initialize() => Load();

        public override void Unload() { }
    }
}
