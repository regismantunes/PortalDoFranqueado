using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Core;
using PortalDoFranqueado.Model.Entities;
using PortalDoFranqueado.Model.Entities.Extensions;

namespace PortalDoFranqueado.Export.Excel
{
    public static class Exporter
    {
        public static async Task GenerateFile(Purchase purchase, string fullAddress)
        {
            if (File.Exists(fullAddress))
                File.Delete(fullAddress);

            await File.WriteAllBytesAsync(fullAddress, Properties.Resources.PurchaseModel);

            var excel = new Application();
            var wbs = excel.Workbooks;
            Workbook wb = null;
            try
            {
                wb = wbs.Open(fullAddress, 0, false);
                var ws = wb.ActiveSheet as Worksheet;

                ws.get_Range("B2").Value = purchase.Store.Name;
                ws.get_Range("P2").Value = purchase.Store.DocumentNumber.ToCnpjFormat();

                var families = purchase.Items.GroupBy(i => i.Product.Family.Name,
                                                      i => i.Product.Price * i.Quantity,
                                                    (familyName, totalValue) => new
                                                    {
                                                        FamilyName = familyName,
                                                        TotalValue = totalValue.Sum()
                                                    });

                var ifamily = 6;
                foreach (var family in families)
                {
                    var contentTest = ws.get_Range($"A{ifamily}").Value as string;
                    if (string.IsNullOrEmpty(contentTest) ||
                        contentTest != "<Familia>")
                    {
                        var line = (Microsoft.Office.Interop.Excel.Range)ws.Rows[ifamily];
                        line.Insert();
                    }

                    ws.get_Range($"A{ifamily}").Value = family.FamilyName;
                    ws.get_Range($"B{ifamily}").Value = family.TotalValue;
                    
                    ifamily++;
                }

                ws.get_Range($"B{ifamily}").Value = $"=SUM(B6:B{ifamily-1})";

                var groupedPurchaseItems = purchase.Items.GroupBy(i => i.Product.Id,
                                                                  i => i,
                                                                  (id, itens) => new
                                                                  {
                                                                      ProductId = id,
                                                                      Product = itens.First().Product,
                                                                      Itens = itens.Select(i => new
                                                                      {
                                                                          Size = i.Size,
                                                                          Quantity = i.Quantity,
                                                                          Order = i.Size.Order
                                                                      }).OrderBy(i => i.Order)
                                                                  });

                var iproduct = ifamily + 3;
                foreach (var gpi in groupedPurchaseItems)
                {
                    var contentTest = ws.get_Range($"A{iproduct}").Value as string;

                    if (string.IsNullOrEmpty(contentTest) ||
                        contentTest != "<FotoProduto>")
                    {
                        var line = (Microsoft.Office.Interop.Excel.Range)ws.Rows[iproduct];
                        line.Insert();
                    }

                    ws.get_Range($"A{iproduct}").Value = string.Empty;

                    var imageCell = (dynamic)ws.Cells[iproduct, 1];
                    var imageMaxHeight = imageCell.Height - 4;
                    var imageMaxWidth = imageCell.Width - 4;
                    var imageInfo = gpi.Product.ImageInformation;
                    var imageSizeRef = imageMaxHeight < imageMaxWidth ? imageMaxHeight : imageMaxWidth;
                    var imageHeight = imageSizeRef * (imageInfo.Width > imageInfo.Height ?
                                                    imageInfo.Height / imageInfo.Width : 
                                                    1);
                    var imageWidth = imageSizeRef * (imageInfo.Width < imageInfo.Height ?
                                                    imageInfo.Width / imageInfo.Height :
                                                    1);
                    var imageLeft = imageCell.Left + 2 + (imageMaxWidth - imageWidth) / 2;
                    var image = ws.Shapes.AddPicture(gpi.Product.ImageInformation.FileAddress, MsoTriState.msoFalse, MsoTriState.msoTrue, imageLeft, imageCell.Top + 2, imageWidth, imageHeight) as Microsoft.Office.Interop.Excel.Shape;
                    image.Placement = XlPlacement.xlMoveAndSize;

                    ws.get_Range($"B{iproduct}").Value = gpi.Product.Description;
                    ws.get_Range($"C{iproduct}").Value = gpi.Product.Family.Name;
                    ws.get_Range($"D{iproduct}").Value = gpi.Product.Supplier?.Name;
                    ws.get_Range($"E{iproduct}").Value = gpi.Product.Price;

                    var qtd1 = gpi.Itens.Where(i => i.Order == 0)?
                                            .Select(i => new { i.Quantity, i.Size.Size })
                                            .FirstOrDefault();
                    var qtd2 = gpi.Itens.Where(i => i.Order == 1)?
                                            .Select(i => new { i.Quantity, i.Size.Size })
                                            .FirstOrDefault();
                    var qtd3 = gpi.Itens.Where(i => i.Order == 2)?
                                            .Select(i => new { i.Quantity, i.Size.Size })
                                            .FirstOrDefault();
                    var qtd4 = gpi.Itens.Where(i => i.Order == 3)?
                                            .Select(i => new { i.Quantity, i.Size.Size })
                                            .FirstOrDefault();
                    var qtd5 = gpi.Itens.Where(i => i.Order == 4)?
                                            .Select(i => new { i.Quantity, i.Size.Size })
                                            .FirstOrDefault();
                    var qtd6 = gpi.Itens.Where(i => i.Order == 5)?
                                            .Select(i => new { i.Quantity, i.Size.Size })
                                            .FirstOrDefault();
                    var qtd7 = gpi.Itens.Where(i => i.Order == 6)?
                                            .Select(i => new { i.Quantity, i.Size.Size })
                                            .FirstOrDefault();

                    ws.get_Range($"F{iproduct}").Value = qtd1?.Size;
                    ws.get_Range($"G{iproduct}").Value = qtd1?.Quantity?.ToString() ?? string.Empty;
                    ws.get_Range($"H{iproduct}").Value = qtd2?.Size;
                    ws.get_Range($"I{iproduct}").Value = qtd2?.Quantity?.ToString() ?? string.Empty;
                    ws.get_Range($"J{iproduct}").Value = qtd3?.Size;
                    ws.get_Range($"K{iproduct}").Value = qtd3?.Quantity?.ToString() ?? string.Empty;
                    ws.get_Range($"L{iproduct}").Value = qtd4?.Size;
                    ws.get_Range($"M{iproduct}").Value = qtd4?.Quantity?.ToString() ?? string.Empty;
                    ws.get_Range($"N{iproduct}").Value = qtd5?.Size;
                    ws.get_Range($"O{iproduct}").Value = qtd5?.Quantity?.ToString() ?? string.Empty;
                    ws.get_Range($"P{iproduct}").Value = qtd6?.Size;
                    ws.get_Range($"Q{iproduct}").Value = qtd6?.Quantity?.ToString() ?? string.Empty;
                    ws.get_Range($"R{iproduct}").Value = qtd7?.Size;
                    ws.get_Range($"S{iproduct}").Value = qtd7?.Quantity?.ToString() ?? string.Empty;
                    
                    ws.get_Range($"T{iproduct}").Value = string.Format("=SUM(G{0},I{0},K{0},M{0},O{0},Q{0},S{0})*E{0}", iproduct);

                    iproduct++;
                }

                wb.Save();
            }
            finally
            {
                int hWnd = excel.Application.Hwnd;

                wb?.Close();
                wbs.Close();
                excel.Quit();

                GetWindowThreadProcessId((IntPtr)hWnd, out uint processID);
                Process.GetProcessById((int)processID).Kill();
            }
        }

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private static int? ZeroToNull(this int qtd)
            => qtd == 0 ? null : qtd;
    }
}
