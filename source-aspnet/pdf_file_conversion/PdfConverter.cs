using IronPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsgReader.Outlook;
using System.IO;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using BitMiracle.LibTiff.Classic;
using Markdig;
using EmailConcatenation.Converters;
using EmailConcatenation.Interfaces;
using Ninject;
using Grpc.Core.Logging;



namespace EmailConcatenation
{
    public class PdfConverter
    {
        private App _app;


        public PdfConverter()
        {
            IKernel kernel = new StandardKernel();
            kernel.Bind<IGeneralImageConverter>().To<GeneralImageConverter>();
            kernel.Bind<ITIFFConverter>().To<TIFFConverter>();
            kernel.Bind<IFormattedTextConverter>().To<FormattedTextConverter>();
            kernel.Bind<IUndiscoveredTextConverter>().To<UndiscoveredTextConverter>();
            kernel.Bind<IExplicitlyUnsupportedTextConverter>().To<ExplicitlyUnsupportedTextConverter>();
            kernel.Bind<IUnrecognizedTextConverter>().To<UnrecognizedTextConverter>();
            kernel.Bind<IWordConverter>().To<WordConverter>();
            kernel.Bind<IExcelConverter>().To<ExcelConverter>();
            kernel.Bind<IHtmlConverter>().To<HtmlConverter>();
            kernel.Bind<IPDFConverter>().To<PDFConverter>();
            kernel.Bind<IRTFConverter>().To<RTFConverter>();
            kernel.Bind<IEmailTextConverter>().To<EmailTextConverter>();
            kernel.Bind<IExcelXLSMConverter>().To<ExcelXLSMConverter>();
            kernel.Bind<IWordDocConverter>().To<WordDocConverter>();

            kernel.Bind<App>().ToSelf();

            var app = kernel.Get<App>();
            _app = app;
        }

        public PdfDocument Convert(Storage.Message incomingMessage)
        {
            return _app.Convert(incomingMessage);
        }

        public PdfDocument Convert(MemoryStream memoryStream, string fileName)
        {
            return _app.Convert(memoryStream, fileName);
        }

        public PdfDocument CreateEmptyDocument()
        {
            return _app.CreateEmptyDocument();
        }
    }
}
