import { Plus } from "lucide-react";

import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";

/* First three answers come from the FAQ on the company's existing website. */
const faqs = [
  {
    q: "What is WHO-GMP certification?",
    a: "GMP stands for Good Manufacturing Practices. It is the part of quality assurance that ensures medicinal products are consistently produced and controlled to the quality standards appropriate to their intended use and as required by the product specification.",
  },
  {
    q: "How is Rashe Lifesciences different from big pharma companies?",
    a: "Our business strategy is very different from big pharma. We follow an efficient business model that focuses on keeping treatments for serious illness available to patients.",
  },
  {
    q: "Why did Rashe Lifesciences start?",
    a: "We started with the aim of reaching quality medications to all people and easing the illness of patients, without any difference in their economic conditions.",
  },
  {
    q: "Do you supply internationally?",
    a: "Yes. Our market coverage is worldwide. Share your country and requirement through the enquiry form and our team will get back to you.",
  },
  {
    q: "How do I request a quotation?",
    a: "Use the B2B enquiry form with your product names, quantities and destination. Our team will reply with pricing and product details.",
  },
];

export function AboutFAQ() {
  return (
    <section className="border-t border-border py-20 sm:py-28">
      <Container>
        <div className="grid gap-12 lg:grid-cols-12 lg:gap-16">
          <Reveal className="lg:col-span-5">
            <SectionHeading
              index="08"
              eyebrow="FAQ"
              title="Questions buyers usually ask."
              description="Can't find what you need? Contact our team and we will respond with the right details."
            />
          </Reveal>

          <Reveal className="lg:col-span-7">
            <div className="divide-y divide-border border-y border-border">
              {faqs.map((item) => (
                <details key={item.q} className="group py-1">
                  <summary className="flex cursor-pointer list-none items-center justify-between gap-6 py-5 text-left text-lg font-medium tracking-tight text-foreground marker:hidden [&::-webkit-details-marker]:hidden">
                    {item.q}
                    <Plus
                      aria-hidden="true"
                      className="size-5 shrink-0 text-primary transition-transform duration-200 group-open:rotate-45"
                    />
                  </summary>
                  <p className="max-w-2xl pb-6 text-sm leading-7 text-muted-foreground">
                    {item.a}
                  </p>
                </details>
              ))}
            </div>
          </Reveal>
        </div>
      </Container>
    </section>
  );
}
