"use client";

import { useState, useEffect } from "react";
import type { UserInfo } from "@/lib/types";
import Header from "./Header";
import NavTabs from "./NavTabs";
import QuickLinks from "./QuickLinks";
import Footer from "./Footer";

interface AppShellProps {
  user: UserInfo;
  children: React.ReactNode;
}

export default function AppShell({ user, children }: AppShellProps) {
  const [quickLinksVisible, setQuickLinksVisible] = useState(true);

  useEffect(() => {
    const stored = sessionStorage.getItem("QuickLinks");
    if (stored === "hidden") {
      setQuickLinksVisible(false);
    }
  }, []);

  function handleToggleQuickLinks() {
    setQuickLinksVisible((prev) => {
      const next = !prev;
      sessionStorage.setItem("QuickLinks", next ? "visible" : "hidden");
      return next;
    });
  }

  return (
    <div className="flex min-h-screen flex-col bg-surface-alt">
      <Header user={user} />
      <NavTabs user={user} onToggleQuickLinks={handleToggleQuickLinks} />

      <main id="main-content" className="flex-1 bg-white p-6 fade-in transition-all duration-300">
        <div className="flex gap-6">
          {quickLinksVisible && (
            <div className="w-[200px] shrink-0 fade-in">
              <QuickLinks user={user} visible={quickLinksVisible} onClose={handleToggleQuickLinks} />
            </div>
          )}
          <div className="flex-1 min-w-0">
            {children}
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}
