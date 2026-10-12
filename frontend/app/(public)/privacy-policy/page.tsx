import type { Metadata } from "next";
import Link from "next/link";

import { Container } from "@/components/ui/container";
import { siteConfig } from "@/lib/site";

export const metadata: Metadata = {
  title: "Privacy Policy | RashePharma",
  description:
    "Learn how RashePharma collects, uses and protects information when you use our website, account and services.",
};

// Keep this as a server-rendered public page so visitors and search crawlers can access the policy without signing in.
export default function PrivacyPolicyPage() {
  return (
    <main className="bg-background py-12 sm:py-16">
      <Container>
        <nav aria-label="Breadcrumb" className="mb-8 text-sm text-muted-foreground">
          <Link href="/" className="transition-colors hover:text-primary">Home</Link>
          <span className="px-2" aria-hidden="true">/</span>
          <span aria-current="page">Privacy Policy</span>
        </nav>

        <article className="mx-auto max-w-4xl rounded-3xl border border-border bg-card px-6 py-8 shadow-sm sm:px-10 sm:py-12">
          <p className="text-xs font-bold uppercase tracking-[0.16em] text-primary">Your information</p>
          <h1 className="mt-3 text-3xl font-extrabold tracking-tight text-foreground sm:text-4xl">Privacy Policy</h1>
          <p className="mt-4 text-sm text-muted-foreground">Last updated: October 10, 2026</p>
          <p className="mt-6 text-base leading-8 text-muted-foreground">
            Rashe Lifesciences Pvt. Ltd. (&quot;RashePharma&quot;, &quot;we&quot;, &quot;us&quot; or &quot;our&quot;) respects your privacy. This policy describes the information we may collect when you visit our website, create an account, request a quotation, place an order or contact us, and how we use and safeguard that information.
          </p>

          <div className="mt-10 space-y-9 text-sm leading-7 text-muted-foreground sm:text-base">
            <section>
              <h2 className="text-xl font-bold text-foreground">1. Information we collect</h2>
              <p className="mt-3">Depending on how you use the site, information may include your name, email address, phone number, account details, delivery and billing address, order and payment references, product enquiries, and messages you send to us. We may also receive basic technical information such as IP address, browser and device type, pages requested, and service or error logs.</p>
            </section>

            <section>
              <h2 className="text-xl font-bold text-foreground">2. How we use information</h2>
              <p className="mt-3">We use information to create and secure accounts, respond to enquiries, prepare quotations, process and fulfil orders, coordinate payments and delivery, provide support, maintain our website, prevent misuse, meet legal obligations, and communicate about a request or transaction.</p>
            </section>

            <section>
              <h2 className="text-xl font-bold text-foreground">3. Payments and service providers</h2>
              <p className="mt-3">Checkout may take you to Stripe or another payment provider. Payment information submitted there is handled under that provider’s privacy terms. We may share only the information needed with payment, hosting, delivery, customer-support and other service providers so they can perform services for us. We may also disclose information when required by law or to protect the rights and security of our customers and business.</p>
            </section>

            <section>
              <h2 className="text-xl font-bold text-foreground">4. Cookies and technical data</h2>
              <p className="mt-3">Our website and browser may use essential storage or similar technologies to support sign-in, security, preferences and core site functions. Your browser provides controls for managing cookies and site data; blocking essential storage may prevent some features from working correctly. Technical logs help us diagnose failures and protect the service.</p>
            </section>

            <section>
              <h2 className="text-xl font-bold text-foreground">5. Retention and security</h2>
              <p className="mt-3">We retain information for as long as it is reasonably needed to provide the service, manage orders and enquiries, resolve disputes, maintain security, and meet applicable record-keeping requirements. We use reasonable administrative and technical safeguards, but no internet transmission or storage system can be guaranteed to be completely secure.</p>
            </section>

            <section>
              <h2 className="text-xl font-bold text-foreground">6. Your choices and requests</h2>
              <p className="mt-3">You can review or update some account details through your profile. To ask a privacy question or request access, correction or deletion of information, contact us using the details below. We may need to verify your identity and may retain information where the law or a legitimate business record requires it.</p>
            </section>

            <section>
              <h2 className="text-xl font-bold text-foreground">7. External websites</h2>
              <p className="mt-3">Our site may link to services operated by third parties. Their privacy practices are governed by their own policies, so review those policies before submitting information to them.</p>
            </section>

            <section>
              <h2 className="text-xl font-bold text-foreground">8. Changes to this policy</h2>
              <p className="mt-3">We may revise this policy when our services or practices change. The updated version will be posted on this page with a revised date. Continued use after an update means the updated policy applies to future use of the site, subject to applicable law.</p>
            </section>

            <section className="rounded-2xl bg-primary-light/50 p-5 sm:p-6">
              <h2 className="text-xl font-bold text-foreground">9. Contact us</h2>
              <p className="mt-3">For privacy questions or requests, contact Rashe Lifesciences Pvt. Ltd.:</p>
              <address className="mt-3 space-y-1 not-italic">
                <p>{siteConfig.address.join(" ")}</p>
                <p><a className="font-semibold text-primary hover:underline" href={siteConfig.emailHref}>{siteConfig.email}</a></p>
                <p><a className="font-semibold text-primary hover:underline" href={siteConfig.phoneHref}>{siteConfig.phone}</a></p>
              </address>
            </section>
          </div>
        </article>
      </Container>
    </main>
  );
}
