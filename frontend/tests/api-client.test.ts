
import { afterEach, describe, expect, it, vi } from "vitest";

const sessionMocks = vi.hoisted(() => ({
  clearStoredAuth: vi.fn(),
  getStoredAuth: vi.fn(),
  getStoredToken: vi.fn(),
  setStoredAuth: vi.fn(),
}));

vi.mock("@/lib/auth/session", () => sessionMocks);

import { apiClient, ApiError } from "@/lib/api/client";

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("API client authentication", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    vi.clearAllMocks();
  });

  it("refreshes after 401 and retries the original request", async () => {
    sessionMocks.getStoredToken.mockReturnValue("expired-token");
    sessionMocks.getStoredAuth.mockReturnValue({
      user: { userId: 7, token: "expired-token" },
      token: "expired-token",
    });

    const fetchMock = vi.fn()
      .mockResolvedValueOnce(jsonResponse({ message: "Unauthorized" }, 401))
      .mockResolvedValueOnce(jsonResponse({ token: "new-access-token" }))
      .mockResolvedValueOnce(jsonResponse({ id: 7, name: "Alok" }));

    vi.stubGlobal("fetch", fetchMock);

    const result = await apiClient<{ id: number; name: string }>("/Auth/profile");

    expect(result).toEqual({ id: 7, name: "Alok" });
    expect(fetchMock).toHaveBeenCalledTimes(3);
    expect(fetchMock.mock.calls[1][0]).toContain("/Auth/refresh");
    expect(fetchMock.mock.calls[2][1]?.headers).toBeDefined();
    expect(sessionMocks.setStoredAuth).toHaveBeenCalled();
  });

  it("does not refresh again when the retried request returns 401", async () => {
    sessionMocks.getStoredToken.mockReturnValue("expired-token");

    const fetchMock = vi.fn()
      .mockResolvedValueOnce(jsonResponse({}, 401))
      .mockResolvedValueOnce(jsonResponse({ token: "new-access-token" }))
      .mockResolvedValueOnce(jsonResponse({}, 401));

    vi.stubGlobal("fetch", fetchMock);

    await expect(apiClient("/Products")).rejects.toBeInstanceOf(ApiError);

    expect(fetchMock).toHaveBeenCalledTimes(3);
  });

  
 
it("shares one refresh request across concurrent 401 responses", async () => {
  sessionMocks.getStoredToken.mockReturnValue("expired-token");
  sessionMocks.getStoredAuth.mockReturnValue({
    user: { userId: 7, token: "expired-token" },
    token: "expired-token",
  });

  let releaseRefresh!: (response: Response) => void;

  const pendingRefresh = new Promise<Response>((resolve) => {
    releaseRefresh = resolve;
  });

  let profileCalls = 0;
  let refreshCalls = 0;

  const fetchMock = vi.fn((url: string | URL | Request) => {
    const requestUrl = String(url);

    if (requestUrl.includes("/Auth/refresh")) {
      refreshCalls++;
      return pendingRefresh;
    }

    if (requestUrl.includes("/Auth/profile")) {
      profileCalls++;

      // First three calls are the initial unauthorized requests.
      if (profileCalls <= 3) {
        return Promise.resolve(
          jsonResponse({ message: "Unauthorized" }, 401),
        );
      }

      // Retried requests succeed.
      return Promise.resolve(
        jsonResponse({ id: 7, name: "Alok" }),
      );
    }

    return Promise.resolve(jsonResponse({ ok: true }));
  });

  vi.stubGlobal("fetch", fetchMock);

  const requests = [
    apiClient("/Auth/profile"),
    apiClient("/Auth/profile"),
    apiClient("/Auth/profile"),
  ];

  // Let initial requests and their 401 handlers run.
  await vi.waitFor(() => {
    expect(refreshCalls).toBe(1);
  });

  releaseRefresh(jsonResponse({ token: "new-access-token" }));

  const results = await Promise.all(requests);

  expect(results).toHaveLength(3);
  expect(refreshCalls).toBe(1);
  expect(profileCalls).toBe(6);
});


});
