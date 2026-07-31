using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class InvoicePrintService
    {
        public bool Print(Invoice invoice, IEnumerable<InvoicePrintLine> lines)
        {
            var dialog = new PrintDialog();
            if (dialog.ShowDialog() != true) return false;
            var document = new FlowDocument { PagePadding = new Thickness(40), FontFamily = new FontFamily("Segoe UI"), FontSize = 13 };
            document.Blocks.Add(new Paragraph(new Bold(new Run("RESTAURANT POS\nHÓA ĐƠN THANH TOÁN"))) { TextAlignment = TextAlignment.Center, FontSize = 20 });
            document.Blocks.Add(new Paragraph(new Run($"Số hóa đơn: {invoice.InvoiceNumber}\nNgày: {invoice.PaidAt:dd/MM/yyyy HH:mm}\nMã phiên: {invoice.SessionId}")));
            var table = new Table { CellSpacing = 0 };
            table.Columns.Add(new TableColumn { Width = new GridLength(240) });
            table.Columns.Add(new TableColumn { Width = new GridLength(50) });
            table.Columns.Add(new TableColumn { Width = new GridLength(90) });
            table.Columns.Add(new TableColumn { Width = new GridLength(100) });
            var group = new TableRowGroup(); table.RowGroups.Add(group);
            AddRow(group, "Món", "SL", "Đơn giá", "Thành tiền", true);
            foreach (var line in lines) AddRow(group, line.DishName, line.Quantity.ToString(), $"{line.UnitPrice:N0}", $"{line.Amount:N0}", false);
            document.Blocks.Add(table);
            document.Blocks.Add(new Paragraph(new Run($"Tạm tính: {invoice.Subtotal:N0} đ\nGiảm giá: {invoice.Discount:N0} đ\nTỔNG TIỀN: {invoice.TotalAmount:N0} đ")) { FontWeight = FontWeights.SemiBold });
            document.Blocks.Add(new Paragraph(new Run($"Trạng thái: {invoice.Status}\nCảm ơn quý khách!")));
            dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, invoice.InvoiceNumber);
            return true;
        }

        private static void AddRow(TableRowGroup group, string name, string quantity, string price, string amount, bool bold)
        {
            var row = new TableRow(); group.Rows.Add(row);
            foreach (string text in new[] { name, quantity, price, amount })
                row.Cells.Add(new TableCell(new Paragraph(new Run(text))) { FontWeight = bold ? FontWeights.Bold : FontWeights.Normal, Padding = new Thickness(3) });
        }
    }
}
