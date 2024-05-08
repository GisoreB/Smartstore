﻿using DotLiquid;

namespace Smartstore.Templating.Liquid
{
    internal class TestDrop : ITestModel, ILiquidizable, IIndexable, ISafeObject
    {
        private readonly BaseEntity _entity;
        private readonly Type _type;
        private readonly string _modelPrefix;

        public TestDrop(BaseEntity entity, string modelPrefix)
        {
            _entity = entity;
            _type = entity.GetType();

            if (modelPrefix.HasValue())
            {
                _modelPrefix = modelPrefix.EnsureEndsWith('.');
            }

            _modelPrefix ??= string.Empty;
        }

        public string ModelName
            => _type.Name;

        public object GetWrappedObject()
            => _entity;

        public bool ContainsKey(object key)
            => true;

        public object this[object key]
        {
            get
            {
                object value = null;

                if (key is string name)
                {
                    var modelPrefix = _modelPrefix + name;
                    var pi = _type.GetProperty(name);

                    if (pi == null)
                    {
                        value = "{{ " + modelPrefix + " }}";
                    }
                    else if (pi.PropertyType.IsBasicOrNullableType())
                    {
                        value = pi.GetValue(_entity) ?? "{{ " + modelPrefix + " }}";
                    }
                    else if (pi.PropertyType.IsSequenceType(out var elementType))
                    {
                        if (typeof(BaseEntity).IsAssignableFrom(elementType))
                        {
                            value = new List<TestDrop>
                            {
                                new TestDrop((BaseEntity)Activator.CreateInstance(elementType), "it"),
                                new TestDrop((BaseEntity)Activator.CreateInstance(elementType), "it")
                            };
                        }
                    }
                    else if (typeof(BaseEntity).IsAssignableFrom(pi.PropertyType))
                    {
                        value = new TestDrop((BaseEntity)Activator.CreateInstance(pi.PropertyType), modelPrefix);
                    }
                }

                return value;
            }
        }

        public object ToLiquid()
            => this;
    }
}
