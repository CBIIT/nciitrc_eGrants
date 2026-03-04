"use client";

import { useState, useRef, useEffect } from "react";
import Link from "next/link";
import type { UserInfo } from "@/lib/types";

interface NavTabsProps {
  user: UserInfo;
  onToggleQuickLinks: () => void;
}

function parseMenuList(menulist: string): { label: string; href: string }[] {
  const items: { label: string; href: string }[] = [];
  for (const entry of menulist.split(",")) {
    const trimmed = entry.trim();
    if (!trimmed) continue;
    const parts = trimmed.split("|");
    const displayName = parts[0]?.trim();
    if (displayName && displayName.length > 1) {
      const route = displayName.replace(/\s+/g, "").toLowerCase();
      items.push({ label: displayName, href: `/${route}` });
    }
  }
  return items;
}

export default function NavTabs({ user, onToggleQuickLinks }: NavTabsProps) {
  const menuItems = parseMenuList(user.menulist || "");
  const [resourcesOpen, setResourcesOpen] = useState(false);
  const [helpOpen, setHelpOpen] = useState(false);
  const resourcesRef = useRef<HTMLLIElement>(null);
  const helpRef = useRef<HTMLLIElement>(null);

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (resourcesRef.current && !resourcesRef.current.contains(e.target as Node)) {
        setResourcesOpen(false);
      }
      if (helpRef.current && !helpRef.current.contains(e.target as Node)) {
        setHelpOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  const isStaff = (user.position_id ?? 0) >= 2;

  return (
    <nav className="bg-[#6b7280]">
      <div className="px-6">
        <ul className="flex list-none flex-wrap items-center gap-0 m-0 p-0">
          <li>
            <button onClick={onToggleQuickLinks} className="nav-link-gray">
              Quick Links &#9662;
            </button>
          </li>

          {menuItems.map((item) => (
            <li key={item.href}>
              <Link href={item.href} className="nav-link-gray">
                {item.label}
              </Link>
            </li>
          ))}

          <li>
            <Link href="/institutional" className="nav-link-gray">
              Institutional Files
            </Link>
          </li>

          <li ref={resourcesRef} className="relative">
            <button
              onClick={() => { setResourcesOpen(!resourcesOpen); setHelpOpen(false); }}
              className="nav-link-gray"
            >
              Resources &#9662;
            </button>
            {resourcesOpen && (
              <div className="dropdown-menu-gray">
                {isStaff && (
                  <Link href="/documents?mode=audit" className="dropdown-item-gray">
                    Audit File Download (Chrome Only)
                  </Link>
                )}
                <a href="/content/eGrants_Category_Glossary.docx" target="_blank" rel="noopener" className="dropdown-item-gray">
                  eGrants Glossary
                </a>
                <a href="/content/access_number2.pdf" target="_blank" rel="noopener" className="dropdown-item-gray">
                  Accession
                </a>
                <Link href="/funding" className="dropdown-item-gray">
                  Funding Files
                </Link>
              </div>
            )}
          </li>

          <li ref={helpRef} className="relative">
            <button
              onClick={() => { setHelpOpen(!helpOpen); setResourcesOpen(false); }}
              className="nav-link-gray"
            >
              Help &#9662;
            </button>
            {helpOpen && (
              <div className="dropdown-menu-gray">
                <a href="mailto:egrantsissues@mail.nih.gov" className="dropdown-item-gray">
                  Technical Support
                </a>
                <a href="/content/eGrants_Help_Guide.pdf" target="_blank" rel="noopener" className="dropdown-item-gray">
                  Help Guide
                </a>
                <a href="/content/explanation.htm" target="_blank" rel="noopener" className="dropdown-item-gray">
                  Icon Guide
                </a>
                <hr className="my-1 mx-3 border-white/10" />
                <a href="/content/privacy.htm" target="_blank" rel="noopener" className="dropdown-item-gray">
                  Privacy Policy
                </a>
              </div>
            )}
          </li>
        </ul>
      </div>
    </nav>
  );
}
