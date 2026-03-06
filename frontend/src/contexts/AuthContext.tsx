"use client";

import { createContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { getCurrentUser } from "@/lib/api";
import type { UserInfo } from "@/lib/types";

const SESSION_KEY = "user_info_v1";

interface AuthContextValue {
  user: UserInfo | null;
  loading: boolean;
  authError: string | null;
}

export const AuthContext = createContext<AuthContextValue>({
  user: null,
  loading: true,
  authError: null,
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserInfo | null>(null);
  const [loading, setLoading] = useState(true);
  const [authError, setAuthError] = useState<string | null>(null);

  useEffect(() => {
    const cached = sessionStorage.getItem(SESSION_KEY);
    if (cached) {
      const parsed = JSON.parse(cached) as UserInfo;
      if (parsed.authorized) {
        setUser(parsed);
        setLoading(false);
        return;
      }
    }

    getCurrentUser()
      .then((u) => {
        if (!u.authorized) {
          sessionStorage.removeItem(SESSION_KEY);
          setAuthError("You are not authorized to access this application.");
        } else {
          sessionStorage.setItem(SESSION_KEY, JSON.stringify(u));
          setUser(u);
        }
      })
      .catch(() => {
        sessionStorage.removeItem(SESSION_KEY);
        setAuthError(
          "Unable to verify your credentials. Please try again later."
        );
      })
      .finally(() => setLoading(false));
  }, []);

  const value = useMemo(
    () => ({ user, loading, authError }),
    [user, loading, authError]
  );

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}
