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
using EmailConcatenationPOC.Converters;
using EmailConcatenationPOC.Interfaces;
using Ninject;
using Grpc.Core.Logging;

//using Iron

namespace EmailConcatenationPOC
{
    internal class Program
    {


        static void Main(string[] args)
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

            kernel.Bind<App>().ToSelf();

            var app = kernel.Get<App>();
            app.Run();
        }
    }
}
