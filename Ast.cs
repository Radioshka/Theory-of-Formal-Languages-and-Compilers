using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GUIshka
{
    public abstract class AstNode
    {
        public abstract string NodeKind { get; }
    }

    public sealed class ProgramNode : AstNode
    {
        public List<ComplexDeclNode> Statements { get; } = new List<ComplexDeclNode>();

        public override string NodeKind => "ProgramNode";
    }

    public sealed class ComplexDeclNode : AstNode
    {
        public Lexeme NameLexeme { get; }
        public OperandNode RealPart { get; }
        public OperandNode ImagPart { get; }

        public ComplexDeclNode(Lexeme nameLexeme, OperandNode realPart, OperandNode imagPart)
        {
            NameLexeme = nameLexeme ?? throw new ArgumentNullException(nameof(nameLexeme));
            RealPart = realPart ?? throw new ArgumentNullException(nameof(realPart));
            ImagPart = imagPart ?? throw new ArgumentNullException(nameof(imagPart));
        }

        public string Name => NameLexeme.Value ?? string.Empty;

        public string Keyword => "complex";

        public override string NodeKind => "ComplexDeclNode";
    }

    public abstract class OperandNode : AstNode
    {
    }

    public sealed class LiteralNode : OperandNode
    {
        public IReadOnlyList<Lexeme> Lexemes { get; }

        public LiteralNode(IList<Lexeme> lexemes)
        {
            if (lexemes == null || lexemes.Count == 0)
            {
                throw new ArgumentException("Literal requires at least one lexeme.");
            }

            Lexemes = new List<Lexeme>(lexemes);
        }

        public int Line => Lexemes[0].Line;

        public int StartPos => Lexemes[0].StartPos;

        public string GetRawText()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Lexemes.Count; i++)
            {
                sb.Append(Lexemes[i].Value ?? string.Empty);
            }

            return sb.ToString();
        }

        public string GetLiteralType()
        {
            Lexeme numberLexeme = Lexemes.LastOrDefault(l =>
                l != null &&
                (l.Code == 1 || l.Code == 2 || l.Code == 3 || l.Code == 4));

            if (numberLexeme == null)
            {
                return "unknown";
            }

            if (numberLexeme.Code == 1 || numberLexeme.Code == 2)
            {
                return "integer";
            }

            if (numberLexeme.Code == 3 || numberLexeme.Code == 4)
            {
                return "float";
            }

            return "unknown";
        }

        public override string NodeKind => "LiteralNode";
    }

    public sealed class IdentifierRefNode : OperandNode
    {
        public Lexeme IdentifierLexeme { get; }

        public IdentifierRefNode(Lexeme identifierLexeme)
        {
            IdentifierLexeme = identifierLexeme ?? throw new ArgumentNullException(nameof(identifierLexeme));
        }

        public string Name => IdentifierLexeme.Value ?? string.Empty;

        public override string NodeKind => "IdentifierRefNode";
    }

    public static class AstPrinter
    {
        public static string ToTreeText(ProgramNode program)
        {
            if (program == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.AppendLine(program.NodeKind);

            int count = program.Statements.Count;

            for (int i = 0; i < count; i++)
            {
                bool isLast = i == count - 1;
                AppendComplexDecl(sb, program.Statements[i], string.Empty, isLast);
            }

            return sb.ToString().TrimEnd();
        }

        private static void AppendComplexDecl(
            StringBuilder sb,
            ComplexDeclNode node,
            string indent,
            bool isLast)
        {
            string branch = isLast ? "└── " : "├── ";
            string childIndent = indent + (isLast ? "    " : "│   ");

            sb.Append(indent);
            sb.Append(branch);
            sb.AppendLine(node.NodeKind);

            AppendProperty(sb, childIndent, "name", Escape(node.Name), false);
            AppendProperty(sb, childIndent, "keyword", Escape(node.Keyword), false);

            AppendOperand(
                sb,
                node.RealPart,
                childIndent,
                "realPart",
                "действительная часть комплексного числа",
                false);

            AppendOperand(
                sb,
                node.ImagPart,
                childIndent,
                "imagPart",
                "мнимая часть комплексного числа",
                true);
        }

        private static void AppendOperand(
            StringBuilder sb,
            OperandNode operand,
            string indent,
            string propertyName,
            string semanticRole,
            bool isLast)
        {
            string branch = isLast ? "└── " : "├── ";
            string childIndent = indent + (isLast ? "    " : "│   ");

            sb.Append(indent);
            sb.Append(branch);
            sb.Append(propertyName);
            sb.Append(": ");

            if (operand is LiteralNode literal)
            {
                sb.AppendLine(literal.NodeKind);

                AppendProperty(sb, childIndent, "rawValue", Escape(literal.GetRawText()), false);
                AppendProperty(sb, childIndent, "literalType", Escape(literal.GetLiteralType()), false);
                AppendProperty(sb, childIndent, "semanticRole", Escape(semanticRole), true);

                return;
            }

            if (operand is IdentifierRefNode identifierRef)
            {
                sb.AppendLine(identifierRef.NodeKind);

                AppendProperty(sb, childIndent, "name", Escape(identifierRef.Name), false);
                AppendProperty(sb, childIndent, "semanticRole", Escape("ссылка на ранее объявленное значение"), true);

                return;
            }

            sb.AppendLine("UnknownOperandNode");
        }

        private static void AppendProperty(
            StringBuilder sb,
            string indent,
            string propertyName,
            string value,
            bool isLast)
        {
            string branch = isLast ? "└── " : "├── ";

            sb.Append(indent);
            sb.Append(branch);
            sb.Append(propertyName);
            sb.Append(": ");
            sb.AppendLine(value);
        }

        private static string Escape(string value)
        {
            if (value == null)
            {
                return "\"\"";
            }

            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }
    }

    public static class AstJson
    {
        public static string ToJson(ProgramNode program)
        {
            if (program == null)
            {
                return "null";
            }

            var sb = new StringBuilder();

            sb.Append("{\"kind\":\"ProgramNode\",\"statements\":[");

            for (int i = 0; i < program.Statements.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                AppendComplexDecl(sb, program.Statements[i]);
            }

            sb.Append("]}");

            return sb.ToString();
        }

        private static void AppendComplexDecl(StringBuilder sb, ComplexDeclNode node)
        {
            sb.Append("{\"kind\":\"ComplexDeclNode\"");

            sb.Append(",\"name\":");
            AppendString(sb, node.Name);

            sb.Append(",\"keyword\":");
            AppendString(sb, node.Keyword);

            sb.Append(",\"realPart\":");
            AppendOperand(sb, node.RealPart, "действительная часть комплексного числа");

            sb.Append(",\"imagPart\":");
            AppendOperand(sb, node.ImagPart, "мнимая часть комплексного числа");

            sb.Append("}");
        }

        private static void AppendOperand(StringBuilder sb, OperandNode operand, string semanticRole)
        {
            if (operand is LiteralNode literal)
            {
                sb.Append("{\"kind\":\"LiteralNode\"");

                sb.Append(",\"rawValue\":");
                AppendString(sb, literal.GetRawText());

                sb.Append(",\"literalType\":");
                AppendString(sb, literal.GetLiteralType());

                sb.Append(",\"semanticRole\":");
                AppendString(sb, semanticRole);

                sb.Append(",\"line\":");
                sb.Append(literal.Line.ToString(CultureInfo.InvariantCulture));

                sb.Append(",\"position\":");
                sb.Append(literal.StartPos.ToString(CultureInfo.InvariantCulture));

                sb.Append("}");

                return;
            }

            if (operand is IdentifierRefNode identifierRef)
            {
                sb.Append("{\"kind\":\"IdentifierRefNode\"");

                sb.Append(",\"name\":");
                AppendString(sb, identifierRef.Name);

                sb.Append(",\"semanticRole\":");
                AppendString(sb, "ссылка на ранее объявленное значение");

                sb.Append(",\"line\":");
                sb.Append(identifierRef.IdentifierLexeme.Line.ToString(CultureInfo.InvariantCulture));

                sb.Append(",\"position\":");
                sb.Append(identifierRef.IdentifierLexeme.StartPos.ToString(CultureInfo.InvariantCulture));

                sb.Append("}");

                return;
            }

            sb.Append("{\"kind\":\"UnknownOperandNode\"}");
        }

        private static void AppendString(StringBuilder sb, string value)
        {
            sb.Append('"');

            if (value != null)
            {
                foreach (char c in value)
                {
                    switch (c)
                    {
                        case '\\':
                            sb.Append("\\\\");
                            break;

                        case '"':
                            sb.Append("\\\"");
                            break;

                        case '\n':
                            sb.Append("\\n");
                            break;

                        case '\r':
                            sb.Append("\\r");
                            break;

                        case '\t':
                            sb.Append("\\t");
                            break;

                        default:
                            sb.Append(c);
                            break;
                    }
                }
            }

            sb.Append('"');
        }
    }
}