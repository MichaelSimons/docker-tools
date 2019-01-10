// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.DotNet.ImageBuilder.Commands
{
    public class ListTagsOptions : Options
    {
        protected override string CommandHelp => "List the full set of tags";
        protected override string CommandName => "listTags";

        public ListTagsOptions() : base()
        {
        }
    }
}