using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Bootstrap.Business.Components.Services.Options
{

    public class DynamicCacheOptions<TResource>
    {
        public static bool Enabled { get; set; }
        private static Func<TResource, object> _keySelector;

        public static Func<TResource, object> KeySelector
        {
            get
            {
                if (_keySelector == null)
                {
                    var type = typeof(TResource);
                    var argParam = Expression.Parameter(type, "s");
                    var properties = typeof(TResource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    var keyPropertyName =
                        (properties.FirstOrDefault(t =>
                             t.CustomAttributes.Any(a => a.AttributeType == typeof(KeyAttribute))) ??
                         properties.FirstOrDefault(t => t.Name.Equals("Id")))?.Name;
                    if (string.IsNullOrEmpty(keyPropertyName))
                    {
                        throw new ArgumentException(
                            $"There must be a property with KeyAttribute in type: {type.FullName}");
                    }

                    var nameProperty = Expression.Property(argParam, keyPropertyName);
                    var returnValue = Expression.TypeAs(nameProperty, typeof(object));

                    var lambda = Expression.Lambda<Func<TResource, object>>(returnValue, argParam);
                    _keySelector = lambda.Compile();
                }

                return _keySelector;
            }
        }
    }
}