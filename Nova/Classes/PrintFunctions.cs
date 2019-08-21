using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Nova.PrintFunctions
{
       

    public class TransferTicket
    {


    }

    
    class PrintFunctions
    {

        public static double PixelPermmValue = 3.5; // Page width = 3.5 * Page mm
        public static int PaperWidthmm = 80;

        public static int PagwWith;

        public static Font printHeaderFont;
        public static Font printFont;
        public static Font printFontBold;
        public static Font printTinyFont;

        public static void PrintTranslateTicket(TransferTicket PrintData)
        {
            PrinterSettings settings = new PrinterSettings();
            PrintDocument PrintDoc = new PrintDocument();
            PrintDoc.PrintPage += PrintDoc_PrintPage;           
            //PrintDoc.DefaultPageSettings.PaperSize = new PaperSize("A7 Rotated", 200, 78);
            PrintDoc.DefaultPageSettings.Margins = new Margins(4, 4, 8, 4);
            PrintDoc.Print();
        }

        public static void Print(Classes.Enums.PrintPages PrintPageType, dynamic PrintData)
        {
            //SET PAGE
            PrinterSettings settings = new PrinterSettings();
            PrintDocument PrintDoc = new PrintDocument();
            PrintDoc.DefaultPageSettings.PaperSize = new PaperSize("Test", PrintDoc.DefaultPageSettings.PaperSize.Width, 5000);
            PrintDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

            PagwWith = Convert.ToInt32(Math.Round((PaperWidthmm * PixelPermmValue)));


            //SET FONTS
            printFont = new Font("Microsoft Sans Serif", 9, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
            printFontBold = new Font("Microsoft Sans Serif", 9, System.Drawing.FontStyle.Bold, GraphicsUnit.Point);
            printHeaderFont = new Font("Microsoft Sans Serif", 10, System.Drawing.FontStyle.Bold, GraphicsUnit.Point);
            printTinyFont = new Font("Microsoft Sans Serif", 7, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);

            //GO TO PRINT!
            switch (PrintPageType)
            {
                case Classes.Enums.PrintPages.FinalTicket:
                    PrintDoc.PrintPage += delegate (object sender, PrintPageEventArgs e)
                    {
                        PrintTicket(sender, e, PrintData);
                    };
                    break;
                case Classes.Enums.PrintPages.CotiTicket:
                    break;
                case Classes.Enums.PrintPages.LeftPaymentTicket:
                    break;
                case Classes.Enums.PrintPages.TransferTicket:
                    break;
                case Classes.Enums.PrintPages.TestTicket:
                    break;
                case Classes.Enums.PrintPages.BoxDetail:
                    PrintDoc.PrintPage += delegate (object sender, PrintPageEventArgs e)
                    {
                        PrintBoxDetail(sender, e);
                    };
                    break;
                default:
                    break;
            }

            PrintDoc.Print();
        }

        private static void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                //PRINT HEADER

                e.Graphics.DrawString("123456789123456789123456789123456789", printFont, Brushes.Black, 0, 0);
                e.Graphics.DrawString("123456789123456789123456789123456789", printFont, Brushes.Black, 0, 10);
                e.Graphics.DrawString("123456789123456789123456789123456789", printFont, Brushes.Black, 0, 20);
                e.Graphics.DrawString("------------------------------------", printFont, Brushes.Black, 0, 30);
            }
            catch (Exception)
            {                            }
        }

        //Print ticket
        private static void PrintTicket(object sender, PrintPageEventArgs e, NovaAPI.APITickets.TicketData TicketData)
        {
            try
            {
                //Get values
                int maxPageSize = e.PageBounds.Height;
                int XPosition = 0;

                //Set values
                var g = e.Graphics;
                int LineHeightValue = 14;

                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;


                string Header = TicketData.ticket_coti == "1" ? $"{DataConfig.PrintHeader}{Environment.NewLine}{Environment.NewLine}COTIZACIÓN" : TicketData.ticket_h == "0" ? DataConfig.PrintHeader : "RECIBO DE VENTA";

                //PRINT HEADER
                int HeaderLines = Header.Split('\n').Length;
                Rectangle HeaderRectangle = new Rectangle(0, 0, PagwWith, 20 * HeaderLines);

                //Header TEXT print
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    g.DrawString(Header, printHeaderFont, Brushes.Black, HeaderRectangle, sf);
                }

                XPosition = HeaderRectangle.Height + 30; //SET POSITION

                if (TicketData.ticket_copy == "1")
                {
                    Rectangle CopyRectangle = new Rectangle(0, XPosition, PagwWith, (LineHeightValue * 2));

                    //Header TEXT print
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        g.DrawString("---- COPIA ----", printHeaderFont, Brushes.Black, CopyRectangle, sf);
                    }

                    XPosition += LineHeightValue * 2; //SET POSITION
                }

                string ExpireDate = "";

                if (TicketData.expiration_date != "")
                {
                    ExpireDate = $"VENCE: {TicketData.expiration_date}{Environment.NewLine}";
                }

                //PRINT TICKET INFO
                string TicketInfo = $"RECIBO DE CAJA: {TicketData.ticketid}{Environment.NewLine}FECHA: {DateTime.Now.ToString()}{Environment.NewLine}{ExpireDate}{Environment.NewLine}" +
                                    $"CLIENTE: {TicketData.client_name}{Environment.NewLine}IDENTIFICACIÓN: {TicketData.client_documentid}{Environment.NewLine}" +
                                    $"DIRECCIÓN: {TicketData.client_address}{Environment.NewLine}TELEFONOS: {TicketData.client_phones}";

                if (TicketData.ticket_h == "1")
                {
                    TicketInfo = $"FECHA: {DateTime.Now.ToString()}{Environment.NewLine}{ExpireDate}{Environment.NewLine}" +
                                    $"CLIENTE: {TicketData.client_name}{Environment.NewLine}IDENTIFICACIÓN: {TicketData.client_documentid}{Environment.NewLine}" +
                                    $"DIRECCIÓN: {TicketData.client_address}{Environment.NewLine}TELEFONOS: {TicketData.client_phones}";
                }

                if (TicketData.ticket_coti =="1")
                {
                    TicketInfo = $"FECHA: {DateTime.Now.ToString()}{Environment.NewLine}{ExpireDate}{Environment.NewLine}" +
                                    $"CLIENTE: {TicketData.client_name}{Environment.NewLine}IDENTIFICACIÓN: {TicketData.client_documentid}{Environment.NewLine}" +
                                    $"DIRECCIÓN: {TicketData.client_address}{Environment.NewLine}TELEFONOS: {TicketData.client_phones}";
                }

                int TicketInfoLines = TicketInfo.Split('\n').Length;

                Rectangle TicketInfoRectangle = new Rectangle(0, XPosition, PagwWith, LineHeightValue * TicketInfoLines);

                using (StringFormat sf = new StringFormat())
                {                   
                    g.DrawString(TicketInfo, printFontBold, Brushes.Black, TicketInfoRectangle, sf);
                }

                XPosition += TicketInfoRectangle.Height + 20; //SET POSITION


                //TICKET INFO BOX VALUES

                string PaymentCondition = TicketData.payment_type == "0" ? "CONTADO" : "CREDITO";
                string PaymentMethod = TicketData.payment_method == "0" ? "EFECTIVO" : "";

                string PlusInfo = $"CAJERO: {TicketData.user_realname}{Environment.NewLine}CONDICIONES DE PAGO: {PaymentCondition}{Environment.NewLine}FORMA DE PAGO: {PaymentMethod}";
                int PlusInfoLines = PlusInfo.Split('\n').Length;

                Rectangle PlusInfoRectangle = new Rectangle(0, XPosition, PagwWith, LineHeightValue * PlusInfoLines);

                using (StringFormat sf = new StringFormat())
                {
                    g.DrawString(PlusInfo, printFont, Brushes.Black, PlusInfoRectangle, sf);
                }

                XPosition += PlusInfoRectangle.Height + 20; //SET POSITION

                //PRINT TICKET PRODUCTS HEADER
                g.DrawString("COD.", printHeaderFont, Brushes.Black,0, XPosition);
                g.DrawString("DESCRIPCIÓN", printHeaderFont, Brushes.Black, 100, XPosition);
                g.DrawString("CANT.", printHeaderFont, Brushes.Black, 0, XPosition + LineHeightValue);
                g.DrawString("VLR. UNITARIO.", printHeaderFont, Brushes.Black, 100, XPosition + LineHeightValue);
                g.DrawString("TOTAL", printHeaderFont, Brushes.Black, 220, XPosition + LineHeightValue);
                g.DrawString("DESCUENTO", printHeaderFont, Brushes.Black, 100, XPosition + (LineHeightValue * 2));

                XPosition += PlusInfoRectangle.Height + 10; //SET POSITION

                //PRINT TICKET PRODUCTS
                for (int i = 0; i < TicketData.products.Count; i++)
                {
                    
                    bool Discount = false;                    

                    g.DrawString(TicketData.products[i].product_code, printFont, Brushes.Black, 0, XPosition); //COD
                    g.DrawString(TicketData.products[i].product_name, printFontBold, Brushes.Black, 100, XPosition); //DESCRIPCION
                    g.DrawString($"{TicketData.products[i].product_count} \t {GeneralFunctions.GetUnityTypeString(TicketData.products[i].product_unity)}", printFontBold, Brushes.Black, 0, XPosition + LineHeightValue); //CANT
                    g.DrawString(string.Format("{0:C0}", Convert.ToInt32(TicketData.products[i].product_pricevalue)), printFontBold, Brushes.Black, 100, XPosition + LineHeightValue); //V UNITARIO
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Far;
                        Rectangle TotalRectangle = new Rectangle(180, XPosition + LineHeightValue, PagwWith - 180, LineHeightValue);
                        g.DrawString(string.Format("{0,10:C0}", Convert.ToInt32(TicketData.products[i].product_total)), printFontBold, Brushes.Black, TotalRectangle, sf); //TOTAL
                    }

                    //DISCOUNT PRINT
                    if (TicketData.products[i].product_discountpercent != "0")
                    {
                        g.DrawString(string.Format("{0}% {1:C0}", TicketData.products[i].product_discountpercent, Convert.ToInt32(TicketData.products[i].product_discountvalue)), printFont, Brushes.Black, 100, XPosition + (LineHeightValue * 2)); //DESCUENTO
                        Discount = true;
                    }


                    XPosition += Discount == false ? LineHeightValue * 2 : LineHeightValue * 3;
                    XPosition += 5;
                }

                //PRINT TICKET VALUE INFO
                XPosition += LineHeightValue * 2; //SET POSITION

                Rectangle TicketValuesRectangle = new Rectangle(0, XPosition, PagwWith, LineHeightValue);

                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Far;

                    if (TicketData.ticket_h == "0" && TicketData.ticket_coti == "0")
                    {
                        g.DrawString("SUBTOTAL:", printFont, Brushes.Black, TicketValuesRectangle); //SUBTOTAL VALUE
                        g.DrawString(string.Format("{0:C0}", Convert.ToInt32(TicketData.ticket_subtotal)), printFont, Brushes.Black, TicketValuesRectangle, sf);

                        TicketValuesRectangle.Y += LineHeightValue;

                        g.DrawString($"IVA ({DataConfig.ivaValue}%):", printFont, Brushes.Black, TicketValuesRectangle); //IVA VALUE
                        g.DrawString(string.Format("{0:C0}", Convert.ToInt32(TicketData.ticket_iva)), printFont, Brushes.Black, TicketValuesRectangle, sf);

                        TicketValuesRectangle.Y += LineHeightValue;

                        g.DrawString($"IVA ({DataConfig.iva5Value}%):", printFont, Brushes.Black, TicketValuesRectangle); //IVA 5% VALUE
                        g.DrawString(string.Format("{0:C0}", Convert.ToInt32(TicketData.ticket_iva5)), printFont, Brushes.Black, TicketValuesRectangle, sf);

                        TicketValuesRectangle.Y += LineHeightValue;

                        g.DrawString($"IMPOCONSUMO ({DataConfig.iacValue}%):", printFont, Brushes.Black, TicketValuesRectangle); //IAC VALUE
                        g.DrawString(string.Format("{0:C0}", Convert.ToInt32(TicketData.ticket_iac)), printFont, Brushes.Black, TicketValuesRectangle, sf);

                        TicketValuesRectangle.Y += LineHeightValue;
                    }

                    g.DrawString("TOTAL:", printFontBold, Brushes.Black, TicketValuesRectangle); //IAC VALUE
                    g.DrawString(string.Format("{0:C0}", Convert.ToInt32(TicketData.ticket_total)), printFont, Brushes.Black, TicketValuesRectangle, sf);
                }

                XPosition += TicketData.ticket_h == "0" ? LineHeightValue * 6 : LineHeightValue * 2; //SET POSITION

                Rectangle TicketMoneyRectangle = new Rectangle(0, XPosition, PagwWith, LineHeightValue);

                if (TicketData.ticket_coti != "1")
                {
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Far;
                        g.DrawString("RECIBIDO:", printFont, Brushes.Black, TicketMoneyRectangle); //SUBTOTAL VALUE
                        g.DrawString(string.Format("{0:C0}", Convert.ToInt32(TicketData.ticket_totalpayment)), printFont, Brushes.Black, TicketMoneyRectangle, sf);

                        TicketMoneyRectangle.Y += LineHeightValue;

                        g.DrawString($"CAMBIO :", printFont, Brushes.Black, TicketMoneyRectangle); //IVA VALUE
                        g.DrawString(string.Format("{0:C0}", Convert.ToInt32(TicketData.ticket_changepayment)), printFont, Brushes.Black, TicketMoneyRectangle, sf);

                        TicketMoneyRectangle.Y += LineHeightValue;

                        g.DrawString($"SALDO :", printFont, Brushes.Black, TicketMoneyRectangle); //IAC VALUE
                        g.DrawString(string.Format("{0:C0}", Convert.ToInt32(TicketData.ticket_leftpayment)), printFont, Brushes.Black, TicketMoneyRectangle, sf);
                    }
                }              


                //PRINT TICKET FOOTER
                XPosition += TicketData.ticket_coti == "1" ? 0 : TicketData.ticket_h == "0" ? LineHeightValue * 6 : LineHeightValue * 2; //SET POSITION

                int FooterLines = DataConfig.PrintFooter.Split('\n').Length;

                if (TicketData.ticket_h == "0" && TicketData.ticket_coti == "0")
                {
                    Rectangle PFooterRectangle = new Rectangle(0, XPosition, PagwWith, LineHeightValue * FooterLines);

                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        g.DrawString(DataConfig.PrintFooter, printTinyFont, Brushes.Black, PFooterRectangle, sf);
                    }

                    //PRINT TICKET WATERMARK
                    XPosition += (LineHeightValue * (FooterLines)); //SET POSITION

                    int WaterMarkLines = DataConfig.PrintWaterMark.Split('\n').Length;
                    Rectangle WaterMarkRectangle = new Rectangle(0, XPosition, PagwWith, LineHeightValue * WaterMarkLines);
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        g.DrawString(DataConfig.PrintWaterMark, printTinyFont, Brushes.Black, WaterMarkRectangle, sf);
                    }
                }
                else
                {
                    Rectangle WaterMarkRectangle = new Rectangle(0, XPosition + (LineHeightValue * 2), PagwWith, LineHeightValue);

                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        g.DrawString("...", printTinyFont, Brushes.Black, WaterMarkRectangle, sf);
                    }
                }        

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static void PrintBoxDetail(object sender, PrintPageEventArgs e)
        {
            try
            {
                //Get values
                int maxPageSize = e.PageBounds.Height;
                int XPosition = 0;

                //Set values
                var g = e.Graphics;
                int LineHeightValue = 14;

                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                //PRINT HEADER
                Rectangle HeaderRectangle = new Rectangle(0, 0, PagwWith, 20);

                //Header TEXT print
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    g.DrawString("CUADRE DE CAJA", printHeaderFont, Brushes.Black, HeaderRectangle, sf);
                }

                XPosition = HeaderRectangle.Height + 30; //SET POSITION

              
                Rectangle TicketMoneyRectangle = new Rectangle(0, XPosition, PagwWith, LineHeightValue);

                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Far;
                    g.DrawString("FECHA DE APERTURA:", printFontBold, Brushes.Black, TicketMoneyRectangle); //SUBTOTAL VALUE
                    g.DrawString(NovaAPI.APIBoxMovements.opendate, printFont, Brushes.Black, TicketMoneyRectangle, sf);

                    TicketMoneyRectangle.Y += LineHeightValue;

                    g.DrawString($"REALIZADO POR:", printFontBold, Brushes.Black, TicketMoneyRectangle); //IVA VALUE
                    g.DrawString(NovaAPI.APIBoxMovements.openuser, printFont, Brushes.Black, TicketMoneyRectangle, sf);

                    TicketMoneyRectangle.Y += LineHeightValue;

                    g.DrawString($"FECHA DE CIERRE:", printFontBold, Brushes.Black, TicketMoneyRectangle); //IAC VALUE
                    g.DrawString(NovaAPI.APIBoxMovements.closedate, printFont, Brushes.Black, TicketMoneyRectangle, sf);

                    TicketMoneyRectangle.Y += LineHeightValue;

                    g.DrawString($"REALIZADO POR:", printFontBold, Brushes.Black, TicketMoneyRectangle); //IAC VALUE
                    g.DrawString(NovaAPI.APIBoxMovements.closeuser, printFont, Brushes.Black, TicketMoneyRectangle, sf);

                    TicketMoneyRectangle.Y += LineHeightValue * 2;

                    g.DrawString($"SALDO INICIAL:", printFontBold, Brushes.Black, TicketMoneyRectangle); //IAC VALUE
                    g.DrawString(string.Format("{0:C0}", Convert.ToInt32(NovaAPI.APIBoxMovements.openvalue)), printFont, Brushes.Black, TicketMoneyRectangle, sf);

                    TicketMoneyRectangle.Y += LineHeightValue;

                    g.DrawString($"SALDO FINAL:", printFontBold, Brushes.Black, TicketMoneyRectangle); //IAC VALUE
                    g.DrawString(string.Format("{0:C0}", Convert.ToInt32(NovaAPI.APIBoxMovements.closevalue)), printFont, Brushes.Black, TicketMoneyRectangle, sf);

                    TicketMoneyRectangle.Y += LineHeightValue * 2;

                    g.DrawString($"TOTAL DE VENTA:", printFontBold, Brushes.Black, TicketMoneyRectangle); //IAC VALUE
                    g.DrawString(string.Format("{0:C0}", Convert.ToInt32(NovaAPI.APIBoxMovements.totalsell)), printFont, Brushes.Black, TicketMoneyRectangle, sf);

                    TicketMoneyRectangle.Y += LineHeightValue;

                    g.DrawString($"EFECTIVO:", printFontBold, Brushes.Black, TicketMoneyRectangle); //IAC VALUE
                    g.DrawString(string.Format("{0:C0}", Convert.ToInt32(NovaAPI.APIBoxMovements.cash)), printFont, Brushes.Black, TicketMoneyRectangle, sf);

                    TicketMoneyRectangle.Y += LineHeightValue;

                    g.DrawString($"OTROS:", printFontBold, Brushes.Black, TicketMoneyRectangle); //IAC VALUE
                    g.DrawString(string.Format("{0:C0}", Convert.ToInt32(NovaAPI.APIBoxMovements.others)), printFont, Brushes.Black, TicketMoneyRectangle, sf);

                    TicketMoneyRectangle.Y += LineHeightValue * 2;
                    g.DrawString($"OBSERVACIONES:", printFont, Brushes.Black, TicketMoneyRectangle); //IAC VALUE
                    g.DrawString(NovaAPI.APIBoxMovements.comments, printFont, Brushes.Black, 0, TicketMoneyRectangle.Y + LineHeightValue);

                }


                XPosition += TicketMoneyRectangle.Y + LineHeightValue * 2; //SET POSITION

                int WaterMarkLines = DataConfig.PrintWaterMark.Split('\n').Length;
                Rectangle WaterMarkRectangle = new Rectangle(0, XPosition, PagwWith, LineHeightValue * WaterMarkLines);
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    g.DrawString(DataConfig.PrintWaterMark, printTinyFont, Brushes.Black, WaterMarkRectangle, sf);
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
