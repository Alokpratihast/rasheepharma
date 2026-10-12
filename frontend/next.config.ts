import type { NextConfig } from "next";

const nextConfig: NextConfig = {
 images: {
  remotePatterns: [
    {
      protocol: "https",
      hostname: "rasheepharmastorage01.blob.core.windows.net",
    },
    {
      protocol: "http",
      hostname: "localhost",
      port: "5104",
    },
  ],
},
};

export default nextConfig;