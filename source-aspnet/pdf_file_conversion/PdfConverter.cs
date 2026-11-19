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
        // The Ninject kernel and configuration are expensive to build (reads
        // appsettings.json from disk and registers all converter bindings).
        // Building them once and sharing them across instances avoids repeating
        // that work for every file during batch conversions. A fresh App is still
        // resolved per PdfConverter instance so callers can safely create a
        // converter per thread/file without sharing mutable state.
        private static readonly IKernel _kernel = BuildKernel();

        private readonly App _app;

        public PdfConverter()
        {
            _app = _kernel.Get<App>();
        }

        private static IKernel BuildKernel()
        {
            IKernel kernel = new StandardKernel();

            // Build configuration first
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Apply the IronPdf license here so this library is self-sufficient and
            // does not rely on the host (e.g. eGrants Program.cs) having set it. The
            // key is read from the IRONPDF_LICENSE_KEY environment variable (preferred)
            // or the "IronPdf:LicenseKey" configuration entry, so the secret is never
            // stored in source. This runs once because the kernel is built once.
            var ironPdfLicenseKey = configuration["IRONPDF_LICENSE_KEY"]
                ?? configuration["IronPdf:LicenseKey"];

            if (!string.IsNullOrWhiteSpace(ironPdfLicenseKey))
            {
                IronPdf.License.LicenseKey = ironPdfLicenseKey;
            }

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

            return kernel;
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
