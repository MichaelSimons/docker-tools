// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using Microsoft.DotNet.ImageBuilder.Model;
using Microsoft.DotNet.ImageBuilder.ViewModel;

namespace Microsoft.DotNet.ImageBuilder.Commands
{
    public class CopyAcrImagesOptions : Options, IManifestFilterOptions
    {
        protected override string CommandHelp => "Copies the platform images as specified in the manifest between repositories of an ACR";
        protected override string CommandName => "copyAcrImages";

        public string Architecture { get; set; }
        public string Password { get; set; }
        public IEnumerable<string> Paths { get; set; }
        public string OsType { get; set; }
        public string OsVersion { get; set; }
        public string SourceRepoPrefix { get; set; }
        public string Subscription { get; set; }
        public string Tenant { get; set; }
        public string Username { get; set; }

        public CopyAcrImagesOptions() : base()
        {
        }

        public override void ParseCommandLine(ArgumentSyntax syntax)
        {
            base.ParseCommandLine(syntax);

            DefineManifestFilterOptions(syntax, this);

            string sourceRepoPrefix = null;
            syntax.DefineParameter("source-repo-prefix", ref sourceRepoPrefix, "Prefix of the source ACR repository to copy images from");
            SourceRepoPrefix = sourceRepoPrefix;

            string username = null;
            syntax.DefineParameter("username", ref username, "The URL or name associated with the service principal to use");
            Username = username;

            string password = null;
            syntax.DefineParameter("password", ref password, "The service principal password or the X509 certificate to use");
            Password = password;

            string tenant = null;
            syntax.DefineParameter("tenant", ref tenant, "The tenant associated with the service principal to use");
            Tenant = tenant;

            string subscription = null;
            syntax.DefineParameter("subscription", ref subscription, "Azure subscription to operate on");
            Subscription = subscription;
        }
    }
}
