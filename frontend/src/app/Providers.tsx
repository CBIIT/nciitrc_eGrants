"use client";

import { AuthProvider } from "@/contexts/AuthContext";
import AuthGate from "@/components/common/AuthGate";
import type { ReactNode } from "react";

export default function Providers({ children }: { children: ReactNode }) {
  return (
    <AuthProvider>
      <AuthGate>{children}</AuthGate>
    </AuthProvider>
  );
}
