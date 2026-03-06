"use client";

import Link from "next/link";
import { useAuth } from "@/hooks/useAuth";
import Footer from "@/components/layout/Footer";

export default function AuthGate({ children }: { children: React.ReactNode }) {
  const { loading, authError } = useAuth();

  if (loading) {
    return (
      <div className="flex min-h-screen flex-col bg-surface-alt">
        <header className="bg-white border-b border-border" role="banner">
          <div className="flex items-center px-6 py-3">
            <Link href="/">
              <img src="/images/eglogo.png" alt="eGrants" width={400} height={50} />
            </Link>
          </div>
        </header>
        <main id="main-content" className="flex-1 bg-white p-6 flex items-center justify-center" role="status">
          <div className="flex flex-col items-center gap-3">
            <div className="h-8 w-8 animate-spin rounded-full border-2 border-primary/20 border-t-primary" />
            <span className="text-sm text-text-muted">Loading...</span>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  if (authError) {
    return (
      <div className="flex min-h-screen flex-col bg-surface-alt">
        <header className="bg-white border-b border-border" role="banner">
          <div className="flex items-center px-6 py-3">
            <Link href="/">
              <img src="/images/eglogo.png" alt="eGrants" width={400} height={50} />
            </Link>
          </div>
        </header>
        <main id="main-content" className="flex-1 bg-white p-6 flex items-center justify-center">
          <div
            className="w-full max-w-lg rounded-lg border-2 border-red-300 bg-red-50 p-8 text-center"
            role="alert"
          >
            <svg
              className="mx-auto mb-4 h-12 w-12 text-red-500"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={1.5}
              aria-hidden="true"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126ZM12 15.75h.007v.008H12v-.008Z"
              />
            </svg>
            <h2 className="mb-2 text-xl font-semibold text-red-800">
              Access Denied
            </h2>
            <p className="text-red-700">{authError}</p>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  return <>{children}</>;
}
