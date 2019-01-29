// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.ImageBuilder.ViewModel;

namespace Microsoft.DotNet.ImageBuilder.Commands
{
    public class ListTagsCommand : Command<ListTagsOptions>
    {
        public ListTagsCommand() : base()
        {
        }

        public override Task ExecuteAsync()
        {
            Logger.WriteHeading("GENERATING TAGS LIST");
            foreach (RepoInfo repo in Manifest.FilteredRepos)
            {
                var variantGroups = repo.AllImages
                    .Select(image => new
                    {
                        Variant = image.AllPlatforms.First().DockerfilePath.Split('/')[1],
                        Tags = image.SharedTags.Concat(image.AllPlatforms.SelectMany(platform => platform.Tags))
                    })
                    .GroupBy(info => info.Variant);

                foreach (var variantGroup in variantGroups)
                {
                    Logger.WriteSubheading($"{variantGroup.Key}:");
                    string tags = variantGroup.SelectMany(info => info.Tags)
                        .Select(tag => GetTagYaml(tag))
                        .Aggregate((working, next) => $"{working}{Environment.NewLine}{next}");
                    Logger.WriteMessage(tags);
                }
            }

            return Task.CompletedTask;
        }

        private string GetTagYaml(TagInfo tag)
        {
            string sourceTag = tag.Model.IsUndocumented ? "<UNDOCUMENTED>" : tag.Name;
            string[] variants = new string [] { "runtime-deps", "aspnetcore-runtime", "runtime", "sdk" };
            foreach (string variant in variants)
            {
                int index = sourceTag.IndexOf(variant);
                if (index != -1)
                {
                    sourceTag = sourceTag.Remove(index, variant.Length);
                }

                index = sourceTag.IndexOf("--");
                if (index != -1)
                {
                    sourceTag = sourceTag.Remove(index, 1);
                }

                sourceTag = sourceTag.TrimEnd('-');
            }

            if (sourceTag == string.Empty)
            {
                sourceTag = "<TODO>";
            }

            return $"    - source: {sourceTag}{Environment.NewLine}      target: {tag.Name}";
        }
    }
}