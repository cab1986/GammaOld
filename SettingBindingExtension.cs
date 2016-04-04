using System.Windows.Data;

namespace Gamma
{
    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension()
        {
            Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Source = Gamma.Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}
