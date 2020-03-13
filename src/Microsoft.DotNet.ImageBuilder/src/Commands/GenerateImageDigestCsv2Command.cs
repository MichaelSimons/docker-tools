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
    public class GenerateImageDigestCsv2Command : ManifestCommand<GenerateImageDigestCsv2Options>
    {
        public GenerateImageDigestCsv2Command() : base()
        {
        }

        public override Task ExecuteAsync()
        {
            string[] imageList = File.ReadAllLines(Options.ImageListPath);
            Logger.WriteMessage(imageList.Length.ToString());

            string info = imageList
                .AsParallel()
                .Select(image => image.Trim().TrimEnd(',').Trim('"'))
                .Select(image => GetImageInfoAsync(image))
                .Where(info => info != null)
                .Aggregate((working, next) => $"{working}{next}");

            if (!Options.IsDryRun)
            {
                File.AppendAllText(Options.CsvPath, info);
            }

            TrimCsv();

            return Task.CompletedTask;
        }

        private void TrimCsv()
        {
            string[] info = File.ReadAllLines(Options.CsvPath);
            Dictionary<string, string> infoDict = new Dictionary<string, string>();
            foreach (string i in info)
            {
                string[] parts = i.Split(',');
                string key = $"{parts[parts.Length - 1]}:{parts[0]}";  // Proper CSV parser is need to handler os version which contains quoted commas
                //Logger.WriteMessage(key);
                if (!infoDict.ContainsKey(key))
                {
                    infoDict.Add(key,i);
                    //Logger.WriteMessage("Not Found");
                }
                else
                {
                    //Logger.WriteMessage("Found");
                }
            }

            string trimmedInfo = infoDict.Values.Aggregate((working, next) => $"{working}{Environment.NewLine}{next}");
            trimmedInfo += Environment.NewLine;
            File.WriteAllText(Options.CsvPath, trimmedInfo);
        }

        private string GetImageInfoAsync(string image)
        {
            string[] imageParts = image.Split('-');

            if (imageParts.Length != 4)
            {
                return null;
            }

            string manifest = DockerHelper.GetManifest($"{Options.Repo}:{image}", Options.IsDryRun);
            Dictionary<object, object> manifestYaml = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Deserialize<Dictionary<object, object>>(manifest);

            string digest = (string)((Dictionary<object, object>)manifestYaml["Descriptor"])["digest"];
            string arch = "amd64";
            string os = "windows";
            string osVariant = GetOSDisplayName($"{imageParts[2]}-{imageParts[3]}");
            string productVersion = imageParts[0].Contains('.') ? imageParts[0] : imageParts[1];
            if (!productVersion.Contains('.'))
            {
                throw new ArgumentException("image");
            }
            string repo = Options.Repo.Substring(18);

            string info = $"{digest},{arch},{os},\"{osVariant}\",{productVersion},,{repo}{Environment.NewLine}";
            info += $"{image},{arch},{os},\"{osVariant}\",{productVersion},,{repo}{Environment.NewLine}";

            return info;
        }

        private string GetOSDisplayName(string os)
        {
            string displayName;
            if (os.Contains("2016"))
            {
                displayName = "Windows Server 2016";
            }
            else if (os.Contains("2019") || os.Contains("1809"))
            {
                displayName = "Windows Server 2019";
            }
            else
            {
                string version = os.Split('-')[1];
                displayName = $"Windows Server, version {version}";
            }

            return displayName;
        }
    }
}