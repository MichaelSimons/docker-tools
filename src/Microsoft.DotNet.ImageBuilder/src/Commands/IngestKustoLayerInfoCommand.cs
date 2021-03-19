// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DotNet.ImageBuilder.Models.Docker;
using Microsoft.DotNet.ImageBuilder.Models.Image;
using Microsoft.DotNet.ImageBuilder.Services;
using Microsoft.DotNet.ImageBuilder.ViewModel;

namespace Microsoft.DotNet.ImageBuilder.Commands
{
    [Export(typeof(ICommand))]
    public class IngestKustoLayerInfoCommand : ManifestCommand<IngestKustoLayerInfoOptions, IngestKustoLayerInfoOptionsBuilder>
    {
        protected override string Description => "Ingests image layer data into Kusto";

        private readonly IKustoClient _kustoClient;
        private readonly ILoggerService _loggerService;

        [ImportingConstructor]
        public IngestKustoLayerInfoCommand(ILoggerService loggerService, IKustoClient kustoClient)
        {
            _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
            _kustoClient = kustoClient ?? throw new ArgumentNullException(nameof(kustoClient));
        }

        public override async Task ExecuteAsync()
        {
            _loggerService.WriteHeading("INGESTING LAYER INFO DATA INTO KUSTO");

            string[] imageData = await File.ReadAllLinesAsync(Options.DataFile);

            List<string> failedData = new List<string>();
            //string image = imageData[1];
            int pageLength = 500;
            int pageCount = (imageData.Length + pageLength - 1) / pageLength;
            for (int i = 0; i < pageCount; i++)
            {
                _loggerService.WriteMessage($"Processing Page: {i}");

                object lockObject = new object();
                string layerData = string.Empty;

                Parallel.ForEach(
                    imageData.Skip(1 + pageLength * i).Take(pageLength),
                    () => string.Empty,
                    (image, loopstate, localData) =>
                    {
                        string[] imageInfo = image.Split('\t');
                        try{
                            Manifest manifest = DockerHelper.InspectManifest($"mcr.microsoft.com/{imageInfo[6]}@{imageInfo[0]}", Options.IsDryRun);
                            int layerCount = manifest.SchemaV2Manifest.Layers.Count();
                            for (int i = 0 ; i < layerCount; i ++)
                            {
                                Descriptor descriptor = manifest.SchemaV2Manifest.Layers.ElementAt(i);
                                localData += $"\"{descriptor.Digest}\",\"{descriptor.Size}\",\"{layerCount-i}\",\"{imageInfo[0]}\",\"{imageInfo[1]}\",\"{imageInfo[2]}\",\"{imageInfo[3].Replace("\"", "")}\",\"{imageInfo[4]}\",\"{imageInfo[5]}\",\"{imageInfo[6]}\",\"{imageInfo[7]}\"{Environment.NewLine}";
                            }
                        }
                        catch (Exception)
                        {
                            lock (failedData)
                            {
                                failedData.Add(image);
                            }
                        }
                        return localData;
                    },
                    (localData) =>
                    {
                        lock (lockObject)
                        {
                            layerData += localData;
                        }
                    }
                );

                _loggerService.WriteMessage($"Layer Info to Ingest:{Environment.NewLine}{layerData}");

                if (string.IsNullOrEmpty(layerData))
                {
                    _loggerService.WriteMessage("Skipping ingestion due to empty layer info data.");
                    break;
                }

                using MemoryStream stream = new MemoryStream();
                using StreamWriter writer = new StreamWriter(stream);
                writer.Write(layerData);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                if (!Options.IsDryRun)
                {
                    await _kustoClient.IngestFromCsvStreamAsync(
                        stream, Options.ServicePrincipal, Options.Cluster, Options.Database, Options.LayerTable, Options.IsDryRun);
                }
            }

            _loggerService.WriteMessage($"Failures:{Environment.NewLine}{string.Join(Environment.NewLine, failedData)}");
        }
    }
}
