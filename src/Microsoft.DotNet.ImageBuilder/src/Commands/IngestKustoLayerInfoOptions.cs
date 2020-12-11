// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;

namespace Microsoft.DotNet.ImageBuilder.Commands
{
    public class IngestKustoLayerInfoOptions : KustoOptions
    {
        protected override string CommandHelp => "Ingests layer info data into Kusto";

        public string DataFile { get; set; }

        public override void DefineOptions(ArgumentSyntax syntax)
        {
            base.DefineOptions(syntax);

            string dataFile = null;
            syntax.DefineOption(
                "dataFile",
                ref dataFile,
                "Image data file");
            DataFile = dataFile;
        }
    }
}
