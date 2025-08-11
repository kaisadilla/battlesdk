using battlesdk.data.definitions;
using System.Collections.Immutable;

namespace battlesdk.data;
public class BagData {
    public ImmutableList<string> SectionNames { get; }
    public ImmutableDictionary<string, int> SectionIndices { get; }

    public BagData (BagDefinition def) {
        Dictionary<string, int> indices = [];

        SectionNames = [.. def.Sections];

        for (int i = 0; i < SectionNames.Count; i++) {
            indices[SectionNames[i]] = i;
        }

        SectionIndices = indices.ToImmutableDictionary();
    }
}
