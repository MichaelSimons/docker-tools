// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.ImageBuilder.ViewModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.DotNet.ImageBuilder.Commands
{
    [Export(typeof(ICommand))]
    public class GenerateImageDigestCsvCommand : ManifestCommand<GenerateImageDigestCsvOptions>
    {
        public GenerateImageDigestCsvCommand() : base()
        {
        }

        public override Task ExecuteAsync()
        {
            string info = Manifest.GetFilteredPlatforms()
                .AsParallel()
                .Select(platform => GetImageInfoAsync(platform))
                .Aggregate((working, next) => $"{working}{next}");

            if (!Options.IsDryRun)
            {
                File.AppendAllText(Options.CsvPath, info);
            }

            return Task.CompletedTask;
        }

        private string GetImageInfoAsync(PlatformInfo platform)
        {
            string manifest = DockerHelper.GetManifest(platform.Tags.First().FullyQualifiedName, Options.IsDryRun);
            Dictionary<object, object> manifestYaml = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Deserialize<Dictionary<object, object>>(manifest);

            string digest = (string)((Dictionary<object, object>)manifestYaml["Descriptor"])["digest"];
            string arch = platform.Model.Architecture.GetDisplayName();
            string os = platform.Model.OS.GetDockerName();
            string osVariant = McrTagsMetadataGenerator.GetOSDisplayName(platform);
            string productVersion = platform.Tags.First().FullyQualifiedName.Split(":")[1].Split("-")[0];
            string repo = platform.RepoName.Substring(18);
            string buildTimestamp = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");

            string info = $"{digest},{arch},{os},\"{osVariant}\",{productVersion},{platform.DockerfilePathRelativeToManifest},{repo},{buildTimestamp}{Environment.NewLine}";

            foreach (TagInfo tag in platform.Tags)
            {
                info += $"{tag.Name},{arch},{os},\"{osVariant}\",{productVersion},{platform.DockerfilePathRelativeToManifest},{repo},{buildTimestamp}{Environment.NewLine}";
            }

            return info;
        }
    }
}