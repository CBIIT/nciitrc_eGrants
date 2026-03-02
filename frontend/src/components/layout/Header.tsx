"use client";

import type { UserInfo } from "@/lib/types";

interface HeaderProps {
  user: UserInfo;
}

export default function Header({ user }: HeaderProps) {
  return (
    <header className="bg-white border-b border-border" role="banner">
      <div className="flex items-center justify-between px-6 py-3">
        <div className="flex items-center gap-5">
          <a href="/" title="eGrant Links">
            <img src="/images/eglogo.png" alt="eGrants" width={400} height={50} />
          </a>
        </div>

        <div className="flex items-center gap-2.5">
          <div
            className="flex h-8 w-8 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary"
            aria-hidden="true"
          >
            {(user.full_name || "U").charAt(0).toUpperCase()}
          </div>
          <div className="hidden md:block">
            <div className="text-sm font-medium text-text-primary leading-tight">
              {user.full_name}
            </div>
            <div className="text-[11px] text-text-muted leading-tight">
              version:{user.version}
            </div>
          </div>
        </div>
      </div>
    </header>
  );
}
