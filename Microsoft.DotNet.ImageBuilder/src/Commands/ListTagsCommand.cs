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
            foreach (RepoInfo repo in Manifest.Repos)
            {
                var variantGroups = repo.Images
                    .Select(image => new {
                            Variant = image.Platforms.First().DockerfilePath.Split('/')[1],
                            Tags = image.SharedTags.Concat(image.Platforms.SelectMany(platform => platform.Tags))
                        })
                    .GroupBy(info => info.Variant);

                foreach (var variantGroup in variantGroups)
                {
                    Logger.WriteSubheading($"{variantGroup.Key}:");
                    string tags = variantGroup.SelectMany(info => info.Tags)
                        .Select(tag => $"    - source: {tag.Name}{Environment.NewLine}      target: {tag.Name}")
                        .Aggregate((working, next) => $"{working}{Environment.NewLine}{next}");
                    Logger.WriteMessage(tags);
                }
            }

            return Task.CompletedTask;
        }
    }
}
