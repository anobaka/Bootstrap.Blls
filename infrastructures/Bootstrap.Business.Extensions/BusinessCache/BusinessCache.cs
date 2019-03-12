using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bootstrap.Business.Extensions.BusinessCache
{
    /// <summary>
    /// TODO: This feature is used with business handler, putting it here temporarily until the stable business handler borns.
    /// </summary>
    public class BusinessCache
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelFullnames">Assembly - Fullnames</param>
        public static void Init(Dictionary<string, IEnumerable<string>> modelFullnames)
        {
            foreach (var ak in modelFullnames)
            {
                var assembly = Assembly.Load(ak.Key);
                if (ak.Value != null)
                {
                    foreach (var f in ak.Value)
                    {
                        var type = typeof(BusinessCacheOptions<>).MakeGenericType(
                            assembly.GetType(f));
                        type.GetProperty("Enabled").SetValue(null, true);
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources">AssembleName - Namespace - Model names</param>
        public static void Init(Dictionary<string, Dictionary<string, IEnumerable<string>>> resources) => Init(
            resources.ToDictionary(t => t.Key, t => t.Value.SelectMany(a => a.Value.Select(b => $"{a.Key}.{b}"))));
    }
}