#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DRFV.Global.Utilities;

namespace Milthm.BuildScript.Context
{
    public class BuildContext
    {
        private Dictionary<string, string> OptionsFromCode { get; set; } = new Dictionary<string, string>();

        public BuildContext(Dictionary<string, string> options)
        {
            foreach (var (key, value) in options)
            {
                OptionsFromCode.Add(NamingStrategyUtil.ToUpperCamelCase(key), value);
            }
        }

        public bool OptionsContains(string key)
        {
            var exists = OptionsFromCode.ContainsKey(NamingStrategyUtil.ToUpperCamelCase(key));
            if (exists)
            {
                return true;
            }

            var envVal = Environment.GetEnvironmentVariable(NamingStrategyUtil.ToUpperSnakeCase(key));
            return envVal != null;
        }

        public bool OptionsTryGet(string key, [NotNullWhen(true)] out string? result)
        {
            if (OptionsFromCode.TryGetValue(NamingStrategyUtil.ToUpperCamelCase(key), out result))
            {
                return true;
            }

            result = Environment.GetEnvironmentVariable(NamingStrategyUtil.ToUpperSnakeCase(key));
            return result != null;
        }

        public string? OptionsGet(string key, string? defaultValue = "")
        {
            if (OptionsFromCode.TryGetValue(NamingStrategyUtil.ToUpperCamelCase(key), out var result))
            {
                return result;
            }

            result = Environment.GetEnvironmentVariable(NamingStrategyUtil.ToUpperSnakeCase(key));
            if (result != null)
            {
                return result;
            }

            return defaultValue;
        }

        public bool? OptionsGetBool(string key, bool? defaultValue)
        {
            var str = OptionsGet(key, null);
            if (str == null)
            {
                return defaultValue;
            }

            if (bool.TryParse(str, out var ret))
            {
                return ret;
            }

            return defaultValue;
        }
    }
}
