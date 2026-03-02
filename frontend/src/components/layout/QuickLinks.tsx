"use client";

import type { UserInfo } from "@/lib/types";

interface QuickLinksProps {
  user: UserInfo;
  visible: boolean;
  onClose?: () => void;
}

const STAFF_LINKS = [
  { label: "myOGA", href: "https://myoga.cancer.gov", external: true },
  { label: "eRA GM", href: "https://era.nih.gov/eragov/gm/gmhome.htm", external: true },
  { label: "Grant Text Tool", href: "https://granttext.nci.nih.gov", external: true },
  { label: "PMS", href: "https://pms.psc.gov", external: true },
  { label: "OT", href: "https://ot.nci.nih.gov", external: true },
];

const PROGRAM_LINKS = [
  { label: "Grant Text Tool", href: "https://granttext.nci.nih.gov", external: true },
  { label: "PMM", href: "https://pmm.cancer.gov", external: true },
  { label: "QVR", href: "https://qvr.grants.nih.gov", external: true },
  { label: "OT", href: "https://ot.nci.nih.gov", external: true },
];

export default function QuickLinks({ user, visible, onClose }: QuickLinksProps) {
  const isStaff = (user.position_id ?? 0) >= 2;
  const links = isStaff ? STAFF_LINKS : PROGRAM_LINKS;

  if (!visible) return null;

  return (
    <div className="rounded-xl border border-border bg-white shadow-sm overflow-hidden">
      <div className="flex items-center justify-between px-4 py-3 bg-[#f9fafb] border-b border-border">
        <h3 className="text-[13px] font-semibold text-text-primary">Quick Links</h3>
        {onClose && (
          <button
            onClick={onClose}
            className="rounded-full p-0.5 text-text-muted transition-colors hover:bg-surface-alt hover:text-text-primary"
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        )}
      </div>
      <nav className="p-2">
        <ul className="m-0 list-none p-0">
          {links.map((link) => (
            <li key={link.label}>
              <a
                href={link.href}
                target={link.external ? "_blank" : undefined}
                rel={link.external ? "noopener noreferrer" : undefined}
                className="block rounded-md px-3 py-2 text-[13px] font-medium text-primary border-l-2 border-transparent transition-all duration-150 hover:bg-surface-alt hover:border-primary"
              >
                {link.label}
              </a>
            </li>
          ))}
        </ul>
      </nav>
    </div>
  );
}
