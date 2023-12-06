using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using PdfSharp;
using PdfSharp.Drawing.Layout;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Xml.Linq;

namespace PdfSharpExample
{
    internal class Program
    {
        static int Main(string[] args)
        {
            string txtFilePath = GetTxtFilePath();

            if (txtFilePath == null)
            {
                Console.WriteLine("ERROR : There is no \".txt\" file");
                return 1;
            }

            PdfDocument document = new PdfDocument();

            string fontFamily = ConfigurationManager.AppSettings["FONT_FAMILY"];
            double fontSize = Double.Parse(ConfigurationManager.AppSettings["FONT_SIZE"]);
            XFont font = new XFont(fontFamily, fontSize, XFontStyle.Regular);

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XTextFormatter textFormatter = new XTextFormatter(gfx);

            double borderX = Double.Parse(ConfigurationManager.AppSettings["PAGE_BORDER_X"]);
            double borderY = Double.Parse(ConfigurationManager.AppSettings["PAGE_BORDER_Y"]);
            double pEndY = page.Height - borderY * 2;

            double pStartX = borderX;
            double pStartY = borderY;

            using (StreamReader reader = new StreamReader(txtFilePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    XSize textSize = gfx.MeasureString(line, font);
                    //* 當文字超過頁高，新增一頁
                    //* 當文字超過頁高，新增一頁
                    if (pStartY + textSize.Height > pEndY)
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        textFormatter = new XTextFormatter(gfx);
                        pStartY = borderY;
                    }
                    XRect rect = new XRect(pStartX, pStartY, gfx.PageSize.Width-borderX, textSize.Height);
                    textFormatter.DrawString(line, font, XBrushes.Black, rect);
                    pStartY += textSize.Height;
                }
            }

            string pdfFileDir = ConfigurationManager.AppSettings["PDF_FILE_DIR"];
            string pdfFileName = ConfigurationManager.AppSettings["PDF_FILE_NAME"];
            string pdfFilePath = pdfFileDir + "\\" + pdfFileName;

            document.Save(pdfFilePath);
            document.Close();

            return 0;
        }

        /* 獲取 txt 路徑 (若有複數檔案則讀取第一份) **/
        static string GetTxtFilePath()
        {
            string txtFileDir = ConfigurationManager.AppSettings["TXT_FILE_DIR"];
            string txtFilePath;

            try
            {
                string[] fileNameList = Directory.GetFiles(txtFileDir, "*.txt");
                if (fileNameList.Length == 0)
                {
                    return null;
                }
                txtFilePath = fileNameList.First();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR : {0}", ex.ToString());
                return null;
            }

            return txtFilePath;
        }
    }
}
