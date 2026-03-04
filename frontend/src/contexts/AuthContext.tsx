"use client";

import { createContext, useEffect, useState, type ReactNode } from "react";
import { useRouter } from "next/navigation";
import { getCurrentUser } from "@/lib/api";
import type { UserInfo } from "@/lib/types";

interface AuthContextValue {
  user: UserInfo | null;
  loading: boolean;
}

export const AuthContext = createContext<AuthContextValue>({
  user: null,
  loading: true,
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const router = useRouter();
  const [user, setUser] = useState<UserInfo | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getCurrentUser()
      .then((u) => {
        if (!u.authorized) {
          router.push("/not-authorized");
        } else {
          setUser(u);
        }
      })
      .catch(() => {
        router.push("/not-authorized");
      })
      .finally(() => setLoading(false));
  }, [router]);

  return (
    <AuthContext.Provider value={{ user, loading }}>
      {children}
    </AuthContext.Provider>
  );
}
