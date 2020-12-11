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
    public class IngestKustoLayerInfoCommand : ManifestCommand<IngestKustoLayerInfoOptions>
    {
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
            int pageLength = 100;
            int pageCount = (imageData.Length + pageLength - 1) / pageLength;
            for (int i = 0; i < pageCount; i++)
            {
                _loggerService.WriteMessage($"Processing Page: {i}");

                string layerData = string.Empty;

                Parallel.ForEach(imageData.Skip(1 + pageLength * i).Take(pageLength), image =>
                //foreach (string image in imageData.Skip(1))
                {
                    string[] imageInfo = image.Split('\t');
                    try{
                        Manifest manifest = DockerHelper.InspectManifest($"mcr.microsoft.com/{imageInfo[6]}@{imageInfo[0]}", Options.IsDryRun);
                        lock (layerData)
                        {
                            layerData += $"\"{manifest.SchemaV2Manifest.Layers.Last().Digest}\",\"{imageInfo[1]}\",\"{imageInfo[2]}\",\"{imageInfo[3].Replace("\"", "")}\",\"{imageInfo[4]}\",\"{imageInfo[5]}\",\"{imageInfo[6]}\",\"{imageInfo[7]}\"{Environment.NewLine}";
                            //_loggerService.WriteMessage(layerData);
                        }
                    }
                    catch (Exception)
                    {
                        failedData.Add(image);
                    }
                }
                );

                _loggerService.WriteMessage($"Layer Info to Ingest:{Environment.NewLine}{layerData}");

                if (string.IsNullOrEmpty(layerData))
                {
                    _loggerService.WriteMessage("Skipping ingestion due to empty layer info data.");
                    return;
                }

                using MemoryStream stream = new MemoryStream();
                using StreamWriter writer = new StreamWriter(stream);
                writer.Write(layerData);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                if (!Options.IsDryRun)
                {
                    await _kustoClient.IngestFromCsvStreamAsync(stream, Options);
                }
            }

            _loggerService.WriteMessage($"Failures:{Environment.NewLine}{string.Join(Environment.NewLine, failedData)}");
        }
    }
}
