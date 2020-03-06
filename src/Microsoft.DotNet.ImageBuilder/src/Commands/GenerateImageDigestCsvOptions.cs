// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;

namespace Microsoft.DotNet.ImageBuilder.Commands
{
    public class GenerateImageDigestCsvOptions : ManifestOptions, IFilterableOptions
    {

        protected override string CommandHelp => "Generates a CSV with image digest and assorted info";

        public ManifestFilterOptions FilterOptions { get; } = new ManifestFilterOptions();

        public string CsvPath { get; set; }

        public GenerateImageDigestCsvOptions() : base()
        {
        }

        public override void DefineParameters(ArgumentSyntax syntax)
        {
            base.DefineParameters(syntax);

            string csvPath = null;
            syntax.DefineParameter(
                "path",
                ref csvPath,
                "Path to the CSV file to write to");
            CsvPath = csvPath;
        }

        public override void DefineOptions(ArgumentSyntax syntax)
        {
            base.DefineOptions(syntax);

            FilterOptions.DefineOptions(syntax);
        }
    }
}