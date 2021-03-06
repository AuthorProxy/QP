using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.FileSystemReaders
{
    internal static class XmlReaderProcessor
    {
        internal static string Process(IList<string> filePathes, string configPath, XmlSettingsModel settingsTemp = null)
        {
            Program.Logger.Debug($"Begin parsing documents: {filePathes.ToJsonLog()} with next config: {configPath ?? "<default>"}");
            var orderedFilePathes = new List<string>();
            foreach (var path in filePathes)
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    throw new FileNotFoundException($"Неправильно указан путь к файлам записанных действий: {path}");
                }

                if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    orderedFilePathes.AddRange(GetOrderedDirectoryFilePathes(path, configPath));
                }
                else
                {
                    orderedFilePathes.Add(path);
                }
            }

            Program.Logger.Info($"Total files readed from disk: {orderedFilePathes.Count}.");
            Program.Logger.Debug($"Documents will be processed in next order: {orderedFilePathes.ToJsonLog()}.");

            return CombineMultipleDocumentsWithSameRoot(orderedFilePathes.Select(XDocument.Load).ToList()).ToNormalizedString(SaveOptions.DisableFormatting);
        }

        private static IEnumerable<string> GetOrderedDirectoryFilePathes(string absDirPath, string absOrRelativeConfigPath)
        {
            var config = string.IsNullOrWhiteSpace(absOrRelativeConfigPath)
                ? GetDefaultXmlReaderSettings(absDirPath)
                : GetXmlReaderSettings(absDirPath, absOrRelativeConfigPath);

            return config.FilePathes;
        }

        private static XmlReaderSettings GetDefaultXmlReaderSettings(string absDirPath)
        {
            return new XmlReaderSettings(Directory.EnumerateFiles(absDirPath, "*.xml", SearchOption.TopDirectoryOnly).OrderBy(fn => fn).ToList());
        }

        private static XmlReaderSettings GetXmlReaderSettings(string absDirPath, string absOrRelativeConfigPath)
        {
            var configPath = absOrRelativeConfigPath;
            if (!File.Exists(configPath))
            {
                configPath = Path.Combine(absDirPath, absOrRelativeConfigPath);
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException("Неправильно указан путь к файлу конфигурации: " + configPath);
                }
            }

            var xmlData = XDocument.Load(configPath);
            var actionNodes = xmlData.Root?.Elements(XmlReaderSettings.ConfigElementNodeName).Where(el => el.NodeType != XmlNodeType.Comment);
            var filePathes = (actionNodes ?? throw new InvalidOperationException())
                .Select(node => Path.Combine(absDirPath, node.Attribute(XmlReaderSettings.ConfigElementPathAttribute)?.Value ?? throw new InvalidOperationException()))
                .ToList();

            return new XmlReaderSettings(filePathes);
        }

        private static XDocument CombineMultipleDocumentsWithSameRoot(IList<XDocument> xmlDocuments)
        {
            if (!xmlDocuments.Any())
            {
                Program.Logger.Warn("There are no xml files for replay or all of them already used before.");
                ConsoleHelpers.ExitProgram(ExitCode.Success);
            }

            if (xmlDocuments.Count == 1)
            {
                return xmlDocuments.Single();
            }

            var root = new XDocument(xmlDocuments[0].Root);
            root.Root?.RemoveNodes();

            return xmlDocuments.Aggregate(root, (result, xd) =>
            {
                result.Root?.Add(xd.Root?.Elements());
                return result;
            });
        }
    }
}
