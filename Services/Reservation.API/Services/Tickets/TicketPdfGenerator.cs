using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Reservation.API.ExternalServices;
using Entities = Reservation.API.Domain.Entities;

namespace Reservation.API.Services.Tickets;

public class TicketPdfGenerator : ITicketPdfGenerator
{
    public byte[] Generate(Entities.Ticket ticket, ScreeningDetails screening, CinemaDetails? cinema, MovieDetails? movie)
    {
        var cinemaName = cinema?.Name ?? "Unknown Cinema";
        var movieTitle = movie?.Title ?? "Unknown Movie";
        var subtitle = cinema == null ? cinemaName : $"{cinemaName} ({cinema.City})";
        var qrImageBytes = GenerateQrCode(ticket.QrCode);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(560, 230);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily("DejaVu Sans").FontSize(10).FontColor(Colors.Black));

                page.Content().Row(row =>
                {
                    row.RelativeItem(2).Background(Colors.White).Padding(20).Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("CineMATF").FontSize(22).Bold().FontColor(Colors.Red.Darken2);
                            r.AutoItem().AlignRight().Text("E-TICKET").FontSize(10).FontColor(Colors.Grey.Darken1);
                        });

                        col.Item().PaddingTop(10).Text(movieTitle).FontSize(18).Bold();
                        col.Item().PaddingTop(2).Text(subtitle).FontSize(11).FontColor(Colors.Grey.Darken2);

                        col.Item().PaddingTop(16).Row(r =>
                        {
                            r.RelativeItem(1.6f).Column(c => AddLabelValue(c, "DATE & TIME", $"{screening.StartTime:dd MMM yyyy}, {screening.StartTime:HH:mm}"));
                            r.RelativeItem(0.8f).Column(c => AddLabelValue(c, "FORMAT", FormatScreeningFormat(screening.Format)));
                            r.RelativeItem(1.3f).Column(c => AddLabelValue(c, "SEAT", $"Row {ticket.SeatRow} · Seat {ticket.SeatNumber}"));
                            r.RelativeItem(0.8f).Column(c => AddLabelValue(c, "PRICE", $"{ticket.Price:0.00}"));
                        });

                        col.Item().PaddingTop(20).Text($"Ticket ID: {ticket.Id}").FontSize(7).FontColor(Colors.Grey.Darken1);
                        col.Item().Text($"Reservation ID: {ticket.ReservationId}").FontSize(7).FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(1).Background(Colors.Grey.Lighten2);

                    row.RelativeItem(1).Background(Colors.Grey.Lighten4).Padding(14).Column(col =>
                    {
                        col.Item().AlignCenter().Text("SCAN AT ENTRANCE").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                        col.Item().PaddingTop(8).AlignCenter().Width(100).Height(100).Image(qrImageBytes);
                        col.Item().PaddingTop(8).AlignCenter().Text($"Seat {ticket.SeatRow}-{ticket.SeatNumber}").FontSize(11).Bold();
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string FormatScreeningFormat(string format) => format switch
    {
        "TwoD" => "2D",
        "ThreeD" => "3D",
        _ => format
    };

    private static void AddLabelValue(ColumnDescriptor column, string label, string value)
    {
        column.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
        column.Item().Text(value).FontSize(12).Bold();
    }

    private static byte[] GenerateQrCode(string payload)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var pngQr = new PngByteQRCode(data);
        return pngQr.GetGraphic(20);
    }
}
