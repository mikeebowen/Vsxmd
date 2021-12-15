//-----------------------------------------------------------------------
// <copyright file="Converter.cs" company="Junle Li">
//     Copyright (c) Junle Li. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Vsxmd
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Vsxmd.Units;

    /// <inheritdoc/>
    public class Converter : IConverter
    {
        private readonly XDocument document;

        /// <summary>
        /// Initializes a new instance of the <see cref="Converter"/> class.
        /// </summary>
        /// <param name="document">The XML document.</param>
        public Converter(XDocument document)
        {
            this.document = document;
        }

        /// <summary>
        /// Convert VS XML document to Markdown syntax.
        /// </summary>
        /// <param name="document">The XML document.</param>
        /// <returns>The generated Markdown content.</returns>
        public static IEnumerable<string> ToMarkdown(XDocument document) =>
            new Converter(document).ToMarkdown();

        /// <inheritdoc/>
        public IEnumerable<string> ToMarkdown() =>
            ToUnits(this.document.Root);
                //.Select(x => x.ToMarkdown().Join("\n\n").Suffix("\n"));
                //.Select(x => x.ToMarkdown().Join("\n\n").Suffix("\n"));
                //.Join("\n\n")
                //.Suffix("\n");

        private static IEnumerable<string> ToUnits(XElement docElement)
        {
            // assembly unit
            var assemblyUnit = new AssemblyUnit(docElement.Element("assembly"));

            // member units
            var memberUnits = docElement
                .Element("members")
                .Elements("member")
                .Select(element => new MemberUnit(element))
                .Where(member => member.Kind != MemberKind.NotSupported)
                .GroupBy(unit => unit.TypeName)
                .Select(MemberUnit.ComplementType)
                .SelectMany(group => group)
                .OrderBy(member => member, MemberUnit.Comparer);

            // table of contents
            var tableOfContents = new TableOfContents(memberUnits);

            //return new IUnit[] { assemblyUnit }
            //    .Concat(new[] { tableOfContents }),
            //    .Concat(memberUnits);
            //return memberUnits;

            Dictionary<string, List<MemberUnit>> units = new Dictionary<string, List<MemberUnit>>();

            foreach (MemberUnit memberUnit in memberUnits)
            {
                if (units.ContainsKey(memberUnit.TypeName))
                {
                    units[memberUnit.TypeName].Add(memberUnit);
                }
                else
                {
                    units[memberUnit.TypeName] = new List<MemberUnit>();
                    units[memberUnit.TypeName].Add(memberUnit);
                }
            }

            List<string> mds = new List<string>();

            foreach (KeyValuePair<string, List<MemberUnit>> unit in units)
            {
                var foo = unit.Value.Select(x => x.ToMarkdown());
                string md = unit.Value.Select(x => x.ToMarkdown().Join("\n\n").Suffix("\n")).Join("\n");

                mds.Add(md);
            }

            return mds;
        }
    }
}
