
using System.Collections.Generic;

namespace ReMime.ContentResolvers
{
    public interface IMagicValueResolver
    {
        void AddMagicValue(MagicValueMediaType value);

        void AddMagicValues(IEnumerable<MagicValueMediaType> values)
        {
            foreach (MagicValueMediaType value in values)
            {
                AddMagicValue(value);
            }
        }
    }
}
