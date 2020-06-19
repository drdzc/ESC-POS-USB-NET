﻿using ESC_POS_USB_NET.Printer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Drawing;
using System.Text;
using TestPrinterApp;
using Topshelf;

namespace ESCPrinterService
{
    class Program
    {
        static void Main(string[] args)
        {
            var printerListener = new PrinterService();
            printerListener.CreateRabbitMQListener();
        }

        private static void PrintTicket(PurchaseOrderSingleItem po, Bitmap image)
        {
            var printer = new Printer("Star BSC10", "utf-8");

            //printer.Image(image);
            printer.BoldMode("MUNDO ALBERCAS");
            printer.NewLine();
            printer.AlignLeft();
            printer.Append("CARRETERA NACIONAL KM 258, COL. EL FAISAN,PLAZA LA MISION #328, C.P. 67303");
            printer.Append("NUEVO LEON, MEXICO");
            printer.NewLine();
            //printer.Append("COL. EL FAISAN, PLAZA LA MISION #328, C.P. 67303");
            printer.Append("RFC ROVN870614IW0");
            printer.NewLine();
            printer.Append("TEL. 8110869783    CEL. 8113008880");
            printer.NewLine();
            // printer.BoldMode("SUC CIUDAD, ESTADO");
            printer.NewLines(1);
            printer.Append("CLIENTE - " + po.client);
            printer.Append("EMPLEADO(A) - " + po.employee);
            printer.Append("Fecha: " + po.createdDt.ToString("dd/MMMM/yyyy") + " Hora: " + po.createdDt.Hour + ":" + po.createdDt.Minute);
            printer.NewLines(3);

            //productos
            var format = "{0, -2} {1, -22} {2, 7} {3, 7}";
            var headerStrBuilder = new StringBuilder().AppendFormat(format, "Ctd", "Descripcion", "P. Unit", "Importe");
            printer.NewLine();
            printer.Append(headerStrBuilder.ToString());
            format = "{0, -3} {1, -30} {2, 10} {3, 10}";
            headerStrBuilder.Clear();
            printer.NewLines(2);
            foreach (var product in po.products)
            {
                headerStrBuilder.AppendFormat(format, "", product.model, "", "");
                if (product.isGift == 1)
                {
                    headerStrBuilder.AppendFormat(format, product.quantity, product.Truncate(product.name, 30), "", "- " + product.costWithDiscountStr);
                }
                else
                {
                    headerStrBuilder.AppendFormat(format, product.quantity, product.Truncate(product.name, 30), product.baseCostStr, product.costStr);
                    if (product.calculatedDiscount > 0)
                    {
                        headerStrBuilder.AppendFormat(format, "", "Descuento del producto", "", "- $ " + product.calculatedDiscount);
                    }
                }
                //printer.Append(product.name + ' ' + product.baseCostStr);
                //printer.Append("(Descuento " + product.calculatedDiscount + ")");
            }
            printer.Font(headerStrBuilder.ToString(), ESC_POS_USB_NET.Enums.Fonts.FontB);
            printer.NewLines(4);

            format = "{0, 31} {1, 10}";
            headerStrBuilder.Clear();

            if (po.debitCardTotal != "$0.00")
            {
                headerStrBuilder.AppendFormat(format, "TOTAL TARJ DEBITO", po.debitCardTotal);
                headerStrBuilder.AppendLine(Environment.NewLine);
            }
            if (po.creditCardTotal != "$0.00")
            {
                headerStrBuilder.AppendFormat(format, "TOTAL TARJ CREDITO", po.creditCardTotal);
                headerStrBuilder.AppendLine(Environment.NewLine);
            }
            if (po.checkTotal != "$0.00")
            {
                headerStrBuilder.AppendFormat(format, "TOTAL CHEQUE", po.checkTotal);
                headerStrBuilder.AppendLine(Environment.NewLine);
            }
            if (po.transferTotal != "$0.00")
            {
                headerStrBuilder.AppendFormat(format, "TOTAL TRANSFERENCIA", po.transferTotal);
                headerStrBuilder.AppendLine(Environment.NewLine);
            }
            if (po.cashPaidTotal != "$0.00")
            {
                headerStrBuilder.AppendFormat(format, "TOTAL EFECTIVO", po.cashPaidTotal);
                headerStrBuilder.AppendLine(Environment.NewLine);
                headerStrBuilder.AppendFormat(format, "CAMBIO", po.changeTotal);
                headerStrBuilder.AppendFormat(Environment.NewLine);
            }
            headerStrBuilder.AppendFormat(format, "TOTAL", po.total);
            headerStrBuilder.AppendLine(Environment.NewLine);
            printer.BoldMode(headerStrBuilder.ToString());

            printer.AlignCenter();
            printer.Append("Gracias por tu compra!");
            printer.NewLine();
            printer.Code128(po.id.ToString(), ESC_POS_USB_NET.Enums.Positions.BelowBarcode);
            printer.OpenDrawer();
            printer.FullPaperCut();
            printer.PrintDocument();
        }

        private static Bitmap ResizeBMP(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }
            return result;
        }
    }
}
