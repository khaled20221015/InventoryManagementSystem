using InventoryManagementSystem.DataAccess.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InventoryManagementSystem.Business.Reports
{
    // Describes how the inventory PDF looks. QuestPDF turns this description into the real file.
    public class InventoryReportDocument : IDocument
    {
        private readonly List<Product> _products;
        private readonly int _categoryCount;
        private readonly string _generatedBy;
        private readonly DateTime _generatedAt;

        public InventoryReportDocument(List<Product> products, int categoryCount, string generatedBy)
        {
            _products = products;
            _categoryCount = categoryCount;
            _generatedBy = generatedBy;
            _generatedAt = DateTime.Now;
        }

        public DocumentMetadata GetMetadata() => new()
        {
            Title = "Inventory Report",
            Author = "Inventory Management System"
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(text => text.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.PaddingBottom(15).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().Text("Inventory Report").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                        left.Item().Text("Inventory Management System").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(180).Column(right =>
                    {
                        right.Item().AlignRight().Text($"Generated: {_generatedAt:yyyy-MM-dd HH:mm}").FontSize(9);
                        right.Item().AlignRight().Text($"By: {_generatedBy}").FontSize(9);
                    });
                });

                column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Element(ComposeSummary);
                column.Item().PaddingTop(20).Element(ComposeProductsTable);
            });
        }

        private void ComposeSummary(IContainer container)
        {
            var lowStockCount = _products.Count(p => p.StockQuantity <= p.MinimumStockLevel);
            var inventoryValue = _products.Sum(p => p.Price * p.StockQuantity);

            container.Row(row =>
            {
                row.RelativeItem().Element(c => SummaryBox(c, "Products", _products.Count.ToString(), Colors.Blue.Lighten4));
                row.ConstantItem(10);
                row.RelativeItem().Element(c => SummaryBox(c, "Categories", _categoryCount.ToString(), Colors.Green.Lighten4));
                row.ConstantItem(10);
                row.RelativeItem().Element(c => SummaryBox(c, "Low stock", lowStockCount.ToString(), Colors.Red.Lighten4));
                row.ConstantItem(10);
                row.RelativeItem().Element(c => SummaryBox(c, "Total value", inventoryValue.ToString("N2"), Colors.Grey.Lighten3));
            });
        }

        private static void SummaryBox(IContainer container, string label, string value, string background)
        {
            container
                .Background(background)
                .Padding(10)
                .Column(column =>
                {
                    column.Item().Text(label.ToUpper()).FontSize(8).FontColor(Colors.Grey.Darken2);
                    column.Item().Text(value).FontSize(16).Bold();
                });
        }

        private void ComposeProductsTable(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().PaddingBottom(8).Text("Products").FontSize(13).Bold();

                if (_products.Count == 0)
                {
                    column.Item().Text("No products recorded yet.").Italic().FontColor(Colors.Grey.Darken1);
                    return;
                }

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Product");
                        header.Cell().Element(HeaderCell).Text("Category");
                        header.Cell().Element(HeaderCell).AlignRight().Text("Price");
                        header.Cell().Element(HeaderCell).AlignRight().Text("Stock");
                        header.Cell().Element(HeaderCell).AlignRight().Text("Min. level");
                        header.Cell().Element(HeaderCell).AlignCenter().Text("Status");
                    });

                    foreach (var product in _products)
                    {
                        var isLow = product.StockQuantity <= product.MinimumStockLevel;

                        table.Cell().Element(BodyCell).Text(product.Name);
                        table.Cell().Element(BodyCell).Text(product.Category?.Name ?? "-");
                        table.Cell().Element(BodyCell).AlignRight().Text(product.Price.ToString("N2"));
                        table.Cell().Element(BodyCell).AlignRight().Text(product.StockQuantity.ToString());
                        table.Cell().Element(BodyCell).AlignRight().Text(product.MinimumStockLevel.ToString());
                        table.Cell().Element(BodyCell).AlignCenter()
                            .Text(isLow ? "LOW" : "OK")
                            .FontColor(isLow ? Colors.Red.Darken2 : Colors.Green.Darken2)
                            .Bold();
                    }
                });
            });
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten2)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Darken1)
                .PaddingVertical(6)
                .PaddingHorizontal(4)
                .DefaultTextStyle(text => text.SemiBold());
        }

        private static IContainer BodyCell(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(5)
                .PaddingHorizontal(4);
        }

        private void ComposeFooter(IContainer container)
        {
            container.PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text("Inventory Management System").FontSize(8).FontColor(Colors.Grey.Darken1);

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(style => style.FontSize(8).FontColor(Colors.Grey.Darken1));
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        }
    }
}
