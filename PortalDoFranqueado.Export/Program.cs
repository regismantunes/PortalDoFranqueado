using PortalDoFranqueado.Model.Entities;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Export
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
#if DEBUG
                if (args.Length == 0)
                    args =
                    [
                        @"C:\Users\regis\AppData\Local\Temp\tmp822D.tmp.json",
                        @"D:\Meus Arquivos\Documentos\Teste Purchase.xlsx"
                    ];
#endif
                Console.Out.WriteLine("Iniciando validações...");
                if (args.Length == 0)
                {
                    Console.Error.WriteLine("Nenhum parâmetro foi informado.");
                    return;
                }

                if (args.Length != 2)
                {
                    Console.Error.WriteLine("Número de argumentos incorretos.");
                    return;
                }

                var originFileAddress = args[0];
                var destinationFileAddress = args[1];

                if (!Path.IsPathFullyQualified(originFileAddress))
                {
                    Console.Error.WriteLine($"O endereço {originFileAddress} não é um endereço de arquivo válido.");
                    return;
                }

                if (!Path.IsPathFullyQualified(destinationFileAddress))
                {
                    Console.Error.WriteLine($"O endereço {destinationFileAddress} não é um endereço de arquivo válido.");
                    return;
                }

                if (!File.Exists(originFileAddress))
                {
                    Console.Error.WriteLine($"Não foi possível encontrar arquivo {originFileAddress}.");
                    return;
                }

                var destinationDirectory = Path.GetDirectoryName(destinationFileAddress);
                if (!Directory.Exists(destinationDirectory))
                {
                    Console.Error.WriteLine($"Não foi possível encontrar diretório {destinationDirectory}.");
                    return;
                }

                Console.Out.WriteLine("Carregando arquivo de origem...");
                var jsonContent = File.ReadAllText(originFileAddress);
                Console.Out.WriteLine("Organizando informações...");
                var purchase = JsonSerializer.Deserialize<Purchase>(jsonContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));

                if (purchase == null)
                {
                    Console.Error.WriteLine($"Não foi possível obter as informações do arquivo {originFileAddress}.");
                    return;
                }

                Console.Out.WriteLine("Gerando o arquivo Excel...");
                var finished = false;
                var task = Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        await Excel.Exporter.GenerateFile(purchase, destinationFileAddress);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Falha ao gerar arquivo Excel\r\n{ex}");
                    }
                    finally
                    {
                        finished = true;
                    }
                });

                task.Wait(Timeout.Infinite);

                while(!finished)
                    Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            finally
            {
                Console.Out.WriteLine("Finished");
            }
        }
    }
}
