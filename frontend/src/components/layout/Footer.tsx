export default function Footer() {
  return (
    <footer className="mt-12 border-t border-border bg-white text-center text-xs" role="contentinfo">
      <div className="py-4 flex flex-wrap items-center justify-center gap-x-4 gap-y-1 text-text-muted">
        <a
          href="https://www.hhs.gov/"
          target="_blank"
          rel="noopener noreferrer"
          className="hover:text-primary transition-colors"
        >
          U.S. Department of Health and Human Services
        </a>
        <span className="text-border" aria-hidden="true">|</span>
        <a
          href="https://www.nih.gov"
          target="_blank"
          rel="noopener noreferrer"
          className="hover:text-primary transition-colors"
        >
          National Institutes of Health
        </a>
        <span className="text-border" aria-hidden="true">|</span>
        <a
          href="https://www.cancer.gov"
          target="_blank"
          rel="noopener noreferrer"
          className="hover:text-primary transition-colors"
        >
          National Cancer Institute
        </a>
        <span className="text-border" aria-hidden="true">|</span>
        <a
          href="https://usa.gov"
          target="_blank"
          rel="noopener noreferrer"
          className="hover:text-primary transition-colors"
        >
          USA.gov
        </a>
      </div>
    </footer>
  );
}
