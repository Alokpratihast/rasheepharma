import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",
 images: {
  remotePatterns: [
    {
      protocol: "http",
      hostname: "localhost",
      port: "5104",
    },
  ],
},
};

export default nextConfig;
