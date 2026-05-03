using System.Collections.Generic;

namespace GUIshka
{
    public sealed class SymbolEntry
    {
        public string Name { get; set; }
        public int DeclLine { get; set; }
        public int DeclStartPos { get; set; }
        public double Real { get; set; }
        public double Imag { get; set; }
    }

    public sealed class SymbolTable
    {
        private readonly Dictionary<string, SymbolEntry> _symbols =
            new Dictionary<string, SymbolEntry>(System.StringComparer.Ordinal);

        public bool TryDeclare(SymbolEntry entry, out SymbolEntry existing)
        {
            existing = null;
            if (entry == null || string.IsNullOrEmpty(entry.Name))
            {
                return false;
            }

            if (_symbols.TryGetValue(entry.Name, out existing))
            {
                return false;
            }

            _symbols[entry.Name] = entry;
            return true;
        }

        public bool TryGet(string name, out SymbolEntry entry)
        {
            return _symbols.TryGetValue(name, out entry);
        }
    }
}
