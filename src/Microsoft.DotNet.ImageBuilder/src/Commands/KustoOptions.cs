// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;

namespace Microsoft.DotNet.ImageBuilder.Commands
{
    public abstract class KustoOptions : ImageInfoOptions, IFilterableOptions
    {
        public ManifestFilterOptions FilterOptions { get; } = new ManifestFilterOptions();

        public string Cluster { get; set; }
        public string Database { get; set; }
        public ServicePrincipalOptions ServicePrincipal { get; } = new ServicePrincipalOptions();
        public string Table { get; set; }

        protected KustoOptions()
        {
        }

        public override void DefineOptions(ArgumentSyntax syntax)
        {
            base.DefineOptions(syntax);

            FilterOptions.DefineOptions(syntax);
        }

        public override void DefineParameters(ArgumentSyntax syntax)
        {
            base.DefineParameters(syntax);

            string cluster = null;
            syntax.DefineParameter("cluster", ref cluster, "The cluster to ingest the data to");
            Cluster = cluster;

            string database = null;
            syntax.DefineParameter("database", ref database, "The database to ingest the data to");
            Database = database;

            string table = null;
            syntax.DefineParameter("table", ref table, "The table to ingest the data to");
            Table = table;

            ServicePrincipal.DefineParameters(syntax);
        }
    }
}
