"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { getCurrentUser } from "@/lib/api";
import type { UserInfo } from "@/lib/types";

export function useAuth() {
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

  return { user, loading };
}
