using PortalDoFranqueado.Model.Entities;
using PortalDoFranqueado.ViewModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Export
{
    public static class Exporter
    {
        public static async Task ExportToExcel(Purchase purchase, string fullAddress, ILegendable? legendable = null)
        {
            var tmpFile = string.Concat(Path.GetTempFileName(), ".json");
            var jsonSerialized = JsonSerializer.Serialize(purchase);
            await File.WriteAllTextAsync(tmpFile, jsonSerialized);

            var exportDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Export");

            var psi = new ProcessStartInfo()
            {
                Arguments = $"\"{tmpFile}\" \"{fullAddress}\"",
                CreateNoWindow = true,
                FileName = Path.Combine(exportDirectory, "PortalDoFranqueado.Export.exe"),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = exportDirectory
            };
            var process = Process.Start(psi);

            if (process == null)
                throw new Exception("Não foi possível iniciar o processo de exportação.");

            var streamError = process.StandardError;
            var streamOutput = process.StandardOutput;
            var taskInfo = Task.Factory.StartNew(async () =>
            {
                while (!process.HasExited)
                {
                    var message = await streamOutput.ReadLineAsync();
                    if (message != null)
                    {
                        legendable?.SendMessage(message);
                        if (message == "Finished")
                            break;
                    }
                }
            });

            await process.WaitForExitAsync();

            var error = await streamError.ReadToEndAsync();
            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);
            else
            {
                legendable?.SendMessage("O arquivo foi gerado com sucesso.");
                Process.Start("explorer.exe", fullAddress);
            }
        }
    }
}
