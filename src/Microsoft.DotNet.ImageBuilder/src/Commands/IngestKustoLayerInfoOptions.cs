// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using static Microsoft.DotNet.ImageBuilder.Commands.CliHelper;

namespace Microsoft.DotNet.ImageBuilder.Commands
{
    public class IngestKustoLayerInfoOptions : IngestKustoImageInfoOptions
    {
        public string DataFile { get; set; } = string.Empty;
    }

    public class IngestKustoLayerInfoOptionsBuilder : IngestKustoImageInfoOptionsBuilder
    {
        public override IEnumerable<Option> GetCliOptions() =>
            base.GetCliOptions()
            .Concat(new Option[]
                {
                    CreateOption<string>("dataFile", nameof(IngestKustoLayerInfoOptions.DataFile),
                        "Image data file"),
                });

        public override IEnumerable<Argument> GetCliArguments() => base.GetCliArguments();
    }
}
