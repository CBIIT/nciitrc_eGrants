using System.IO;

using EmailConcatenation.Converters;
using EmailConcatenation.Interfaces;

using IronPdf;

using Microsoft.Extensions.Configuration;

using MsgReader.Outlook;

using Ninject;



namespace EmailConcatenation
{
    public class PdfConverter
    {
        private App _app;


        public PdfConverter()
        {
            IKernel kernel = new StandardKernel();

            // Build configuration first
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Register IConfiguration with Ninject
            kernel.Bind<IConfiguration>().ToConstant(configuration);

            kernel.Bind<IGeneralImageConverter>().To<GeneralImageConverter>();
            kernel.Bind<ITIFFConverter>().To<TIFFConverter>();
            kernel.Bind<IFormattedTextConverter>().To<FormattedTextConverter>();
            kernel.Bind<IWordConverter>().To<WordConverter>();
            kernel.Bind<IHtmlConverter>().To<HtmlConverter>();
            kernel.Bind<IPDFConverter>().To<PDFConverter>();
            kernel.Bind<IRTFConverter>().To<RTFConverter>();
            kernel.Bind<IEmailTextConverter>().To<EmailTextConverter>();
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
    }
}
