using YamlDotNet.Serialization;

namespace Milthm.UnityExtension.Model
{
    public class AndroidKeystoreConfig
    {
        [YamlMember(Alias = "androidKeystoreName")]
        public string AndroidKeystoreName { get; set; }

        [YamlMember(Alias = "androidKeystorePass")]
        public string AndroidKeystorePass { get; set; }

        [YamlMember(Alias = "androidKeyaliasName")]
        public string AndroidKeyaliasName { get; set; }

        [YamlMember(Alias = "androidKeyaliasPass")]
        public string AndroidKeyaliasPass { get; set; }
    }
}
