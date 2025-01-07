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
            double pEndX = page.Width - borderX * 2;
            double pEndY = page.Height - borderY * 2;

            double pStartX = borderX;
            double pStartY = borderY;

            using (StreamReader reader = new StreamReader(txtFilePath))
            {
                while (!reader.EndOfStream)
                {
                    List<string> textList = new List<string> { reader.ReadLine() };
                    while (textList.Count() > 0) {
                        string line = textList.First();
                        textList.RemoveAt(0);
                        XSize textSize = gfx.MeasureString(line, font);

                        //* 當文字超過頁高，新增一頁
                        if (pStartY + textSize.Height > pEndY)
                        {
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            textFormatter = new XTextFormatter(gfx);
                            pStartY = borderY;
                        }

                        //* 當文字超過頁寬，進行裁剪後再繪製，剪裁後剩餘的文字加入 List 最頂部
                        if (pStartX + textSize.Width > pEndX)
                        {
                            /* 首先會測量 5 個字元長度有沒有超過頁寬，若沒有則測量 10 個、15 個、20 個... 的情況
                             * 一旦超過，則將測量長度減去 offset 個字元單位
                             * 再以這個長度為基準，加上遞減後的 offet 個字元長度 (4、3、2、1) 做測量
                             * 直到 offet 遞減至 0，獲得的長度即為要剪裁的文字長度
                             **/
                            int offset = 5;
                            int subLineLength = offset;
                            string subLine = line.Substring(0, subLineLength);
                            textSize = gfx.MeasureString(subLine, font);
                            while (offset > 0)
                            {
                                while (pStartX + textSize.Width <= pEndX && subLine.Length + offset <= line.Length)
                                {
                                    subLineLength += offset;
                                    subLine = line.Substring(0, subLineLength);
                                    textSize = gfx.MeasureString(subLine, font);
                                }
                                subLineLength -= offset;
                                offset--;
                                subLine = line.Substring(0, subLineLength);
                                textSize = gfx.MeasureString(subLine, font);
                            }
                            string leftover = line.Substring(subLineLength);
                            textList.Insert(0, leftover);
                            XRect rect = new XRect(pStartX, pStartY, gfx.PageSize.Width - borderX, textSize.Height);
                            textFormatter.DrawString(subLine, font, XBrushes.Black, rect);
                            pStartY += textSize.Height;
                        }
                        else
                        {
                            XRect rect = new XRect(pStartX, pStartY, gfx.PageSize.Width - borderX, textSize.Height);
                            textFormatter.DrawString(line, font, XBrushes.Black, rect);
                            pStartY += textSize.Height;

                        }
                    }
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
