using ESC_POS_USB_NET.Printer;
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
            HostFactory.Run(x => {
                x.Service<QueuePrinterService>();
                x.EnableServiceRecovery(r => r.RestartService(TimeSpan.FromSeconds(10)));
                x.SetServiceName("QueuePrinterService");
                x.SetDescription("Servicio para la impresion de la cola de mensajes");
                x.StartAutomatically();
            });
            /*Bitmap image = new Bitmap(Bitmap.FromFile(@"logoma.bmp"));
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
            using (var conn = factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "orders", durable: false, type: ExchangeType.Direct);
                    var queueName = channel.QueueDeclare().QueueName;
                    channel.QueueBind(queue: queueName, exchange: "orders", routingKey: "orders");
                    Console.WriteLine(" [*] Waiting for messages.");
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var messageStr = Encoding.UTF8.GetString(body.ToArray());
                        Console.WriteLine(" [x] Received {0}", messageStr);
                        var test = Newtonsoft.Json.JsonConvert.DeserializeObject<PurchaseOrderSingleItem>(messageStr);
                        PrintTestPage(test, image);
                    };

                    channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }*/
        }

        private static void PrintTestPage(PurchaseOrderSingleItem po, Bitmap image)
        {
            var printer = new Printer("Star BSC10", "utf-8");
            //image = ResizeBMP(image, 100, 100);

            printer.Image(image);
            printer.BoldMode("MUNDO ALBERCAS, S.A. de C.V.");
            printer.NewLine();
            printer.AlignLeft();
            printer.Append("CALLE #1234, CIUDAD, ESTADO, PAIS");
            printer.NewLine();
            printer.Append("COL. COLONIA, C.P. 60000 RFC XXXX111111XX1");
            printer.NewLine();
            printer.BoldMode("SUC CIUDAD, ESTADO");
            printer.NewLines(2);
            printer.Append("CLIENTE - " + po.client);
            printer.Append("EMPLEADO(A) - " + po.employee);
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
                    headerStrBuilder.AppendFormat(format, product.quantity, product.Truncate(product.name, 30), "", "- "+product.costWithDiscountStr);
                } else
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

            headerStrBuilder.AppendFormat(format, "TOTAL", po.total);
            headerStrBuilder.AppendLine(Environment.NewLine);
            headerStrBuilder.AppendFormat(format, "TOTAL TARJ DEBITO", po.debitCardTotal);
            headerStrBuilder.AppendLine(Environment.NewLine);
            headerStrBuilder.AppendFormat(format, "TOTAL TARJ CREDITO", po.creditCardTotal);
            headerStrBuilder.AppendLine(Environment.NewLine);
            headerStrBuilder.AppendFormat(format, "TOTAL CHEQUE", po.checkTotal);
            headerStrBuilder.AppendLine(Environment.NewLine);
            headerStrBuilder.AppendFormat(format, "TOTAL TRANSFERENCIA", po.transferTotal);
            headerStrBuilder.AppendLine(Environment.NewLine);
            headerStrBuilder.AppendFormat(format, "TOTAL EFECTIVO", po.cashPaidTotal);
            headerStrBuilder.AppendLine(Environment.NewLine);
            headerStrBuilder.AppendFormat(format, "CAMBIO", po.debitCardTotal);
            headerStrBuilder.AppendLine();
            /*printer.AlignRight();
            printer.BoldMode("TOTAL " + po.total);
            printer.BoldMode("TOTAL TARJ DEBITO " + po.debitCardTotal);
            printer.BoldMode("TOTAL TARJ CREDITO " + po.creditCardTotal);
            printer.BoldMode("TOTAL CHEQUE " + po.checkTotal);
            printer.BoldMode("TOTAL TRANSFERENCIA " + po.transferTotal);
            printer.BoldMode("TOTAL EFECTIVO\t" + po.cashTotal);
            printer.BoldMode("CAMBIO\t" + po.debitCardTotal);*/
            printer.BoldMode(headerStrBuilder.ToString());

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
